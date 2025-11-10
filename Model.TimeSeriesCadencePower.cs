using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using Microsoft.Extensions.Hosting;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    [MemoryPackable]
    public partial class TimeSeriesCadencePower : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesCadencePower()  //for use by memory pack deserialization only
        {
        }

        public TimeSeriesCadencePower(string name,
                                        Activity parent,
                                        bool persistCache,
                                        List<string>? requiredFields,
                                        List<string>? opposingFields = null,
                                        List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
        }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesCadencePower - Forced recalculating {this.Name}");

            Logging.Instance.ContinueAccumulator("GetSpmPwr");
            if (forceJustMe)
                Logging.Instance.Debug($"Forced recalculating SpmPwr");
            TimeValueList? spmData = RequiredTimeSeries[0].GetData(forceCount, forceJustMe);
            TimeValueList? pwrData = RequiredTimeSeries[1].GetData(forceCount, forceJustMe);

            if (spmData == null || pwrData == null) { return null; } //should never happen

            if (ParentActivity == null || ParentActivity.ParentAthlete == null || ParentActivity.StartDateNoTimeLocal == null) { return null; }//should never happen

            Athlete athlete = ParentActivity.ParentAthlete;

            AlignedTimeSeries? alignedTimeSeries = AlignedTimeSeries.Align(spmData, pwrData);
            if (alignedTimeSeries == null) { return null; }

            float[] values = new float[alignedTimeSeries.Length];

            for (int i = 0; i < alignedTimeSeries.Length; i++)
            {
                float spm = alignedTimeSeries.Primary[i];
                float pwr = alignedTimeSeries.Secondary[i];
                float SpmPwr = 0;
                if (spm > 0)
                {
                    SpmPwr = pwr * 60 / spm;
                }
                if (float.IsNormal(SpmPwr) || SpmPwr == 0)
                {
                    values[i] = SpmPwr;
                }
            }

            Logging.Instance.PauseAccumulator("GetSpmPwr");

            TimeValueList retval = new TimeValueList(values);

            return retval;

        }

    }
}
