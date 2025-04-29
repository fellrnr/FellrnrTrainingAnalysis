using FellrnrTrainingAnalysis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class Cumulative
    {
        public Cumulative(List<string>? sportsToInclude, string targetColumn, string activityFieldname, List<Model.Period> periods)
        {
            SportsToInclude = sportsToInclude;
            TargetColumn = targetColumn;
            ActivityFieldname = activityFieldname;
            Periods = periods;
        }
        public abstract void UpdateActivityCumulatives(Database database, bool force);
        public string TargetColumn { get; set; }
        public string ActivityFieldname { get; set; }
        protected List<string>? SportsToInclude { get; set; }

        public List<Model.Period> Periods { get; }

        public abstract Dictionary<Model.Period, float>? GetCumulativeUpdate(Database database, Day target);

    }


    public class VolumeCumulative : Cumulative
    {
        public VolumeCumulative(List<string>? sportsToInclude, string targetColumn, string activityFieldname, List<Model.Period> periods)
            : base(sportsToInclude, targetColumn, activityFieldname, periods)
        {
        }


        public override void UpdateActivityCumulatives(Database database, bool force)
        {
            Athlete athlete = database.CurrentAthlete;

            //this is currently fast enough we don't need to optimise
            //if (!force)
            //    return; 

            Logging.Instance.ContinueAccumulator("UpdateActivityCumulatives");

            Dictionary<Model.Period, float> rolling = new Dictionary<Model.Period, float>();
            Dictionary<Period, Queue<Tuple<DateTime, float>>> queues = new Dictionary<Period, Queue<Tuple<DateTime, float>>>();
            foreach (Period period in Periods)
            {
                rolling.Add(period, 0);
                queues.Add(period, new Queue<Tuple<DateTime, float>>());
            }


            foreach (KeyValuePair<DateTime, Day> kvp2 in athlete.Days)
            {
                Day day = kvp2.Value;

                float dailyAccumulator = 0;
                foreach (Activity activity in day.Activities)
                {
                    if (!activity.CheckSportType(SportsToInclude))
                        continue;

                    float? value = activity.GetNamedFloatDatum(TargetColumn);

                    if (value != null)
                        dailyAccumulator += (float)value;
                }


                foreach (KeyValuePair<Model.Period, float> periodValue in rolling)
                {
                    Period period = periodValue.Key;
                    Queue<Tuple<DateTime, float>> queue = queues[period];

                    queue.Enqueue(new Tuple<DateTime, float>(day.Date, dailyAccumulator));
                    rolling[period] += dailyAccumulator;

                    while (!period.IsWithinPeriod(queue.First().Item1, day.Date))
                    {
                        Tuple<DateTime, float> first = queue.Dequeue();
                        rolling[period] -= first.Item2;
                    }

                    string CumulativeActivityFieldname = FieldName(period);
                    //if we've done the hard work of calculation, replace regardless of force
                    day.AddOrReplaceDatum(new TypedDatum<float>(CumulativeActivityFieldname, false, rolling[period])); 
                    
                    //if(day.Date == new DateTime(year:2024, month:2, day:27))
                    //{
                    //    Logging.Instance.Debug($"On {day.Date}, Period {period}, {rolling[period]}");
                    //}
                }

            }

            Logging.Instance.PauseAccumulator("UpdateActivityCumulatives");
        }

        private string FieldName(Period p)
        {
            return string.Format("{0} {1}", ActivityFieldname, p.ShortName);
        }

        public override Dictionary<Model.Period, float>? GetCumulativeUpdate(Database database, Day target)
        {
            Logging.Instance.ContinueAccumulator("GetCumulativeUpdate");
            DateTime targetDate = target.Date;

            Dictionary<Model.Period, float> rolling = new Dictionary<Model.Period, float>();
            foreach (Period period in Periods)
            {
                rolling.Add(period, 0);
            }

            foreach (KeyValuePair<string, Activity> kvpActivity in database.CurrentAthlete.Activities)
            {
                Activity activity = kvpActivity.Value;
                if (activity.StartDateNoTimeLocal == null)
                    continue;

                DateTime activityDateTime = (DateTime)activity.StartDateNoTimeLocal;

                if (activityDateTime > targetDate)
                    continue; //this is after our target

                if (!activity.CheckSportType(SportsToInclude))
                    continue;

                if (activity.HasNamedDatum(TargetColumn))
                {
                    Datum valueDatum = activity.GetNamedDatum(TargetColumn)!;
                    if (valueDatum is not TypedDatum<float>)
                        continue;
                    TypedDatum<float> valueDatumFloat = (TypedDatum<float>)valueDatum;

                    float targetValue = valueDatumFloat.Data;

                    //double daysFromToday = (targetDateTime - activityDateTime).TotalDays;
                    foreach (Period period in Periods)
                    {
                        if (period.IsWithinPeriod(activityDateTime, targetDate))
                        {
                            rolling[period] += targetValue;
                        }
                    }
                }
            }
            Logging.Instance.PauseAccumulator("GetCumulativeUpdate");
            return rolling;
        }

    }
    public class CumulativeFactory
    {
        public static List<Cumulative> GetCumulatives()
        {
            //Σ🏃🚶→
            List<Cumulative> retval = new List<Cumulative>
            {
                new VolumeCumulative(Activity.ActivityTypeRun, "Distance", "Σ🏃→", Model.Period.DefaultStorePeriods),
                new VolumeCumulative(Activity.ActivityTypeRun, "Elevation Gain", "Σ🏃⬆", Model.Period.DefaultStorePeriods),
                new VolumeCumulative(Activity.ActivityTypeRun, "Grade Adjusted Distance", "Σ🏃📐", Model.Period.DefaultStorePeriods),

                new VolumeCumulative(Activity.ActivityTypeOnFoot, "Distance", "Σ🦶→", Model.Period.DefaultStorePeriods),
                new VolumeCumulative(Activity.ActivityTypeOnFoot, "Elevation Gain", "Σ🦶⬆", Model.Period.DefaultStorePeriods),
                new VolumeCumulative(Activity.ActivityTypeOnFoot, "Grade Adjusted Distance", "Σ🦶📐", Model.Period.DefaultStorePeriods),


            };

            for (int i = 0; i < Utils.Options.Instance.StartingHR5Zones.Length - 1; i++)
            {
                retval.Add(new VolumeCumulative(null, $"5-Zone-{i+1}", $"Σ 5-Zone-{i + 1}", Model.Period.ShortStorePeriods));
            }

            for (int i = 0; i < Utils.Options.Instance.StartingHR3Zones.Length - 1; i++)
            {
                retval.Add(new VolumeCumulative(null, $"3-Zone-{i + 1}", $"Σ 3-Zone-{i + 1}", Model.Period.ShortStorePeriods));
            }

            retval.Add(new VolumeCumulative(null, $"5-Zone-5a", $"Σ 5-Zone-5a", Model.Period.ShortStorePeriods));
            return retval;
        }
    }
}
