using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.ExchangeActiveSyncProvisioning;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    [MemoryPackable]
    public partial class TimeSeriesEnergyCostOfRunning : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesEnergyCostOfRunning()  //for use by memory pack deserialization only
        {
        }

        public TimeSeriesEnergyCostOfRunning(string name,
                                        Activity parent,
                                        bool persistCache,
                                        List<string>? requiredFields,
                                        List<string>? opposingFields = null,
                                        List<string>? sportsToInclude = null,
                                        float ignoreStart = 0) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
            Parameter(IGNORESTART, ignoreStart);
        }

        private const string IGNORESTART = "ignoreStart";

        private const string WEIGHT = "Weight";
        [MemoryPackIgnore]
        private float Weight { get { return ParameterOrZero(WEIGHT); } set { Parameter(WEIGHT, value); } }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesEnergyCostOfRunning - Forced recalculating {this.Name}");

            if (forceJustMe)
                Logging.Instance.Debug($"Forced recalculating Energy cost of running (ECOR)");
            TimeValueList? speedData = RequiredTimeSeries[0].GetData(forceCount, forceJustMe);
            TimeValueList? pwrData = RequiredTimeSeries[1].GetData(forceCount, forceJustMe);

            if (speedData == null || pwrData == null) { return null; } //should never happen

            if (ParentActivity == null || ParentActivity.ParentAthlete == null || ParentActivity.StartDateNoTimeLocal == null) { return null; }//should never happen

            Athlete athlete = ParentActivity.ParentAthlete;
            if (Weight == 0)
            {
                Weight = athlete.FindDailyValueOrDefault((DateTime)ParentActivity.StartDateNoTimeLocal, CalendarNode.TagWeight, Options.Instance.StartingWeight);
            }

            AlignedTimeSeries? alignedTimeSeries = AlignedTimeSeries.Align(speedData, pwrData);
            if (alignedTimeSeries == null) { return null; }

            float[] values = new float[alignedTimeSeries.Length];

            int ignoreStart = (int)ParameterOrZero(IGNORESTART);
            int lastTime = alignedTimeSeries.Length;

            if (ignoreStart > lastTime)
                return null;

            float w = Weight;
            float prev_ecor = 0;
            float first_ecor = -1;
            for (int i = 0; i < alignedTimeSeries.Length; i++)
            {
                if (i > ignoreStart)
                {
                    float speed = alignedTimeSeries.Primary[i]; // m/s
                    float pwr = alignedTimeSeries.Secondary[i];
                    float pwrkg = pwr / w;
                    float distance = speed; //speed in m/s for one second is meters

                    float ecor = pwrkg / distance;
                    if (!float.IsNaN(ecor) && ecor > 0.5 && ecor < 1.5 && speed > 1.5) //ignore out of range ecor and speeds below walking
                    {
                        values[i] = ecor;
                        prev_ecor = ecor;
                        if (first_ecor < 0)
                            first_ecor = ecor;
                        //if(ecor > 2)
                        //    Logging.Instance.Debug($"ECOR too high {ecor}, speed {speed}, pwr {pwr}");
                    }
                    else
                    {
                        values[i] = prev_ecor;
                    }
                }
            }

            for (int i = 0; i < alignedTimeSeries.Length && i <= ignoreStart; i++)
            {
                values[i] = first_ecor;
            }

            //LinearRegression? regression = LinearRegression.EvaluateLinearRegression(alignedTimeSeries, false, true, true);
            //if (regression != null) { regression.Save(ParentActivity, Name); }

            TimeValueList retval = new TimeValueList(values);

            return retval;

        }

    }
}
