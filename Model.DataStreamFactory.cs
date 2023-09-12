namespace FellrnrTrainingAnalysis.Model
{
    public class DataStreamFactory
    {
        public const string GRADE_ADUJUSTED_PACE = "Grade Adjusted Pace";
        public const string GRADE_ADJUSTED_DISTANCE = "Grade Adjusted Distance";

        private DataStreamFactory()
        {
            DataStreams = new List<IDataStream>
            {
                new DataStreamDelta("Calc.Climb", new List<string> { "Altitude" }, period: 60 ), //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new DataStreamGradeAdjustedDistance(GRADE_ADJUSTED_DISTANCE, new List<string> {  "Distance", "Altitude" }),

                new DataStreamDelta(GRADE_ADUJUSTED_PACE, new List<string> { GRADE_ADJUSTED_DISTANCE }), //meters per second

            };
        }
        public static DataStreamFactory Instance { get; set; } = new DataStreamFactory();

        public List<IDataStream> DataStreams { get; set; }

    }
}
