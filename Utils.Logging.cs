using System.Diagnostics;
using System.Text;

namespace FellrnrTrainingAnalysis.Utils
{
    public class Logging
    {
        private Logging() 
        {
        }

        static Logging() { }
        public static Logging Instance { get; set; } = new Logging();

        private Dictionary<string, Stopwatch> timing = new Dictionary<string, Stopwatch> { { "", new Stopwatch() } };

        private StringBuilder DebugStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter DebugFile { get; } = new("FellrnrTrainingAnalysis_debug.txt");
        private StringBuilder LogStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter LogFile { get; } = new("FellrnrTrainingAnalysis_log.txt");
        private StringBuilder ErrorStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter ErrorFile { get; } = new("FellrnrTrainingAnalysis_error.txt");

        public bool HasErrors { get; set; } = false;

        private Stopwatch Timer(string name) { if (!timing.ContainsKey(name)) timing.Add(name, new Stopwatch());  return timing[name]; }

        public void StartTimer(string name = "") { Timer(name).Reset(); Timer(name).Start(); }

        public TimeSpan GetAndResetTime(string name = "") { Timer(name).Stop(); TimeSpan retval = Timer(name).Elapsed; Timer(name).Reset(); Timer(name).Start();  return retval; }

        public void Debug(string message)
        {
            if (Options.Instance.LogLevel == Options.Level.Debug)
            {
                if(Options.Instance.InMemory) DebugStringBuilder.Append("DEBUG: ").Append(message).Append("\r\n");
                DebugFile.WriteLine(message);
                DebugFile.Flush();
            }
        }
        //Logs also get writting to the Debug
        public void Log(string message)
        {
            if (Options.Instance.LogLevel == Options.Level.Debug || Options.Instance.LogLevel == Options.Level.Log)
            {
                if (Options.Instance.InMemory) LogStringBuilder.Append("LOG: ").Append(message).Append("\r\n");
                LogFile.WriteLine(message);
                Debug(message);
            }
        }

        //Errors also get writting to the Log
        public void Error(string message) 
        {
            HasErrors = true;
            if (Options.Instance.InMemory) ErrorStringBuilder.Append("ERROR: " ).Append(message).Append("\r\n");
            ErrorFile.WriteLine(message);
            Log(message); 
        }

        public string Debug() { return DebugStringBuilder.ToString(); }
        public string Log() { return LogStringBuilder.ToString(); }
        public string Error() { return ErrorStringBuilder.ToString(); }


        public void Clear() { DebugStringBuilder.Clear(); LogStringBuilder.Clear(); ErrorStringBuilder.Clear(); }


        public void Close() { DebugFile.Close(); LogFile.Close(); ErrorFile.Close(); }
    }
}
