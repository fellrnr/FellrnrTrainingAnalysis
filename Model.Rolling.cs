using de.schumacher_bw.Strava.Endpoint;
using FellrnrTrainingAnalysis.Utils;
using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class Rolling
    {
        public Rolling(List<string> sportsToInclude, string fieldNameToAdd)
        {
            SportsToInclude = sportsToInclude;
            FieldNameToAdd = fieldNameToAdd;
        }
        public string FieldNameToAdd { get; set; }
        protected List<string> SportsToInclude { get; set; }

        public abstract void Recalculate(Database database, bool force);
    }


    //For where you don't need to keep track from day to day
    public abstract class RollingSimple : Rolling
    {
        public RollingSimple(List<string> sportsToInclude, string fieldNameToAdd) : base(sportsToInclude, fieldNameToAdd)
        {
        }

        public override void Recalculate(Database database, bool force)
        {
            foreach (KeyValuePair<DateTime, Day> kvp2 in database.CurrentAthlete.Days)
            {
                Day day = kvp2.Value;
                if (force)
                    day.RemoveNamedDatum(FieldNameToAdd);

                Recalculate(day);
            }
        }

        public abstract void Recalculate(Day day);

    }


    //Roll up (add) values from activites to their day
    public class RollingRollUpActivityToDay : RollingSimple
    {
        public RollingRollUpActivityToDay(List<string> sportsToInclude, string fieldNameToAdd, string firstField, ModeEnum mode) : base(sportsToInclude, fieldNameToAdd)
        {
            FirstField = firstField;
            this.Mode = mode;
        }

        private string FirstField { get; set; }

        public enum ModeEnum { Sum, Avg }

        private ModeEnum Mode {  get; set; }

        public override void Recalculate(Day day)
        {
            float dailyAccumulator = 0;
            int count = 0;
            foreach (Activity activity in day.Activities)
            {
                if (!activity.CheckSportType(SportsToInclude))
                    continue;

                float? value = activity.GetNamedFloatDatum(FirstField);


                if (value != null)
                {
                    dailyAccumulator += (float)value;
                    count++;
                }
            }
            if (Mode == ModeEnum.Avg && count > 0)
                dailyAccumulator = dailyAccumulator / count;

            if (dailyAccumulator != 0)
                day.AddOrReplaceDatum(new TypedDatum<float>(FieldNameToAdd, false, dailyAccumulator));
        }
    }

    public class RollingNormaliseAbsolute : RollingSimple
    {
        public RollingNormaliseAbsolute(List<string> sportsToInclude, string fieldNameToAdd, string firstField, float divisor) : base(sportsToInclude, fieldNameToAdd)
        {
            FirstField = firstField;
            Divisor = divisor;
        }

        private string FirstField { get; set; }
        private float Divisor { get; set; }

        public override void Recalculate(Day day)
        {
            float? value1 = day.GetNamedFloatDatum(FirstField);

            if (value1 != null)
            {
                float delta = (float)value1 / Divisor;
                if (float.IsNaN(delta))
                    Logging.Instance.Error($"Oops, hit NaN for {this}, {day}");
                else
                    day.AddOrReplaceDatum(new TypedDatum<float>(FieldNameToAdd, false, delta));
            }
        }
    }


    public class RollingDifference : RollingSimple
    {
        public RollingDifference(List<string> sportsToInclude, string fieldNameToAdd, string firstField, string secondField) : base(sportsToInclude, fieldNameToAdd)
        {
            FirstField = firstField;
            SecondField = secondField;
        }

        private string FirstField { get; set; }
        private string SecondField { get; set; }

        public override void Recalculate(Day day)
        {

            float? value1 = day.GetNamedFloatDatum(FirstField);
            float? value2 = day.GetNamedFloatDatum(SecondField);

            if (value1 != null && value2 != null)
            {
                float delta = (float)value1 - (float)value2;
                day.AddOrReplaceDatum(new TypedDatum<float>(FieldNameToAdd, false, delta));
            }
        }
    }

    public class RollingRatio: RollingSimple
    {
        public RollingRatio(List<string> sportsToInclude, string fieldNameToAdd, string firstField, string secondField) : base(sportsToInclude, fieldNameToAdd)
        {
            FirstField = firstField;
            SecondField = secondField;
        }

        private string FirstField { get; set; }
        private string SecondField { get; set; }

        public override void Recalculate(Day day)
        {

            float? value1 = day.GetNamedFloatDatum(FirstField);
            float? value2 = day.GetNamedFloatDatum(SecondField);

            if (value1 != null && value2 != null && value2 != 0)
            {
                float ratio = (float)value1 / (float)value2 * 100.0f;
                if (float.IsNaN(ratio))
                    Logging.Instance.Error($"Oops, hit NaN for {this}, {day}");
                else
                    day.AddOrReplaceDatum(new TypedDatum<float>(FieldNameToAdd, false, ratio));
            }
        }
    }

    public class RollingForceOverwrite : RollingSimple
    {
        public RollingForceOverwrite(List<string> sportsToInclude, string fieldNameToAdd, float value) : base(sportsToInclude, fieldNameToAdd)
        {
            Value = value;
        }

        private float Value { get; set; }

        public override void Recalculate(Day day)
        {
            day.AddOrReplaceDatum(new TypedDatum<float>(FieldNameToAdd, false, Value));
        }
    }



    public class RollingPercentMax : Rolling
    {
        public RollingPercentMax(List<string> sportsToInclude, string fieldNameToAdd, string firstField) : base(sportsToInclude, fieldNameToAdd)
        {
            FirstField = firstField;
        }

        private string FirstField { get; set; }


        public override void Recalculate(Database database, bool force)
        {
            //first pass - find max
            float max = float.MinValue;

            foreach (KeyValuePair<DateTime, Day> kvp2 in database.CurrentAthlete.Days)
            {
                Day day = kvp2.Value;

                float? value1 = day.GetNamedFloatDatum(FirstField);

                if (value1 != null)
                {
                    max = float.Max((float)value1, max);
                }
            }

            //second pass, apply max
            if (max != float.MinValue && max != 0)
            {
                foreach (KeyValuePair<DateTime, Day> kvp2 in database.CurrentAthlete.Days)
                {
                    Day day = kvp2.Value;

                    float? value1 = day.GetNamedFloatDatum(FirstField);

                    if (force)
                        day.RemoveNamedDatum(FieldNameToAdd);

                    if (value1 != null && value1 != 0)
                    {
                        float percent = (float)value1 / max * 100.0f;
                        if(percent > 1) //less than one we ignore
                            day.AddOrReplaceDatum(new TypedDatum<float>(FieldNameToAdd, false, percent));
                    }
                }
            }
        }

    }


    public class RollingFactory
    {
        public static List<Rolling> GetRollings()
        {
            //Σ🏃🚶→
            return new List<Rolling>
            {
                new RollingNormaliseAbsolute(Activity.ActivityTypeRun, "Σ🏃→X̄ 7D", "Σ🏃→ 7D", 7.0f),
                new RollingNormaliseAbsolute(Activity.ActivityTypeRun, "Σ🏃→X̄ 30D", "Σ🏃→ 30D", 30.0f),
                new RollingRatio(Activity.ActivityTypeRun, "🏃→X̄ 7/30", "Σ🏃→X̄ 7D", "Σ🏃→X̄ 30D"),

                new RollingNormaliseAbsolute(Activity.ActivityTypeRun, "Σ🏃📐X̄ 7D", "Σ🏃📐 7D", 7.0f),
                new RollingNormaliseAbsolute(Activity.ActivityTypeRun, "Σ🏃📐X̄ 30D", "Σ🏃📐 30D", 30.0f),
                new RollingRatio(Activity.ActivityTypeRun, "🏃📐X̄ 7/30", "Σ🏃📐X̄ 7D", "Σ🏃📐X̄ 30D"),

                new RollingRollUpActivityToDay(Activity.ActivityTypeRun, "ΣTRIMP downhill", "TRIMP downhill", RollingRollUpActivityToDay.ModeEnum.Sum),
                new RollingPercentMax(Activity.ActivityTypeRun, "ΣTRIMP downhill%", "ΣTRIMP downhill"),

                new RollingRollUpActivityToDay(Activity.ActivityTypeRun, "ΣTRIMP aerobic", "TRIMP aerobic", RollingRollUpActivityToDay.ModeEnum.Sum),
                new RollingPercentMax(Activity.ActivityTypeRun, "ΣTRIMP aerobic%", "ΣTRIMP aerobic"),

                new RollingRollUpActivityToDay(Activity.ActivityTypeRun, "ΣTRIMP anaerobic", "TRIMP anaerobic", RollingRollUpActivityToDay.ModeEnum.Sum),
                new RollingPercentMax(Activity.ActivityTypeRun, "ΣTRIMP anaerobic%", "ΣTRIMP anaerobic"),

                new RollingRollUpActivityToDay(Activity.ActivityTypeRun, "Avg HrPwr 5 Min", "Avg HrPwr 5 Min", RollingRollUpActivityToDay.ModeEnum.Avg),
                new RollingPercentMax(Activity.ActivityTypeRun, "HrPwr%", "Avg HrPwr 5 Min"),
                //new RollingForceOverwrite(new List<string>(), Day.RestingHeartRateTag, 45.0f), //hack to correct problems
            };

        }
    }

}
