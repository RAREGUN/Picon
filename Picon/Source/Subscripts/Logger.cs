using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Picon.Subscripts
{
    public static class Logger
    {
        private const string LogFileExtension = ".log";

        static Logger()
        {
            Directory.CreateDirectory(App.LogPath);
            
            string latestPath = App.LogPath + "\\latest" + LogFileExtension;
            
            if (File.Exists(latestPath))
            {
                // Getting first log line representing future name of the old log file
                IEnumerable<string> fileLinesEnumerable = File.ReadLines(latestPath);
                IEnumerator<string> fileLinesEnumerator = fileLinesEnumerable.GetEnumerator();
                string dateName = fileLinesEnumerator.MoveNext() ? fileLinesEnumerator.Current : null;
                fileLinesEnumerator.Dispose();

                string name = string.IsNullOrWhiteSpace(dateName)
                    ? "\\corrupted_" + (int) (DateTime.Now - App.Epoch).TotalSeconds
                    : dateName;

                File.Move(latestPath, App.LogPath + "\\" + name + LogFileExtension);
                
                // TODO clear very old logs
            }
            
            Log(GetDate()); // First line of log, future name of the old log file
        }
        
        public static void Log(string text)
        {
            StreamWriter logFile = null;
                
            try
            {
                logFile = File.AppendText(App.LogPath + "\\latest" + LogFileExtension);
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