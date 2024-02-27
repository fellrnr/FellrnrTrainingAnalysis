using de.schumacher_bw.Strava.Endpoint;
using System.Text;
using System.Windows.Forms;
using static GMap.NET.Entity.OpenStreetMapGraphHopperRouteEntity;

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
            foreach (KeyValuePair<string, Athlete> kvp1 in database.Athletes)
            {
                Athlete athlete = kvp1.Value;
                //foreach (KeyValuePair<string, Activity> kvp2 in athlete.Activities)

                foreach (KeyValuePair<DateTime, Day> kvp2 in athlete.Days)
                {
                    Day target = kvp2.Value;

                    bool alreadyDone = true;
                    foreach (Period p in periods)
                    {
                        string goalActivityFieldname = string.Format("{0} {1}", ActivityFieldname, p.ShortName);
                        if (!target.HasNamedDatum(goalActivityFieldname))
                            alreadyDone = false;
                    }


                    if (alreadyDone && !force) { continue; }


                    Dictionary<Model.Period, float>? goals = GetGoalUpdate(database, periods, target);
                    if (goals == null)
                        return;
                    foreach (KeyValuePair<Model.Period, float> goal in goals)
                    {
                        string goalActivityFieldname = string.Format("{0} {1}", ActivityFieldname, goal.Key.ShortName);
                        if (!force && target.HasNamedDatum(goalActivityFieldname))
                            return;

                        target.AddOrReplaceDatum(new TypedDatum<float>(goalActivityFieldname, false, goal.Value));
                    }

                }
            }
        }

        

        public override Dictionary<Model.Period, float>? GetGoalUpdate(Database database, List<Model.Period> periods, Day target)
        {
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
                new VolumeGoal(new List<string> { "Run" }, "Run", "Distance", 1.0f / 1000f, "0,0.0", "Km", 4000 * 1000, "Σ🏃→"),
                new VolumeGoal(new List<string> { "Run" }, "Run", "Elevation Gain", 1.0f, "N0", "m", 130 * 1000, "Σ🏃⬆"),
                new VolumeGoal(new List<string> { "Run" }, "Run", "Grade Adjusted Distance", 1.0f / 1000f, "0,0", "Km", 5000 * 1000, "Σ🏃📐"),

                new VolumeGoal(new List<string> { "Run", "Walk", "Hike" }, "On Foot", "Distance", 1.0f / 1000f, "0,0.0", "Km", 5000 * 1000, "Σ🦶→"),
                new VolumeGoal(new List<string> { "Run", "Walk", "Hike" }, "On Foot", "Elevation Gain", 1.0f, "0,0", "m", 161 * 1000, "Σ🦶⬆"),
                new VolumeGoal(new List<string> { "Run", "Walk", "Hike" }, "On Foot", "Grade Adjusted Distance", 1.0f / 1000f, "0,0", "Km", 6000 * 1000, "Σ🦶📐"),
            };

        }
    }
}
