using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;

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
        private Dictionary<string, Stopwatch> accumulators = new Dictionary<string, Stopwatch>();

        private StringBuilder DebugStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter DebugFile { get; } = new("FellrnrTrainingAnalysis_debug.txt");
        private StringBuilder LogStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter LogFile { get; } = new("FellrnrTrainingAnalysis_log.txt");
        private StringBuilder ErrorStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter ErrorFile { get; } = new("FellrnrTrainingAnalysis_error.txt");

        public bool HasErrors { get; set; } = false;

        private int depth = 0;
        private Stack<string> names = new Stack<string>();

        private Stopwatch Timer(string name) { if (!timing.ContainsKey(name)) timing.Add(name, new Stopwatch()); return timing[name]; }
        private Stopwatch Accumulator(string name) { if (!accumulators.ContainsKey(name)) accumulators.Add(name, new Stopwatch()); return accumulators[name]; }

        public void ResetAndStartTimer(string name = "") { Timer(name).Reset(); Timer(name).Start(); }

        public void ContinueAccumulator(string name) { Accumulator(name).Start(); }
        public void PauseAccumulator(string name) { Accumulator(name).Stop(); }

        public void DumpAndResetAccumulators()
        {
            Debug($"Accumulation timers {accumulators.Count}");
            foreach (KeyValuePair<string, Stopwatch> kvp in accumulators)
            {
                Stopwatch sw = kvp.Value;
                TimeSpan ts = sw.Elapsed;
                string seconds = new string('#', (int)ts.TotalSeconds);
                Debug($"{kvp.Key} - {ts} {seconds}");
                sw.Reset();
            }
            accumulators.Clear();
        }

        public void TraceEntry(string name, bool announce = true)
        {
            depth++;
            ResetAndStartTimer(name);
            if (announce) Log("Entering " + name);
            names.Push(name);
        }

        public void TraceLeave()
        {
            string name = names.Pop();
            string ts = Logging.Instance.GetAndResetTime(name);
            Logging.Instance.Log($"{name} took {ts} took");
            depth--;
        }


        public string GetAndResetTime(string name = "") 
        { 
            Timer(name).Stop(); 
            TimeSpan retval = Timer(name).Elapsed; 
            Timer(name).Reset(); 
            Timer(name).Start();
            
            string seconds = new string('#', (int)retval.TotalSeconds);
            
            return $"{retval} {seconds}"; 
        }
        public TimeSpan GetAndStopTime(string name = "") { Timer(name).Stop(); TimeSpan retval = Timer(name).Elapsed; return retval; }

        public void Debug(string message)
        {
            if (Options.Instance.LogLevel == Options.Level.Debug)
            {
                if(Options.Instance.InMemory) DebugStringBuilder.Append("DEBUG: ").Append(new string('>', depth)).Append(message).Append("\r\n");
                DebugFile.WriteLine(message);
                DebugFile.Flush();
            }
        }
        //Logs also get writting to the Debug
        public void Log(string message)
        {
            if (Options.Instance.LogLevel == Options.Level.Debug || Options.Instance.LogLevel == Options.Level.Log)
            {
                if (Options.Instance.InMemory) LogStringBuilder.Append("LOG: ").Append(new string('>', depth)).Append(message).Append("\r\n");
                LogFile.WriteLine(message);
                Debug(message);
            }
        }

        //Errors also get writting to the Log
        public void Error(string message) 
        {
            HasErrors = true;
            if (Options.Instance.InMemory) ErrorStringBuilder.Append("ERROR: " ).Append(new string('>', depth)).Append(message).Append("\r\n");
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
