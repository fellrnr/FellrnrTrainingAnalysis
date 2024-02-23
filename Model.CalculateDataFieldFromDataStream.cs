using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateDataFieldFromDataStreamBase : CalculateFieldBase
    {
        public CalculateDataFieldFromDataStreamBase(string activityFieldname, string sourceStreamName, List<string>? sportsToInclude = null)
        {
            SourceStreamName = sourceStreamName;
            SourceDataStream = null;
            ActivityFieldname = activityFieldname;
            SportsToInclude = sportsToInclude;
        }

        List<string>? SportsToInclude;
        private string SourceStreamName;
        private DataStreamBase? SourceDataStream = null;


        private DataStreamBase? DataStream(Activity activity)
        {
            if (SourceDataStream != null)
                return SourceDataStream;


            if (!activity.TimeSeries.ContainsKey(SourceStreamName))
                return null;

            return activity.TimeSeries[SourceStreamName];
        }

        private Tuple<uint[], float[]>? GetUnderlyingDataStream(Activity parent)
        {
            //calling DataStream(parent) does the computation,
            DataStreamBase? dataStreamBase = DataStream(parent);
            return dataStreamBase == null ? null : dataStreamBase!.GetData();
        }


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

            if (activity.HasNamedDatum(ActivityFieldname) && !force)
                return;

            //always remove if we're recalculating
            activity.RemoveNamedDatum(ActivityFieldname);

            if (SportsToInclude != null && !activity.CheckSportType(SportsToInclude))
                return;

            if (DataStream(activity) == null)
                return;

            DataStream(activity)!.Recalculate(forceCount, false);


            Tuple<uint[], float[]>? data = GetUnderlyingDataStream(activity);
            if (data == null)
            {
                return;
            }

            float value = ExtractValue(data);
            if(value != 0)
                activity.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value));

        }
        protected abstract float ExtractValue(Tuple<uint[], float[]> data);

    }

    public class CalculateDataFieldFromDataStreamSimple : CalculateDataFieldFromDataStreamBase
    {
        public CalculateDataFieldFromDataStreamSimple(string activityFieldname, Mode extractionMode, string sourceStreamName, List<string>? sportsToInclude = null) : 
            base(activityFieldname, sourceStreamName, sportsToInclude)
        {
            ExtractionMode = extractionMode;
        }
        public enum Mode { LastValue, Average, Min, Max }
        Mode ExtractionMode { get; set; }

        protected override float ExtractValue(Tuple<uint[], float[]> data)
        {
            float value = 0;
            if (ExtractionMode == Mode.LastValue)
            {
                value = data.Item2.Last();
            }
            else if (ExtractionMode == Mode.Average)
            {
                value = data.Item2.Average(); //TODO: Add average ignoring zeros
            }
            else if (ExtractionMode == Mode.Max)
            {
                value = data.Item2.Max();
            }
            else if (ExtractionMode == Mode.Min)
            {
                value = data.Item2.Min();
            }
            return value;
        }
    }
    public class CalculateDataFieldFromDataStreamThreashold : CalculateDataFieldFromDataStreamBase
    {
        public CalculateDataFieldFromDataStreamThreashold(string activityFieldname, Mode extractionMode, float threashold, string sourceStreamName, List<string>? sportsToInclude = null) : 
            base(activityFieldname, sourceStreamName, sportsToInclude)
        {
            ExtractionMode = extractionMode;
            Threashold = threashold;
        }
        public enum Mode { AboveAbs, BelowAbs, AbovePercent, BelowPercent }
        Mode ExtractionMode { get; set; }

        float Threashold {  get; set; }

        protected override float ExtractValue(Tuple<uint[], float[]> data)
        {
            uint pastThreashold = 0;
            uint lastTime = 0;
            for (int i = 0; i < data.Item1.Length; i++)
            {
                uint thisTime = data.Item1[i] - lastTime;
                float thisValue = data.Item2[i];
                if (ExtractionMode == Mode.AboveAbs && thisValue > Threashold)
                {
                    pastThreashold += thisTime;
                }
                else if (ExtractionMode == Mode.BelowAbs && thisValue < Threashold)
                {
                    pastThreashold += thisTime;
                }
                else if (ExtractionMode == Mode.AbovePercent && thisValue > Threashold)
                {
                    pastThreashold += thisTime;
                }
                else if (ExtractionMode == Mode.BelowPercent && thisValue < Threashold)
                {
                    pastThreashold += thisTime;
                }

                lastTime = data.Item1[i];
            }

            if(ExtractionMode == Mode.AbovePercent || ExtractionMode == Mode.BelowPercent)
            {
                float percent = (pastThreashold * 100.0f) / ((float)lastTime);
                return percent;
            }
            else
            {
                return pastThreashold;
            }
        }
    }

    public class CalculateDataFieldFromDataStreamAUC : CalculateDataFieldFromDataStreamBase
    {
        public CalculateDataFieldFromDataStreamAUC(string activityFieldname, bool negate, float min, float? max, string sourceStreamName, List<string>? sportsToInclude = null) : 
            base(activityFieldname, sourceStreamName, sportsToInclude)
        {
            Negate = negate; Min = min; Max = max;
        }

        bool Negate { get; set; }
        float Min { get; set; }
        float? Max { get; set; }

        protected override float ExtractValue(Tuple<uint[], float[]> data)
        {
            float sum = 0;
            uint lastTime = 0;

            for (int i = 0; i < data.Item1.Length; i++)
            {
                uint time = data.Item1[i];
                double value = data.Item2[i];
                if(Negate)
                    value = -value;
                if (value > Min)
                {
                    uint timespan = time - lastTime;
                    double collared = Max is null ? value : Math.Min((double)Max, (double)value);
                    double offset = collared - Min;
                    double areaUnderCurve = offset * timespan;
                    sum += (float)areaUnderCurve;
                }
                lastTime = time;
            }
            return sum;
        }
    }
}
