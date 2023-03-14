using de.schumacher_bw.Strava.Endpoint;
using de.schumacher_bw.Strava.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FellrnrTrainingAnalysis.Model.DataStreamToDataField;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace FellrnrTrainingAnalysis.Model
{
    public class Goal
    {
        private Goal(List<string> sportsToInclude, string sportDescription, string targetColumn, float scalingFactor, string format, string units, float target, string activityFieldname) 
        { 
            SportsToInclude = sportsToInclude;
            SportDescription = sportDescription;
            TargetColumn = targetColumn;
            ScalingFactor = scalingFactor;
            Format = format;
            Units = units;
            Target = target;
            ActivityFieldname = activityFieldname;
        }
        public static List<Goal> GoalFactory()
        {
            return new List<Goal>
            {
                new Goal(new List<string> { "Run" }, "Run", "Distance", 1.0f / 1000f, "0,0.0", "Km", 6000, "Σ🏃📏"),
                new Goal(new List<string> { "Run" }, "Run", "Elevation Gain", 1.0f, "N0", "m", 150 * 1000, "Σ🏃⬆"),
                new Goal(new List<string> { "Run", "Walk", "Hike" }, "On Foot", "Distance", 1.0f / 1000f, "0,0.0", "Km", 6000, "Σ🏃🚶📏"),
                new Goal(new List<string> { "Run", "Walk", "Hike" }, "On Foot", "Elevation Gain", 1.0f, "0,0", "m", 150 * 1000, "Σ🏃🚶⬆"),
                new Goal(new List<string> { "Run" }, "Run", "Grade Adjusted Distance", 1.0f / 1000f, "0,0", "Km", 6000, "Σ🏃📐"),
            };

        }
        public List<string> SportsToInclude { get; set; }

        public string SportDescription { get; set; }
        public string TargetColumn { get; set; }

        public float ScalingFactor { get; set; }

        public string Format { get; set; }

        public string Units { get; set; }

        public float Target { get; set; }

        public string ActivityFieldname { get; set; }

        public class Period
        {
            public Period(int years, int months, int days)
            {
                Years = years;
                Months = months;
                Days = days;
            }

            public bool IsWithinPeriod(DateTime sample, DateTime target)
            {
                target = target.Date;
                sample = sample.Date;
                if(sample > target) return false; //past the target date

                DateTime yearOffset = Years > 0 ? sample.AddYears(Years) : sample;
                DateTime monthOffset = Months > 0 ? yearOffset.AddMonths(Months) : yearOffset;
                DateTime dayOffset = Days > 0 ? monthOffset.AddDays(Days) : monthOffset;

                if (dayOffset <= target) return false; //now past the target date

                return true;
            }

            public string FullName { get { return (Years > 0 ? $"{Years} Years" : "") + (Months > 0 ? $"{Months} Months" : "") + (Days > 0 ? $"{Days} Days" : ""); } }
            public string ShortName { get { return (Years > 0 ? $"{Years}Y" : "") + (Months > 0 ? $"{Months}M" : "") + (Days > 0 ? $"{Days}D" : ""); } }

            public int Years { get; set; }
            public int Months { get; set; }
            public int Days { get; set; }

            public int ApproxDays {  get {  return Years * 365 + Months * 30 + Days; } }

        }

        public static List<Period> DefaultDisplayPeriods = new List<Period> { new Period(0, 0, 6), new Period(0, 0, 7), new Period(0, 1, 0), new Period(1, 0, 0) };
        public static List<Period> DefaultEmailPeriods = new List<Period> { new Period(0, 0, 6), new Period(0, 0, 7), new Period(30, 0, 0), new Period(0, 1, 0), new Period(1, 0, 0) };
        public static List<Period> DefaultStorePeriods = new List<Period> { new Period(0, 0, 7), new Period(0, 1, 0), new Period(0, 0, 30), new Period(1, 0, 0) };

        public static void test()
        {
            StringBuilder sb = new StringBuilder();
            for(DateTime dt = DateTime.Now.AddYears(-1).AddDays(-1); dt < DateTime.Now; dt = dt.AddDays(1))
            {
                foreach(Period p in DefaultDisplayPeriods)
                {
                    bool result = p.IsWithinPeriod(dt, DateTime.Now);
                    sb.Append($"{p.ShortName} ${dt} is ${result}\r\n");
                }
            }
            LargeTextDialogForm d = new LargeTextDialogForm(sb.ToString());
            d.ShowDialog();
        }

        public string FormatResult(float result)
        {
            return result.ToString(Format) + Units;
        }

        public string AsPercentTarget(float actual, int period)
        {
            //all targets are anual (so far)
            float fractionOfYear = period / 365.0f;
            float anualized = actual / fractionOfYear;
            float percentOfTarget = anualized / Target * 100;
            return string.Format("{0:0}%", percentOfTarget);
        }


        //TODO: doing all goals for all activities is taking n^2 time for activities. It would be much faster to have a queue for the periods and queue/dequeue each activity, keeping a set of sums
        public void UpdateActivityGoals(Database database, List<Period> periods, bool force)
        {
            foreach (KeyValuePair<string, Athlete> kvp1 in database.Athletes)
            {
                Athlete athlete = kvp1.Value;
                foreach (KeyValuePair<string, Activity> kvp2 in athlete.Activities)
                {
                    Activity target = kvp2.Value;

                    if (!CheckSportType(target))
                        continue;

                    bool alreadyDone = true;
                    foreach (Period p in periods)
                    {
                        string goalActivityFieldname = string.Format("{0} {1}", ActivityFieldname, p.ShortName);
                        if (!target.HasNamedDatum(goalActivityFieldname))
                            alreadyDone = false;
                    }


                    if (alreadyDone && !force) { continue; }


                    Dictionary<Period, float>? goals = GetGoalUpdate(database, periods, target);
                    if (goals == null)
                        return;
                    foreach (KeyValuePair<Period, float> goal in goals)
                    {
                        string goalActivityFieldname = string.Format("{0} {1}", ActivityFieldname, goal.Key.ShortName);
                        if (!force && target.HasNamedDatum(goalActivityFieldname))
                            return;

                        target.AddOrReplaceDatum(new TypedDatum<float>(goalActivityFieldname, false, goal.Value));
                    }

                }
            }
        }

        public Dictionary<Period, float>? GetGoalUpdate(Database database, List<Period> periods, Activity target)
        {
            if (target.StartDateNoTime == null)
                return null;

            DateTime targetDateTime = (DateTime)target.StartDateNoTime;

            Dictionary<Period, float> rolling = new Dictionary<Period, float>();
            foreach (Period period in periods)
            {
                rolling.Add(period, 0);
            }

            foreach (KeyValuePair<string, Activity> kvpActivity in database.CurrentAthlete.Activities)
            {
                Activity activity = kvpActivity.Value;
                if (activity.StartDateNoTime == null)
                    continue;

                DateTime activityDateTime = (DateTime)activity.StartDateNoTime;

                if (activityDateTime > targetDateTime)
                    continue; //this is after our target


                if(!CheckSportType(activity))
                    continue;

                if (activity.HasNamedDatum(TargetColumn))
                {
                    Datum valueDatum = activity.GetNamedDatum(TargetColumn)!;
                    if (valueDatum is not TypedDatum<float>)
                        continue;
                    TypedDatum<float> valueDatumFloat = (TypedDatum<float>)valueDatum;

                    float targetValue = valueDatumFloat.Data;

                    //double daysFromToday = (targetDateTime - activityDateTime).TotalDays;
                    foreach (Period period in periods)
                    {
                        if (period.IsWithinPeriod(activityDateTime, targetDateTime))
                        {
                            rolling[period] += targetValue * ScalingFactor;
                        }
                    }
                }
            }
            return rolling;
        }

        private bool CheckSportType(Activity activity)
        {
            string? activitySportType = activity.ActivityType?.Trim(); //had spaces after sport
            //if (activity.StartDateNoTime == DateTime.Now.AddDays(-1).Date)
            //{
            //    MessageBox.Show(activitySportType);
            //}
            if (activitySportType == null)
                return false;
            if (!SportsToInclude.Contains(activitySportType))
                return false;
            return true;
        }
    }
}
