using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class DataStreamCalculated : DataStreamEphemeral
    {
        [MemoryPackConstructor]
        protected DataStreamCalculated()  //for use by memory pack deserialization only
        {
            SportsToInclude = new List<string>(); //keep the compiler happy
        }
        public DataStreamCalculated(string name, List<string> requiredFields, Activity activity, Mode calculationMode, List<string> sportsToInclude) : base(name, requiredFields, activity)
        {
            CalculationMode = calculationMode;
            SportsToInclude = sportsToInclude;
        }

        public enum Mode { HrPwr}
        Mode CalculationMode { get; set; }

        List<string> SportsToInclude;

        public override Tuple<uint[], float[]>? CalculateData()
        {
            if (Parent == null) { return null; }

            if (!Parent.CheckSportType(SportsToInclude))
                return null;

            ReadOnlyDictionary<string, DataStreamBase> timeSeries = Parent.TimeSeries;

            List<Tuple<uint[], float[]>> requiredTimeSeries = new List<Tuple<uint[], float[]>>();

            foreach (string field in RequiredFields)
            {
                DataStreamBase dataStream = timeSeries[field];
                Tuple<uint[], float[]>? data = dataStream.GetData();
                if (data == null) { return null; }

                if (data.Item2.Min() == 0 && data.Item2.Max() == 0) //all zeros and we don't really have any data
                    return null;

                requiredTimeSeries.Add(data);
            }

            switch(CalculationMode)
            {
                case Mode.HrPwr:
                    return GetHrPwr(requiredTimeSeries);
                default:
                    return null;
            }
        }

        [MemoryPackIgnore]

        float? Weight = null;

        [MemoryPackIgnore]

        float? RestingHeartRate = null;

        private Tuple<uint[], float[]>? GetHrPwr(List<Tuple<uint[], float[]>> requiredTimeSeries)
        {
            Tuple<uint[], float[]> hrData = requiredTimeSeries[0];
            Tuple<uint[], float[]> pwrData = requiredTimeSeries[1];
            
            if (Parent == null || Parent.Parent == null || Parent.StartDateNoTimeLocal == null) { return null; }
            
            Athlete athlete = Parent.Parent;
            if (Weight == null)
            {
                Weight = athlete.FindDailyValueOrDefault((DateTime)Parent.StartDateNoTimeLocal, Day.WeightTag, Options.Instance.StartingWeight);
            }

            if (RestingHeartRate == null)
            {
                RestingHeartRate = athlete.FindDailyValueOrDefault((DateTime)Parent.StartDateNoTimeLocal, Day.RestingHeartRateTag, Options.Instance.StartingRestingHeartRate);
            }

            Utils.TimeSeries.AlignedTimeSeries? alignedTimeSeries = Utils.TimeSeries.Align(hrData, pwrData);
            if(alignedTimeSeries == null) { return null; }

            Tuple<uint[], float[]> retval = new Tuple<uint[], float[]>(new uint[alignedTimeSeries.Time.Length], new float[alignedTimeSeries.Time.Length]);

            for(int i=0;  i<alignedTimeSeries.Time.Length; i++)
            {
                float hr = alignedTimeSeries.Primary[i];
                float pwr = alignedTimeSeries.Secondary[i] * 1000.0f;
                float pwrkg = pwr / Weight.Value;
                float deltahr = hr - RestingHeartRate.Value;
                float hrpwr = pwrkg / deltahr;
                retval.Item1[i] = alignedTimeSeries.Time[i];
                retval.Item2[i] = hrpwr;
            }

            return retval;
        }


    }
}
