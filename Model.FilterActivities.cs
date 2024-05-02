using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json.Serialization;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class FilterActivities
    {
        private const string fileName = @"FellrnrFilter.bin";
        static string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string AppDataSubFolder = "FellrnrTrainingData";
        static string AppDataPath = Path.Combine(AppDataFolder, AppDataSubFolder);


        //todo: the serialization doesn't work, lists of polymorphic types is beyond
        public static FilterActivities? LoadFilters()
        {
            string path = Path.Combine(AppDataPath, fileName);
            if (!File.Exists(path))
                return null;

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)) //TODO: allow for loading specific database file, and history of files
            {
#pragma warning disable SYSLIB0011
                Object deserialized = formatter.Deserialize(stream);
                if (deserialized != null && deserialized is Model.FilterActivities)
                {
                    FilterActivities retval = (FilterActivities)deserialized;
                    return retval;
                }
#pragma warning restore SYSLIB0011
                return null;

                //var options = new JsonSerializerOptions { IncludeFields = true, };
                //string path = Path.Combine(AppDataPath, fileName);
                //if (File.Exists(path))
                //{
                //    string jsonFromFile = File.ReadAllText(path);
                //    FilterActivities? Filters = JsonSerializer.Deserialize<FilterActivities>(jsonFromFile, options);
                //    if (Filters != null)
                //    {
                //        return Filters;
                //    }
                //    else
                //    {
                //        Logging.Instance.Error(string.Format("Failed to deserialize options from {0}", path));
                //    }
                //}
                //else
                //{
                //    Logging.Instance.Error(string.Format("File does not exist for options - {0}", path));
                //}
                //return null;
            }
        }

        public void SaveFilters()
        {
            //var options = new JsonSerializerOptions { IncludeFields = true, };
            //string json = JsonSerializer.Serialize(this, options);
            string path = Path.Combine(AppDataPath, fileName);
            //File.WriteAllText(path, json);

            IFormatter formatter = new BinaryFormatter();
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
#pragma warning disable SYSLIB0011
                formatter.Serialize(stream, this);
#pragma warning restore SYSLIB0011
            }
        }


        public List<FilterBase> Filters { get; set; } = new List<FilterBase>();

        public List<Activity> GetActivities(Database database)
        {
            if (database == null || database.CurrentAthlete == null) { return new List<Activity>(); }

            Athlete athlete = database.CurrentAthlete;
            List<Activity> activities = athlete.ActivitiesByUTCDateTime.Values.ToList();

            foreach (FilterBase filter in Filters)
            {
                activities = filter.GetActivities(activities); //TODO support OR as well as AND for filters
            }

            return activities;
        }
    }


    [Serializable]
    public class FilterBase
    {
        public string Tag;

        public FilterBase(string tag)
        {
            Tag = tag;
        }

        //note, always return the list sorted by start date/time
        public virtual List<Activity> GetActivities(List<Activity> activities)
        {
            return activities;
        }
    }



    [Serializable]
    public class FilterDateTime : FilterBase
    {
        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">", "between", "1M", "6M", "1Y", "in", "has", "missing" };

        public FilterDateTime(string fieldName, string command, DateTime? startDateTime, DateTime? endDateTime, string? list) : base(fieldName)
        {
            if (command == "in" && list != null)
            {
                string[] strings = list.Split(',');
                ListOfDates = new List<DateTime>();
                foreach (string s in strings)
                {
                    string ds = s.Trim();
                    if (string.IsNullOrEmpty(ds)) continue;
                    DateTime dateTime;
                    if (DateTime.TryParse(ds, out dateTime))
                        ListOfDates.Add(dateTime);
                }
            }
            else
            {
                StartDateTime = startDateTime;
                EndDateTime = endDateTime;
            }
            Command = command;
            FieldName = fieldName;
        }

        [JsonInclude]
        private string FieldName { get; set; }
        [JsonInclude]
        public string Command { get; set; }

        [JsonInclude]
        public DateTime? StartDateTime { get; set; }
        [JsonInclude]
        public DateTime? EndDateTime { get; set; }
        [JsonInclude]
        public List<DateTime>? ListOfDates;

        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {
                DateTime? dateTime = activity.GetNamedDateTimeDatum(FieldName);

                bool addIt = false;
                switch (Command)
                {
                    case "<":
                        if (dateTime.HasValue && StartDateTime.HasValue && dateTime.Value < StartDateTime)
                            addIt = true;
                        break;
                    case "<=":
                        if (dateTime.HasValue && StartDateTime.HasValue && dateTime.Value <= StartDateTime)
                            addIt = true;
                        break;
                    case "=":
                        if (dateTime.HasValue && StartDateTime.HasValue && dateTime.Value.Date == StartDateTime.Value.Date) //lets do equals as on the day
                            addIt = true;
                        break;
                    case ">=":
                        if (dateTime.HasValue && StartDateTime.HasValue && dateTime.Value >= StartDateTime)
                            addIt = true;
                        break;
                    case ">":
                        if (dateTime.HasValue && StartDateTime.HasValue && dateTime.Value > StartDateTime)
                            addIt = true;
                        break;
                    case "between":
                        if (dateTime.HasValue && StartDateTime.HasValue && EndDateTime.HasValue && dateTime.Value >= StartDateTime && dateTime.Value <= EndDateTime)
                            addIt = true;
                        break;
                    case "has":
                        if (dateTime.HasValue)
                            addIt = true;
                        break;
                    case "missing":
                        if (!dateTime.HasValue)
                            addIt = true;
                        break;
                    case "1M":
                        if (dateTime.HasValue && dateTime.Value.AddMonths(1) >= DateTime.Now)
                            addIt = true;
                        break;
                    case "6M":
                        if (dateTime.HasValue && dateTime.Value.AddMonths(6) >= DateTime.Now)
                            addIt = true;
                        break;
                    case "1Y":
                        if (dateTime.HasValue && dateTime.Value.AddMonths(12) >= DateTime.Now)
                            addIt = true;
                        break;
                    case "in":
                        if (dateTime.HasValue && ListOfDates != null && ListOfDates.Count > 0)
                        {
                            foreach (DateTime dt in ListOfDates)
                            {
                                if (dt.Date == dateTime.Value.Date)
                                {
                                    addIt = true; break;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (addIt)
                {
                    returnActivities.Add(activity);
                }
            }
            return returnActivities;
        }
    }

    [Serializable]
    public class FilterFloat : FilterBase
    {
        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">", "between", "has", "missing" };


        public FilterFloat(string fieldName, string command, float? firstValue, float? secondValue) : base(fieldName)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
            Command = command;
            FieldName = fieldName;
        }

        [JsonInclude]
        private string FieldName { get; set; }
        [JsonInclude]
        public string Command { get; set; }

        [JsonInclude]
        public float? FirstValue { get; set; }
        [JsonInclude]
        public float? SecondValue { get; set; }


        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {
                float? value = activity.GetNamedFloatDatum(FieldName);

                bool addIt = false;
                switch (Command)
                {
                    case "<":
                        if (value.HasValue && FirstValue.HasValue && value.Value < FirstValue)
                            addIt = true;
                        break;
                    case "<=":
                        if (value.HasValue && FirstValue.HasValue && value.Value <= FirstValue)
                            addIt = true;
                        break;
                    case "=":
                        if (value.HasValue && FirstValue.HasValue && value.Value == FirstValue) //lets do equals as on the day
                            addIt = true;
                        break;
                    case ">=":
                        if (value.HasValue && FirstValue.HasValue && value.Value >= FirstValue)
                            addIt = true;
                        break;
                    case ">":
                        if (value.HasValue && FirstValue.HasValue && value.Value > FirstValue)
                            addIt = true;
                        break;
                    case "between":
                        if (value.HasValue && FirstValue.HasValue && SecondValue.HasValue && value.Value >= FirstValue && value.Value <= SecondValue)
                            addIt = true;
                        break;
                    case "missing":
                        if (!value.HasValue)
                            addIt = true;
                        break;
                    case "has":
                        if (value.HasValue)
                            addIt = true;
                        break;
                    default:
                        break;
                }

                if (addIt)
                {
                    returnActivities.Add(activity);
                }
            }
            return returnActivities;
        }
    }

    [Serializable]
    public class FilterTimeSeries : FilterBase
    {
        public static readonly string[] FilterCommands = new string[] { "",
            "max <", "max <=", "max =", "max >=", "max >", "max between",
            "avg <", "avg <=", "avg =", "avg >=", "avg >", "avg between",
            "min <", "min <=", "min =", "min >=", "min >", "min between", "has", "missing", "virtual", "not virtual" };


        public FilterTimeSeries(string fieldName, string command, float? firstValue, float? secondValue) : base(fieldName)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
            Command = command;
            FieldName = fieldName;
        }

        [JsonInclude]
        private string FieldName { get; set; }
        [JsonInclude]
        public string Command { get; set; }

        [JsonInclude]
        public float? FirstValue { get; set; }
        [JsonInclude]
        public float? SecondValue { get; set; }


        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {

                float? value;
                bool isvirtual = false;

                if (!activity.TimeSeries.ContainsKey(FieldName))
                {
                    value = null;
                }
                else
                {
                    TimeSeriesBase datastream = activity.TimeSeries[FieldName];
                    TimeValueList? dataTuple = datastream.GetData(forceCount: 0, forceJustMe: false);
                    if (dataTuple == null)
                    {
                        value = null;
                    }
                    else
                    {
                        isvirtual = datastream.IsVirtual();
                        float[] data = dataTuple.Values;
                        string statistic = Command.Substring(0, 3);
                        if (statistic == "max")
                            value = data.Max();
                        else if (statistic == "avg")
                            value = data.Average();
                        else if (statistic == "min")
                            value = data.Min();
                        else
                            value = 0; //the presence of data will be enough
                    }

                }

                bool addIt = false;

                if (Command == "has")
                {
                    addIt = (value != null);
                }
                else if (Command == "missing")
                {
                    addIt = (value == null);
                }
                else if (Command == "virtual")
                {
                    addIt = (value != null && isvirtual);
                }
                else if (Command == "not virtual")
                {
                    addIt = (value != null && !isvirtual);
                }
                else
                {
                    string operation = Command.Substring(4);
                    switch (operation)
                    {
                        case "<":
                            if (value.HasValue && FirstValue.HasValue && value.Value < FirstValue)
                                addIt = true;
                            break;
                        case "<=":
                            if (value.HasValue && FirstValue.HasValue && value.Value <= FirstValue)
                                addIt = true;
                            break;
                        case "=":
                            if (value.HasValue && FirstValue.HasValue && value.Value == FirstValue) //lets do equals as on the day
                                addIt = true;
                            break;
                        case ">=":
                            if (value.HasValue && FirstValue.HasValue && value.Value >= FirstValue)
                                addIt = true;
                            break;
                        case ">":
                            if (value.HasValue && FirstValue.HasValue && value.Value > FirstValue)
                                addIt = true;
                            break;
                        default:
                            break;
                    }
                }
                if (addIt)
                {
                    returnActivities.Add(activity);
                }
            }
            return returnActivities;
        }
    }

    [Serializable]

    public class FilterString : FilterBase
    {
        public const string IN = "in"; //used to fix search results to a list of strava ids
        public static readonly string[] filterCommands = new string[] { "", "=", "contains", "doesn't contain", "has", "missing", IN };


        public FilterString(string fieldName, string command, string? value) : base(fieldName)
        {
            Value1 = value;
            Command = command;
            FieldName = fieldName;
        }

        [JsonInclude]
        private string FieldName { get; set; }
        [JsonInclude]
        public string Command { get; set; }

        [JsonInclude]
        public string? Value1 { get; set; }


        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {
                string? value = activity.GetNamedStringDatum(FieldName);

                bool addIt = false;
                switch (Command)
                {
                    case "missing":
                        if (value == null)
                            addIt = true;
                        break;
                    case "has":
                        if (value != null)
                            addIt = true;
                        break;
                    case "contains":
                        if (value != null && Value1 != null && value.ToLower().Contains(Value1.ToLower()))
                            addIt = true;
                        break;
                    case "=":
                        if (value != null && Value1 != null && value.ToLower() == Value1.ToLower())
                            addIt = true;
                        break;
                    case "doesn't contain":
                        if (value != null && Value1 != null && !value.ToLower().Contains(Value1.ToLower()))
                            addIt = true;
                        break;
                    case "in":
                        if (Value1 != null && value != null)
                        {
                            string[] inList = Value1.Split(',');
                            foreach (string s in inList)
                            {
                                if (value.ToLower() == s.ToLower())
                                {
                                    addIt = true;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (addIt)
                {
                    returnActivities.Add(activity);
                }
            }
            return returnActivities;
        }
    }

    [Serializable]
    public class FilterBadData : FilterBase
    {
        public const string HasBadValueTag = "Has Bad Data";
        public static readonly string[] filterCommands = new string[] { "", HasBadValueTag };


        public FilterBadData(string command) : base(HasBadValueTag)
        {
            Command = command;
        }

        [JsonInclude]
        public string Command { get; set; }

        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {
                bool addIt = false;
                switch (Command)
                {
                    case HasBadValueTag:
                        if (activity.DataQualityIssues != null && activity.DataQualityIssues.Count > 0)
                            addIt = true;
                        break;
                    default:
                        break;
                }

                if (addIt)
                {
                    returnActivities.Add(activity);
                }
            }
            return returnActivities;
        }
    }

    [Serializable]
    public class FilterLocation : FilterBase
    {
        public const string HasLocationTag = "Has Location";
        public static readonly string[] filterCommands = new string[] { "", HasLocationTag };


        public FilterLocation(string command) : base(HasLocationTag)
        {
            Command = command;
        }

        [JsonInclude]
        public string Command { get; set; }

        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {
                bool addIt = false;
                switch (Command)
                {
                    case HasLocationTag:
                        if (activity.LocationStream != null && activity.LocationStream.Times != null && activity.LocationStream.Times.Length > 0)
                            addIt = true;
                        break;
                    default:
                        break;
                }

                if (addIt)
                {
                    returnActivities.Add(activity);
                }
            }
            return returnActivities;
        }
    }
    [Serializable]
    public class FilterRelative : FilterBase
    {
        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">" };


        public FilterRelative(string fieldName, string command, string otherFieldName) : base("Relative")
        {
            OtherFieldName = otherFieldName;
            Command = command;
            FieldName = fieldName;
        }

        [JsonInclude]
        public string FieldName { get; set; }
        [JsonInclude]
        public string Command { get; set; }

        [JsonInclude]
        public string OtherFieldName { get; set; }

        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {
                bool addIt = false;
                float? value = activity.GetNamedFloatDatum(FieldName);
                float? OtherValue = null;
                if (activity.HasNamedDatum(OtherFieldName))
                {
                    Datum? datum = activity.GetNamedDatum(OtherFieldName);
                    if (datum != null && datum is TypedDatum<float>)
                    {
                        OtherValue = activity.GetNamedFloatDatum(OtherFieldName);
                    }
                }
                switch (Command)
                {
                    case "<":
                        if (value.HasValue && OtherValue.HasValue && value.Value < OtherValue)
                            addIt = true;
                        break;
                    case "<=":
                        if (value.HasValue && OtherValue.HasValue && value.Value <= OtherValue)
                            addIt = true;
                        break;
                    case "=":
                        if (value.HasValue && OtherValue.HasValue && value.Value == OtherValue) //lets do equals as on the day
                            addIt = true;
                        break;
                    case ">=":
                        if (value.HasValue && OtherValue.HasValue && value.Value >= OtherValue)
                            addIt = true;
                        break;
                    case ">":
                        if (value.HasValue && OtherValue.HasValue && value.Value > OtherValue)
                            addIt = true;
                        break;
                    default:
                        break;
                }

                if (addIt)
                {
                    returnActivities.Add(activity);
                }
            }
            return returnActivities;
        }
    }

}
