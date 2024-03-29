﻿namespace FellrnrTrainingAnalysis.Model
{
    public class CaclulateFieldFactory
    {
        private CaclulateFieldFactory()
        {
            //TODO: replace this with configuration driven dynamic load
            PostTimeSeriesCalulators = new List<CalculateFieldBase>
            {
                new CalculateDataFieldFromTimeSeriesSimple("Avg Pace", CalculateDataFieldFromTimeSeriesSimple.Mode.Average, "Speed", Activity.ActivityTypeRun),

                //TODO: removed ", limit: 120" from climb calculation to let data quality sort out the issues
                //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new CalculateDataFieldFromTimeSeriesSimple("Max Climb", CalculateDataFieldFromTimeSeriesSimple.Mode.Max, "Calc.Climb", Activity.ActivityTypeRun),

                new CalculateDataFieldFromTimeSeriesSimple("Min Climb", CalculateDataFieldFromTimeSeriesSimple.Mode.Min, "Calc.Climb", new List < string > { "Run", "Virtual Run" }),
                new CalculateDataFieldFromTimeSeriesSimple("Grade Adjusted Distance", CalculateDataFieldFromTimeSeriesSimple.Mode.LastValue, "Grade Adjusted Distance", Activity.ActivityTypeRun),

                new CalculateDataFieldFromTimeSeriesSimple("Avg GAP", CalculateDataFieldFromTimeSeriesSimple.Mode.Average, TimeSeriesFactory.GRADE_ADUJUSTED_PACE, Activity.ActivityTypeRun), //meters per second

                new CalculateDataFieldFromTimeSeriesSimple("Max HR", CalculateDataFieldFromTimeSeriesSimple.Mode.Max, "Heart Rate"),

                new CalculateDataFieldFromTimeSeriesSimple("Avg HrPwr", CalculateDataFieldFromTimeSeriesSimple.Mode.Average, Activity.TagHrPwr, Activity.ActivityTypeRun),

                new CalculateDataFieldFromTimeSeriesWindow("Avg HrPwr 5 Min", CalculateDataFieldFromTimeSeriesWindow.Mode.Average, Activity.TagHrPwr, Activity.ActivityTypeRun, 5*60, 10*60),

                new CalculateDataFieldFromTimeSeriesAUC("TRIMP aerobic", false, 138, 180, "Heart Rate"), //hard code zone 4 as 138 and max as 180 as anythign above is bad data

                new CalculateDataFieldFromTimeSeriesAUC("TRIMP anaerobic", false, 250, null, "Power"), //hard code critical power as 250 

                new CalculateDataFieldFromTimeSeriesAUC("TRIMP downhill", true, 10, null, "Calc.Climb", Activity.ActivityTypeRun), //hard code start of downhill as 10 meters/minute

                new CalculateDataFieldFromTimeSeriesThreashold("Percent Run", CalculateDataFieldFromTimeSeriesThreashold.Mode.AbovePercent, 75, "Cadence", Activity.ActivityTypeRun), //cadence is both legs, so 75 = 150

                new CalculateFieldSimpleDefault("Distance", "Elapsed Time", 2.98f, CalculateFieldSimpleDefault.Mode.Multiply, true, Activity.ActivityTypeRun), //9 min/mile is 2.98 m/s

                new CalculateFieldSimpleMath(activityFieldname: "GADΔ",
                                           firstFieldName: "Distance",
                                           secondFieldname: "Grade Adjusted Distance",
                                           extractionMode: CalculateFieldSimpleMath.Mode.Subtract,
                                           overrideRecordedZeroOnly: false,
                                           sportsToInclude:Activity.ActivityTypeRun),

            };

            PreTimeSeriesCalulators = new List<CalculateFieldBase>
            {
                new CalculateFieldSimpleDefault("Distance", "Elapsed Time", 2.98f, CalculateFieldSimpleDefault.Mode.Multiply, true, Activity.ActivityTypeRun), //9 min/mile is 2.98 m/s

            };


        }

        public static CaclulateFieldFactory Instance { get; set; } = new CaclulateFieldFactory();

        public List<CalculateFieldBase> PostTimeSeriesCalulators { get; set; }
        public List<CalculateFieldBase> PreTimeSeriesCalulators { get; set; }
    }
}
