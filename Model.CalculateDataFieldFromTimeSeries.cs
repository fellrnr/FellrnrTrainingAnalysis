using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateDataFieldFromTimeSeriesBase : CalculateFieldBase
    {
        public CalculateDataFieldFromTimeSeriesBase(string activityFieldname, string sourceStreamName, List<string>? sportsToInclude = null)
        {
            SourceStreamName = sourceStreamName;
            ActivityFieldname = activityFieldname;
            SportsToInclude = sportsToInclude;
        }

        List<string>? SportsToInclude;
        private string SourceStreamName;

        public string ActivityFieldname { get; set; }


        public override void Recalculate(Extensible extensible, int forceCount, bool forceJustMe)
        {
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (forceJustMe) Logging.Instance.TraceEntry($"CalculateDataFieldFromTimeSeriesBase Forced recalculating {ActivityFieldname}");


            if (extensible == null || extensible is not Activity)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No activity");
                return;
            }

            Activity activity = (Activity)extensible;

            if (activity.HasNamedDatum(ActivityFieldname) && !force)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No activity");
                return;
            }
            //always remove if we're recalculating
            activity.RemoveNamedDatum(ActivityFieldname);

            if (SportsToInclude != null && !activity.CheckSportType(SportsToInclude))
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"Wrong type {activity.ActivityType}");
                return;
            }

            if (!activity.TimeSeries.ContainsKey(SourceStreamName))
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No time series {SourceStreamName}");
                return;
            }

            TimeSeriesBase ts = activity.TimeSeries[SourceStreamName];

            //ts.Recalculate(forceCount, forceJustMe); //if recalculation is required, it will have been done earlier

            if (!ts.IsValid())
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"Not valid {SourceStreamName}");
                return;
            }

            TimeValueList? data = ts.GetData(forceCount, forceJustMe);
            if (data == null)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No underlying time series data");
                return;
            }

            float value = ExtractValue(data, force);


            if (value != 0)
                activity.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value));

            if (forceJustMe) Logging.Instance.TraceLeave($"CalculateDataFieldFromTimeSeriesBase Forced ExtractValue {ActivityFieldname} retval {value}");
        }
        protected abstract float ExtractValue(TimeValueList data, bool forceJustMe);

        public override string ToString()
        {
            return $"CalculateDataFieldFromTimeSeriesBase: Type {this.GetType().Name} ActivityFieldname {ActivityFieldname}";
        }
    }

    public class CalculateDataFieldFromTimeSeriesSimple : CalculateDataFieldFromTimeSeriesBase
    {
        public CalculateDataFieldFromTimeSeriesSimple(string activityFieldname, Mode extractionMode, string sourceStreamName, List<string>? sportsToInclude = null) :
            base(activityFieldname, sourceStreamName, sportsToInclude)
        {
            ExtractionMode = extractionMode;
        }
        public enum Mode { LastValue, Average, Min, Max }
        Mode ExtractionMode { get; set; }

        protected override float ExtractValue(TimeValueList data, bool forceJustMe)
        {
            float value = 0;
            if (ExtractionMode == Mode.LastValue)
            {
                value = data.Values.Last();
            }
            else if (ExtractionMode == Mode.Average)
            {
                value = data.Values.Average(); //TODO: Add average ignoring zeros
            }
            else if (ExtractionMode == Mode.Max)
            {
                value = data.Values.Max();
            }
            else if (ExtractionMode == Mode.Min)
            {
                value = data.Values.Min();
            }
            return value;
        }
    }
    public class CalculateDataFieldFromTimeSeriesThreashold : CalculateDataFieldFromTimeSeriesBase
    {
        public CalculateDataFieldFromTimeSeriesThreashold(string activityFieldname, Mode extractionMode, float threashold, string sourceStreamName, List<string>? sportsToInclude = null) :
            base(activityFieldname, sourceStreamName, sportsToInclude)
        {
            ExtractionMode = extractionMode;
            Threashold = threashold;
        }
        public enum Mode { AboveAbs, BelowAbs, AbovePercent, BelowPercent }
        Mode ExtractionMode { get; set; }

        float Threashold { get; set; }

        protected override float ExtractValue(TimeValueList data, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateDataFieldFromTimeSeriesThreashold Forced ExtractValue {ActivityFieldname}");
            uint pastThreashold = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float thisValue = data.Values[i];
                if (ExtractionMode == Mode.AboveAbs && thisValue > Threashold)
                {
                    pastThreashold++;
                }
                else if (ExtractionMode == Mode.BelowAbs && thisValue < Threashold)
                {
                    pastThreashold++;
                }
                else if (ExtractionMode == Mode.AbovePercent && thisValue > Threashold)
                {
                    pastThreashold++;
                }
                else if (ExtractionMode == Mode.BelowPercent && thisValue < Threashold)
                {
                    pastThreashold++;
                }
            }

            if (ExtractionMode == Mode.AbovePercent || ExtractionMode == Mode.BelowPercent)
            {
                float percent = (pastThreashold * 100.0f) / ((float)data.Length);
                return percent;
            }
            else
            {
                return pastThreashold;
            }
        }
    }

    public class CalculateDataFieldFromTimeSeriesAUC : CalculateDataFieldFromTimeSeriesBase
    {
        public CalculateDataFieldFromTimeSeriesAUC(string activityFieldname, bool negate, float min, float? max, string sourceStreamName, List<string>? sportsToInclude = null) :
            base(activityFieldname, sourceStreamName, sportsToInclude)
        {
            Negate = negate; Min = min; Max = max;
        }

        bool Negate { get; set; }
        float Min { get; set; }
        float? Max { get; set; }

        protected override float ExtractValue(TimeValueList data, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateDataFieldFromTimeSeriesAUC Forced ExtractValue {ActivityFieldname}");

            float sum = 0;

            for (int i = 0; i < data.Length; i++)
            {
                double value = data.Values[i];
                if (Negate)
                    value = -value;
                if (value > Min)
                {
                    uint timespan = 1;
                    double collared = Max is null ? value : Math.Min((double)Max, (double)value);
                    double offset = collared - Min;
                    double areaUnderCurve = offset * timespan;
                    sum += (float)areaUnderCurve;
                }
            }
            return sum;
        }
    }

    public class CalculateDataFieldFromTimeSeriesWindow : CalculateDataFieldFromTimeSeriesBase
    {
        //zero end means to the end
        public CalculateDataFieldFromTimeSeriesWindow(string activityFieldname,
                                                      Mode extractionMode,
                                                      string sourceStreamName,
                                                      List<string>? sportsToInclude,
                                                      int start,
                                                      int end = 0) :
            base(activityFieldname, sourceStreamName, sportsToInclude)
        {
            ExtractionMode = extractionMode;
            Start = start;
            End = end;
        }
        public enum Mode { LastValue, Average, Min, Max }
        Mode ExtractionMode { get; set; }

        int Start { get; set; }
        int End { get; set; }

        protected override float ExtractValue(TimeValueList data, bool forceJustMe)
        {
            float value = 0;

            TimeValueList? dataSubset = TimeValueList.ExtractWindow(data, Start, End);
            if (dataSubset == null) { return 0; }

            if (ExtractionMode == Mode.LastValue)
            {
                value = dataSubset.Values.Last();
            }
            else if (ExtractionMode == Mode.Average)
            {
                value = dataSubset.Values.Average(); //TODO: Add average ignoring zeros
            }
            else if (ExtractionMode == Mode.Max)
            {
                value = dataSubset.Values.Max();
            }
            else if (ExtractionMode == Mode.Min)
            {
                value = dataSubset.Values.Min();
            }
            return value;
        }
    }


}
