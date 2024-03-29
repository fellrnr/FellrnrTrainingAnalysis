using FellrnrTrainingAnalysis.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace FellrnrTrainingAnalysis.Model
{
    public class TimeSeriesFactory
    {
        public const string GRADE_ADUJUSTED_PACE = "Grade Adjusted Pace";
        public const string GRADE_ADJUSTED_DISTANCE = "Grade Adjusted Distance";
        public const string HEART_RATE_POWER = "HrPwr";
        public const string ALTITUDE = "Altitude";
        public const string DISTANCE = "Distance";
        public const string POWER = "Power";

        public static TimeSeriesFactory Instance { get; set; } = new TimeSeriesFactory();

        public List<TimeSeriesBase> TimeSeries(Activity activity)
        {
            return new List<TimeSeriesBase>
            {
                //NB Order is important - the underlying data has to be calcualted first

                new TimeSeriesCalculateDistance(name:DISTANCE, parent: activity, persistCache:false, requiredFields: null, opposingFields: null, sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesCalculateAltitude(name:ALTITUDE, parent: activity, persistCache:false, requiredFields: null, opposingFields: null, sportsToInclude:Activity.ActivityTypeRun),


                new TimeSeriesDelta(name:"Calc.Climb",
                                    parent: activity,
                                    persistCache:false,
                                    requiredFields: new List<string> { "Altitude"},
                                    opposingFields: null,
                                    sportsToInclude:Activity.ActivityTypeRun,
                                    period: 60), //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing


                new TimeSeriesGradeAdjustedDistance(name:GRADE_ADJUSTED_DISTANCE,
                                                    parent: activity,
                                                    persistCache:true,  //this is more expensive than most ts
                                                    requiredFields: new List<string> { "Distance", "Altitude" },
                                                    opposingFields: null,
                                                    sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesDelta(name:GRADE_ADUJUSTED_PACE,
                                    parent: activity,
                                    persistCache:false,
                                    requiredFields: new List<string> { GRADE_ADJUSTED_DISTANCE },
                                    opposingFields: null,
                                    sportsToInclude:Activity.ActivityTypeRun,
                                    period: null), //meters per second

                new TimeSeriesCalculatePower(name:POWER,
                                             parent: activity,
                                             persistCache:false,
                                             requiredFields: new List<string> { GRADE_ADUJUSTED_PACE },
                                             opposingFields: null,
                                             sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesCalculatePower(name:"Power Estimate",
                                             parent: activity,
                                             persistCache:false,
                                             requiredFields: new List<string> { GRADE_ADUJUSTED_PACE },
                                             opposingFields: null,
                                             sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesPowerEstimateError(name:"Power Estimate Error",
                                                parent: activity,
                                                persistCache:false,
                                                requiredFields: new List<string> { "Power", "Grade Adjusted Pace" },
                                                opposingFields: null,
                                                sportsToInclude:Activity.ActivityTypeRun),


                new TimeSeriesHeartRatePower(name:HEART_RATE_POWER,
                                                    parent: activity,
                                                    persistCache:true,  //this is more expensive than most ts
                                                    requiredFields: new List<string> { "Heart Rate", "Power" },
                                                    opposingFields: null,
                                                    sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesHeartRatePower(name:HEART_RATE_POWER + "Offset",
                                                    parent: activity,
                                                    persistCache:true,  //this is more expensive than most ts
                                                    requiredFields: new List<string> { "Heart Rate", "Power" },
                                                    opposingFields: null,
                                                    sportsToInclude:Activity.ActivityTypeRun,
                                                    offset:Options.Instance.RestingHeartRateToStanding),

                new TimeSeriesWPrimeBalance(name:"W' Balance",
                                            parent: activity,
                                            persistCache:false,  //this is more expensive than most ts
                                            requiredFields: new List<string> { "Power" },
                                            opposingFields: null,
                                            sportsToInclude:Activity.ActivityTypeRun),

                //You can grade adjust speed rather than delta on distance, but the result is surprisingly close
                //Watch out for differences in smoothing making GAP look wrong compared with speed/pace. 
                //new TimeSeriesGradeAdjustedDistance(GRADE_ADUJUSTED_PACE, new List<string> {  "Speed", "Altitude" }, activity),

                // Minetti
                //=(POWER(A2,5)*155.4 - POWER(A2,4)*30.4 - POWER(A2,3)*43.3+POWER(A2,2)*46.3+A2*16.5+3.6)/3.6
                new TimeSeriesGradeAdjustedDistance(name:GRADE_ADJUSTED_DISTANCE + "_Minetti",
                                                    parent: activity,
                                                    persistCache:true,  //this is more expensive than most ts
                                                    requiredFields: new List<string> { "Distance", "Altitude" },
                                                    opposingFields: null,
                                                    sportsToInclude:Activity.ActivityTypeRun,
                                                    gradeAdjustmentX5: +155.4f,
                                                    gradeAdjustmentX4: -30.4f,
                                                    gradeAdjustmentX3: -43.3f,
                                                    gradeAdjustmentX2: +46.3f,
                                                    gradeAdjustmentX: +16.5f,
                                                    gradeAdjustmentFactor: 3.6f,
                                                    gradeAdjustmentOffset: 3.6f),

                new TimeSeriesDelta(name:GRADE_ADUJUSTED_PACE+ "_Minetti",
                                    parent: activity,
                                    persistCache:false,
                                    requiredFields: new List<string> { GRADE_ADJUSTED_DISTANCE+ "_Minetti" },
                                    opposingFields: null,
                                    sportsToInclude:Activity.ActivityTypeRun,
                                    period: null), //meters per second


                new TimeSeriesCalculatePower(name:"Power Estimate"+ "_Minetti",
                                             parent: activity,
                                             persistCache:false,
                                             requiredFields: new List<string> { GRADE_ADUJUSTED_PACE + "_Minetti"},
                                             opposingFields: null,
                                             sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesPowerEstimateError(name:"Power Estimate Error"+ "_Minetti",
                                                parent: activity,
                                                persistCache:false,
                                                requiredFields: new List<string> { "Power", "Grade Adjusted Pace"+ "_Minetti" },
                                                opposingFields: null,
                                                sportsToInclude:Activity.ActivityTypeRun),




            };

        }

    }
}
