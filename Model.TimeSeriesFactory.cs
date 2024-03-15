using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Model
{
    public class TimeSeriesFactory
    {
        public const string GRADE_ADUJUSTED_PACE = "Grade Adjusted Pace";
        public const string GRADE_ADJUSTED_DISTANCE = "Grade Adjusted Distance";
        public const string HEART_RATE_POWER = "HrPwr";
        public const string EFFECTIVE_ALTITUDE = "Effective Altitude";
        public const string EFFECTIVE_DISTANCE = "Effective Distance";

        public static TimeSeriesFactory Instance { get; set; } = new TimeSeriesFactory();

        public List<TimeSeriesBase> TimeSeries(Activity activity)
        {
            return new List<TimeSeriesBase>
            {
                //NB Order is important - the underlying data has to be calcualted first

                new TimeSeriesCalculated(EFFECTIVE_DISTANCE, 
                                        new List<List<string>> { }, 
                                        activity, 
                                        TimeSeriesCalculated.Mode.EffectiveDistance, 
                                        Activity.ActivityTypeRun, 
                                        new List<string> { "Distance" }),

                new TimeSeriesCalculated(EFFECTIVE_ALTITUDE, //new List<string> { }, activity, TimeSeriesCalculated.Mode.EffectiveAltitude, Activity.ActivityTypeRun),
                                        new List<List<string>> { },
                                        activity,
                                        TimeSeriesCalculated.Mode.EffectiveAltitude,
                                        Activity.ActivityTypeRun,
                                        new List<string> { "Altitude" }),


                new TimeSeriesDelta("Calc.Climb", 
                                    new List<List<string>> { new List<string> { "Altitude", "Effective Altitude" } }, 
                                    activity, 
                                    period: 60 ), //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new TimeSeriesGradeAdjustedDistance(GRADE_ADJUSTED_DISTANCE, 
                                                    new List<List<string>>{ new List<string> { "Distance", "Effective Distance" }, new List<string> { "Altitude", "Effective Altitude" } }, 
                                                    activity, 
                                                    Activity.ActivityTypeRun),

                new TimeSeriesDelta(GRADE_ADUJUSTED_PACE, 
                                    new List<List<string>> { new List<string> { GRADE_ADJUSTED_DISTANCE } }, 
                                    activity), //meters per second


                new TimeSeriesHeartRatePower(HEART_RATE_POWER, 
                                            new List<List<string>> { new List<string> { "Heart Rate" }, new List<string> { "Power" } }, 
                                            activity, 
                                            Activity.ActivityTypeRun), //meters per second

                new TimeSeriesHeartRatePower(HEART_RATE_POWER + "Offset",
                                            new List<List<string>> { new List<string> { "Heart Rate" }, new List<string> { "Power" } },
                                            activity,
                                            Activity.ActivityTypeRun,
                                            Options.Instance.RestingHeartRateToStanding), //meters per second

                //You can grade adjust speed rather than delta on distance, but the result is surprisingly close
                //Watch out for differences in smoothing making GAP look wrong compared with speed/pace. 
                //new TimeSeriesGradeAdjustedDistance(GRADE_ADUJUSTED_PACE, new List<string> {  "Speed", "Altitude" }, activity),

            };

        }

    }
}
