using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateDataFieldFromTimeSeriesBase : CalculateFieldBase
    {
        public CalculateDataFieldFromTimeSeriesBase(string activityFieldname, string sourceStreamName, List<string>? sportsToInclude = null, bool flatStartOnly = false)
        {
            SourceStreamName = sourceStreamName;
            ActivityFieldnames = new string[] { activityFieldname };
            SportsToInclude = sportsToInclude;
            FlatStartOnly = flatStartOnly;
        }

        public CalculateDataFieldFromTimeSeriesBase(string[] activityFieldnames, string sourceStreamName, List<string>? sportsToInclude = null, bool flatStartOnly = false)
        {
            SourceStreamName = sourceStreamName;
            ActivityFieldnames = activityFieldnames;
            SportsToInclude = sportsToInclude;
            FlatStartOnly = flatStartOnly;
        }

        List<string>? SportsToInclude;
        private string SourceStreamName;
        bool FlatStartOnly { get; set; }

        public string[] ActivityFieldnames { get; set; }


        public override void Recalculate(Extensible extensible, int forceCount, bool forceJustMe)
        {
            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (forceJustMe) Logging.Instance.TraceEntry($"CalculateDataFieldFromTimeSeriesBase Forced recalculating {ActivityFieldnames[0]}");


            if (extensible == null || extensible is not Activity)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No activity");
                return;
            }

            Activity activity = (Activity)extensible;

            if (FlatStartOnly)
            {
                const string startele = "Vertical 5 Min";
                float? ele5min = activity.GetNamedFloatDatum(startele);
                if (forceJustMe) Logging.Instance.Debug($"starting elevation change is {ele5min}");
                if (ele5min != null && ele5min > 50)
                {
                    if (forceJustMe) Logging.Instance.TraceLeave($"starting vertical changes too much {ele5min}");
                    return;
                }

                const string startMinSpeed = "Min Speed 5 Min";
                float? mspeed5min = activity.GetNamedFloatDatum(startMinSpeed);
                if (forceJustMe) Logging.Instance.Debug($"starting speed is {mspeed5min}");
                if (mspeed5min != null && mspeed5min < 2.0) //probably walking
                {
                    if (forceJustMe) Logging.Instance.TraceLeave($"starting speed drops too low {mspeed5min}");
                    return;
                }
            }

            foreach (string activityFieldname in ActivityFieldnames)
            {
                if (activity.HasNamedDatum(activityFieldname) && !force)
                {
                    if (forceJustMe) Logging.Instance.TraceLeave($"No activity");
                    return;
                }
                //always remove if we're recalculating
                activity.RemoveNamedDatum(activityFieldname);
            }
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

            float[]? values = ExtractValue(data, force);


            if (values != null)
            {
                for(int i=0; i < values.Length; i++)
                {
                    string activityFieldname = ActivityFieldnames[i];
                    float v = values[i];
                    activity.AddOrReplaceDatum(new TypedDatum<float>(activityFieldname, false, v));
                }
            }
            if (forceJustMe) Logging.Instance.TraceLeave($"CalculateDataFieldFromTimeSeriesBase Forced ExtractValue {ActivityFieldnames[0]} retval {values}");
        }
        protected abstract float[]? ExtractValue(TimeValueList data, bool forceJustMe);

        public override string ToString()
        {
            return $"CalculateDataFieldFromTimeSeriesBase: Type {this.GetType().Name} ActivityFieldname {ActivityFieldnames[0]}";
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

        protected override float[]? ExtractValue(TimeValueList data, bool forceJustMe)
        {
            float? value = null;
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
            if (value == null)
                return null;
            else
                return new float[] { value.Value };
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

        protected override float[]? ExtractValue(TimeValueList data, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateDataFieldFromTimeSeriesThreashold Forced ExtractValue {ActivityFieldnames[0]}");
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
                return new float[] { percent };
            }
            else
            {
                return new float[] { pastThreashold };
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

        protected override float[]? ExtractValue(TimeValueList data, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateDataFieldFromTimeSeriesAUC Forced ExtractValue {ActivityFieldnames[0]}");


            if (data.Length == 0)
                return null;

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
            return new float[] { sum };
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
                                                      int end = 0, 
                                                      bool flatStartOnly = false) :
            base(activityFieldname, sourceStreamName, sportsToInclude, flatStartOnly)
        {
            ExtractionMode = extractionMode;
            Start = start;
            End = end;
        }
        public enum Mode { LastValue, Average, Min, Max, SumAbsDeltas }
        Mode ExtractionMode { get; set; }

        int Start { get; set; }
        int End { get; set; }
        protected override float[]? ExtractValue(TimeValueList data, bool forceJustMe)
        {
            float? value = null;

            TimeValueList? dataSubset = TimeValueList.ExtractWindow(data, Start, End);
            if (dataSubset == null) { return null; } //Note, was return 0;

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
            else if (ExtractionMode == Mode.SumAbsDeltas)
            {
                float? prev = null;
                foreach (var entry in dataSubset.Values)
                {
                    if (value == null) value = 0;

                    if(prev != null)
                    {
                        value += Math.Abs(entry - prev.Value);
                    }
                    prev = entry;
                }
            }
            if (value == null)
                return null;
            else
                return new float[] { value.Value };
        }
    }

    public class CalculateDataFieldFromTimeSeriesZones : CalculateDataFieldFromTimeSeriesBase
    {

        private static string[] GenerateNames(string activityNameBase, int count)
        {
            List<string> names = new List<string>();
            for(int i = 0; i < count; i++)
            {
                names.Add($"{activityNameBase}{i+1}");
            }
            for (int i = 0; i < count; i++)
            {
                names.Add($"{activityNameBase}{i+1}%");
            }
            return names.ToArray();
        }
        public CalculateDataFieldFromTimeSeriesZones(string activityNameBase, int[] zones, string sourceStreamName, List<string>? sportsToInclude = null) :
            base(GenerateNames(activityNameBase, zones.Length-1), sourceStreamName, sportsToInclude)
        {
            Zones = zones;
        }
        int[] Zones { get; set; }

        protected override float[]? ExtractValue(TimeValueList data, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"CalculateDataFieldFromTimeSeriesZones Forced ExtractValue {ActivityFieldnames[0]}");


            float[] accumulator = new float[Zones.Length-1]; //init to zeros in c#

            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < Zones.Length-1; j++)
                {
                    float v = data.Values[i];
                    if (v >= Zones[j] && v < Zones[j + 1])
                    {
                        accumulator[j]++;
                    }
                }
            }

            float[] percent = new float[Zones.Length-1]; 
            for(int i = 0; i < Zones.Length - 1; i++)
            {
                percent[i] = (float) accumulator[i] / (float)data.Length * 100.0f;
            }

            return accumulator.Concat(percent).ToArray();
        }
    }

}
