using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    [MemoryPackUnion(0, typeof(TimeSeriesDelta))]
    [MemoryPackUnion(1, typeof(TimeSeriesGradeAdjustedDistance))]
    [MemoryPackUnion(2, typeof(TimeSeriesHeartRatePower))]
    [MemoryPackUnion(3, typeof(TimeSeriesCalculateAltitude))]
    [MemoryPackUnion(4, typeof(TimeSeriesCalculateDistance))]
    [MemoryPackUnion(5, typeof(TimeSeriesCalculatePower))]

    public abstract partial class TimeSeriesEphemeral : TimeSeriesBase
    {
        //Note: there is an instance of each TimeSeries object for each activity
        [MemoryPackConstructor]
        protected TimeSeriesEphemeral() 
        { 
            ParameterDictionary = new Dictionary<string, float>();
        } //for use by memory pack deserialization only

        public TimeSeriesEphemeral(string name, Activity parent, bool persistCache, List<string>? requiredFields, List<string>? opposingFields = null, List<string>? sportsToInclude = null) : base(name, parent)
        {
            RequiredFields = requiredFields;
            OpposingFields = opposingFields;
            ParameterDictionary = new Dictionary<string, float>();
            SportsToInclude = sportsToInclude;
            PersistCache = persistCache;
        }


        //we don't persist the cached data, but we hang onto it during the run
        //[MemoryPackIgnore]
        [MemoryPackInclude]
        public TimeValueList? CachedData = null;

        [MemoryPackInclude]
        public bool CacheValid = false;

        [MemoryPackInclude]
        public bool PersistCache;

        [MemoryPackInclude]
        protected List<string>? RequiredFields { get; set; }

        [MemoryPackIgnore]
        protected List<TimeSeriesBase> RequiredTimeSeries = new List<TimeSeriesBase>();

        [MemoryPackInclude]
        protected List<string>? OpposingFields { get; set; }

        [MemoryPackInclude]
        protected List<string>? SportsToInclude;

        public override bool IsValid()
        {
            RequiredTimeSeries = new List<TimeSeriesBase>();

            if (ParentActivity == null) return false;

            //check activity doesn't have a recorded version of this
            if(ParentActivity.TimeSeries.ContainsKey(Name) && !ParentActivity.TimeSeries[Name].IsVirtual())
            {
                return false;
            }


            if (SportsToInclude != null && !ParentActivity.CheckSportType(SportsToInclude))
            {
                return false;
            }

            if (OpposingFields != null)
            {
                foreach (string s in OpposingFields)
                {
                    if (ParentActivity.TimeSeriesNames.Contains(s))
                        return false;
                }
            }

            if (RequiredFields != null)
            {
                foreach (string f in RequiredFields)
                {
                    if (ParentActivity.TimeSeriesNames.Contains(f))
                    {
                        RequiredTimeSeries.Add(ParentActivity.TimeSeries[f]);
                    }
                    else
                    {
                        RequiredTimeSeries.Clear();
                        return false;
                    }
                }
            }

            return true;
        }

        public override bool IsVirtual() { return true; }

        public override void PreSerialize() { if (!PersistCache) { CachedData = null; CacheValid = false; } }

        //This is horrible, but it allows for extensibility without breaking serialization
        [MemoryPackInclude]
        protected Dictionary<string, float> ParameterDictionary { get; set; } //can't be private 'cos mempack

        protected float Parameter(string name) { if (ParameterDictionary.ContainsKey(name)) return ParameterDictionary[name]; else return 0; }

        protected void Parameter(string name, float value) { ParameterDictionary[name] = value; ; }

        public abstract TimeValueList? CalculateData(int forceCount, bool forceJustMe);

        public override TimeValueList? GetData(int forceCount, bool forceJustMe)
        {
            if (!CacheValid)
            {
                Logging.Instance.ContinueAccumulator($"GetData-CalculateData");
                Logging.Instance.ContinueAccumulator($"GetData-CalculateData:{Name}");
                if (IsValid()) //RequiredTimeSeries isn't persisted, so we deserialize and it's not there without calling IsValid as a side effect
                    CachedData = CalculateData(forceCount, forceJustMe);
                else
                    CachedData = null;

                Logging.Instance.PauseAccumulator($"GetData-CalculateData");
                Logging.Instance.PauseAccumulator($"GetData-CalculateData:{Name}");
            }
            CacheValid = true; //cache a null result
            //if (CachedData == null)
            //    Logging.Instance.Debug($"No data returned from CalcualteData {this}");
            
            return CachedData;
        }

        public override void Recalculate(int forceCount, bool forceJustMe) 
        {
            Logging.Instance.ContinueAccumulator($"TimeSeriesEphemeral");
            Logging.Instance.ContinueAccumulator($"TimeSeriesEphemeral{Name}");
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (force)
            {
                CachedData = CalculateData(forceCount, forceJustMe);
                CacheValid = true;
                if (CachedData == null && forceJustMe)
                    Logging.Instance.Debug($"No data returned from CalcualteData in Recalculate {this}");
            }
            Logging.Instance.PauseAccumulator($"TimeSeriesEphemeral");
            Logging.Instance.PauseAccumulator($"TimeSeriesEphemeral{Name}");
        }

        public override string ToString()
        {
            return $"TimeSeries: Type {this.GetType().Name} Name {Name}, IsValid {IsValid()}, IsVirtual {IsVirtual()}, CacheValid {CacheValid}, PersistCache {PersistCache}";
        }

    }
}
