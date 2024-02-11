namespace FellrnrTrainingAnalysis.Model
{
    public class DataStreamFactory
    {
        public const string GRADE_ADUJUSTED_PACE = "Grade Adjusted Pace";
        public const string GRADE_ADJUSTED_DISTANCE = "Grade Adjusted Distance";
        public const string HEART_RATE_POWER = "HrPwr";

        public static DataStreamFactory Instance { get; set; } = new DataStreamFactory();

        public List<DataStreamBase> DataStreams(Activity activity)
        {
            return new List<DataStreamBase>
            {
                new DataStreamDelta("Calc.Climb", new List<string> { "Altitude" }, activity, period: 60 ), //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new DataStreamGradeAdjustedDistance(GRADE_ADJUSTED_DISTANCE, new List<string> {  "Distance", "Altitude" }, activity),

                new DataStreamDelta(GRADE_ADUJUSTED_PACE, new List<string> { GRADE_ADJUSTED_DISTANCE }, activity), //meters per second


                new DataStreamCalculated(HEART_RATE_POWER, new List<string> { "Heart Rate", "Power" }, activity, DataStreamCalculated.Mode.HrPwr, new List<string> { "Run" }), //meters per second

                //You can grade adjust speed rather than delta on distance, but the result is surprisingly close
                //Watch out for differences in smoothing making GAP look wrong compared with speed/pace. 
                //new DataStreamGradeAdjustedDistance(GRADE_ADUJUSTED_PACE, new List<string> {  "Speed", "Altitude" }, activity),

            };

        }

    }
}
