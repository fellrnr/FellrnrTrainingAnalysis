using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using Microsoft.Extensions.Hosting;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    [MemoryPackable]
    public partial class TimeSeriesHeartRatePower : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesHeartRatePower()  //for use by memory pack deserialization only
        {
        }

        public TimeSeriesHeartRatePower(string name,
                                        Activity parent,
                                        bool persistCache,
                                        List<string>? requiredFields,
                                        List<string>? opposingFields = null,
                                        List<string>? sportsToInclude = null,
                                        float offset = 0,
                                        float ignoreStart = 0) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
            Parameter(OFFSET, offset);
            Parameter(IGNORESTART, ignoreStart);
        }

        private const string OFFSET = "Offset";
        private const string IGNORESTART = "ignoreStart";

        private const string RHR = "RestingHeartRate";
        [MemoryPackIgnore]
        private float RestingHeartRate { get { return ParameterOrZero(RHR); } set { Parameter(RHR, value); } }

        private const string WEIGHT = "Weight";
        [MemoryPackIgnore]
        private float Weight { get { return ParameterOrZero(WEIGHT); } set { Parameter(WEIGHT, value); } }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesHeartRatePower - Forced recalculating {this.Name}");

            Logging.Instance.ContinueAccumulator("GetHrPwr");
            if (forceJustMe)
                Logging.Instance.Debug($"Forced recalculating HrPwr");
            TimeValueList? hrData = RequiredTimeSeries[0].GetData(forceCount, forceJustMe);
            TimeValueList? pwrData = RequiredTimeSeries[1].GetData(forceCount, forceJustMe);

            if (hrData == null || pwrData == null) { return null; } //should never happen

            if (ParentActivity == null || ParentActivity.ParentAthlete == null || ParentActivity.StartDateNoTimeLocal == null) { return null; }//should never happen

            Athlete athlete = ParentActivity.ParentAthlete;
            if (Weight == 0)
            {
                Weight = athlete.FindDailyValueOrDefault((DateTime)ParentActivity.StartDateNoTimeLocal, Day.TagWeight, Options.Instance.StartingWeight);
            }

            if (RestingHeartRate == 0)
            {
                RestingHeartRate = athlete.FindDailyValueOrDefault((DateTime)ParentActivity.StartDateNoTimeLocal,
                                                                   Day.TagRestingHeartRate,
                                                                   Options.Instance.StartingRestingHeartRate);
            }

            AlignedTimeSeries? alignedTimeSeries = AlignedTimeSeries.Align(hrData, pwrData);
            if (alignedTimeSeries == null) { return null; }

            float[] values = new float[alignedTimeSeries.Length];

            int ignoreStart = (int)ParameterOrZero(IGNORESTART);
            int lastTime = alignedTimeSeries.Length;

            if (ignoreStart > lastTime)
                return null;

            float w = Weight;
            float rhr = RestingHeartRate;
            float rhroffset = ParameterOrZero(OFFSET);
            float orhr = rhr + rhroffset;
            float prev_hrpwr = 0;
            float first_hrpwr = -1;
            for (int i = 0; i < alignedTimeSeries.Length; i++)
            {
                if (i > ignoreStart)
                {
                    float hr = alignedTimeSeries.Primary[i];
                    float pwr = alignedTimeSeries.Secondary[i] * 1000.0f;
                    float pwrkg = pwr / w;
                    float deltahr = hr - orhr;

                    float hrpwr = deltahr == 0 ? 0 : pwrkg / deltahr;
                    if (float.IsNormal(hrpwr) || hrpwr == 0)
                    {
                        values[i] = hrpwr;
                        prev_hrpwr = hrpwr;
                        if (first_hrpwr < 0)
                            first_hrpwr = hrpwr;
                    }
                    else
                    {
                        values[i] = prev_hrpwr;
                    }
                }
            }

            for (int i = 0; i < alignedTimeSeries.Length && i <= ignoreStart; i++)
            {
                values[i] = first_hrpwr;
            }

            LinearRegression? regression = LinearRegression.EvaluateLinearRegression(alignedTimeSeries, false, true, true);
            if(regression != null) { regression.Save(ParentActivity, Name);  }
            Logging.Instance.PauseAccumulator("GetHrPwr");

            TimeValueList retval = new TimeValueList(values);


            return retval;

        }

    }
}
