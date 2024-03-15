using de.schumacher_bw.Strava.Endpoint;
using FellrnrTrainingAnalysis.Utils;
using System.Collections.Generic;

namespace FellrnrTrainingAnalysis.Model
{

    public abstract class Goal
    {
        public Goal(List<string> sportsToInclude, string sportDescription, string targetColumn, string activityFieldname)
        {
            SportsToInclude = sportsToInclude;
            SportDescription = sportDescription;
            TargetColumn = targetColumn;
            ActivityFieldname = activityFieldname;
        }
        public abstract void UpdateActivityGoals(Database database, List<Model.Period> periods, bool force);
        public string SportDescription { get; set; }
        public string TargetColumn { get; set; }
        public string ActivityFieldname { get; set; }
        protected List<string> SportsToInclude { get; set; }

        public abstract string FormatResult(KeyValuePair<Model.Period, float> kvp);

        public abstract Dictionary<Model.Period, float>? GetGoalUpdate(Database database, List<Model.Period> periods, Day target);

    }


    public class VolumeGoal : Goal
    {
        public VolumeGoal(List<string> sportsToInclude, string sportDescription, string targetColumn, float scalingFactor, string format, string units, float goalValue, string activityFieldname) 
            : base(sportsToInclude, sportDescription, targetColumn, activityFieldname)
        { 
            ScalingFactor = scalingFactor;
            Format = format;
            Units = units;
            GoalValue = goalValue;
        }


        private float ScalingFactor { get; set; }

        private string Format { get; set; }

        private string Units { get; set; }

        private float GoalValue { get; set; }

        public override string FormatResult(KeyValuePair<Model.Period, float> kvp)
        {
            int? days = kvp.Key.ApproxDays;
            if(days.HasValue)
            {
                return string.Format("{0} ({1})", FormatResult(kvp.Value), AsPercentTarget(kvp.Value, days.Value));
            }
            else
            {
                return FormatResult(kvp.Value);
            }
        }


        private string FormatResult(float result)
        {
            float scaled = result * ScalingFactor;
            return scaled.ToString(Format) + Units;
        }

        private string AsPercentTarget(float actual, int period)
        {
            //all targets are anual (so far)
            float fractionOfYear = period / 365.0f;
            float anualized = actual / fractionOfYear;
            float percentOfTarget = anualized / GoalValue * 100;
            return string.Format("{0:0}%", percentOfTarget);
        }


        //TODO: doing all goals for all activities is taking n^2 time for activities. It would be much faster to have a queue for the periods and queue/dequeue each activity, keeping a set of sums
        public override void UpdateActivityGoals(Database database, List<Model.Period> periods, bool force)
        {
            Athlete athlete = database.CurrentAthlete;

            //this is currently fast enough we don't need to optimise
            //if (!force)
            //    return; 

            Logging.Instance.ContinueAccumulator("UpdateActivityGoals");

            Dictionary<Model.Period, float> rolling = new Dictionary<Model.Period, float>();
            Dictionary<Period, Queue<Tuple<DateTime, float>>> queues = new Dictionary<Period, Queue<Tuple<DateTime, float>>>();
            foreach (Period period in periods)
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

                    while(!period.IsWithinPeriod(queue.First().Item1, day.Date))
                    {
                        Tuple<DateTime, float> first = queue.Dequeue(); 
                        rolling[period] -= first.Item2;
                    }

                    string goalActivityFieldname = FieldName(period);
                    day.AddOrReplaceDatum(new TypedDatum<float>(goalActivityFieldname, false, rolling[period])); //if we've done the hard work of calculation, replace regardless of force
                    //if(day.Date == new DateTime(year:2024, month:2, day:27))
                    //{
                    //    Logging.Instance.Debug($"On {day.Date}, Period {period}, {rolling[period]}");
                        
                    //}
                }

            }

            Logging.Instance.PauseAccumulator("UpdateActivityGoals");
        }

        private string FieldName(Period p)
        {
            return string.Format("{0} {1}", ActivityFieldname, p.ShortName);
        }

        public override Dictionary<Model.Period, float>? GetGoalUpdate(Database database, List<Model.Period> periods, Day target)
        {
            Logging.Instance.ContinueAccumulator("GetGoalUpdate");
            DateTime targetDate = target.Date;

            Dictionary<Model.Period, float> rolling = new Dictionary<Model.Period, float>();
            foreach (Period period in periods)
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
                    foreach (Period period in periods)
                    {
                        if (period.IsWithinPeriod(activityDateTime, targetDate))
                        {
                            rolling[period] += targetValue;
                        }
                    }
                }
            }
            Logging.Instance.PauseAccumulator("GetGoalUpdate");
            return rolling;
        }

    }
    public class GoalFactory
    {
        public static List<Goal> GetGoals()
        {
            //Σ🏃🚶→
            return new List<Goal>
            {
                new VolumeGoal(Activity.ActivityTypeRun, "Run", "Distance", 1.0f / 1000f, "0,0.0", "Km", 4000 * 1000, "Σ🏃→"),
                new VolumeGoal(Activity.ActivityTypeRun, "Run", "Elevation Gain", 1.0f, "N0", "m", 130 * 1000, "Σ🏃⬆"),
                new VolumeGoal(Activity.ActivityTypeRun, "Run", "Grade Adjusted Distance", 1.0f / 1000f, "0,0", "Km", 5000 * 1000, "Σ🏃📐"),

                new VolumeGoal(new List<string> { "Run", "Walk", "Hike", "Virtual Run" }, "On Foot", "Distance", 1.0f / 1000f, "0,0.0", "Km", 5000 * 1000, "Σ🦶→"),
                new VolumeGoal(new List<string> { "Run", "Walk", "Hike", "Virtual Run" }, "On Foot", "Elevation Gain", 1.0f, "0,0", "m", 161 * 1000, "Σ🦶⬆"),
                new VolumeGoal(new List<string> { "Run", "Walk", "Hike", "Virtual Run" }, "On Foot", "Grade Adjusted Distance", 1.0f / 1000f, "0,0", "Km", 6000 * 1000, "Σ🦶📐"),
            };

        }
    }
}
