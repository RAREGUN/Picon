using System.Diagnostics;
using System.Windows;

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
                {
                    Debug.WriteLine("#0_ERR Singleton not null!");
                    return;
                }

                _instance = value;
            }
        }

        public string ImagePath { get; private set; } = "";

        protected override void OnStartup(StartupEventArgs e)
        {
            Instance = this;

            if (e.Args.Length > 0)
                ImagePath = e.Args[0];

            base.OnStartup(e);
        }
    }
}