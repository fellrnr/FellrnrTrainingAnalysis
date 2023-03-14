using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public abstract class Extensible
    {

        //change so that the factory returns just the core dictionary of string/datum, then convert that to the actual types.

        public Extensible()
        {
            Data = new Dictionary<string, Datum>();
        }

        protected Dictionary<string, Datum> Data { get; }

        public abstract Utils.DateTimeTree Id { get; } //Hack to see if tree works

        public IReadOnlyCollection<string> DataNames { get { return Data.Keys.ToList().AsReadOnly(); } }
        public IReadOnlyCollection<Datum> DataValues { get { return Data.Values.ToList().AsReadOnly(); } }

        public Datum? GetNamedDatum(string name) { if (Data.ContainsKey(name)) return Data[name]; else return null;  }

        public bool HasNamedDatum(string name) { return Data.ContainsKey(name); }
        public string GetNamedDatumForDisplay(string name) { if (Data.ContainsKey(name)) return Data[name].ToString()!; else return ""; }

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

        public virtual void Recalculate(bool force)
        {
            if (force)
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
}
