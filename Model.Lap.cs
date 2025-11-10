using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    //We could make this a subclass of extensible, but it's rather more fixed for now
    [Serializable]
    [MemoryPackable]
    public partial class Lap
    {
        public Lap(TimeSpan elapsedTime, int startSeconds, int endSeconds)
        {
            ElapsedTime = elapsedTime;
            StartSeconds = startSeconds;
            EndSeconds = endSeconds;
            Name = "";
            if (elapsedTime < TimeSpan.Zero)
            {
                throw new Exception("Negative elapsed time");
                //Logging.Instance.Error("Negative elapsed time");
            }
            if (endSeconds < startSeconds)
            {
                throw new Exception($"endSeconds > startSeconds (${endSeconds} < ${startSeconds})");
                //Logging.Instance.Error($"endSeconds > startSeconds (${endSeconds} < ${startSeconds})");
            }
        }

        public TimeSpan ElapsedTime { get; set; }

        public int StartSeconds { get; set; }
        public int EndSeconds { get; set; }

        [MemoryPackIgnore]
        public int LapSeconds => EndSeconds - StartSeconds;

        [MemoryPackIgnore]
        public TimeSpan LapTime => new TimeSpan(LapSeconds * 10000000L);

        public string Name { get; set; }

        public float? AverageCadence { get; set; }

        public float? AverageSpeed { get; set; }

        public float? Distance { get; set; }

        public float? AverageHeartrate { get; set; }

        public override string ToString()
        {
            return $"Lap ElapsedTime {ElapsedTime}, StartSeconds {StartSeconds}, EndSeconds {EndSeconds}";
        }

    }
}
