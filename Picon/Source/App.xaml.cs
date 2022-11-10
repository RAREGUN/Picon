using System.Diagnostics;
using System.IO;
using System.Windows;
using PiconLibrary.Source;

namespace Picon
{
    public partial class App : Application
    {
        private static App _instance;
        public static App Instance
        {
            get => _instance;
            private set
            {
                if (_instance != null)
                    return;

                _instance = value;
            }
        }
        
        public string WorkingDirectory { get; private set; }
        
        protected override void OnStartup(StartupEventArgs e)
        {
            WorkingDirectory = Directory.GetCurrentDirectory();
            Instance = this;

            Stopwatch s = Stopwatch.StartNew();
            Logger.Initialize(WorkingDirectory, "logs");
            DC.L($"Log init in: {DC.Time(s)}!");
            
            base.OnStartup(e);
        }
    }
}