using FellrnrTrainingAnalysis.Utils;
using static System.Net.WebRequestMethods;

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

            Datum? datum = extensible.GetNamedDatum(ActivityFieldname);
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


            if (extensible.HasNamedDatum(ActivityFieldname) && !force)
                return;

            //always remove if we're recalculating
            extensible.RemoveNamedDatum(ActivityFieldname);

            if (!extensible.CheckSportType(SportsToInclude))
                return;

            float? value = ExtractValue(extensible, forceJustMe);
            if (value != null)
            {
                if (forceJustMe) Logging.Instance.Debug($"Will replace/add datum {ActivityFieldname} with {value} on {extensible}");
                extensible.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value.Value));
            }
        }
        protected abstract float? ExtractValue(Extensible extensible, bool forceJustMe);

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


        protected override float? ExtractValue(Extensible extensible, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleDefault Forced ExtractValue {ActivityFieldname}");
            if (!extensible.HasNamedDatum(DependentFieldname))
                return null;

            float? dependent = extensible.GetNamedFloatDatum(DependentFieldname);

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


        protected override float? ExtractValue(Extensible extensible, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleCopy Forced ExtractValue {ActivityFieldname}");
            if (!extensible.HasNamedDatum(DependentFieldname))
                return null;

            float? dependent = extensible.GetNamedFloatDatum(DependentFieldname);

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

        public enum Mode { Subtract, Max } //add others as needed
        Mode ExtractionMode { get; set; }

        public override string ToString()
        {
            return $"CalculateFieldSimpleMath: [Type {this.GetType().Name} ActivityFieldname {ActivityFieldname} FirstFieldname {FirstFieldname} SecondFieldname {SecondFieldname} Mode {ExtractionMode}]";
        }


        protected override float? ExtractValue(Extensible extensible, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleMath Forced ExtractValue {ActivityFieldname}");
            if (!extensible.HasNamedDatum(FirstFieldname))
                return null;
            if (!extensible.HasNamedDatum(SecondFieldname))
                return null;

            float? f1 = extensible.GetNamedFloatDatum(FirstFieldname);
            float? f2 = extensible.GetNamedFloatDatum(SecondFieldname);
            float? result = null;
            if (f1 != null && f2 != null && ExtractionMode == Mode.Subtract)
            {
                result = (float)f1 - (float)f2;
            }
            if (ExtractionMode == Mode.Max)
            {
                result = Utils.Misc.Max(f1, f2);
            }

            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleMath Forced ExtractValue retval {result}");
            return result;
        }

    }
    public class CalculateFieldPolarizationIndex : CalculateFieldSimple
    {
        public CalculateFieldPolarizationIndex(string activityFieldname) :
            base(activityFieldname, OverrideMode.Always, null)
        {
        }

        public override string ToString()
        {
            return $"CalculateFieldPolarizationIndex: [Type {this.GetType().Name} ActivityFieldname {ActivityFieldname}";
        }

        private const string Zone0 = "Σ 3-Zone-0 30D";
        private const string Zone1 = "Σ 3-Zone-1 30D";
        private const string Zone2 = "Σ 3-Zone-2 30D";
        private const string Zone3 = "Σ 3-Zone-3 30D";
        private const string Zone4 = "Σ 3-Zone-4 30D";


        //https://www.frontiersin.org/journals/physiology/articles/10.3389/fphys.2019.00707/full
        protected override float? ExtractValue(Extensible extensible, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleMath Forced ExtractValue {ActivityFieldname}");
            if (!extensible.HasNamedDatum(Zone1))
                return null;
            if (!extensible.HasNamedDatum(Zone2))
                return null;
            if (!extensible.HasNamedDatum(Zone3))
                return null;

            float? z0n = extensible.GetNamedFloatDatum(Zone0);
            float? z1n = extensible.GetNamedFloatDatum(Zone1);
            float? z2n = extensible.GetNamedFloatDatum(Zone2);
            float? z3n = extensible.GetNamedFloatDatum(Zone3);
            float? z4n = extensible.GetNamedFloatDatum(Zone4);
            float? polarization = null;
            if (z0n != null && z1n != null && z2n != null && z3n != null && z4n != null)
            {
                float z0 = z0n.Value;
                float z1 = z1n.Value;
                float z2 = z2n.Value;
                float z3 = z3n.Value;
                float z4 = z4n.Value;
                float total = z0 + z1 + z2 + z3 + z4;
                float z1p = z1 / total;
                float z2p = z2 / total;
                float z3p = z3 / total;
                if (z3p > z1p)
                {
                    polarization = null;// invalid
                }
                else if(z3p == 0)
                {
                    polarization = 0;
                }
                else if (z2p == 0)
                {
                    polarization = (float)Math.Log10(z1p / 0.01 * (z3p - 0.01) * 100.0);
                }
                else
                {
                    polarization = (float)Math.Log10(z1p / z2p * z3p * 100.0);
                }
            }

            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldSimpleMath Forced ExtractValue retval {polarization}");
            return polarization;
        }

    }

    public class CalculateFieldTSS : CalculateFieldSimple
    {
        public CalculateFieldTSS(string activityFieldname, float cpScalingFactor = 1.0f, List<string>? sportsToInclude = null) :
            base(activityFieldname, OverrideMode.Always, sportsToInclude)
        {
            CpScalingFactor = cpScalingFactor;
        }

        float CpScalingFactor { get; set;  }

        public override string ToString()
        {
            return $"CalculateFieldTSS: [Type {this.GetType().Name} ActivityFieldname {ActivityFieldname}";
        }

        //https://www.trainingpeaks.com/learn/articles/estimating-training-stress-score-tss/
        protected override float? ExtractValue(Extensible extensible, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldTSS Forced ExtractValue {ActivityFieldname}");
            if (!extensible.HasNamedDatum(Activity.TagAveragePower))
                return null;
            if (!extensible.HasNamedDatum(Activity.TagElapsedTime))
                return null;

            if (extensible is not Activity)
                return null;

            Activity activity = (Activity)extensible;
            CalendarNode day = activity.Day;

            if (!day.HasNamedDatum(CalendarNode.TagCriticalPower))
                return null;

            float? cp = day.GetNamedFloatDatum(CalendarNode.TagCriticalPower) * CpScalingFactor;
            float? ap = extensible.GetNamedFloatDatum(Activity.TagAveragePower);
            float? sec = extensible.GetNamedFloatDatum(Activity.TagElapsedTime);
            float? trimp = null;
            if (cp != null && ap != null)
            {
                float intensity = (float)ap / (float)cp;
                trimp = (sec * ap * intensity)/ (cp * 3600.0f) * 100.0f;
            }
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldTSS Forced ExtractValue retval {trimp}");
            return trimp;
        }

    }

    public class CalculateFieldIF : CalculateFieldSimple
    {
        public CalculateFieldIF(string activityFieldname, float cpScalingFactor = 1.0f, List<string>? sportsToInclude = null) :
            base(activityFieldname, OverrideMode.Always, sportsToInclude)
        {
            CpScalingFactor = cpScalingFactor;
        }

        float CpScalingFactor { get; set; }

        public override string ToString()
        {
            return $"CalculateFieldIF [Type {this.GetType().Name} ActivityFieldname {ActivityFieldname}";
        }

        //https://www.trainingpeaks.com/learn/articles/estimating-training-stress-score-tss/
        protected override float? ExtractValue(Extensible extensible, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldIF Forced ExtractValue {ActivityFieldname}");
            if (!extensible.HasNamedDatum(Activity.TagAveragePower))
                return null;
            if (!extensible.HasNamedDatum(Activity.TagElapsedTime))
                return null;

            if (extensible is not Activity)
                return null;

            Activity activity = (Activity)extensible;
            CalendarNode day = activity.Day;

            if (!day.HasNamedDatum(CalendarNode.TagCriticalPower))
                return null;

            float? cp = day.GetNamedFloatDatum(CalendarNode.TagCriticalPower) * CpScalingFactor;
            float? ap = extensible.GetNamedFloatDatum(Activity.TagAveragePower);
            float? intensity = null;
            if (cp != null && ap != null)
            {
                intensity = (float)ap / (float)cp;
            }
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldIF Forced ExtractValue retval {intensity}");
            return intensity;
        }

    }

    public class CalculateFieldArrhythmia : CalculateFieldSimple
    {
        public CalculateFieldArrhythmia(string activityFieldname, List<string>? sportsToInclude = null) :
            base(activityFieldname, OverrideMode.Always, sportsToInclude)
        {
        }

        public override string ToString()
        {
            return $"CalculateFieldArrhythmia [Type {this.GetType().Name} ActivityFieldname {ActivityFieldname}";
        }

        protected override float? ExtractValue(Extensible extensible, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldIF Forced ExtractValue {ActivityFieldname}");
            if (!extensible.HasNamedDatum(Activity.TagAveragePower))
                return null;
            if (!extensible.HasNamedDatum(Activity.TagElapsedTime))
                return null;

            if (extensible is not Activity)
                return null;

            Activity activity = (Activity)extensible;

            if (activity.RRIntervals == null || activity.RRIntervals.Length == 0)
                return null;

            int count = 0;
            int sum = 0;
            const int windowSize = 30;
            for (int i=0; i < activity.RRIntervals.Length; i++)
            {
                short rr = activity.RRIntervals[i];
                sum += rr;
                //wait until we have enough samples
                if (i > windowSize)
                {
                    short rrOld = activity.RRIntervals[i - windowSize];
                    sum -= rrOld;
                    float avg = sum / windowSize;
                    //anything 40% away from the average is an arrhythmia
                    if (Math.Abs(rr - avg) > 0.4 * avg)
                        count++;

                }

            }

            if (forceJustMe)
                Logging.Instance.Debug($"CalculateFieldIF Forced ExtractValue retval {count}");
            return count;
        }

    }


}
