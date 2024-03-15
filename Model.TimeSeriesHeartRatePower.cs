using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    [MemoryPackable]
    public partial class TimeSeriesHeartRatePower : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesHeartRatePower()  //for use by memory pack deserialization only
        {
            SportsToInclude = new List<string>(); //keep the compiler happy
        }

        public TimeSeriesHeartRatePower(string name, List<List<string>> requiredFields, Activity activity, List<string> sportsToInclude, float offset = 0, float ignoreStart=0) : base(name, requiredFields, activity)
        {
            SportsToInclude = sportsToInclude;
            Parameter(OFFSET, offset);
            Parameter(IGNORESTART, ignoreStart);
        }

        [MemoryPackInclude]
        List<string> SportsToInclude;

        private const string OFFSET = "Offset";
        private const string IGNORESTART = "ignoreStart";
        [MemoryPackIgnore]
        private float Offset {  get {  return Parameter(OFFSET); } set { Parameter(OFFSET, value); } }

        private const string RHR = "RestingHeartRate";
        [MemoryPackIgnore]
        private float RestingHeartRate { get { return Parameter(RHR); } set { Parameter(RHR, value); } }

        private const string WEIGHT = "Weight";
        [MemoryPackIgnore]
        private float Weight { get { return Parameter(WEIGHT); } set { Parameter(WEIGHT, value); } }

        public override TimeValueList? CalculateData(bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesHeartRatePower - Forced recalculating {this.Name}");

            if (ParentActivity == null)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No parent");
                return null;
            }

            if (!ParentActivity.CheckSportType(SportsToInclude))
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"Sport not included {ParentActivity.ActivityType}");
                return null;
            }

            Logging.Instance.ContinueAccumulator("GetHrPwr");
            if (forceJustMe)
                Logging.Instance.Debug($"Forced recalculating HrPwr");
            TimeValueList? hrData = RequiredTimeSeries[0].GetData();
            TimeValueList? pwrData = RequiredTimeSeries[1].GetData();

            if (hrData == null || pwrData == null) { return null; } //should never happen

            if (ParentActivity == null || ParentActivity.ParentAthlete == null || ParentActivity.StartDateNoTimeLocal == null) { return null; }//should never happen

            Athlete athlete = ParentActivity.ParentAthlete;
            if (Weight == 0)
            {
                Weight = athlete.FindDailyValueOrDefault((DateTime)ParentActivity.StartDateNoTimeLocal, Day.WeightTag, Options.Instance.StartingWeight);
            }

            if (RestingHeartRate == 0)
            {
                RestingHeartRate = athlete.FindDailyValueOrDefault((DateTime)ParentActivity.StartDateNoTimeLocal, Day.RestingHeartRateTag, Options.Instance.StartingRestingHeartRate);
            }

            AlignedTimeSeries? alignedTimeSeries = AlignedTimeSeries.Align(hrData, pwrData);
            if (alignedTimeSeries == null) { return null; }

            TimeValueList retval = new TimeValueList(new uint[alignedTimeSeries.Time.Length], new float[alignedTimeSeries.Time.Length]);

            int ignoreStart = (int)Parameter(IGNORESTART);
            uint lastTime = alignedTimeSeries.Time.Last();

            if (ignoreStart > lastTime)
                return null;

            float w = Weight;
            float rhr = RestingHeartRate;
            float rhroffset = Parameter(OFFSET);
            float orhr = rhr + rhroffset;
            float prev_hrpwr = 0;
            float first_hrpwr = -1;
            for (int i = 0; i < alignedTimeSeries.Time.Length; i++)
            {
                if (alignedTimeSeries.Time[i] > ignoreStart)
                {
                    float hr = alignedTimeSeries.Primary[i];
                    float pwr = alignedTimeSeries.Secondary[i] * 1000.0f;
                    float pwrkg = pwr / w;
                    float deltahr = hr - orhr;

                    float hrpwr = deltahr == 0 ? 0 : pwrkg / deltahr;
                    retval.Times[i] = alignedTimeSeries.Time[i];
                    if (float.IsNormal(hrpwr) || hrpwr == 0)
                    {
                        retval.Values[i] = hrpwr;
                        prev_hrpwr = hrpwr;
                        if(first_hrpwr < 0)
                            first_hrpwr = hrpwr;
                    }
                    else
                    {
                        retval.Values[i] = prev_hrpwr;
                    }
                }
            }

            for (int i = 0; i < alignedTimeSeries.Time.Length && alignedTimeSeries.Time[i] <= ignoreStart; i++)
            {
                retval.Values[i] = first_hrpwr;
            }

            AlignedTimeSeries.LinearRegressionResults? regression = alignedTimeSeries.LinearRegression(false);
            if (regression != null)
            {
                ParentActivity.AddOrReplaceDatum(new TypedDatum<float>($"{Name}-Slope", false, (float)regression.Slope));
                ParentActivity.AddOrReplaceDatum(new TypedDatum<float>($"{Name}-YIntercept", false, (float)regression.YIntercept));
                ParentActivity.AddOrReplaceDatum(new TypedDatum<float>($"{Name}-RSquared", false, (float)regression.RSquared));
                //if(force)
                //{
                //    string s = alignedTimeSeries.ToCsv();
                //    Clipboard.SetText(s, TextDataFormat.Text);
                //    LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(s);
                //    largeTextDialogForm.ShowDialog();
                //}
            }
            Logging.Instance.PauseAccumulator("GetHrPwr");

            return retval;

        }

    }
}
