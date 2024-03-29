using MemoryPack;
using System.Text;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    [MemoryPackUnion(0, typeof(TimeSeriesRecorded))]
    [MemoryPackUnion(1, typeof(TimeSeriesDelta))]
    [MemoryPackUnion(2, typeof(TimeSeriesEphemeral))]
    [MemoryPackUnion(3, typeof(TimeSeriesGradeAdjustedDistance))]
    [MemoryPackUnion(4, typeof(TimeSeriesHeartRatePower))]
    [MemoryPackUnion(5, typeof(TimeSeriesCalculateAltitude))]
    [MemoryPackUnion(6, typeof(TimeSeriesCalculateDistance))]
    [MemoryPackUnion(7, typeof(TimeSeriesCalculatePower))]
    [MemoryPackUnion(8, typeof(TimeSeriesWPrimeBalance))]
    [MemoryPackUnion(9, typeof(TimeSeriesPowerEstimateError))]
    public abstract partial class TimeSeriesBase
    {
        //Note: there is an instance of each TimeSeries object for each activity
        [MemoryPackConstructor]
        protected TimeSeriesBase()  //for use by memory pack deserialization only
        {
            Name = "Memory Pack Default"; //check Name is overritten on memory pack load
        }

        public TimeSeriesBase(string name, Activity parent_)
        {
            Name = name;
            this.parent_ = parent_;
        }


        public abstract TimeValueList? GetData(int forceCount = 0, bool forceJustMe = false);

        public abstract bool IsValid();

        public abstract bool IsVirtual();

        [MemoryPackIgnore]
        public List<Tuple<uint, uint>>? Highlights = null;

        public void AddHighlight(Tuple<uint, uint> area)
        {
            if (Highlights == null)
                Highlights = new List<Tuple<uint, uint>>();
            Highlights.Add(area);
        }

        [MemoryPackInclude]
        public string Name { get; set; } //Ohhh, memory pack requires a public setter! 

        //do a full recalculate (forced) if forceCount is greater than our LastForceCount OR if forceJustMe is true
        public abstract bool Recalculate(int forceCount, bool forceJustMe);

        public void PostDeserialize(Activity parent)
        {
            //Name = name;
            parent_ = parent;
        }

        public virtual void PreSerialize() { }

        [MemoryPackIgnore]
        protected Activity? ParentActivity { get { return parent_; } }
        //[MemoryPackInclude]
        [MemoryPackIgnore]
        private Activity? parent_;

        [MemoryPackIgnore]
        protected int LastForceCount = 0;

        public override string ToString()
        {
            return $"TimeSeries: Type {this.GetType().Name} Name {Name}, IsValid {IsValid()}, IsVirtual {IsVirtual()}";
        }

        //percentiles - min, 0.03, 5, 32, 50, 68, 95, 99.7, max
        public enum StaticsValue { Min, SD3Low, SD2Low, SD1Low, Low10PC, Median, SD1High, High90PC, SD2High, SD3High, Max, StandardDeviation, Mean}
        private float[]? _percentiles;
        private static int StaticsValueLength = Enum.GetNames(typeof(StaticsValue)).Length;
        public static string[] StaticsValueNames = Enum.GetNames(typeof(StaticsValue));
        public static StaticsValue StatisticsValueFromName(string s) { return (StaticsValue)Enum.Parse(typeof(StaticsValue), s);  }

        public string ToStatisticsString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string name in Enum.GetNames<StaticsValue>())
            {  
                stringBuilder.Append($"{name}, {Percentile(StatisticsValueFromName(name))}, ");
            }

            return stringBuilder.ToString();
        }

        public float Percentile(StaticsValue staticsValue)
        {
            
            if (_percentiles == null || _percentiles.Length != StaticsValueLength)
            {
                TimeValueList? data = GetData(forceCount: 0, forceJustMe: false);
                if (data == null || data.Values.Length == 0)
                    return float.MinValue;

                List<float> sorted = data.Values.ToList();
                sorted.Sort();
                _percentiles = new float[StaticsValueLength];
                _percentiles[(int)StaticsValue.Min] = sorted[0];
                _percentiles[(int)StaticsValue.Max] = sorted[sorted.Count - 1];
                _percentiles[(int)StaticsValue.SD3Low] = Utils.TimeSeriesUtils.Percentile(sorted, 0.03f);
                _percentiles[(int)StaticsValue.SD2Low] = Utils.TimeSeriesUtils.Percentile(sorted, 5f);
                _percentiles[(int)StaticsValue.SD1Low] = Utils.TimeSeriesUtils.Percentile(sorted, 32f);
                _percentiles[(int)StaticsValue.Low10PC] = Utils.TimeSeriesUtils.Percentile(sorted, 10f);
                _percentiles[(int)StaticsValue.Median] = Utils.TimeSeriesUtils.Percentile(sorted, 50f);
                _percentiles[(int)StaticsValue.SD1High] = Utils.TimeSeriesUtils.Percentile(sorted, 68f);
                _percentiles[(int)StaticsValue.High90PC] = Utils.TimeSeriesUtils.Percentile(sorted, 90f);
                _percentiles[(int)StaticsValue.SD2High] = Utils.TimeSeriesUtils.Percentile(sorted, 95f);
                _percentiles[(int)StaticsValue.SD3High] = Utils.TimeSeriesUtils.Percentile(sorted, 99.7f);

                float average = sorted.Average();
                _percentiles[(int)StaticsValue.Mean] = average;

                float sum = 0;
                foreach (float f in sorted)
                {
                    float diff = f - average;
                    sum += diff * diff;
                }
                float sd = (float)Math.Sqrt(sum / sorted.Count);
                _percentiles[(int)StaticsValue.StandardDeviation] = sd;
            }
            return _percentiles[(int)staticsValue];
        }

    }
}