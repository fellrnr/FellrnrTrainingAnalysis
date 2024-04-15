using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

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
            InclineSeries = "Incline";
        }


        public TimeSeriesGradeAdjustedDistance(string name, 
                                    Activity parent, 
                                    bool persistCache,
                                    List<string>? requiredFields, 
                                    List<string>? opposingFields = null, 
                                    List<string>? sportsToInclude = null,
                                    string? inclineSeries = null,
                                    float? gradeAdjustmentX2 = null,
                                    float? gradeAdjustmentX3 = null,
                                    float? gradeAdjustmentX4 = null,
                                    float? gradeAdjustmentX5 = null,
                                    float? gradeAdjustmentX = null,
                                    float? gradeAdjustmentFactor = null,
                                    float? gradeAdjustmentOffset = null) :
            base(name, parent, persistCache, requiredFields, opposingFields, sportsToInclude)
        {
            if (gradeAdjustmentX2 != null) Parameter("gradeAdjustmentX2", gradeAdjustmentX2.Value);
            if (gradeAdjustmentX3 != null) Parameter("gradeAdjustmentX3", gradeAdjustmentX3.Value);
            if (gradeAdjustmentX4 != null) Parameter("gradeAdjustmentX4", gradeAdjustmentX4.Value);
            if (gradeAdjustmentX5 != null) Parameter("gradeAdjustmentX5", gradeAdjustmentX5.Value);
            if (gradeAdjustmentX != null) Parameter("gradeAdjustmentX", gradeAdjustmentX.Value);
            if (gradeAdjustmentFactor != null) Parameter("gradeAdjustmentFactor", gradeAdjustmentFactor.Value);
            if (gradeAdjustmentOffset != null) Parameter("gradeAdjustmentOffset", gradeAdjustmentOffset.Value);
            if (inclineSeries != null)
                InclineSeries = inclineSeries;
            else
                InclineSeries = "Incline";
        }

        [MemoryPackInclude]
        public string InclineSeries;


        public override TimeValueList? CalculateData(int forceCount, bool forceJustMe)
        {
            if (forceJustMe) Logging.Instance.TraceEntry($"TimeSeriesGradeAdjustedincline - Forced recalculating {this.Name}");

            TimeSeriesBase distanceStream = RequiredTimeSeries[0];
            TimeValueList? distanceData = distanceStream.GetData(forceCount, forceJustMe);
            if (distanceData == null || distanceData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No distance");
                return null;
            }


            if (!ParentActivity!.TimeSeriesNames.Contains(InclineSeries))
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No incline stream");
                return distanceData;
            }

            TimeSeriesBase inclineStream = ParentActivity.TimeSeries[InclineSeries];
            TimeValueList? inclineData = inclineStream.GetData(forceCount, forceJustMe);
            if (inclineData == null || inclineData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"Incline too short");
                return distanceData;
            }


            float? gradeAdjustmentX2 = ParameterOrNull("gradeAdjustmentX2");
            float? gradeAdjustmentX3 = ParameterOrNull("gradeAdjustmentX3");
            float? gradeAdjustmentX4 = ParameterOrNull("gradeAdjustmentX4");
            float? gradeAdjustmentX5 = ParameterOrNull("gradeAdjustmentX5");
            float? gradeAdjustmentX = ParameterOrNull("gradeAdjustmentX");
            float? gradeAdjustmentFactor = ParameterOrNull("gradeAdjustmentFactor");
            float? gradeAdjustmentOffset = ParameterOrNull("gradeAdjustmentOffset");

            Utils.GradeAdjustedDistance gradeAdjustedDistance = new Utils.GradeAdjustedDistance(inclineData,
                                                                                                distanceData,
                                                                                                gradeAdjustmentX2,
                                                                                                gradeAdjustmentX3,
                                                                                                gradeAdjustmentX4,
                                                                                                gradeAdjustmentX5,
                                                                                                gradeAdjustmentX,
                                                                                                gradeAdjustmentFactor,
                                                                                                gradeAdjustmentOffset);

            TimeValueList retval = gradeAdjustedDistance.GetGradeAdjustedDistance();

            if (retval == null)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"return distance, GAD failed {retval}");
                return distanceData;
            }
            else
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"return GAD {retval}");
                return retval;
            }
        }



    }
}
