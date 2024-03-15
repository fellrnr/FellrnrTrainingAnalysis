using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class TimeSeriesCalculated : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesCalculated()  //for use by memory pack deserialization only
        {
            SportsToInclude = new List<string>(); //keep the compiler happy
        }
        public TimeSeriesCalculated(string name,
                                    List<List<string>> requiredFields,
                                    Activity activity,
                                    Mode calculationMode,
                                    List<string> sportsToInclude,
                                    List<string>? opposingFields = null) : 
            base(name, requiredFields, activity, opposingFields)
        {
            CalculationMode = calculationMode;
            SportsToInclude = sportsToInclude; 
        }

        public enum Mode { EffectiveAltitude, EffectiveDistance }
        [MemoryPackInclude]
        Mode CalculationMode { get; set; }

        [MemoryPackInclude]
        List<string> SportsToInclude;

        public override TimeValueList? CalculateData(bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesCalculated - Forced recalculating {this.Name}");

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

            ReadOnlyDictionary<string, TimeSeriesBase> timeSeries = ParentActivity.TimeSeries;

            TimeValueList? retval = null;
            switch (CalculationMode)
            {
                case Mode.EffectiveAltitude:
                    retval = GetEffectiveAltitude(forceJustMe);
                    break;
                case Mode.EffectiveDistance:
                    retval = GetEffectiveDistance(forceJustMe);
                    break;
            }

            if (forceJustMe) Logging.Instance.TraceLeave($"Calcualted {retval}");
            return retval;
        }


        private TimeValueList? GetEffectiveAltitude(bool force)
        {
            Logging.Instance.ContinueAccumulator("GetEffectiveAltitude");
            if (force)
                Logging.Instance.Debug($"Forced recalculating effective altitude");

            if (ParentActivity == null)
            {
                Logging.Instance.PauseAccumulator("GetEffectiveAltitude");
                return null;
            }

            if (ParentActivity.TimeSeries.ContainsKey(Activity.TagAltitude))
            {
                Logging.Instance.PauseAccumulator("GetEffectiveAltitude");
                return ParentActivity.TimeSeries[Activity.TagAltitude].GetData();
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
                TimeValueList DistanceData = ParentActivity.TimeSeries[Activity.TagDistance]!.GetData()!;

                float[] altitudes = new float[DistanceData.Length];
                for (int i = 0; i < DistanceData.Length; i++)
                {
                    float f = DistanceData.Values[i];
                    float alt = f * angle / 100f;
                    altitudes[i] = alt;
                }

                Logging.Instance.PauseAccumulator("GetEffectiveAltitude");

                TimeValueList retval = new TimeValueList(DistanceData.Times, altitudes);

                if (force)
                    Logging.Instance.Debug($"CalculateFieldSimpleDefault Forced GetEffectiveAltitude  retval {retval}");

                return retval;
            }
            else
            {
                if (!ParentActivity.HasNamedDatum(Activity.TagDistance))
                {
                    Logging.Instance.PauseAccumulator("GetEffectiveAltitude");
                    return null;
                }
                if (!ParentActivity.HasNamedDatum(Activity.TagElapsedTime))
                {
                    Logging.Instance.PauseAccumulator("GetEffectiveAltitude");
                    return null;
                }

                float distance = ParentActivity.GetNamedFloatDatum(Activity.TagDistance)!.Value;
                float elapsed  = ParentActivity.GetNamedFloatDatum(Activity.TagElapsedTime)!.Value;
                int seconds = (int)elapsed;

                float[] dists = new float[seconds]; //tempting to add this as a data stream, but probably should be a new computation
                float[] altitudes = new float[seconds];
                uint[] times = new uint[seconds];
                for (uint i = 0; i < seconds; i++)
                {
                    float d = ((float)i / seconds) * distance;
                    float alt = d * angle / 100f;
                    altitudes[i] = alt;
                    dists[i] = d;
                    times[i] = i;
                }

                TimeValueList retval = new TimeValueList(times, altitudes);
                if (force)
                    Logging.Instance.Debug($"Forced recalculating effective altude {retval}");
                Logging.Instance.PauseAccumulator("GetEffectiveAltitude");
                return retval;
            }


        }

        private TimeValueList? GetEffectiveDistance(bool force)
        {
            Logging.Instance.ContinueAccumulator("GetEffectiveDistance");
            if (force)
                Logging.Instance.Debug($"Forced recalculating effective distance");

            if (ParentActivity == null) 
            {
                Logging.Instance.PauseAccumulator("GetEffectiveDistance");
                return null;
            }
            if (ParentActivity.TimeSeries.ContainsKey(Activity.TagDistance))
            {
                Logging.Instance.PauseAccumulator("GetEffectiveDistance");
                return ParentActivity.TimeSeries[Activity.TagDistance].GetData();
            }

            if (!ParentActivity.HasNamedDatum(Activity.TagDistance))
            {
                Logging.Instance.PauseAccumulator("GetEffectiveDistance");
                return null;
            }
            if (!ParentActivity.HasNamedDatum(Activity.TagElapsedTime))
            {
                Logging.Instance.PauseAccumulator("GetEffectiveDistance");
                return null;
            }

            float distance = ParentActivity.GetNamedFloatDatum(Activity.TagDistance)!.Value;
            float elapsed = ParentActivity.GetNamedFloatDatum(Activity.TagElapsedTime)!.Value;
            int seconds = (int)elapsed;

            float[] dists = new float[seconds]; //tempting to add this as a data stream, but probably should be a new computation
            uint[] times = new uint[seconds];
            for (uint i = 0; i < seconds; i++)
            {
                float t = (float)i;
                float d = t / elapsed * distance;
                dists[i] = d;
                times[i] = i;
            }

            TimeValueList retval = new TimeValueList(times, dists);
            if (force)
                Logging.Instance.Debug($"Forced recalculating effective distance {retval}");
            Logging.Instance.PauseAccumulator("GetEffectiveDistance");
            return retval;
        }

    }
}
