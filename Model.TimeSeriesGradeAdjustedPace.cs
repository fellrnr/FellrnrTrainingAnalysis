using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]

    //A class that calculates grade adjusted speed from horizontal speed and elevation changes
    public partial class TimeSeriesGradeAdjustedPace : TimeSeriesEphemeral
    {
        [MemoryPackConstructor]
        protected TimeSeriesGradeAdjustedPace()  //for use by memory pack deserialization only
        {
            InclineSeries = "Incline";
        }


        public TimeSeriesGradeAdjustedPace(string name, 
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

            TimeSeriesBase speedStream = RequiredTimeSeries[0];
            TimeValueList? speedData = speedStream.GetData(forceCount, forceJustMe);
            if (speedData == null || speedData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No speed");
                return null;
            }

            //if speed is virtual because it's based on virtual distance, we can't do GAP. The virtual speed will be constant, and power will be meaningless
            if(speedStream.IsVirtual())
            {
                if (!ParentActivity!.TimeSeries.ContainsKey(Activity.TagDistance))
                    return speedData;

                if(ParentActivity!.TimeSeries[Activity.TagDistance].IsVirtual())
                    return speedData;
            }


            if (!ParentActivity!.TimeSeriesNames.Contains(InclineSeries))
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"No incline stream");
                return speedData;
            }

            TimeSeriesBase inclineStream = ParentActivity.TimeSeries[InclineSeries];
            TimeValueList? inclineData = inclineStream.GetData(forceCount, forceJustMe);
            if (inclineData == null || inclineData.Length < 1)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"Incline too short");
                return speedData;
            }


            float? gradeAdjustmentX2 = ParameterOrNull("gradeAdjustmentX2");
            float? gradeAdjustmentX3 = ParameterOrNull("gradeAdjustmentX3");
            float? gradeAdjustmentX4 = ParameterOrNull("gradeAdjustmentX4");
            float? gradeAdjustmentX5 = ParameterOrNull("gradeAdjustmentX5");
            float? gradeAdjustmentX = ParameterOrNull("gradeAdjustmentX");
            float? gradeAdjustmentFactor = ParameterOrNull("gradeAdjustmentFactor");
            float? gradeAdjustmentOffset = ParameterOrNull("gradeAdjustmentOffset");

            Utils.GradeAdjustedDistance gradeAdjustedDistance = new Utils.GradeAdjustedDistance(inclineData,
                                                                                                speedData,
                                                                                                gradeAdjustmentX2,
                                                                                                gradeAdjustmentX3,
                                                                                                gradeAdjustmentX4,
                                                                                                gradeAdjustmentX5,
                                                                                                gradeAdjustmentX,
                                                                                                gradeAdjustmentFactor,
                                                                                                gradeAdjustmentOffset);

            TimeValueList retval = gradeAdjustedDistance.GetGradeAdjustedPace();

            if (retval == null)
            {
                if (forceJustMe) Logging.Instance.TraceLeave($"return speed, GAD failed {retval}");
                return speedData;
            }
            else
            {
                float gad = gradeAdjustedDistance.GetGradeAdjustedDistance();
                ParentActivity.AddOrReplaceDatum(new TypedDatum<float>(Activity.TagGradeAdjustedDistance, false, gad));
                if (forceJustMe) Logging.Instance.TraceLeave($"return GAD {retval}");
                return retval;
            }
        }



    }
}
