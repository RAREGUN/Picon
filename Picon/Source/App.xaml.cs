using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Picon.Subscripts;

namespace Picon
{
    public partial class App : Application
    {
        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        public static readonly string WorkingDirectory = Directory.GetCurrentDirectory();
        public static readonly string LogPath = WorkingDirectory + "\\logs";
        
        private static App _instance;
        public static App Instance
        {
            get => _instance;
            private set
            {
                if (_instance != null)
                {
                    DC.L("#0_ERR Singleton not null!");
                    return;
                }

                _instance = value;
            }
        }

        public string ImagePath { get; private set; } = "";

        protected override void OnStartup(StartupEventArgs e)
        {
            Stopwatch s = Stopwatch.StartNew();
            DC.L($"OnStartup() called with {(e.Args.Length > 0 ? string.Join(", ", e.Args) : "no")} args!");
            DC.L($"Log init in: {DC.Time(s)}!");
            
            Instance = this;

            if (e.Args.Length > 0)
                ImagePath = e.Args[0];

            base.OnStartup(e);
        }
    }
}