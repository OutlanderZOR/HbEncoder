using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ClipManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 1)
                return;
            var path = e.Args[0];
            if (!File.Exists(path))
                return;
            var window = new MainWindow { DataContext = new MainWindowViewModel(path) };
            window.Show();
        }
    }
}
