using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using RpcSelf;

namespace PiconLauncher
{
    internal class App
    {
#if DEBUG
        
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("psapi.dll")]
        private static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetProcessPath(int pid)
        {
            IntPtr processHandle = OpenProcess(0x1000, false, pid);

            if (processHandle == IntPtr.Zero)
            {
                return null;
            }

            const int lengthSb = 4000;

            StringBuilder sb = new StringBuilder(lengthSb);

            string result = null;

            if (GetModuleFileNameEx(processHandle, IntPtr.Zero, sb, lengthSb) > 0)
            {
                result = sb.ToString();
            }

            CloseHandle(processHandle);

            return result;
        }
#endif

        
        private static RpcSelfClient<string> FilePathSendClient { get; set; }
        private static RpcSelfHost<object> PiconViewerInitializedHost { get; set; }

        public static void Main(string[] args)
        {
            // Must be path to opened image only
            if (args.Length != 1)
                return;

            string filePath = args[0];
            
            // Checks for file existence
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(args[0]))
                return;
            
            // Creating client for sending image path to PiconViewer
            FilePathSendClient = new RpcSelfClient<string>(11100);

            // Trying to send image path, otherwise we need to start PiconViewer manually
            if (FilePathSendClient.SendData(filePath))
                return;
            
            const string piconViewerName = "Picon.exe";
            string workingDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            string piconViewerExecutablePath = workingDirectory + "\\" + piconViewerName;
            Process piconViewerProcess = null;
            
            #if DEBUG

            Stopwatch s = Stopwatch.StartNew();

            // Wait until PiconViewer be started manually (for debug reasons)
            while (piconViewerProcess == null && s.Elapsed.TotalMinutes >= 1)
            {
                foreach (Process process in Process.GetProcesses())
                {
                    try
                    {
                        string processPath = GetProcessPath(process.Id);
                        
                        if (processPath != piconViewerExecutablePath)
                            continue;
                        
                        piconViewerProcess = process;
                        break;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                        piconViewerProcess = null;
                    }
                }
                
                if (piconViewerProcess == null)
                    Thread.Sleep(100);
            }
            
            #else
            
            Console.WriteLine("new Process()...");
            // Process of creating process...
            piconViewerProcess = new Process();
            piconViewerProcess.StartInfo.FileName = piconViewerExecutablePath;
            /*piconViewerProcess.StartInfo.Arguments = "-n";*/
            /*piconViewerProcess.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;*/
            Console.WriteLine("piconViewerProcess.Start()...");
            piconViewerProcess.Start();
            
            #endif

            // We are started a process of PiconViewer, but we need to wait until it fully initialized
            // And because of it we are creating Host which can receive signal about successfully initialized PiconViewer
            PiconViewerInitializedHost = new RpcSelfHost<object>(11101);
            
            PiconViewerInitializedHost.MessageReceived += (sender, messageArgs) =>
            {
                // When signal of initialization received we are sending information about image path back
                FilePathSendClient.SendData(filePath);
            };
            
            if (FilePathSendClient.SendData(filePath))
                PiconViewerInitializedHost.Close();
        }
    }
}