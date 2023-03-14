using CsvHelper;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using de.schumacher_bw.Strava.Endpoint;
using System.Collections.ObjectModel;
using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class Athlete : Extensible
    {
        public Athlete(Database parent)
        {
            _calendarTree = new SortedList<DateTime, CalendarNode>();
            Parent= parent;
        }

        private SortedList<DateTime, CalendarNode> _calendarTree { get; set; }
        
        public ReadOnlyDictionary<DateTime, CalendarNode> CalendarTree { get { return _calendarTree.AsReadOnly(); } }

        private Dictionary<string, Activity> _activities { get; set; } = new Dictionary<string, Activity>(); //primary key (strava id) against activity

        public ReadOnlyDictionary<string, Activity> Activities { get { return _activities.AsReadOnly(); } }

        public override Utils.DateTimeTree Id { get { return new DateTimeTree(); } } //HACK: Hack to see if tree works

        private SortedDictionary<DateTime, Activity> _activitiesByDateTime { get; set; } = new SortedDictionary<DateTime, Activity>(); //we sometimes need to access activities in date order
        public ReadOnlyDictionary<DateTime, Activity> ActivitiesByDateTime { get { return _activitiesByDateTime.AsReadOnly(); } }

        public IReadOnlyCollection<String> TimeSeriesNames //generate dynamically, don't cache = new List<string>();
        { 
            get
            {
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
                return timeSeriesNames.AsReadOnly();
            }
        } 
        public IReadOnlyCollection<String> ActivityFieldNames
        {
            get
            {
                List<string> activityFieldNames = new List<string>();
                foreach (KeyValuePair<string, Activity> kvp in _activities)
                {
                    Activity activity = kvp.Value;
                    foreach (string s in activity.DataNames)
                    {
                        if (!activityFieldNames.Contains(s))
                        {
                            activityFieldNames.Add(s);
                        }
                    }
                }
                activityFieldNames.Sort();
                return activityFieldNames.AsReadOnly();
            }
        }


        public Activity? AddOrUpdateActivity(Dictionary<string, Datum> activityData)
        {
            if (activityData == null || !Activity.WillBeValid(activityData))
            {
                Logging.Instance.Error(string.Format("Couldn't add data without primary key and time fields"));
                return null;
            }
            string stravaId = Activity.ExpectedPrimaryKey(activityData);
            if (_activities.ContainsKey(stravaId))
            {
                Activity activity= _activities[stravaId];
                foreach (KeyValuePair<string, Datum> kvp in activityData)
                {
                    activity.AddOrReplaceDatum(kvp.Value);
                }
                return activity;
            }
            else
            {
                Activity activity = new Activity();
                foreach (KeyValuePair<string, Datum> kvp in activityData)
                {
                    activity.AddOrReplaceDatum(kvp.Value);
                }

                _activities.Add(stravaId, activity);

                //if we had to add the activity to _activities, we have to add it everywhere else too
                AddActivityToCalenderTree(activity);
                DateTime? startDateTime = activity.StartDateTime;
                if(startDateTime.HasValue)
                {
                    _activitiesByDateTime.Add(startDateTime.Value, activity);
                }

                return activity;
            }
        }

        private void AddActivityToCalenderTree(Activity activity)
        {
            DateTime? dt = activity.StartDateTime;
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
            if(_activitiesByDateTime.Count > 0)
            {
                return _activitiesByDateTime.Last().Key;
            }
            return null;
        }

        /// Override ToString() for debugging
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Current athlete is {0} {1}\r\n", GetNamedDatumForDisplay(Model.Athlete.FirstNameTag), GetNamedDatumForDisplay(Model.Athlete.LastNameTag)));
            sb.Append(string.Format("Current athlete has {0} attributes\r\n", Data.Count));
            foreach (KeyValuePair<string, Datum> kvp in Data)
            {
                sb.Append(string.Format("{0} is {1}\r\n", kvp.Key, kvp.Value));
            }
            IReadOnlyCollection<string> activityFieldNames = ActivityFieldNames;
            sb.Append(string.Format("Current athlete has activities with {0} attributes\r\n", activityFieldNames.Count));
            foreach (string s in activityFieldNames)
            {
                sb.Append(string.Format("{0}\r\n", s));
            }
            sb.Append(string.Format("Current athlete has {0} days with activities\r\n", _calendarTree.Count));
            sb.Append(string.Format("Current athlete has {0} activities\r\n", _activities.Count));
            sb.Append(string.Format("\r\nCurrent athlete has {0} field names\r\n", TimeSeriesNames.Count));
            foreach (string s in TimeSeriesNames)
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
                sb.Append(activity);

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

        public override void Recalculate(bool force)
        {
            base.Recalculate(force);

            List<IDataStream> dataStreams = DataStreamFactory.Instance.DataStreams;

            foreach(KeyValuePair<DateTime, CalendarNode> kvp1 in _calendarTree)
            {
                CalendarNode calendarNode = kvp1.Value;
                calendarNode.Recalculate(force); //Note that this will iterated down to the activities, so don't need to call recalculate on them below
            }


        }


        [JsonIgnore]
        public Database Parent { get; set; } 

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
