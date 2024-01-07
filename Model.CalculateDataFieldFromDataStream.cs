using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FellrnrTrainingAnalysis.Model
{
    public abstract class CalculateDataFieldFromDataStreamBase : ICalculateField
    {
        public CalculateDataFieldFromDataStreamBase(string activityFieldname, DataStreamBase ds)
        {
            dataStream = ds;
            SourceStreamName = ds.Name;
            ActivityFieldname = activityFieldname;
        }

        public CalculateDataFieldFromDataStreamBase(string activityFieldname, string sourceStreamName)
        {
            SourceStreamName = sourceStreamName;
            dataStream = null;
            ActivityFieldname = activityFieldname;
        }

        private string SourceStreamName;
        private DataStreamBase? dataStream = null;


        private DataStreamBase? DataStream(Activity activity)
        {
            if (dataStream != null)
                return dataStream;


            if (!activity.TimeSeries.ContainsKey(SourceStreamName))
                return null;

            return activity.TimeSeries[SourceStreamName];
        }

        private Tuple<uint[], float[]>? GetUnderlyingDataStream(Activity parent)
        {
            return DataStream(parent) == null ? null : DataStream(parent)!.GetData();
        }


        public string ActivityFieldname { get; set; }


        public void Recalculate(Extensible extensible, bool force)
        {
            if (extensible == null || extensible is not Activity)
            {
                return;
            }

            Activity activity = (Activity)extensible;


            if (DataStream(activity) == null)
                return;

            DataStream(activity)!.Recalculate(force);


            if (activity.HasNamedDatum(ActivityFieldname) && !force)
                return;

            Tuple<uint[], float[]>? data = GetUnderlyingDataStream(activity);
            if (data == null)
            {
                activity.RemoveNamedDatum(ActivityFieldname);
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
        public CalculateDataFieldFromDataStreamSimple(string activityFieldname, Mode extractionMode, DataStreamBase ds) : base(activityFieldname, ds)
        {
            ExtractionMode = extractionMode;
        }
        public CalculateDataFieldFromDataStreamSimple(string activityFieldname, Mode extractionMode, string sourceStreamName) : base(activityFieldname, sourceStreamName)
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

    public class CalculateDataFieldFromDataStreamAUC : CalculateDataFieldFromDataStreamBase
    {
        public CalculateDataFieldFromDataStreamAUC(string activityFieldname, bool negate, float min, float? max, DataStreamBase ds) : base(activityFieldname, ds)
        {
            Negate = negate; Min = min; Max = max;
        }
        public CalculateDataFieldFromDataStreamAUC(string activityFieldname, bool negate, float min, float? max, string sourceStreamName) : base(activityFieldname, sourceStreamName)
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
