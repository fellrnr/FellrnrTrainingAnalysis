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
    public partial class TimeSeriesCalculatePower : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesCalculatePower()  //for use by memory pack deserialization only
        {
            SportsToInclude = new List<string>(); //keep the compiler happy
        }
        public TimeSeriesCalculatePower(string name, Activity parent, bool persistCache, List<string>? requiredFields, List<string>? opposingFields = null, List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {

        }
        private const string WEIGHT = "Weight";
        [MemoryPackIgnore]
        private float Weight { get { return Parameter(WEIGHT); } set { Parameter(WEIGHT, value); } }


        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe)
                Logging.Instance.Debug($"Forced recalculating effective power");

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
                Weight = athlete.FindDailyValueOrDefault((DateTime)ParentActivity!.StartDateNoTimeLocal!, Day.WeightTag, Options.Instance.StartingWeight);
            }

            TimeValueList retval = new TimeValueList(new uint[gapData.Times.Length], new float[gapData.Times.Length]);

            float w = Weight;
            for (int i = 0; i < gapData.Times.Length; i++)
            {
                float watts = w * gapData.Values[i];
                retval.Values[i] = watts;
                retval.Times[i] = gapData.Times[i];
            }

            return retval;
        }

    }
}
