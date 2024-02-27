using FellrnrTrainingAnalysis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateFieldSimple : CalculateFieldBase
    {
        public CalculateFieldSimple(string activityFieldname, bool overrideRecordedZeroOnly, List<string>? sportsToInclude = null)
        {
            ActivityFieldname = activityFieldname;
            SportsToInclude = sportsToInclude;
            OverrideRecordedZeroOnly = overrideRecordedZeroOnly;
        }

        List<string>? SportsToInclude;
        bool OverrideRecordedZeroOnly;

        public string ActivityFieldname { get; set; }

        public override void Recalculate(Extensible extensible, int forceCount, bool forceJustMe)
        {
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (extensible == null || extensible is not Activity)
            {
                return;
            }

            Activity activity = (Activity)extensible;

            Datum? datum = activity.GetNamedDatum(ActivityFieldname);
            //if we have a recorded non-zero datum and we don't override these, then return
            if(OverrideRecordedZeroOnly && datum != null && datum.Recorded == true && datum is TypedDatum<float> && ((TypedDatum<float>)datum).Data != 0)
            {
                return;
            }

            //on the other hand, if we have a recorded zero, then we want to force a calculation (next time through it will be a recorded value)
            if (OverrideRecordedZeroOnly && datum != null && datum.Recorded == true && datum is TypedDatum<float> && ((TypedDatum<float>)datum).Data == 0)
            {
                force = true;
            }


            if (activity.HasNamedDatum(ActivityFieldname) && !force)
                return;

            //always remove if we're recalculating
            activity.RemoveNamedDatum(ActivityFieldname);

            if (SportsToInclude != null && !activity.CheckSportType(SportsToInclude))
                return;

            float? value = ExtractValue(activity);
            if (value != null)
            {
                Logging.Instance.Debug($"Will replace/add datum {ActivityFieldname} with {value} on {activity}");
                activity.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value.Value));
            }
        }
        protected abstract float? ExtractValue(Activity activity);

    }

    public class CalculateFieldSimpleDefault : CalculateFieldSimple
    {
        public CalculateFieldSimpleDefault(string activityFieldname, string dependentFieldname, float defaultValue, Mode extractionMode, bool overrideRecordedZeroOnly, List<string>? sportsToInclude = null) : 
            base(activityFieldname, overrideRecordedZeroOnly, sportsToInclude)
        {
            DependentFieldname = dependentFieldname;
            DefaultValue = defaultValue;
            ExtractionMode = extractionMode;
        }

        public string DependentFieldname { get; set; }
        public float DefaultValue { get; set; }

        public enum Mode { Multiply } //add others as needed
        Mode ExtractionMode { get; set; }

        protected override float? ExtractValue(Activity activity)
        {
            if (!activity.HasNamedDatum(DependentFieldname))
                return null;

            float? dependent = activity.GetNamedFloatDatum(DependentFieldname);

            if(dependent != null && ExtractionMode == Mode.Multiply)
            {
                dependent *= DefaultValue;
            }

            return dependent;
        }

    }

}
