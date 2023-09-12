using System.Collections.ObjectModel;
using static FellrnrTrainingAnalysis.Utils.Utils;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]

    //A simple class to calculate the delta of another data stream, such as speed as the delta of distance
    public class DataStreamDelta : DataStreamEphemeral
    {
        public DataStreamDelta(string name, List<string> requiredFields, float scalingFactor = 1, float? numerator = null, float? period = null, float? limit = null) : base(name, requiredFields)
        {
            if (requiredFields.Count != 1) throw new ArgumentException("DataStreamDelta must have only one required field");
            ScalingFactor = scalingFactor;
            Numerator = numerator;
            Period = period;
            Limit = limit;
        }

        float ScalingFactor { get; set; }
        float? Numerator { get; set; }

        float? Period { get; set; }

        float? Limit{ get; set; }

        public override Tuple<uint[], float[]>? GetData(Activity parent)
        {
            ReadOnlyDictionary<string, IDataStream> timeSeries = parent.TimeSeries;
            string field = RequiredFields[0];
            IDataStream dataStream = timeSeries[field];
            Tuple<uint[], float[]>? data = dataStream.GetData(parent);
            if (data == null) { return null; }

            if (data.Item2.Min() == 0 && data.Item2.Max() == 0) //all zeros and we don't really have any data
                return null;

                Tuple<uint[], float[]>? newData;

            if(Period == null || Period == 1)
            {
                newData = TimeSeries.SimpleDeltas(data, ScalingFactor, Numerator, Limit);
            }
            else
            {
                newData = TimeSeries.SpanDeltas(data, ScalingFactor, Numerator, Limit, (float)Period);
            }
            return newData;
        }

        float InterpolateValue(float value1, float time1, float value2, float time2, float time)
        {
            return value1 + ((value2 - value1) * (time - time1)) / (time2 - time1);
        }




        //TODO:Calculate averages and put them on activity. 
        public override void Recalculate(Activity parent, bool force) { return; }

    }
}
