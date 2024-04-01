using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]

    //A class that calculates grade adjusted power from horizontal power and elevation changes
    public partial class TimeSeriesPowerEstimateError : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesPowerEstimateError()  //for use by memory pack deserialization only
        {
        }

        public TimeSeriesPowerEstimateError(string name, Activity parent, bool persistCache, List<string>? requiredFields, List<string>? opposingFields = null, List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
        }

        private const string WEIGHT = "Weight";
        [MemoryPackIgnore]
        private float Weight { get { return ParameterOrZero(WEIGHT); } set { Parameter(WEIGHT, value); } }


        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesDifference - Forced recalculating {this.Name}");

            TimeSeriesBase powerStream = RequiredTimeSeries[0];
            TimeValueList? powerData = powerStream.GetData(forceCount, forceJustMe);
            if (powerData == null || powerData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No power");
                return null;
            }

            TimeSeriesBase gadStream = RequiredTimeSeries[1];
            TimeValueList? gadData = gadStream.GetData(forceCount, forceJustMe);
            if (gadData == null || gadData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No gad");
                return null;
            }

            Athlete athlete = ParentActivity!.ParentAthlete!;
            if (Weight == 0)
            {
                Weight = athlete.FindDailyValueOrDefault((DateTime)ParentActivity!.StartDateNoTimeLocal!, Day.TagWeight, Options.Instance.StartingWeight);
            }

            int finalTimegad = gadData.Length;
            int finalTimepower = powerData.Length;
            int finalTimepower90 = finalTimepower * 90 / 100;
            if (finalTimegad < finalTimepower90)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"gad less than 90 power, {finalTimegad}, {finalTimepower}, {finalTimepower90}");
                return null;
            }

            //uint powerTimegad = gadData.Times.First();
            //uint powerTimepower = powerData.Times.First();
            //if (powerTimegad > powerTimepower + 5 * 60)
            //{
            //    if (forceJustMe) Logging.Instance.TraceLeave($"gad starts more than 5 min after power, {powerTimegad}, {powerTimepower}");
            //    return null;
            //}


            AlignedTimeSeries? aligned = AlignedTimeSeries.Align(powerData, gadData);

            if (aligned == null)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No aligned data");
                return null;
            }

            float[] difs = new float[aligned.Length];
            for(int i=0; i < aligned.Length; i++)
            {
                float p = aligned.Primary[i];
                float gap = aligned.Secondary[i];
                float pest = gap * Weight;
                float diff = p - pest;
                difs[i] = diff;
            }


            TimeValueList retval = new TimeValueList(difs);

            LinearRegression? regression = LinearRegression.EvaluateLinearRegression(aligned, false);
            if (regression != null) { regression.Save(ParentActivity, Name); }


            if (forceJustMe) Logging.Instance.TraceLeave($"return difference {retval}");
            return retval;
        }



    }
}
