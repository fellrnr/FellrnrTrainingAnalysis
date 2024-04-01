using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class TimeSeriesCalculateDistance : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesCalculateDistance()  //for use by memory pack deserialization only
        {
            SportsToInclude = new List<string>(); //keep the compiler happy
        }
        public TimeSeriesCalculateDistance(string name, Activity parent, bool persistCache, List<string>? requiredFields, List<string>? opposingFields = null, List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {

        }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.Debug($"TimeSeriesCalculateDistance Forced recalculating distance");

            if (!ParentActivity!.HasNamedDatum(Activity.TagDistance))
            {
                if (forceJustMe) Logging.Instance.Debug($"TimeSeriesCalculateDistance no distance datum");
                Logging.Instance.PauseAccumulator("TimeSeriesCalculateDistance.CalculateData");
                return null;
            }
            if (!ParentActivity!.HasNamedDatum(Activity.TagElapsedTime))
            {
                if (forceJustMe) Logging.Instance.Debug($"TimeSeriesCalculateDistance no elapsed time");
                Logging.Instance.PauseAccumulator("TimeSeriesCalculateDistance.CalculateData");
                return null;
            }

            float distance = ParentActivity.GetNamedFloatDatum(Activity.TagDistance)!.Value;
            float elapsed = ParentActivity.GetNamedFloatDatum(Activity.TagElapsedTime)!.Value;
            int seconds = (int)elapsed;

            float[] dists = new float[seconds]; //tempting to add this as a data stream, but probably should be a new computation
            for (uint i = 0; i < seconds; i++)
            {
                float t = (float)i;
                float d = t / elapsed * distance;
                dists[i] = d;
            }

            TimeValueList retval = new TimeValueList(dists);
            if (forceJustMe) Logging.Instance.Debug($"TimeSeriesCalculateDistance: Forced recalculating effective distance {retval}");
            Logging.Instance.PauseAccumulator("TimeSeriesCalculateDistance.CalculateData");
            return retval;
        }

    }
}
