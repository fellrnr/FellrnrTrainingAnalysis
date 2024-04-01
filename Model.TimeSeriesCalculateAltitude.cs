using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class TimeSeriesCalculateAltitude : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesCalculateAltitude()  //for use by memory pack deserialization only
        {
            SportsToInclude = new List<string>(); //keep the compiler happy
        }
        public TimeSeriesCalculateAltitude(string name, Activity parent, bool persistCache, List<string>? requiredFields, List<string>? opposingFields = null, List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {

        }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            Logging.Instance.ContinueAccumulator("TimeSeriesCalculateAltitude.CalculateData");
            if (forceJustMe)
                Logging.Instance.Debug($"Forced recalculating altitude");

            if (ParentActivity!.TimeSeries.ContainsKey(Activity.TagAltitude) && !ParentActivity!.TimeSeries[Activity.TagAltitude].IsVirtual())
            {
                Logging.Instance.PauseAccumulator("TimeSeriesCalculateAltitude.CalculateData");
                return ParentActivity.TimeSeries[Activity.TagAltitude].GetData(forceCount, forceJustMe);
            }

            float angle;
            if (!ParentActivity.HasNamedDatum(Activity.TagTreadmillAngle))
            {
                angle = 0;
            }
            else
            {
                angle = ParentActivity.GetNamedFloatDatum(Activity.TagTreadmillAngle)!.Value;
            }
            if (ParentActivity.TimeSeries.ContainsKey(Activity.TagDistance))
            {
                TimeValueList DistanceData = ParentActivity.TimeSeries[Activity.TagDistance]!.GetData(forceCount, forceJustMe)!;

                float[] altitudes = new float[DistanceData.Length];
                for (int i = 0; i < DistanceData.Length; i++)
                {
                    float f = DistanceData.Values[i];
                    float alt = f * angle / 100f;
                    altitudes[i] = alt;
                }

                Logging.Instance.PauseAccumulator("TimeSeriesCalculateAltitude.CalculateData");

                TimeValueList retval = new TimeValueList(altitudes);

                if (forceJustMe)
                    Logging.Instance.Debug($"TimeSeriesCalculateAltitude.CalculateData Forced  retval {retval}");

                return retval;
            }
            else
            {
                if (!ParentActivity.HasNamedDatum(Activity.TagDistance))
                {
                    Logging.Instance.PauseAccumulator("TimeSeriesCalculateAltitude.CalculateData");
                    return null;
                }
                if (!ParentActivity.HasNamedDatum(Activity.TagElapsedTime))
                {
                    Logging.Instance.PauseAccumulator("TimeSeriesCalculateAltitude.CalculateData");
                    return null;
                }

                float distance = ParentActivity.GetNamedFloatDatum(Activity.TagDistance)!.Value;
                float elapsed = ParentActivity.GetNamedFloatDatum(Activity.TagElapsedTime)!.Value;
                int seconds = (int)elapsed;

                float[] dists = new float[seconds]; //tempting to add this as a data stream, but probably should be a new computation
                float[] altitudes = new float[seconds];
                for (uint i = 0; i < seconds; i++)
                {
                    float d = ((float)i / seconds) * distance;
                    float alt = d * angle / 100f;
                    altitudes[i] = alt;
                    dists[i] = d;
                }

                TimeValueList retval = new TimeValueList(altitudes);
                if (forceJustMe)
                    Logging.Instance.Debug($"Forced recalculating altude {retval}");
                Logging.Instance.PauseAccumulator("TimeSeriesCalculateAltitude.CalculateData");
                return retval;
            }


        }


    }
}
