using MemoryPack;
using System.ComponentModel;

namespace FellrnrTrainingAnalysis.Model
{

    //this represents the expandable tree of dates for activities
    [MemoryPackable]
    [Serializable]
    public partial class CalendarNode : Extensible
    {
        public CalendarNode(Utils.DateTimeTree dateTimeTree) 
        {
//            DateFormat = dateFormat;
            DateTimeTree = dateTimeTree;
        }

        //public string DateFormat { get;  }

        //public const string FormatAsYear = "yyyy";
        //public const string FormatAsMonth = "MMM yyyy";
        //public const string FormatAsDay = "ddd dd MMM yyyy";

        public override Utils.DateTimeTree Id() { return DateTimeTree; } //Hack to see if tree works

        [MemoryPackInclude]
        public Utils.DateTimeTree DateTimeTree { get; set;  } //setter for MemoryPack

        //public string DisplayString { get { return string.Format(DateFormat, DateTime); } }

        [MemoryPackInclude]
        private SortedDictionary<DateTime, Extensible> _children = new SortedDictionary<DateTime, Extensible>();

        [MemoryPackIgnore]
        public IReadOnlyDictionary<DateTime, Extensible> Children { get { return _children; } }

        public void AddChild(DateTime dateAndTime, Extensible child)
        {
            if (!_children.ContainsKey(dateAndTime))
            {
                _children.Add(dateAndTime, child);
            }
        }

        public bool HasChild(DateTime date)
        {
            return _children.ContainsKey(date);
        }


        public override void Recalculate(int forceCount, bool forceJustMe, BackgroundWorker? worker = null)
        {
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (force)
                base.Clean();


            //TODO: Don't accumulate goal results as they don't add. (Other things may not accumulate either, so we need to make this configurable.)

            //recalculate each field of the calendar node to reflect the underlying children

            Dictionary<string, float> floatAccumulators = new Dictionary<string, float>();
            Dictionary<string, string> stringAccumulators = new Dictionary<string, string>();
            //iterate bottom up, so we can summarise their summaries
            foreach (KeyValuePair<DateTime, Extensible> kvp in _children)
            {
                Extensible child = kvp.Value;
                child.Recalculate(force);
                foreach (string name in child.DataNames)
                {
                    Datum? datum = child.GetNamedDatum(name);
                    if (datum == null)
                        continue;
                    if (datum is TypedDatum<float>)  
                    {
                        TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
                        if (!floatAccumulators.ContainsKey(name))
                            floatAccumulators.Add(name, 0);

                        floatAccumulators[name] += floatDatum.Data; //TODO: add averaging or addition, and by sport type
                    }
                    else if (datum is TypedDatum<string>)
                    {
                        TypedDatum<string> stringDatum = (TypedDatum<string>)datum;
                        if (!stringAccumulators.ContainsKey(name))
                            stringAccumulators.Add(name, stringDatum.Data);
                        else if (stringAccumulators[name] != stringDatum.Data)
                            stringAccumulators[name] = "mixed";
                    }
                    else if (datum is TypedDatum<DateTime>)
                    {
                        if (!stringAccumulators.ContainsKey(name))
                            stringAccumulators.Add(name, ""); //TODO: handle combining dates better in tree structure
                    }
                }
            }
            foreach (KeyValuePair<string, float> kvp in floatAccumulators)
            {
                string name = kvp.Key;
                float value = kvp.Value;
                TypedDatum<float> floatDatum = new TypedDatum<float>(name, false, value);
                AddOrReplaceDatum(floatDatum);
            }
            foreach (KeyValuePair<string, string> kvp in stringAccumulators)
            {
                string name = kvp.Key;
                string value = kvp.Value;
                TypedDatum<string> floatDatum = new TypedDatum<string>(name, false, value);
                AddOrReplaceDatum(floatDatum);
            }

        }


    }
}
