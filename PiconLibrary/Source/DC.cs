using System;
using System.Diagnostics;

namespace PiconLibrary.Source
{
    public static class DC
    {
        public static void L(string content)
        {
            Output(content);
        }
	
        public static void L(bool assert, string content)
        {
            if (assert)
                Output(content);
        }
	
        private static void Output(string content)
        {
            string value = $"[{GetTime()}] - " + content;
		
            Debug.WriteLine(value);
            Logger.Log(value);
        }

        public static string Time(double milliseconds)
        {
            return Time(TimeSpan.FromTicks((long) (milliseconds * TimeSpan.TicksPerMillisecond)));
        }
	
        public static string Time(Stopwatch stopwatch)
        {
            return Time(stopwatch.Elapsed);
        }
	
        public static string Time(TimeSpan timeSpan)
        {
            return $"{Math.Truncate(timeSpan.TotalMilliseconds)}.{timeSpan.Ticks % 10000:0000}ms";
        }
	
        public static string GetTime()
        {
            return $"{DateTime.Now.Hour:00}:{DateTime.Now.Minute:00}:{DateTime.Now.Second:00}.{DateTime.Now.Millisecond:0000}";
        }
    }
}