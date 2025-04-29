using FellrnrTrainingAnalysis.Utils;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace FellrnrTrainingAnalysis.Model
{
    public class TimeSeriesFactory
    {

        public static TimeSeriesFactory Instance { get; set; } = new TimeSeriesFactory();

        bool DefaultPersistCache = false;
        bool DefaultPersistCache2 = false;

        public List<TimeSeriesBase> TimeSeries(Activity activity)
        {
            return new List<TimeSeriesBase>
            {
                //NB Order is important - the underlying data has to be calcualted first

                new TimeSeriesCalculateDistance(name:Activity.TagDistance, 
                                                parent: activity, 
                                                persistCache:false, 
                                                requiredFields: null, 
                                                opposingFields: null,
                                                sportsToInclude:Activity.ActivityTypeOnFoot),

                new TimeSeriesDelta(name:Activity.TagSpeed,
                                    parent: activity,
                                    persistCache:DefaultPersistCache2,
                                    requiredFields: new List<string> { "Distance"},
                                    opposingFields: null,
                                    sportsToInclude:Activity.ActivityTypeOnFoot),

                new TimeSeriesCalculateAltitude(name:Activity.TagAltitude, parent: activity, persistCache:false, requiredFields: null, opposingFields: null, sportsToInclude:Activity.ActivityTypeRun),


                new TimeSeriesDelta(name:"Calc.Climb",
                                    parent: activity,
                                    persistCache:DefaultPersistCache2,
                                    requiredFields: new List<string> { "Altitude"},
                                    opposingFields: null,
                                    sportsToInclude:Activity.ActivityTypeOnFoot,
                                    period: 60), //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing


                new TimeSeriesIncline(name:Activity.TagIncline,
                                    parent: activity,
                                    persistCache:true,  //this is more expensive than most ts
                                    requiredFields: new List<string> { "Distance", "Altitude" },
                                    opposingFields: null,
                                    sportsToInclude:Activity.ActivityTypeOnFoot,
                                    spanPeriod: 15), //15 seems better than 30, but maybe TODO: goal seek span period for incline?


                new TimeSeriesGradeAdjustedPace(name:Activity.TagGradeAdjustedPace,
                                                    parent: activity,
                                                    persistCache:DefaultPersistCache2,
                                                    requiredFields: new List<string> { "Speed" }, //we can do without Incline and just return speed
                                                    opposingFields: null,
                                                    sportsToInclude:Activity.ActivityTypeOnFoot,
                                                    inclineSeries: "Incline"),

                new TimeSeriesCalculatePower(name:Activity.TagPower,
                                             parent: activity,
                                             persistCache:DefaultPersistCache2,
                                             requiredFields: new List<string> { Activity.TagGradeAdjustedPace },
                                             opposingFields: null,
                                             sportsToInclude:Activity.ActivityTypeRun),


                //TODO: persisting this power estimate kills serialization at exit
                new TimeSeriesCalculatePower(name:"Power Estimate",
                                             parent: activity,
                                             persistCache:DefaultPersistCache,
                                             requiredFields: new List<string> { Activity.TagGradeAdjustedPace },
                                             opposingFields: null,
                                             sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesPowerEstimateError(name:"Power Estimate Error",
                                                parent: activity,
                                                persistCache:DefaultPersistCache,
                                                requiredFields: new List<string> { "Power", "Grade Adjusted Pace" },
                                                opposingFields: null,
                                                sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesHeartRatePower(name:Activity.TagHrPwr,
                                                    parent: activity,
                                                    persistCache:true,  //this is more expensive than most ts
                                                    requiredFields: new List<string> { "Heart Rate", "Power" },
                                                    opposingFields: null,
                                                    sportsToInclude:Activity.ActivityTypeRun),

                new TimeSeriesHeartRatePower(name:Activity.TagHrPwr + "Offset",
                                                    parent: activity,
                                                    persistCache:true,  //this is more expensive than most ts
                                                    requiredFields: new List<string> { "Heart Rate", "Power" },
                                                    opposingFields: null,
                                                    sportsToInclude:Activity.ActivityTypeRun,
                                                    offset:Options.Instance.RestingHeartRateToStanding),

                new TimeSeriesWPrimeBalance(name:"W' Balance",
                                            parent: activity,
                                            persistCache:true,  //this is more expensive than most ts
                                            requiredFields: new List<string> { "Power" },
                                            opposingFields: null,
                                            sportsToInclude:Activity.ActivityTypeRun),

                new PowerDistributionCurve(name:Activity.TagPowerDistributionCurve,
                                            parent: activity,
                                            persistCache:true,
                                            requiredFields: new List<string> { Activity.TagPower },
                                            opposingFields: null,
                                            sportsToInclude:Activity.ActivityTypeRun),


                //You can grade adjust speed rather than delta on distance, but the result is surprisingly close
                //Watch out for differences in smoothing making GAP look wrong compared with speed/pace. 
                //new TimeSeriesGradeAdjustedDistance(GRADE_ADUJUSTED_PACE, new List<string> {  "Speed", "Altitude" }, activity),

                /*
                // Minetti
                //=(POWER(A2,5)*155.4 - POWER(A2,4)*30.4 - POWER(A2,3)*43.3+POWER(A2,2)*46.3+A2*16.5+3.6)/3.6
                new TimeSeriesGradeAdjustedDistance(name:GRADE_ADJUSTED_DISTANCE + "_Minetti",
                                                    parent: activity,
                                                    persistCache:false,  //this is more expensive than most ts
                                                    requiredFields: new List<string> { "Distance", "Altitude" },
                                                    opposingFields: null,
                                                    sportsToInclude:Activity.ActivityTypeRun,
                                                    inclineSeries: "Incline",
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

                */


            };

        }

    }
}
