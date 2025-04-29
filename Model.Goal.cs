using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Model
{

    public abstract class Goal
    {
        public Goal(List<string>? sportsToInclude, string sportDescription, string targetColumn, string activityFieldname, List<Model.Period> periods)
        {
            SportsToInclude = sportsToInclude;
            SportDescription = sportDescription;
            TargetColumn = targetColumn;
            ActivityFieldname = activityFieldname;
            Periods = periods;
        }
        public string SportDescription { get; set; }
        public string TargetColumn { get; set; }
        public string ActivityFieldname { get; set; }
        protected List<string>? SportsToInclude { get; set; }

        public List<Model.Period> Periods { get; }

        public abstract string FormatResult(KeyValuePair<Model.Period, float> kvp);

        public abstract Dictionary<Model.Period, float>? GetGoalsForDay(Database database, Day target);

    }


    public class VolumeGoal : Goal
    {
        public VolumeGoal(List<string> sportsToInclude, string sportDescription, string targetColumn, float scalingFactor, string format, string units, float goalValue, string activityFieldname, List<Model.Period> periods)
            : base(sportsToInclude, sportDescription, targetColumn, activityFieldname, periods)
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
            if (days.HasValue)
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


        private string FieldName(Period p)
        {
            return string.Format("{0} {1}", ActivityFieldname, p.ShortName);
        }

        public override Dictionary<Model.Period, float>? GetGoalsForDay(Database database, Day target)
        {
            Logging.Instance.ContinueAccumulator("GetGoalUpdate");
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
                new VolumeGoal(Activity.ActivityTypeRun, "Run", "Distance", 1.0f / 1000f, "0,0.0", "Km", 4000 * 1000, "Σ🏃→", Model.Period.DefaultStorePeriods),
                new VolumeGoal(Activity.ActivityTypeRun, "Run", "Elevation Gain", 1.0f, "N0", "m", 130 * 1000, "Σ🏃⬆", Model.Period.DefaultStorePeriods),
                new VolumeGoal(Activity.ActivityTypeRun, "Run", "Grade Adjusted Distance", 1.0f / 1000f, "0,0", "Km", 5000 * 1000, "Σ🏃📐", Model.Period.DefaultStorePeriods),

                new VolumeGoal(Activity.ActivityTypeOnFoot, "On Foot", "Distance", 1.0f / 1000f, "0,0.0", "Km", 5000 * 1000, "Σ🦶→", Model.Period.DefaultStorePeriods),
                new VolumeGoal(Activity.ActivityTypeOnFoot, "On Foot", "Elevation Gain", 1.0f, "0,0", "m", 161 * 1000, "Σ🦶⬆", Model.Period.DefaultStorePeriods),
                new VolumeGoal(Activity.ActivityTypeOnFoot, "On Foot", "Grade Adjusted Distance", 1.0f / 1000f, "0,0", "Km", 6000 * 1000, "Σ🦶📐" , Model.Period.DefaultStorePeriods),

            };

        }
    }
}
