﻿using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class TimeSeriesCalculatePower : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesCalculatePower()  //for use by memory pack deserialization only
        {
            SportsToInclude = new List<string>(); //keep the compiler happy
        }
        public TimeSeriesCalculatePower(string name,
                                        Activity parent,
                                        bool persistCache,
                                        List<string>? requiredFields,
                                        List<string>? opposingFields = null,
                                        List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {

        }
        private const string WEIGHT = "Weight";
        [MemoryPackIgnore]
        private float Weight { get { return ParameterOrZero(WEIGHT); } set { Parameter(WEIGHT, value); } }


        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.TraceEntry($"TimeSeriesCalculatePower.CalculateData");

            if (ParentActivity != null && ParentActivity.TimeSeries.ContainsKey(Activity.TagDistance) && ParentActivity.TimeSeries[Activity.TagDistance].IsVirtual())
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"Distance is virtual, aborting");
                return null;
            }


            TimeSeriesBase gapStream = RequiredTimeSeries[0];
            TimeValueList? gapData = gapStream.GetData(forceCount, forceJustMe);
            if (gapData == null)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No gap");
                return null;
            }

            Athlete athlete = ParentActivity!.ParentAthlete!;
            if (Weight == 0)
            {
                Weight = athlete.FindDailyValueOrDefault((DateTime)ParentActivity!.StartDateNoTimeLocal!, Day.TagWeight, Options.Instance.StartingWeight);
            }


            float w = Weight;
            float[] values = new float[gapData.Length];
            for (int i = 0; i < gapData.Length; i++)
            {
                float watts = w * gapData.Values[i];
                values[i] = watts;
            }
            TimeValueList retval = new TimeValueList(values);

            return retval;
        }

    }
}
