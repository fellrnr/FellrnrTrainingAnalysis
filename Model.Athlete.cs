using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    [MemoryPackable]
    public partial class Athlete : Extensible
    {
        public Athlete()
        {
            _days = new SortedDictionary<DateTime, Day>();
            _calendarTree = new SortedList<DateTime, CalendarNode>();
        }

        [MemoryPackInclude]
        private SortedDictionary<DateTime, Day> _days { get; set; }

        [MemoryPackIgnore]
        public ReadOnlyDictionary<DateTime, Day> Days { get { return _days.AsReadOnly(); } }

        public Day GetOrAddDay(DateTime date)
        {
            lock (_days)
            {
                DateTime dateNoTime = date.Date; //just in case
                if (!_days.ContainsKey(dateNoTime))
                    _days.Add(dateNoTime, new Day(dateNoTime));
                return _days[dateNoTime];
            }
        }

        //look for the given date and work backwards to find one with a datum with the name provided
        //Another n^2 problem if there are no days with the value
        public float? FindRecentDayWithDatum(DateTime date, string name)
        {
            DateTime dateNoTime = date.Date; //just in case

            //lets see if we're lucky
            if (Days.ContainsKey(dateNoTime) && Days[dateNoTime].HasNamedDatum(name))
            {
                Day day = Days[dateNoTime];
                float? retval = day.GetNamedFloatDatum(name);
                return retval;
            }

            Logging.Instance.ContinueAccumulator("FindRecentDayWithDatum(search)");
            DateTime start = Days.Keys.First();
            DateTime scan = dateNoTime;
            while (scan > start)
            {
                scan = scan.AddDays(-1);

                if (Days.ContainsKey(scan) && Days[scan].HasNamedDatum(name))
                {
                    Day day = Days[scan];
                    float? retval = day.GetNamedFloatDatum(name);

                    Day originalday = GetOrAddDay(dateNoTime);
                    originalday.AddOrReplaceDatum(new TypedDatum<float>(name, true, retval!.Value));

                    Logging.Instance.PauseAccumulator("FindRecentDayWithDatum(search)");
                    return retval;
                }
            }

            Logging.Instance.PauseAccumulator("FindRecentDayWithDatum(search)");
            return null;
        }

        public float FindDailyValueOrDefault(DateTime dateNoTime, string name, float defaultValue)
        {
            Logging.Instance.ContinueAccumulator("FindDailyValueOrDefault");

            float? retval = FindRecentDayWithDatum(dateNoTime, name); ;
            if (retval == null)
            {
                //simple optimization = add the values to the dates so next time around we'll be fast
                Day day = GetOrAddDay(dateNoTime);
                lock (day)
                {
                    day.AddOrReplaceDatum(new TypedDatum<float>(name, true, defaultValue));
                }
                retval = defaultValue;
            }

            Logging.Instance.PauseAccumulator("FindDailyValueOrDefault");
            return retval.Value;
        }

        [MemoryPackInclude]
        private SortedList<DateTime, CalendarNode> _calendarTree { get; set; }

        [MemoryPackIgnore]
        public ReadOnlyDictionary<DateTime, CalendarNode> CalendarTree { get { return _calendarTree.AsReadOnly(); } }

        [MemoryPackInclude]
        private Dictionary<string, Activity> _activities { get; set; } = new Dictionary<string, Activity>(); //primary key (strava id) against activity

        //Strava Id to activity
        [MemoryPackIgnore]
        public ReadOnlyDictionary<string, Activity> Activities { get { return _activities.AsReadOnly(); } }


        [MemoryPackInclude]
        private SortedDictionary<DateTime, Activity> _activitiesByUTCDateTime { get; set; } = new SortedDictionary<DateTime, Activity>(); //we sometimes need to access activities in date order

        [MemoryPackInclude]
        private SortedDictionary<DateTime, Activity> _activitiesByLocalDateTime { get; set; } = new SortedDictionary<DateTime, Activity>(); //we sometimes need to access activities in date order

        [MemoryPackIgnore]
        public ReadOnlyDictionary<DateTime, Activity> ActivitiesByLocalDateTime { get { return _activitiesByLocalDateTime.AsReadOnly(); } }

        [MemoryPackIgnore]
        public ReadOnlyDictionary<DateTime, Activity> ActivitiesByUTCDateTime { get { return _activitiesByUTCDateTime.AsReadOnly(); } }

        [MemoryPackIgnore]
        List<String>? _allTimeSeriesNamesCache = null;

        [MemoryPackIgnore]
        public IReadOnlyCollection<String> AllTimeSeriesNames //generate dynamically, don't cache = new List<string>();
        {
            get
            {
                if (_allTimeSeriesNamesCache != null)
                    return _allTimeSeriesNamesCache;

                List<string> timeSeriesNames = new List<string>();
                foreach (KeyValuePair<string, Activity> kvp in _activities)
                {
                    Activity activity = kvp.Value;
                    foreach (string s in activity.TimeSeriesNames)
                    {
                        if (!timeSeriesNames.Contains(s))
                        {
                            timeSeriesNames.Add(s);
                        }
                    }
                }
                timeSeriesNames.Sort();
                _allTimeSeriesNamesCache = timeSeriesNames;
                return timeSeriesNames.AsReadOnly();
            }
        }

        [MemoryPackIgnore]
        List<String>? _allNonVirtualTimeSeriesNamesCache = null;

        [MemoryPackIgnore]
        public IReadOnlyCollection<String> AllNonVirtualTimeSeriesNames //generate dynamically, don't cache = new List<string>();
        {
            get
            {
                if (_allNonVirtualTimeSeriesNamesCache != null)
                    return _allNonVirtualTimeSeriesNamesCache;

                _allNonVirtualTimeSeriesNamesCache = new List<string>();
                foreach (KeyValuePair<string, Activity> kvp in _activities)
                {
                    Activity activity = kvp.Value;
                    foreach (KeyValuePair<string, TimeSeriesBase> kvp2 in activity.TimeSeries)
                    {
                        string name = kvp2.Key;
                        TimeSeriesBase ts = kvp2.Value;
                        if (!ts.IsVirtual() && !_allNonVirtualTimeSeriesNamesCache.Contains(name))
                        {
                            _allNonVirtualTimeSeriesNamesCache.Add(name);
                        }
                    }
                }
                _allNonVirtualTimeSeriesNamesCache.Sort();
                return _allNonVirtualTimeSeriesNamesCache.AsReadOnly();
            }
        }


        [MemoryPackIgnore]
        List<String>? _allActivityTypesCache = null;

        [MemoryPackIgnore]
        public IReadOnlyCollection<String> AllActivityTypes //generate dynamically, don't cache = new List<string>();
        {
            get
            {
                if (_allActivityTypesCache != null)
                    return _allActivityTypesCache;
                List<string> activityTypes = new List<string>();
                foreach (KeyValuePair<string, Activity> kvp in _activities)
                {
                    Activity activity = kvp.Value;
                    string? s = activity.ActivityType;
                    if (s != null && !activityTypes.Contains(s))
                    {
                        activityTypes.Add(s);
                    }
                }
                activityTypes.Sort();
                _allActivityTypesCache = activityTypes;
                return activityTypes.AsReadOnly();
            }
        }


        [MemoryPackIgnore]
        List<Tuple<String, Type>>? _activityFieldMetaDataCache = null;

        [MemoryPackIgnore]
        public IReadOnlyCollection<Tuple<String, Type>> ActivityFieldMetaData
        {
            get
            {
                if (_activityFieldMetaDataCache != null)
                    return _activityFieldMetaDataCache;
                List<Tuple<String, Type>> activityFieldMetaData = new List<Tuple<string, Type>>();
                foreach (KeyValuePair<string, Activity> kvp in _activities)
                {
                    Activity activity = kvp.Value;
                    foreach (Datum datum in activity.DataValues)
                    {
                        Tuple<string, Type> metadata = new Tuple<string, Type>(datum.Name, datum.GetType());
                        if (!activityFieldMetaData.Contains(metadata))
                        {
                            activityFieldMetaData.Add(metadata);
                        }
                    }
                }
                activityFieldMetaData.Sort();
                _activityFieldMetaDataCache = activityFieldMetaData;
                return activityFieldMetaData.AsReadOnly();
            }
        }



        [MemoryPackIgnore]
        private List<string>? _activityFieldNamesCache = null;
        [MemoryPackIgnore]
        public IReadOnlyCollection<String> ActivityFieldNames
        {
            get
            {
                if (_activityFieldNamesCache != null)
                    return _activityFieldNamesCache;

                _activityFieldNamesCache = new List<string>();
                foreach (KeyValuePair<string, Activity> kvp in _activities)
                {
                    Activity activity = kvp.Value;
                    foreach (string s in activity.DataNames)
                    {
                        if (!_activityFieldNamesCache.Contains(s))
                        {
                            _activityFieldNamesCache.Add(s);
                        }
                    }
                }
                _activityFieldNamesCache.Sort();
                return _activityFieldNamesCache.AsReadOnly();
            }
        }


        [MemoryPackIgnore]
        private List<string>? _activityRecordedFieldNamesCache = null;
        [MemoryPackIgnore]
        public IReadOnlyCollection<String> ActivityRecordedFieldNames
        {
            get
            {
                if (_activityRecordedFieldNamesCache != null)
                    return _activityRecordedFieldNamesCache;

                _activityRecordedFieldNamesCache = new List<string>();
                foreach (KeyValuePair<string, Activity> kvp in _activities)
                {
                    Activity activity = kvp.Value;
                    foreach (Datum d in activity.DataValues)
                    {
                        if (d.Recorded && !_activityRecordedFieldNamesCache.Contains(d.Name))
                        {
                            _activityRecordedFieldNamesCache.Add(d.Name);
                        }
                    }
                }
                _activityRecordedFieldNamesCache.Sort();
                return _activityRecordedFieldNamesCache.AsReadOnly();
            }
        }


        [MemoryPackIgnore]
        private List<string>? _dayFieldNamesCache = null;
        [MemoryPackIgnore]
        public IReadOnlyCollection<String> DayFieldNames
        {
            get
            {
                if (_dayFieldNamesCache != null)
                    return _dayFieldNamesCache;

                _dayFieldNamesCache = new List<string>();
                foreach (KeyValuePair<DateTime, Day> kvp in _days)
                {
                    Day day = kvp.Value;
                    foreach (string s in day.DataNames)
                    {
                        if (!_dayFieldNamesCache.Contains(s))
                        {
                            _dayFieldNamesCache.Add(s);
                        }
                    }
                }
                _dayFieldNamesCache.Sort();
                return _dayFieldNamesCache.AsReadOnly();
            }
        }


        public Activity? InitialAddOrUpdateActivity(Dictionary<string, Datum> activityData)
        {
            if (activityData == null || !Activity.WillBeValid(activityData))
            {
                Logging.Instance.Error(string.Format("Couldn't add data without primary key and time fields"));
                return null;
            }
            string stravaId = Activity.ExpectedPrimaryKey(activityData);
            if (_activities.ContainsKey(stravaId))
            {
                Activity activity = _activities[stravaId];
                foreach (KeyValuePair<string, Datum> kvp in activityData)
                {
                    activity.AddOrReplaceDatum(kvp.Value);
                }
                return activity;
            }
            else
            {
                Activity activity = new Activity(this);
                foreach (KeyValuePair<string, Datum> kvp in activityData)
                {
                    activity.AddOrReplaceDatum(kvp.Value);
                }

                _activities.Add(stravaId, activity);

                //if we had to add the activity to _activities, we have to add it everywhere else too
                //but we can't because we might not have local time, so do it in finalize Add

                return activity;
            }
        }

        public void FinalizeAdd(Activity activity)
        {
            //if we had to add the activity to _activities, we have to add it everywhere else too
            //Without a start date/time, things don't appear in the display
            if (!activity.StartDateTimeUTC.HasValue)
            {
                Logging.Instance.Error($"Activity {activity} without startDateTime");
                return;
            }
            if (!activity.StartDateTimeLocal.HasValue)
            {
                activity.StartDateTimeLocal = activity.StartDateTimeUTC; //default to UTC if no local time
            }

            AddActivityToCalenderTree(activity);
            if (_activitiesByUTCDateTime.ContainsKey(activity.StartDateTimeUTC.Value))
                _activitiesByUTCDateTime[activity.StartDateTimeUTC.Value] = activity;
            else
                _activitiesByUTCDateTime.Add(activity.StartDateTimeUTC.Value, activity);

            if (_activitiesByLocalDateTime.ContainsKey(activity.StartDateTimeLocal.Value))
                _activitiesByLocalDateTime[activity.StartDateTimeLocal!.Value] = activity;
            else
                _activitiesByLocalDateTime.Add(activity.StartDateTimeLocal!.Value, activity);
            Day day = GetOrAddDay(activity.StartDateTimeLocal.Value.Date);
            day.AddActivity(activity);

        }

        private void AddActivityToCalenderTree(Activity activity)
        {
            DateTime? dt = activity.StartDateTimeLocal;
            if (dt == null)
                return;
            DateTime dateTimeOfActivity = dt.Value;
            DateTime dateOnlyOfActivity = dateTimeOfActivity.Date;
            DateTime monthOnlyOfActivity = new DateTime(dateOnlyOfActivity.Year, dateOnlyOfActivity.Month, 1);
            DateTime yearOnlyOfActivity = new DateTime(dateOnlyOfActivity.Year, 1, 1);

            //add the Day to the Athlete if not already there
            CalendarNode year;
            if (!_calendarTree.ContainsKey(yearOnlyOfActivity))
            {
                year = new CalendarNode(new DateTimeTree(yearOnlyOfActivity, DateTimeTree.DateTreeType.Year));
                _calendarTree.Add(yearOnlyOfActivity, year);
            }
            else
            {
                year = _calendarTree[yearOnlyOfActivity];
            }

            CalendarNode month;
            if (!year.HasChild(monthOnlyOfActivity))
            {
                month = new CalendarNode(new DateTimeTree(monthOnlyOfActivity, DateTimeTree.DateTreeType.Month));
                year.AddChild(monthOnlyOfActivity, month);
            }
            else
            {
                month = (CalendarNode)year.Children[monthOnlyOfActivity];
            }

            CalendarNode day;
            if (!month.HasChild(dateOnlyOfActivity))
            {
                day = new CalendarNode(new DateTimeTree(dateOnlyOfActivity, DateTimeTree.DateTreeType.Day));
                month.AddChild(dateOnlyOfActivity, day);
            }
            else
            {
                day = (CalendarNode)month.Children[dateOnlyOfActivity];
            }


            day.AddChild(dateTimeOfActivity, activity);

            //string s = $"year {year.DateTimeTree}, month {month.DateTimeTree}, day {day.DateTimeTree}";
            //MessageBox.Show(s);
        }


        public DateTime? GetLatestActivityDateTime()
        {
            if (_activitiesByUTCDateTime.Count > 0) //whatever's really the last
            {
                return _activitiesByUTCDateTime.Last().Key;
            }
            return null;
        }

        /// Override ToString() for debugging
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Athlete is {0} {1}\r\n", GetNamedDatumForDisplay(FirstNameTag), GetNamedDatumForDisplay(LastNameTag)));
            sb.Append(string.Format("Athlete has {0} attributes\r\n", Data.Count));
            foreach (KeyValuePair<string, Datum> kvp in Data)
            {
                sb.Append(string.Format("{0} is {1}\r\n", kvp.Key, kvp.Value));
            }
            IReadOnlyCollection<string> activityFieldNames = ActivityFieldNames;
            sb.Append(string.Format("athlete has activities with {0} attributes\r\n", activityFieldNames.Count));
            foreach (string s in activityFieldNames)
            {
                sb.Append(string.Format("{0}\r\n", s));
            }
            sb.Append(string.Format("athlete has {0} days with activities\r\n", _calendarTree.Count));
            sb.Append(string.Format("athlete has {0} activities\r\n", _activities.Count));
            sb.Append(string.Format("\r\athlete has {0} field names\r\n", AllTimeSeriesNames.Count));
            foreach (string s in AllTimeSeriesNames)
            {
                sb.Append(string.Format(">> {0}\r\n", s));
            }

            List<string> activityTypes = new List<string>();
            Dictionary<string, List<string>> fieldNamesByActivityType = new Dictionary<string, List<string>>();
            Dictionary<string, int> activitiesByActivityType = new Dictionary<string, int>();

            int countWithoutActivityType = 0;
            foreach (KeyValuePair<string, Model.Activity> kvp in _activities)
            {
                Model.Activity activity = kvp.Value;
                sb.Append($"Activity {activity}\r\n");

                string? activityType = activity.ActivityType;
                if (activityType != null)
                {
                    if (!activityTypes.Contains(activityType))
                    {
                        activityTypes.Add(activityType);
                    }
                    if (!fieldNamesByActivityType.ContainsKey(activityType))
                    {
                        fieldNamesByActivityType.Add(activityType, new List<string>());
                        activitiesByActivityType[activityType] = 0;
                    }
                    activitiesByActivityType[activityType]++;
                    foreach (string s in activity.TimeSeriesNames)
                    {
                        if (!fieldNamesByActivityType[activityType].Contains(s))
                        {
                            fieldNamesByActivityType[activityType].Add(s);
                        }
                    }
                }
                else
                {
                    countWithoutActivityType++;
                }
            }

            sb.Append(string.Format("\r\nCurrent athlete has {0} activity types and {1} without an activity type\r\n", activityTypes.Count, countWithoutActivityType));
            foreach (string s in activityTypes)
            {
                sb.Append(string.Format("\r\nActivity type {0} has {1} fields and {2} instances\r\n", s, fieldNamesByActivityType[s].Count, activitiesByActivityType[s]));
                foreach (string field in fieldNamesByActivityType[s])
                {
                    sb.Append(string.Format(">> {0}\r\n", field));
                }
            }


            return sb.ToString();
        }

        public void DeleteActivityBeforeRecalcualte(Activity activity) //the name is a hint to force a recalculate
        {
            if (_activities.ContainsKey(activity.PrimaryKey()))
                _activities.Remove(activity.PrimaryKey());
        }
        public override void Recalculate(int forceCount, bool forceJustMe, BackgroundWorker? worker = null)
        {
            Logging.Instance.TraceEntry($"Athlete.Recalculate {forceCount}, {forceJustMe}");
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (force)
            {
                Logging.Instance.Debug("Athelete force recalculation clean");
                RefreshLists();
            }
            //don't report on calendar progress; it's too high level
            //if (worker != null) worker.ReportProgress(0, new Misc.ProgressReport($"Recalculate Calendar Entries ({_calendarTree.Count})", _calendarTree.Count));
            if (worker != null) worker.ReportProgress(0, new Misc.ProgressReport($"Recalculate Activities Entries ({Activities.Count})", Activities.Count + 1));
            Activity.CurrentRecalculateProgress = 0; //ugly hack for progress count

            if (!Options.Instance.DebugBlockParallel)
            {

                //Activities
                //    .AsParallel()
                //    .ForAll(activity => activity.Value.Recalculate(forceCount, forceJustMe))
                //    .WithProgressReporting(progress => worker.ReportProgress(progress));
                Activities
                    .AsParallel()
                    .ForAll(activity => activity.Value.Recalculate(forceCount, forceJustMe, worker));
            }
            else
            {
                foreach(var kvp in Activities)
                    kvp.Value.Recalculate(forceCount, forceJustMe);
            }

            //do the calendar recalculation after activities rather than recursively so we can multithread the activity recalculation
            foreach (KeyValuePair<DateTime, CalendarNode> kvp1 in _calendarTree)
            {
                CalendarNode calendarNode = kvp1.Value;
                calendarNode.Recalculate(forceCount, forceJustMe, worker); //Note that this will iterated down to the activities, so don't need to call recalculate on them below
            }

            Logging.Instance.TraceLeave();

        }

        //this shouldn't be needed, but it's possible for an activity to change start time (or maybe even id?) when refreshing from strava
        private void RefreshLists()
        {
            Dictionary<string, Activity> oldList = _activities;

            base.Clean();
            _dayFieldNamesCache = null;
            _activityFieldNamesCache = null;
            _days = new SortedDictionary<DateTime, Day>();
            _calendarTree = new SortedList<DateTime, CalendarNode>();
            _activities = new Dictionary<string, Activity>();
            _activitiesByUTCDateTime = new SortedDictionary<DateTime, Activity>();
            _activitiesByLocalDateTime = new SortedDictionary<DateTime, Activity>();
            _allTimeSeriesNamesCache = null;
            _allActivityTypesCache = null;
            _activityFieldMetaDataCache = null;

            foreach (KeyValuePair<string, Activity> kvp in oldList)
            {
                Activity activity = kvp.Value;
                string id = activity.PrimaryKey();
                _activities.Add(id, activity);
                FinalizeAdd(activity);
            }
        }

        public void PostDeserialize()
        {
            foreach (KeyValuePair<string, Activity> kvp in _activities)
            {
                kvp.Value.PostDeserialize(this);
            }
        }

        public void PreSerialize()
        {
            foreach (KeyValuePair<string, Activity> kvp in _activities)
            {
                kvp.Value.PreSerialize(this);
            }
        }

        public const string AthleteIdTag = "AthleteId";
        public const string EmailAddressTag = "Email Address";
        public const string FirstNameTag = "First Name";
        public const string LastNameTag = "Last Name";
        public const string SexTag = "Sex";
        public const string DescriptionTag = "Description";
        public const string CityTag = "City";
        public const string StateTag = "State";
        public const string CountryTag = "Country";
    }
}
