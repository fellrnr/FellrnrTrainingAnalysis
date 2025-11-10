namespace FellrnrTrainingAnalysis.Model
{
    public class CaclulateFieldFactory
    {
        private CaclulateFieldFactory()
        {
            //TODO: replace this with configuration driven dynamic load
            PostTimeSeriesCalulators = new List<CalculateFieldBase>
            {
                new CalculateDataFieldFromTimeSeriesSimple("Avg ECOR", TimeValueList.StaticsValue.Mean, "ECOR", Activity.ActivityTypeRun),

                new CalculateDataFieldFromTimeSeriesSimple("Avg Pace", TimeValueList.StaticsValue.Mean, "Speed", Activity.ActivityTypeRun),

                //TODO: removed ", limit: 120" from climb calculation to let data quality sort out the issues
                //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new CalculateDataFieldFromTimeSeriesSimple("Max Climb", TimeValueList.StaticsValue.Max, "Calc.Climb", Activity.ActivityTypeRun),

                new CalculateDataFieldFromTimeSeriesSimple("Min Climb",
                                                           TimeValueList.StaticsValue.Min,
                                                           "Calc.Climb",
                                                           new List < string > { "Run", "Virtual Run" }),


                new CalculateDataFieldFromTimeSeriesSimple("Avg GAP",
                                                           TimeValueList.StaticsValue.Mean,
                                                           Activity.TagGradeAdjustedPace,
                                                           Activity.ActivityTypeOnFoot), //meters per second

                new CalculateDataFieldFromTimeSeriesSimple(Activity.TagAveragePower,
                                                           TimeValueList.StaticsValue.Mean,
                                                           Activity.TagPower), 

                new CalculateDataFieldFromTimeSeriesSimple("Max HR", TimeValueList.StaticsValue.Max, "Heart Rate"),

                new CalculateDataFieldFromTimeSeriesSimple("Avg HrPwr", TimeValueList.StaticsValue.Mean, Activity.TagHrPwr, Activity.ActivityTypeRun),

                new CalculateDataFieldFromTimeSeriesSimple("Avg SpmPwr", TimeValueList.StaticsValue.Mean, Activity.TagSpmPwr, Activity.ActivityTypeRun),

                new CalculateDataFieldFromTimeSeriesSimple("90% SpmPwr", TimeValueList.StaticsValue.High90PC, Activity.TagSpmPwr, Activity.ActivityTypeRun),
                new CalculateDataFieldFromTimeSeriesSimple("2sd SpmPwr", TimeValueList.StaticsValue.SD2High, Activity.TagSpmPwr, Activity.ActivityTypeRun),
                new CalculateDataFieldFromTimeSeriesSimple("3sd SpmPwr", TimeValueList.StaticsValue.SD3High, Activity.TagSpmPwr, Activity.ActivityTypeRun),


                //find out how flat the last five mins of the first ten mins were
                new CalculateDataFieldFromTimeSeriesWindow("Vertical 5 Min", TimeValueList.StaticsValue.SumAbsDeltas, Activity.TagAltitude, Activity.ActivityTypeRun, 5*60, 10*60),

                //find out how low our speed was for the last five mins of the first ten mins were
                new CalculateDataFieldFromTimeSeriesWindow("Min Speed 5 Min", TimeValueList.StaticsValue.Min, Activity.TagSpeed, Activity.ActivityTypeRun, 5*60, 10*60),


                new CalculateDataFieldFromTimeSeriesWindow("Avg HrPwr 5 Min", TimeValueList.StaticsValue.Mean, Activity.TagHrPwr, Activity.ActivityTypeRun, 5*60, 10*60, flatStartOnly: true),


                //TRIMP fields will be rolled up using Model.Rolling
                //new CalculateDataFieldFromTimeSeriesAUC("TRIMP aerobic", false, 138, 180, "Heart Rate"), //hard code zone 4 as 138 and max as 180 as anythign above is bad data

                //new CalculateDataFieldFromTimeSeriesAUC("TRIMP anaerobic", false, 250, null, "Power"), //hard code critical power as 250 

                //new CalculateDataFieldFromTimeSeriesAUC("TRIMP downhill", true, 10, null, "Calc.Climb", Activity.ActivityTypeRun), //hard code start of downhill as 10 meters/minute

                //calculate percent of time spent running
                new CalculateDataFieldFromTimeSeriesThreashold("Percent Run",
                                                               CalculateDataFieldFromTimeSeriesThreashold.Mode.AbovePercent,
                                                               75,
                                                               ignoreZeros: true,
                                                               "Cadence",
                                                               Activity.ActivityTypeRun), //cadence is both legs, so 75 = 150

                //fill in a rough distance based on elapsed time and a 9 min/mile pace
                new CalculateFieldSimpleDefault("Distance",
                                                "Elapsed Time",
                                                2.98f,
                                                CalculateFieldSimpleDefault.Mode.Multiply,
                                                CalculateFieldSimple.OverrideMode.OverrideRecordedZeroOnly,
                                                Activity.ActivityTypeRun), //9 min/mile is 2.98 m/s

                //in the asbsense of any other data, copy distance to GAD
                new CalculateFieldSimpleCopy(activityFieldname: "Grade Adjusted Distance",
                                            dependentFieldname: "Distance",
                                            CalculateFieldSimple.OverrideMode.AbsentOnly,
                                            sportsToInclude: Activity.ActivityTypeRun),
                /*
                //difference between GAD and raw distance
                new CalculateFieldSimpleMath(activityFieldname: "GADΔ",
                                           firstFieldName: "Distance",
                                           secondFieldname: "Grade Adjusted Distance",
                                           extractionMode: CalculateFieldSimpleMath.Mode.Subtract,
                                           overrideRecordedZeroOnly: false,
                                           sportsToInclude:Activity.ActivityTypeRun),
                */


                new CalculateDataFieldFromTimeSeriesZones("5-Zone-", 
                                                          Utils.Options.Instance.StartingHR5Zones,
                                                          Activity.TagHeartRate,
                                                          Activity.ActivityTypeAerobic),

                new CalculateDataFieldFromTimeSeriesZones("3-Zone-",
                                                          Utils.Options.Instance.StartingHR3Zones,
                                                          Activity.TagHeartRate,
                                                          Activity.ActivityTypeAerobic),

                //cheat to get 5a
                new CalculateDataFieldFromTimeSeriesThreashold("5-Zone-5a",
                                                               CalculateDataFieldFromTimeSeriesThreashold.Mode.AboveAbs,
                                                               Utils.Options.Instance.StartingHR5a,
                                                               ignoreZeros: false,
                                                               Activity.TagHeartRate,
                                                               Activity.ActivityTypeAerobic),

                //note: CP only calculated on runs from Stryd, not enough data for bike CP, so offset

                new CalculateFieldIF("IF", cpScalingFactor: 1.0f, Activity.ActivityTypeRun),   //actually calculated on the activity from CP in the day, rolled up
                //new CalculateFieldTRIMP("TRIMP", cpScalingFactor: 1.0f, Activity.ActivityTypeRun), //actually calculated on the activity from CP in the day, rolled up

                new CalculateFieldIF("IF", cpScalingFactor: 0.65f, Activity.ActivityTypeRide),   //actually calculated on the activity from CP in the day, rolled up

                new CalculateFieldArrhythmia("PVCs", Activity.ActivityTypeAll),   //estimate the number of premature ventricular contractions from the HR data
                

                //new CalculateFieldTRIMP("TRIMP", cpScalingFactor: 0.65f, Activity.ActivityTypeRide), //actually calculated on the activity from CP in the day, rolled up

                //new CalculateDataFieldFromTimeSeriesTRIMPi("TRIMPi",
                //                                          Activity.TagPower,
                //                                          cpScalingFactor: 1.0f,
                //                                          Activity.ActivityTypeRun),
                //new CalculateDataFieldFromTimeSeriesTRIMPi("TRIMPi",
                //                                          Activity.TagPower,
                //                                          cpScalingFactor: 0.65f,
                //                                          Activity.ActivityTypeRide),

                new CalculateDataFieldFromTimeSeriesTRIMPhr("TRIMPhr",
                                                          Activity.TagHeartRate),

                new CalculateDataFieldFromTimeSeriesTRIMPnp("TRIMPnp",
                                                          Activity.TagPower,
                                                          cpScalingFactor: 1.0f,
                                                          Activity.ActivityTypeRun),
                new CalculateDataFieldFromTimeSeriesTRIMPnp("TRIMPnp",
                                                          Activity.TagPower,
                                                          cpScalingFactor: 0.65f,
                                                          Activity.ActivityTypeRide),
                new CalculateFieldSimpleMath(activityFieldname: "TRIMP",
                                           firstFieldName: "TRIMPhr",
                                           secondFieldname: "TRIMPnp",
                                           extractionMode: CalculateFieldSimpleMath.Mode.Max,
                                           overrideWhen: CalculateFieldSimple.OverrideMode.OverrideRecordedZeroOnly),
            };

            PreTimeSeriesCalulators = new List<CalculateFieldBase>
            {
                new CalculateFieldSimpleDefault("Distance",
                                                "Elapsed Time",
                                                2.98f,
                                                CalculateFieldSimpleDefault.Mode.Multiply,
                                                CalculateFieldSimple.OverrideMode.OverrideRecordedZeroOnly,
                                                Activity.ActivityTypeRun), //9 min/mile is 2.98 m/s

            };

            DayCalculators = new List<CalculateFieldBase>
            {
                new CalculateFieldPolarizationIndex("PolarizationIndex 30D"),
            };

        }

        public static CaclulateFieldFactory Instance { get; } = new CaclulateFieldFactory();

        public List<CalculateFieldBase> PostTimeSeriesCalulators { get; }
        public List<CalculateFieldBase> PreTimeSeriesCalulators { get; }
        public List<CalculateFieldBase> DayCalculators { get; }
    }
}
