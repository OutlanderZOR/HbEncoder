using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Shared.Model;

namespace HbEnconder
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new[] {@"D:\shadowplay\Final Fantasy XIV  A Realm Reborn"};
            if (args.Length != 1)
                return;

            var rootDir = new DirectoryInfo(args[0]);
            if (!rootDir.Exists)
                return;
            var renDir = rootDir.CreateSubdirectory("ren");
            var confDir = new DirectoryInfo(rootDir.FullName + "\\conf");
            var configs = Load(confDir.FullName);


            foreach (var config in configs)
            {
                var targetFile = new FileInfo($@"{rootDir.FullName}\{config.VideoFileName}");
                if (!targetFile.Exists)
                    continue;
                Console.WriteLine("Iniciando processamento do arquivo {0}...", targetFile.Name);
                foreach (var clip in config.Clips)
                {
                    var clipFileName = $@"{renDir.FullName}\{clip.Name}.mp4";
                    if (File.Exists(clipFileName))
                    {
                        Console.WriteLine("{0} -> Clip: {1} [JÁ PROCESSADO]", targetFile.Name, clip.Name);
                    }
                    else
                    {
                        Console.WriteLine("{0} -> Clip: {1} [PROCESSANDO]", targetFile.Name, clip.Name);
                        Encode(targetFile.FullName, clipFileName,
                            "x264", "Fast", "High", "5.1", "bframes=2:keyint=30", 10000,
                            "av_acc", 192, "dpl2",
                            clip.StartAt,
                            clip.StopAt);
                    }
                }
            }


            Console.WriteLine("Concluido.");
            Console.ReadKey();
        }

        private static IEnumerable<VideoConfig> Load(string path)
        {
            var s = new XmlSerializer(typeof(VideoConfig));
            foreach (var file in Directory.GetFiles(path, "*.xml"))
            {
                VideoConfig vc;
                using (var fs = File.OpenRead(file))
                    vc = (VideoConfig)s.Deserialize(fs);
                yield return vc;
            }
        }

        private static void Encode(string sourcePath, string outputPath, string vEnconder, string vEncPreset, string vEncProfile, string vEncLevel,
            string vExtraOptions, int vBitrate, string aEnconder, int aBitrate, string mixdown, string startAtStr = null, string stopAtStr = null)
        {
            var args =
                $"-i \"{sourcePath}\" -o \"{outputPath}\" -e {vEnconder} --encoder-preset {vEncPreset} --encoder-profile {vEncProfile} " +
                $"--encoder-level {vEncLevel} -x \"{vExtraOptions}\" -b {vBitrate} --vfr -E {aEnconder} -B {aBitrate} --mixdown {mixdown} ";

            if (!string.IsNullOrEmpty(startAtStr))
            {
                var startAt = TimeSpan.Parse(startAtStr);
                args += $"--start-at duration:{startAt.TotalSeconds} ";
                if (!string.IsNullOrEmpty(stopAtStr))
                {
                    var stopAt = TimeSpan.Parse(stopAtStr);
                    var duration = (int)(stopAt - startAt).TotalSeconds;
                    if (duration <= 0)
                        throw new ArgumentException("Duração do video menor ou igual a zero.", nameof(stopAt));
                    args += $"--stop-at duration:{duration} ";
                }
            }

            using (var p = new Process())
            {
                p.StartInfo.FileName = "HandBrakeCLI.exe";
                p.StartInfo.Arguments = args;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                        return;
                    if (e.Data.Contains("Encoding"))
                    {
                        Console.WriteLine(e.Data);
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                    else
                        Console.WriteLine(e.Data);
                };
                p.Start();
                p.BeginOutputReadLine();
                p.WaitForExit();
            }
        }
    }
}
