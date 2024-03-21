using MemoryPack;
using System.Collections.ObjectModel;
using FellrnrTrainingAnalysis.Utils;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]

    //A class that calculates grade adjusted distance from horizontal distance and elevation changes
    public partial class TimeSeriesGradeAdjustedDistance : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesGradeAdjustedDistance()  //for use by memory pack deserialization only
        {
        }

        public TimeSeriesGradeAdjustedDistance(string name, Activity parent, bool persistCache, List<string>? requiredFields, List<string>? opposingFields = null, List<string>? sportsToInclude = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
        }

        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesGradeAdjustedDistance - Forced recalculating {this.Name}");

            TimeSeriesBase distanceStream = RequiredTimeSeries[0];
            TimeValueList? distanceData = distanceStream.GetData(forceCount, forceJustMe);
            if (distanceData == null || distanceData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No distance");
                return null;
            }

            TimeSeriesBase altitudeStream = RequiredTimeSeries[1]; 
            TimeValueList? altitudeData = altitudeStream.GetData(forceCount, forceJustMe);
            if (altitudeData == null || altitudeData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No altitude");
                return distanceData;
            }

            uint finalTimeAltitude = altitudeData.Times.Last();
            uint finalTimeDistance = distanceData.Times.Last();
            uint finalTimeDistance90 = finalTimeDistance * 90 / 100;
            if(finalTimeAltitude < finalTimeDistance90)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"altitude less than 90 distance, {finalTimeAltitude}, {finalTimeDistance}, {finalTimeDistance90}");
                return distanceData;
            }

            uint firstTimeAltitude = altitudeData.Times.First();
            uint firstTimeDistance = distanceData.Times.First();
            if (firstTimeAltitude > firstTimeDistance + 5 * 60)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"altitude starts more than 5 min after distance, {firstTimeAltitude}, {firstTimeDistance}");
                return distanceData;
            }


            AlignedTimeSeries? aligned = AlignedTimeSeries.Align(distanceData, altitudeData);

            if (aligned == null)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No altitude, return distance {distanceData}");
                return distanceData;
            }

            Utils.GradeAdjustedDistance gradeAdjustedDistance = new Utils.GradeAdjustedDistance(aligned);

            TimeValueList retval = gradeAdjustedDistance.GetGradeAdjustedDistance();


            if (forceJustMe) Logging.Instance.TraceLeave($"return GAD {retval}");
            return retval;
        }



    }
}
