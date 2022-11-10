using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PiconLibrary.Source
{
    public static class Logger
    {
        private const string LogFileExtension = ".log";
        private static bool Initialized { get; set; }
        private static string LogPath { get; set; }

        // TODO log tags for multiple sources
        public static void Initialize(string workingDirectory, string logDirectoryName)
        {
            if (Initialized)
            {
                Debug.WriteLine("Logger was initialized before!");
                return;
            }
            
            if (!Directory.Exists(workingDirectory))
                throw new Exception("Logger must be initialized with proper working directory!");

            Initialized = true;
            
            LogPath = workingDirectory + "\\" + logDirectoryName;
            
            Directory.CreateDirectory(LogPath);
            
            string latestPath = LogPath + "\\latest" + LogFileExtension;
            
            if (File.Exists(latestPath))
            {
                // Getting first log line representing future name of the old log file
                IEnumerable<string> fileLinesEnumerable = File.ReadLines(latestPath);
                IEnumerator<string> fileLinesEnumerator = fileLinesEnumerable.GetEnumerator();
                string dateName = fileLinesEnumerator.MoveNext() ? fileLinesEnumerator.Current : null;
                fileLinesEnumerator.Dispose();

                string name = string.IsNullOrWhiteSpace(dateName)
                    ? "\\corrupted_" + (int) (DateTime.Now - MicroUtils.Epoch).TotalSeconds
                    : dateName;

                File.Move(latestPath, LogPath + "\\" + name + LogFileExtension);
                
                // TODO clear very old logs
            }
            
            Log(GetDate()); // First line of log, future name of the old log file
        }
        
        public static void Log(string text)
        {
            StreamWriter logFile = null;
                
            try
            {
                logFile = File.AppendText(LogPath + "\\latest" + LogFileExtension);
                logFile.WriteLine(text);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
            finally
            {
                logFile?.Close();
            }
        }

        private static string GetDate()
        {
            DateTime date = DateTime.Now;
	
            return $"{date.Hour:00}h.{date.Minute:00}m.{date.Second:00}s_{date.Day:00}.{date.Month:00}.{date.Year}";
        }
    }
}