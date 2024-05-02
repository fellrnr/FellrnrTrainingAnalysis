using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateFieldSimple : CalculateFieldBase
    {
        public CalculateFieldSimple(string activityFieldname, OverrideMode overrideWhen, List<string>? sportsToInclude = null)
        {
            ActivityFieldname = activityFieldname;
            SportsToInclude = sportsToInclude;
            OverrideWhen = overrideWhen;
        }

        List<string>? SportsToInclude;
        public enum OverrideMode { Always, OverrideRecordedZeroOnly, AbsentOnly }
        OverrideMode OverrideWhen;

        public string ActivityFieldname { get; set; }

        public override void Recalculate(Extensible extensible, int forceCount, bool forceJustMe)
        {
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimple Forced recalculating {ActivityFieldname}");

            if (extensible == null || extensible is not Activity)
            {
                return;
            }

            Activity activity = (Activity)extensible;

            Datum? datum = activity.GetNamedDatum(ActivityFieldname);
            if (OverrideWhen == OverrideMode.AbsentOnly && datum != null)
            {
                return;
            }

            //if we have a recorded non-zero datum and we don't override these, then return
            if (OverrideWhen == OverrideMode.OverrideRecordedZeroOnly && datum != null && datum.Recorded == true && datum is TypedDatum<float> && ((TypedDatum<float>)datum).Data != 0)
            {
                return;
            }

            //on the other hand, if we have a recorded zero, then we want to force a calculation (next time through it will be a recorded value)
            if (OverrideWhen == OverrideMode.OverrideRecordedZeroOnly && datum != null && datum.Recorded == true && datum is TypedDatum<float> && ((TypedDatum<float>)datum).Data == 0)
            {
                force = true;
            }


            if (activity.HasNamedDatum(ActivityFieldname) && !force)
                return;

            //always remove if we're recalculating
            activity.RemoveNamedDatum(ActivityFieldname);

            if (SportsToInclude != null && !activity.CheckSportType(SportsToInclude))
                return;

            float? value = ExtractValue(activity, forceJustMe);
            if (value != null)
            {
                if (forceJustMe) Logging.Instance.Debug($"Will replace/add datum {ActivityFieldname} with {value} on {activity}");
                activity.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value.Value));
            }
        }
        protected abstract float? ExtractValue(Activity activity, bool forceJustMe);

    }

    public class CalculateFieldSimpleDefault : CalculateFieldSimple
    {
        public CalculateFieldSimpleDefault(string activityFieldname,
                                           string dependentFieldname,
                                           float defaultValue,
                                           Mode extractionMode,
                                           OverrideMode overrideWhen,
                                           List<string>? sportsToInclude = null) :
            base(activityFieldname, overrideWhen, sportsToInclude)
        {
            DependentFieldname = dependentFieldname;
            DefaultValue = defaultValue;
            ExtractionMode = extractionMode;
        }

        public string DependentFieldname { get; set; }
        public float DefaultValue { get; set; }

        public enum Mode { Multiply } //add others as needed
        Mode ExtractionMode { get; set; }

        public override string ToString()
        {
            return $"CalculateFieldSimpleDefault: Type {this.GetType().Name} ActivityFieldname {ActivityFieldname}";
        }


        protected override float? ExtractValue(Activity activity, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleDefault Forced ExtractValue {ActivityFieldname}");
            if (!activity.HasNamedDatum(DependentFieldname))
                return null;

            float? dependent = activity.GetNamedFloatDatum(DependentFieldname);

            if (dependent != null && ExtractionMode == Mode.Multiply)
            {
                dependent *= DefaultValue;
            }

            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleDefault Forced ExtractValue retval {dependent}");
            return dependent;
        }

    }

    public class CalculateFieldSimpleCopy : CalculateFieldSimple
    {
        public CalculateFieldSimpleCopy(string activityFieldname,
                                        string dependentFieldname,
                                        OverrideMode overrideWhen,
                                        List<string>? sportsToInclude = null) :
            base(activityFieldname, overrideWhen, sportsToInclude)
        {
            DependentFieldname = dependentFieldname;
        }

        public string DependentFieldname { get; set; }

        public override string ToString()
        {
            return $"CalculateFieldSimpleCopy: Type {this.GetType().Name} ActivityFieldname {ActivityFieldname}";
        }


        protected override float? ExtractValue(Activity activity, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleCopy Forced ExtractValue {ActivityFieldname}");
            if (!activity.HasNamedDatum(DependentFieldname))
                return null;

            float? dependent = activity.GetNamedFloatDatum(DependentFieldname);

            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleCopy Forced ExtractValue retval {dependent}");
            return dependent;
        }

    }

    public class CalculateFieldSimpleMath : CalculateFieldSimple
    {
        public CalculateFieldSimpleMath(string activityFieldname,
                                           string firstFieldName,
                                           string secondFieldname,
                                           Mode extractionMode,
                                           OverrideMode overrideWhen,
                                           List<string>? sportsToInclude = null) :
            base(activityFieldname, overrideWhen, sportsToInclude)
        {
            FirstFieldname = firstFieldName;
            SecondFieldname = secondFieldname;
            ExtractionMode = extractionMode;
        }

        public string FirstFieldname { get; set; }
        public string SecondFieldname { get; set; }

        public enum Mode { Subtract } //add others as needed
        Mode ExtractionMode { get; set; }

        public override string ToString()
        {
            return $"CalculateFieldSimpleMath: [Type {this.GetType().Name} ActivityFieldname {ActivityFieldname} FirstFieldname {FirstFieldname} SecondFieldname {SecondFieldname} Mode {ExtractionMode}]";
        }


        protected override float? ExtractValue(Activity activity, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleMath Forced ExtractValue {ActivityFieldname}");
            if (!activity.HasNamedDatum(FirstFieldname))
                return null;
            if (!activity.HasNamedDatum(SecondFieldname))
                return null;

            float? f1 = activity.GetNamedFloatDatum(FirstFieldname);
            float? f2 = activity.GetNamedFloatDatum(SecondFieldname);
            float? result = null;
            if (f1 != null && f2 != null && ExtractionMode == Mode.Subtract)
            {
                result = (float)f1 - (float)f2;
            }

            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleMath Forced ExtractValue retval {result}");
            return result;
        }

    }

}
