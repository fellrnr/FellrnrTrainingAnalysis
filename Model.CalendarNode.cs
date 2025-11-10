using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.ComponentModel;
using System.Xml.Linq;

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
        public Utils.DateTimeTree DateTimeTree { get; set; } //setter for MemoryPack


        [MemoryPackIgnore]
        public DateTime Date { get { return DateTimeTree.DateTime; } }


        //public string DisplayString { get { return string.Format(DateFormat, DateTime); } }

        [MemoryPackInclude]
        private SortedDictionary<DateTime, Extensible> _children = new SortedDictionary<DateTime, Extensible>();

        [MemoryPackIgnore]
        public IReadOnlyDictionary<DateTime, Extensible> Children 
        { 
            get 
            {
                //do we want to skip levels with one child? Maybe not
                //if (_children.Count == 1)
                //{
                //    Extensible e = _children.First().Value;
                //    if(e is not CalendarNode)
                //        return 
                //}
                        
                return _children; 
            } 
        }

        [MemoryPackIgnore]
        public IReadOnlyCollection<Activity> Activities
        { 
            get 
            { 
                if (_children == null) 
                { 
                    return new List<Activity>().AsReadOnly(); 
                } 
                else 
                {
                    return _children.Values.Cast<Activity>().ToList();
                    //return _children.Values.ToList(); 
                }
            }
        }

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
            Dictionary<string, int> countAccumulators = new Dictionary<string, int>();
            Dictionary<string, int> timeAccumulators = new Dictionary<string, int>();
            Dictionary<string, string> stringAccumulators = new Dictionary<string, string>();
            //iterate bottom up, so we can summarise their summaries
            List<List<Datum>> datumSet = new List<List<Datum>>();
            foreach (KeyValuePair<DateTime, Extensible> kvp in _children)
            {
                Extensible child = kvp.Value;
                List<Datum> datumList = new List<Datum>();
                datumSet.Add(datumList);


                //change to do activity recalculation seperately for multithreading
                if (child is not Activity)
                    child.Recalculate(forceCount, forceJustMe, worker);

                int time = 0;
                if (child.GetNamedDatum(Activity.TagElapsedTime) is TypedDatum<float> timedatum)
                    time = (int)timedatum.Data;

                foreach (string name in child.DataNames)
                {
                    Datum? datum = child.GetNamedDatum(name);
                    if (datum == null)
                        continue;
                    datumList.Add(datum);

                    if (datum is TypedDatum<float>)
                    {
                        TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
                        if (!floatAccumulators.ContainsKey(name))
                            floatAccumulators.Add(name, 0);

                        //TODO: remember to declare in if statment
                        if (ActivityDatumMetadata.FindMetadata(name) is ActivityDatumMetadata metadata && metadata.AggregationMode != null)
                        {
                            switch (metadata.AggregationMode)
                            {
                                case ActivityDatumMetadata.AggregationModeType.Sum:
                                    floatAccumulators[name] += floatDatum.Data;
                                    break;

                                case ActivityDatumMetadata.AggregationModeType.SumDaysThenAverage:
                                    //if we are a day node, then we just sum the values, otherwise we average them
                                    if (this is CalendarNode cn && this.Id().Type == DateTimeTree.DateTreeType.Day)
                                    {
                                        //just sum
                                        floatAccumulators[name] += floatDatum.Data;
                                    }
                                    else
                                    {
                                        //sum the days first, then average
                                        //if (floatDatum.Data != 0)
                                        {
                                            if (!countAccumulators.ContainsKey(name))
                                                countAccumulators.Add(name, 0);
                                            floatAccumulators[name] += floatDatum.Data;
                                            countAccumulators[name]++;
                                        }
                                    }
                                    break;

                                case ActivityDatumMetadata.AggregationModeType.Average:
                                    //if (floatDatum.Data != 0)
                                    {
                                        floatAccumulators[name] += floatDatum.Data;
                                        if (!countAccumulators.ContainsKey(name))
                                            countAccumulators.Add(name, 0);
                                        countAccumulators[name]++;
                                    }
                                    break;
                                case ActivityDatumMetadata.AggregationModeType.TimeWeightedAverage:
                                    //if (floatDatum.Data != 0)
                                    {
                                        floatAccumulators[name] += floatDatum.Data * time;
                                        if (!countAccumulators.ContainsKey(name))
                                            countAccumulators.Add(name, 0);
                                        countAccumulators[name] += time;
                                    }
                                    break;

                                case ActivityDatumMetadata.AggregationModeType.Max:
                                    if (floatAccumulators[name] < floatDatum.Data)
                                        floatAccumulators[name] = floatDatum.Data;
                                    break;
                                case ActivityDatumMetadata.AggregationModeType.Min:
                                    if (floatAccumulators[name] > floatDatum.Data)
                                        floatAccumulators[name] = floatDatum.Data;
                                    break;
                                case ActivityDatumMetadata.AggregationModeType.Ignore:
                                    break;
                                default:
                                    Logging.Instance.Error($"Sunsupported aggregation mode {metadata.AggregationMode} for {name}");
                                    break;
                            }
                        }
                    }
                    else if (datum is TypedDatum<string>)
                    {
                        if (ActivityDatumMetadata.FindMetadata(name) is ActivityDatumMetadata metadata && 
                            metadata.AggregationMode != null && 
                            metadata.AggregationMode == ActivityDatumMetadata.AggregationModeType.Sum)
                        {
                            TypedDatum<string> stringDatum = (TypedDatum<string>)datum;
                            if (!stringAccumulators.ContainsKey(name))
                                stringAccumulators.Add(name, stringDatum.Data);
                            else if (stringAccumulators[name] != stringDatum.Data)
                                stringAccumulators[name] = "mixed";
                        }
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

                if (ActivityDatumMetadata.FindMetadata(name) is ActivityDatumMetadata metadata)
                {
                    //if (metadata.AggregationMode == ActivityDatumMetadata.AggregationModeType.Average ||
                    //   metadata.AggregationMode == ActivityDatumMetadata.AggregationModeType.SumDaysThenAverage)
                    {
                        if (countAccumulators.ContainsKey(name) && countAccumulators[name] > 0)
                        {
                            value = value / countAccumulators[name];
                        }
                    }

                    TypedDatum<float> floatDatum = new TypedDatum<float>(name, false, value);
                    AddOrReplaceDatum(floatDatum);
                }
            }

            string z0 = "3-Zone-0%";
            string z1 = "3-Zone-1%";
            string z2 = "3-Zone-2%";
            string z3 = "3-Zone-3%";
            string z4 = "3-Zone-4%";
            if (floatAccumulators.ContainsKey(z0) || floatAccumulators.ContainsKey(z1) || floatAccumulators.ContainsKey(z2) || floatAccumulators.ContainsKey(z3) || floatAccumulators.ContainsKey(z4))
            {
                if (!floatAccumulators.ContainsKey(z0) || !floatAccumulators.ContainsKey(z1) || !floatAccumulators.ContainsKey(z2) || !floatAccumulators.ContainsKey(z3) || !floatAccumulators.ContainsKey(z4))
                {
                    Logging.Instance.Error($"Missing one of {z0}, {z1}, {z2}, {z3}, {z4}");
                }
                else
                {
                    float z0p = floatAccumulators[z0] / countAccumulators[z0];
                    float z1p = floatAccumulators[z1] / countAccumulators[z1];
                    float z2p = floatAccumulators[z2] / countAccumulators[z2];
                    float z3p = floatAccumulators[z3] / countAccumulators[z3];
                    float z4p = floatAccumulators[z4] / countAccumulators[z4];
                    float total = z0p + z1p + z2p + z3p + z4p;
                    if (total < 99.5 || total > 100.5)
                    {
                        Logging.Instance.Error($"Total is {total} from {z0p}, {z1p}, {z2p}, {z3p}, {z4p}");
                    }

                }
            }

            foreach (KeyValuePair<string, string> kvp in stringAccumulators)
            {
                string name = kvp.Key;
                string value = kvp.Value;
                TypedDatum<string> floatDatum = new TypedDatum<string>(name, false, value);
                AddOrReplaceDatum(floatDatum);
            }

        }

        public override string ToString()
        {
            return string.Format($"CalendarNode {Id()}");
        }

        public const string TagWeight = "Weight";
        public const string TagRestingHeartRate = "Resting Heart Rate";
        public const string TagCriticalPower = "Critical Power";
        public const string TagWPrime = "W Prime";
    }
}
