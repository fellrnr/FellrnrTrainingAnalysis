namespace FellrnrTrainingAnalysis.Model
{
    public class DataStreamFactory
    {
        public const string GRADE_ADUJUSTED_PACE = "Grade Adjusted Pace";
        public const string GRADE_ADJUSTED_DISTANCE = "Grade Adjusted Distance";

        public static DataStreamFactory Instance { get; set; } = new DataStreamFactory();

        public List<DataStreamBase> DataStreams(Activity activity)
        {
            return new List<DataStreamBase>
            {
                new DataStreamDelta("Calc.Climb", new List<string> { "Altitude" }, activity, period: 60 ), //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new DataStreamGradeAdjustedDistance(GRADE_ADJUSTED_DISTANCE, new List<string> {  "Distance", "Altitude" }, activity),

                new DataStreamDelta(GRADE_ADUJUSTED_PACE, new List<string> { GRADE_ADJUSTED_DISTANCE }, activity), //meters per second

            };

        }

    }
}
