using MemoryPack;
using System.Text;
using static FellrnrTrainingAnalysis.Model.TimeValueList;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    [MemoryPackUnion(0, typeof(TimeSeriesRecorded))]
    [MemoryPackUnion(1, typeof(TimeSeriesDelta))]
    [MemoryPackUnion(2, typeof(TimeSeriesEphemeral))]
    [MemoryPackUnion(3, typeof(TimeSeriesGradeAdjustedPace))]
    [MemoryPackUnion(4, typeof(TimeSeriesHeartRatePower))]
    [MemoryPackUnion(5, typeof(TimeSeriesCalculateAltitude))]
    [MemoryPackUnion(6, typeof(TimeSeriesCalculateSpeed))]
    [MemoryPackUnion(7, typeof(TimeSeriesCalculatePower))]
    [MemoryPackUnion(8, typeof(TimeSeriesWPrimeBalance))]
    [MemoryPackUnion(9, typeof(TimeSeriesPowerEstimateError))]
    [MemoryPackUnion(10, typeof(TimeSeriesIncline))]
    [MemoryPackUnion(11, typeof(TimeSeriesCalculateDistance))]
    [MemoryPackUnion(12, typeof(PowerDistributionCurve))]
    [MemoryPackUnion(13, typeof(TimeSeriesEnergyCostOfRunning))]
    [MemoryPackUnion(14, typeof(TimeSeriesCadencePower))]
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
        public string ToStatisticsString()
        {
            TimeValueList? data = GetData();
            if (data == null || data.Length < 1)
            {
                return $"{Name} has no data";
            }
            else
            {
                return data.ToStatisticsString();
            }
        }

        public float Percentile(StaticsValue staticsValue)
        {
            TimeValueList? data = GetData();
            if (data == null || data.Length < 1)
            {
                return float.MinValue;
            }
            else
            {
                return data.Percentile(staticsValue);
            }
        }
    }
}