using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Prism.Commands;
using Shared.Model;

namespace ClipManager
{
    public class MainWindowViewModel
    {
        private readonly string confFilePath;

        public MainWindowViewModel(string path)
        {
            var fi = new FileInfo(path);
            // ReSharper disable once PossibleNullReferenceException
            var confDir = fi.Directory.CreateSubdirectory("conf");
            this.confFilePath = $@"{confDir.FullName}\{Path.GetFileNameWithoutExtension(path)}.xml";
            if (File.Exists(this.confFilePath))
                this.Load(this.confFilePath);
            else
                this.VideoConfig = new VideoConfig();
            this.VideoConfig.VideoFileName = fi.Name;
            this.SaveCommand = new DelegateCommand<Window>(this.Save);
            this.DefaultFileCommand = new DelegateCommand<Window>(this.DefaultFile);
            this.RemoveClipCommand = new DelegateCommand<Clip>(this.RemoveClip);
        }

        public VideoConfig VideoConfig { get; set; }

        public ICommand SaveCommand { get; }

        public ICommand RemoveClipCommand { get; }

        public ICommand DefaultFileCommand { get; }

        private void Load(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                var s = new XmlSerializer(typeof(VideoConfig));
                this.VideoConfig = (VideoConfig)s.Deserialize(fs);
            }
        }

        private void DefaultFile(Window window)
        {
            var clipName = Path.GetFileNameWithoutExtension(this.VideoConfig.VideoFileName);
            if (this.VideoConfig.Clips.All(c => c.Name != clipName))
                this.VideoConfig.Clips.Add(new Clip() { Name = Path.GetFileNameWithoutExtension(this.VideoConfig.VideoFileName) });
            this.Save(window);
        }

        private bool Validar()
        {
            if (this.VideoConfig.Clips.GroupBy(clip => clip.Name).Any(clip => clip.Count() > 1))
            {
                MessageBox.Show("Nome duplicado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            foreach (var clip in this.VideoConfig.Clips)
            {
                TimeSpan ts;
                if ((!string.IsNullOrEmpty(clip.StartAt) && !TimeSpan.TryParse(clip.StartAt, out ts)) ||
                    (!string.IsNullOrEmpty(clip.StopAt) && !TimeSpan.TryParse(clip.StopAt, out ts)))
                {
                    MessageBox.Show("Intervalo inválido, formato esperado HH:mm:ss.", "Aviso", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        private void Save(Window window)
        {
            if (!this.Validar())
                return;
            using (var fs = File.Create(this.confFilePath))
            {
                var s = new XmlSerializer(typeof(VideoConfig));
                s.Serialize(fs, this.VideoConfig);
            }
            window.Close();
        }

        private void RemoveClip(Clip clip)
        {
            this.VideoConfig.Clips.Remove(clip);
        }
    }
}
