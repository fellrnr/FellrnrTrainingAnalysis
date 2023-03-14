using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis
{
    public class Logging
    {
        private Logging() 
        {
        }

        static Logging() { }
        public static Logging Instance { get; set; } = new Logging();



        private StringBuilder DebugStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter DebugFile { get; } = new("FellrnrTrainingAnalysis_debug.txt");
        private StringBuilder LogStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter LogFile { get; } = new("FellrnrTrainingAnalysis_log.txt");
        private StringBuilder ErrorStringBuilder { get; set; } = new StringBuilder();
        private StreamWriter ErrorFile { get; } = new("FellrnrTrainingAnalysis_error.txt");

        public bool HasErrors { get; set; } = false;

        public void Debug(string message)
        {
            if (Utils.Options.Instance.LogLevel == Utils.Options.Level.Debug)
            {
                if(Utils.Options.Instance.InMemory) DebugStringBuilder.Append(message).Append("\r\n");
                DebugFile.WriteLine(message);
                DebugFile.Flush();
            }
        }
        //Logs also get writting to the Debug
        public void Log(string message)
        {
            if (Utils.Options.Instance.LogLevel == Utils.Options.Level.Debug || Utils.Options.Instance.LogLevel == Utils.Options.Level.Log)
            {
                if (Utils.Options.Instance.InMemory) LogStringBuilder.Append(message).Append("\r\n");
                LogFile.WriteLine(message);
                Debug(message);
            }
        }

        //Errors also get writting to the Log
        public void Error(string message) 
        {
            HasErrors = true;
            if (Utils.Options.Instance.InMemory) ErrorStringBuilder.Append(message).Append("\r\n");
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
