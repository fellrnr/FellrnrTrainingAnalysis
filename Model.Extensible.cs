using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.ComponentModel;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    [MemoryPackUnion(0, typeof(Activity))]
    [MemoryPackUnion(1, typeof(Athlete))]
    [MemoryPackUnion(2, typeof(CalendarNode))]
    [MemoryPackUnion(3, typeof(Day))]
    public abstract partial class Extensible
    {

        //change so that the factory returns just the core dictionary of string/datum, then convert that to the actual types.

        public Extensible()
        {
            Data = new Dictionary<string, Datum>();
        }

        [MemoryPackInclude]
        [MemoryPackOrder(20)] //start at 20 to avoid conflict with Activity
        protected Dictionary<string, Datum> Data { get; set; }

        public virtual Utils.DateTimeTree Id() { return new DateTimeTree(); }

        [MemoryPackIgnore]
        public IReadOnlyCollection<string> DataNames { get { return Data.Keys.ToList().AsReadOnly(); } }
        [MemoryPackIgnore]
        public IReadOnlyCollection<Datum> DataValues { get { return Data.Values.ToList().AsReadOnly(); } }

        public Datum? GetNamedDatum(string name) { if (Data.ContainsKey(name)) return Data[name]; else return null; }

        public bool HasNamedDatum(string name) { return Data.ContainsKey(name); }

        public void RemoveNamedDatum(string name) { Data.Remove(name); }

        public string GetNamedDatumForDisplay(string name) { if (Data.ContainsKey(name)) return Data[name].DataAsString()!; else return ""; }

        public string? GetNamedStringDatum(string name) { return HasNamedDatum(name) ? ((TypedDatum<string>)Data[name]).Data : null; }
        public DateTime? GetNamedDateTimeDatum(string name)
        {
            //return HasNamedDatum(name) ? ((TypedDatum<DateTime>)Data[name]).Data : null; 
            if (HasNamedDatum(name))
            {
                Datum datum = Data[name];
                if (datum == null) return null;
                TypedDatum<DateTime> typedDatum = (TypedDatum<DateTime>)datum;
                DateTime dateTime = typedDatum.Data;
                return dateTime;
            }
            else
            {
                return null;
            }
        }
        public float? GetNamedFloatDatum(string name)
        {
            if (HasNamedDatum(name))
            {
                Datum datum = Data[name];
                if (datum == null) return null;
                if (datum is not TypedDatum<float>) return null;
                TypedDatum<float> typedDatum = (TypedDatum<float>)datum;
                float value = typedDatum.Data;
                return value;
            }
            else
            {
                return null;
            }
        }

        protected virtual void NewDatumNameAdded(string name) { }

        public void AddOrReplaceDatum(Datum datum)
        {
            if (!Data.ContainsKey(datum.Name))
            {
                Data.Add(datum.Name, datum);
                NewDatumNameAdded(datum.Name);
            }
            Data[datum.Name] = datum;
        }

        public void ImportDatum<T>(string name, ActivityDatumMapping.DataSourceEnum source, ActivityDatumMapping.LevelType level, T data)
        {
            ActivityDatumMapping? activityDatumMapping = ActivityDatumMapping.MapRecord(source, level, name);
            if (activityDatumMapping != null && activityDatumMapping.Import)
            {
                TypedDatum<T> datum = new TypedDatum<T>(name, true, data);
                if (!Data.ContainsKey(name))
                {
                    Data.Add(name, datum);
                    NewDatumNameAdded(name);
                }
                Data[name] = datum;
            }
        }

        [MemoryPackIgnore]
        protected int LastForceCount = 0;


        public abstract void Recalculate(int forceCount, bool forceJustMe, BackgroundWorker? worker = null);

        public void Recalculate(bool forceJustMe)
        {
            Recalculate(LastForceCount, forceJustMe);
        }

        public void Clean()
        {
            List<string> toDelete = new List<string>();
            foreach (KeyValuePair<string, Datum> kvp in Data)
            {
                if (!kvp.Value.Recorded)
                    toDelete.Add(kvp.Key);
            }
            foreach (string s in toDelete)
                Data.Remove(s);
        }
    }
}
