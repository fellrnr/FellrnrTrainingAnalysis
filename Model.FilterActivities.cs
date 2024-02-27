namespace FellrnrTrainingAnalysis.Model
{

    public class FilterActivities
    {

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


    public class FilterBase
    {
        //note, always return the list sorted by start date/time

        public virtual List<Activity> GetActivities(List<Activity> activities)
        {
            return activities;
        }
    }



    public class FilterDateTime : FilterBase
    {
        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">", "between", "1M", "6M", "1Y", "in", "has", "missing" };

        public FilterDateTime(string fieldName, string command, DateTime? startDateTime, DateTime? endDateTime, string? list)
        {
            if (command == "in" && list != null)
            {
                string[] strings = list.Split(',');
                ListOfDates = new List<DateTime>();
                foreach(string s in strings)
                {
                    string ds = s.Trim();
                    if (string.IsNullOrEmpty(ds)) continue;
                    DateTime dateTime;
                    if(DateTime.TryParse(ds, out dateTime))
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

        private string FieldName { get; set; }
        private string Command { get; set; }

        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
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
                        if(dateTime.HasValue && ListOfDates != null && ListOfDates.Count > 0)
                        {
                            foreach(DateTime dt in ListOfDates)
                            {
                                if(dt.Date == dateTime.Value.Date)
                                {
                                    addIt= true; break;
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

    public class FilterFloat : FilterBase
    {
        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">", "between", "has", "missing" };


        public FilterFloat(string fieldName, string command, float? firstValue, float? secondValue)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
            Command = command;
            FieldName = fieldName;
        }

        private string FieldName { get; set; }
        private string Command { get; set; }

        public float? FirstValue { get; set; }
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

    public class FilterDataStream : FilterBase
    {
        public static readonly string[] FilterCommands = new string[] { "", 
            "max <", "max <=", "max =", "max >=", "max >", "max between", 
            "avg <", "avg <=", "avg =", "avg >=", "avg >", "avg between", 
            "min <", "min <=", "min =", "min >=", "min >", "min between", "has", "missing" };


        public FilterDataStream(string fieldName, string command, float? firstValue, float? secondValue)
        {
            FirstValue = firstValue;
            SecondValue = secondValue;
            Command = command;
            FieldName = fieldName;
        }

        private string FieldName { get; set; }
        private string Command { get; set; }

        public float? FirstValue { get; set; }
        public float? SecondValue { get; set; }


        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {

                float? value;

                if (!activity.TimeSeries.ContainsKey(FieldName))
                {
                    value = null;
                }
                else
                {
                    DataStreamBase datastream = activity.TimeSeries[FieldName];
                    Tuple<uint[], float[]>? dataTuple = datastream.GetData();
                    if(dataTuple == null)
                    {
                        value = null;
                    }
                    else
                    {
                        float[] data = dataTuple.Item2;
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


    public class FilterString : FilterBase
    {
        public static readonly string[] filterCommands = new string[] { "", "=", "contains", "doesn't contain", "has", "missing", "in" };


        public FilterString(string fieldName, string command, string? value)
        {
            Value1 = value;
            Command = command;
            FieldName = fieldName;
        }

        private string FieldName { get; set; }
        private string Command { get; set; }

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
                            foreach(string s in inList)
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
    public class FilterBadData : FilterBase
    {
        public const string HasBadValueTag = "Has Bad Data";
        public static readonly string[] filterCommands = new string[] { "", HasBadValueTag };


        public FilterBadData(string command)
        {
            Command = command;
        }

        private string Command { get; set; }



        public override List<Activity> GetActivities(List<Activity> activities)
        {
            List<Activity> returnActivities = new List<Activity>();
            foreach (Activity activity in activities)
            {
                bool addIt = false;
                switch (Command)
                {
                    case HasBadValueTag:
                        if(activity.DataQualityIssues != null && activity.DataQualityIssues.Count > 0)
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
