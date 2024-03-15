using MemoryPack;
using System.Collections.ObjectModel;
using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]

    //A simple class to calculate the delta of another data stream, such as speed as the delta of distance
    public partial class TimeSeriesDelta : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesDelta()  //for use by memory pack deserialization only
        {
        }
        public TimeSeriesDelta(string name, List<List<string>> requiredFields, Activity activity, float scalingFactor = 1, float? numerator = null, float? period = null, float? limit = null) : 
            base(name, requiredFields, activity)
        {
            if (requiredFields.Count != 1) throw new ArgumentException("TimeSeriesDelta must have only one required field");
            ScalingFactor = scalingFactor;
            Numerator = numerator;
            Period = period;
            Limit = limit;
        }

        [MemoryPackInclude]
        float ScalingFactor { get; set; }
        [MemoryPackInclude]
        float? Numerator { get; set; }

        [MemoryPackInclude]
        float? Period { get; set; }

        [MemoryPackInclude]
        float? Limit{ get; set; }

        public override TimeValueList? CalculateData(bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"TimeSeriesDelta: Forced recalculating {this.Name}");
            if (ParentActivity  == null) return null;

            //Crude way of debugging a data stream deltas
            bool extraDebug = false;
            if (ParentActivity.PrimaryKey().Contains("10478327023") && Name == TimeSeriesFactory.GRADE_ADUJUSTED_PACE)
            {
                extraDebug = true;
            }

            ReadOnlyDictionary<string, TimeSeriesBase> timeSeries = ParentActivity.TimeSeries;
            TimeSeriesBase dataStream = RequiredTimeSeries[0];
            TimeValueList? data = dataStream.GetData();
            if (data == null) { return null; }

            if (data.Values.Min() == 0 && data.Values.Max() == 0) //all zeros and we don't really have any data
                return null;

            TimeValueList? newData;

            

            if(Period == null || Period == 1)
            {
                newData = TimeValueList.SimpleDeltas(data, ScalingFactor, Numerator, Limit);
            }
            else
            {
                newData = TimeValueList.SpanDeltas(data, ScalingFactor, Numerator, Limit, (float)Period, extraDebug);
            }
            return newData;
        }

        float InterpolateValue(float value1, float time1, float value2, float time2, float time)
        {
            return value1 + ((value2 - value1) * (time - time1)) / (time2 - time1);
        }

    }
}
