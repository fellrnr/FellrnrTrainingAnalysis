using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    [MemoryPackUnion(0, typeof(TimeSeriesDelta))]
    [MemoryPackUnion(1, typeof(TimeSeriesCalculated))]
    [MemoryPackUnion(2, typeof(TimeSeriesGradeAdjustedDistance))]
    [MemoryPackUnion(3, typeof(TimeSeriesHeartRatePower))]
    
    public abstract partial class TimeSeriesEphemeral : TimeSeriesBase
    {
        //Note: there is an instance of each TimeSeries object for each activity
        [MemoryPackConstructor]
        protected TimeSeriesEphemeral() { RequiredFields = new List<List<string>>(); ParameterDictionary = new Dictionary<string, float>();} //for use by memory pack deserialization only

        public TimeSeriesEphemeral(string name, List<List<string>> requiredFields, Activity parent, List<string>? opposingFields = null) : base(name, parent)
        {
            RequiredFields = requiredFields;
            OpposingFields = opposingFields;
            ParameterDictionary = new Dictionary<string, float>();
        }

        public override bool IsValid()
        {
            RequiredTimeSeries = new List<TimeSeriesBase>();

            if (ParentActivity == null) return false;

            if (OpposingFields != null)
            {
                foreach (string s in OpposingFields)
                {
                    if (ParentActivity.TimeSeriesNames.Contains(s))
                        return false;
                }
            }

            foreach (List<string> s in RequiredFields)
            {
                bool found = false;
                foreach(string f in s)
                {
                    if (ParentActivity.TimeSeriesNames.Contains(f))
                    {
                        RequiredTimeSeries.Add(ParentActivity.TimeSeries[f]);
                        found = true;
                    }
                }
                if(!found) return false;
            }


            return true;
        }

        public override bool IsVirtual() { return true; }


        //This is horrible, but it allows for extensibility without breaking serialization
        [MemoryPackInclude]
        protected Dictionary<string, float> ParameterDictionary { get; set; } //can't be private 'cos mempack

        protected float Parameter(string name) { if (ParameterDictionary.ContainsKey(name)) return ParameterDictionary[name]; else return 0; }
        protected void Parameter(string name, float value) { ParameterDictionary[name] = value; ; }


        //we don't persist the cached data, but we hang onto it during the run
        //[MemoryPackIgnore]
        [MemoryPackInclude]
        public TimeValueList? CachedData = null;

        [MemoryPackInclude]
        public bool CacheValid = false;

        public abstract TimeValueList? CalculateData(bool forceJustMe);

        public override TimeValueList? GetData()
        {
            if (!CacheValid)
            {
                Logging.Instance.ContinueAccumulator($"GetData-CalculateData");
                Logging.Instance.ContinueAccumulator($"GetData-CalculateData:{Name}");

                CachedData = CalculateData(false);

                Logging.Instance.PauseAccumulator($"GetData-CalculateData");
                Logging.Instance.PauseAccumulator($"GetData-CalculateData:{Name}");
            }
            CacheValid = true; //cache a null result
            //if (CachedData == null)
            //    Logging.Instance.Debug($"No data returned from CalcualteData {this}");
            
            return CachedData;
        }

        [MemoryPackInclude]
        protected List<List<string>> RequiredFields { get; set; }

        [MemoryPackIgnore]
        protected List<TimeSeriesBase> RequiredTimeSeries = new List<TimeSeriesBase>();

        [MemoryPackInclude]
        protected List<string>? OpposingFields { get; set; }

        public override void Recalculate(int forceCount, bool forceJustMe) 
        {
            Logging.Instance.ContinueAccumulator($"TimeSeriesEphemeral");
            Logging.Instance.ContinueAccumulator($"TimeSeriesEphemeral{Name}");
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (force)
            {
                CachedData = CalculateData(forceJustMe);
                CacheValid = true;
                if (CachedData == null && forceJustMe)
                    Logging.Instance.Debug($"No data returned from CalcualteData in Recalculate {this}");
            }
            Logging.Instance.PauseAccumulator($"TimeSeriesEphemeral");
            Logging.Instance.PauseAccumulator($"TimeSeriesEphemeral{Name}");
        }
    }
}
