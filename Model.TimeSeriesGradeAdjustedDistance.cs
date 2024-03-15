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
            SportsToInclude = new List<string>(); //keep the compiler happy
        }

        public TimeSeriesGradeAdjustedDistance(string name, List<List<string>> requiredFields, Activity activity, List<string> sportsToInclude) : base(name, requiredFields, activity)
        {
            SportsToInclude = sportsToInclude;
        }

        [MemoryPackInclude]
        List<string> SportsToInclude;


        public override TimeValueList? CalculateData(bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesGradeAdjustedDistance - Forced recalculating {this.Name}");

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
            TimeSeriesBase distanceStream = RequiredTimeSeries[0];
            TimeValueList? distanceData = distanceStream.GetData();
            if (distanceData == null)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No distance");
                return null;
            }

            TimeSeriesBase altitudeStream = RequiredTimeSeries[1]; 
            TimeValueList? altitudeData = altitudeStream.GetData();
            if (altitudeData == null)
            {
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
