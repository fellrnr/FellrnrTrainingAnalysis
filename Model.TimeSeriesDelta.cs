using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

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
        public TimeSeriesDelta(string name, 
                        Activity parent, 
                        bool persistCache, 
                        List<string>? requiredFields, 
                        List<string>? opposingFields = null, 
                        List<string>? sportsToInclude = null,
                        float scalingFactor = 1, 
                        float? numerator = null, 
                        int? period = null, 
                        float? limit = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
            if (requiredFields == null || requiredFields.Count != 1) throw new ArgumentException("TimeSeriesDelta must have only one required field");
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
        int? Period { get; set; }

        [MemoryPackInclude]
        float? Limit { get; set; }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"TimeSeriesDelta: Forced recalculating {this.Name}");

            //Crude way of debugging a data stream deltas. Just change the id
            bool extraDebug = false;
            //if (ParentActivity!.PrimaryKey().Contains("10478327023") && Name == Activity.TagHrPwr)
            //{
            //    extraDebug = true;
            //}
            if (RequiredTimeSeries == null || RequiredTimeSeries.Count != 1)
            {
                Logging.Instance.Error($"Somehow got no required time series for {this}, activity {this.ParentActivity}");
                return null;
            }

            TimeSeriesBase dataStream = RequiredTimeSeries[0];
            //debug the issue of the underlying data stream not being ready. It should be ahead of us in the list and already calculated
            if(dataStream is TimeSeriesEphemeral)
            {
                TimeSeriesEphemeral ephemeral = (TimeSeriesEphemeral)dataStream;
                if(!ephemeral.CacheValid)
                {
                    Logging.Instance.Debug("Our underlying ephemeral data stream is not cached");
                }
            }
            TimeValueList? data = dataStream.GetData(forceCount, forceJustMe);
            if (data == null) { return null; }

            if (data.Values.Min() == 0 && data.Values.Max() == 0) //all zeros and we don't really have any data
                return null;

            TimeValueList? newData;

            if (Period == null || Period == 1)
            {
                newData = TimeValueList.SimpleDeltas(data, ScalingFactor, Numerator, Limit);
            }
            else
            {
                newData = TimeValueList.SpanDeltas(data, ScalingFactor, Numerator, Limit, (int)Period, extraDebug);
            }
            return newData;
        }

    }
}
