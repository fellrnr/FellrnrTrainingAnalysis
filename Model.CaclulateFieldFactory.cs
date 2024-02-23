namespace FellrnrTrainingAnalysis.Model
{
    public class CaclulateFieldFactory
    {
        private CaclulateFieldFactory()
        {
            //TODO: replace this with configuration driven dynamic load
            Calulators = new List<CalculateFieldBase>
            {
                new CalculateDataFieldFromDataStreamSimple("Avg Pace", CalculateDataFieldFromDataStreamSimple.Mode.Average, "Speed", new List<string> { "Run" }),

                //TODO: removed ", limit: 120" from climb calculation to let data quality sort out the issues
                //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new CalculateDataFieldFromDataStreamSimple("Calc.Climb", CalculateDataFieldFromDataStreamSimple.Mode.Max, "Max Climb", new List<string> { "Run" }),

                new CalculateDataFieldFromDataStreamSimple("Calc.Climb", CalculateDataFieldFromDataStreamSimple.Mode.Min, "Min Climb", new List < string > { "Run" }),
                new CalculateDataFieldFromDataStreamSimple("Grade Adjusted Distance", CalculateDataFieldFromDataStreamSimple.Mode.LastValue, "Grade Adjusted Distance", new List<string> { "Run" }),

                new CalculateDataFieldFromDataStreamSimple("Avg GAP", CalculateDataFieldFromDataStreamSimple.Mode.Average, DataStreamFactory.GRADE_ADUJUSTED_PACE, new List<string> { "Run" }), //meters per second

                new CalculateDataFieldFromDataStreamSimple("Max HR", CalculateDataFieldFromDataStreamSimple.Mode.Max, "Heart Rate"),

                new CalculateDataFieldFromDataStreamAUC("TRIMP aerobic", false, 138, 180, "Heart Rate"), //hard code zone 4 as 138 and max as 180 as anythign above is bad data

                new CalculateDataFieldFromDataStreamAUC("TRIMP anaerobic", false, 250, null, "Power"), //hard code critical power as 250 

                new CalculateDataFieldFromDataStreamAUC("TRIMP downhill", true, 10, null, "Calc.Climb", new List<string> { "Run" }), //hard code start of downhill as 10 meters/minute

                new CalculateDataFieldFromDataStreamThreashold("Percent Run", CalculateDataFieldFromDataStreamThreashold.Mode.AbovePercent, 75, "Cadence", new List<string> { "Run" }), //cadence is both legs, so 75 = 150

                new CalculateFieldSimpleDefault("Distance", "Elapsed Time", 2.98f, CalculateFieldSimpleDefault.Mode.Multiply, true, new List<string> { "Run" }) //9 min/mile is 2.98 m/s
            };
        }
            
        public static CaclulateFieldFactory Instance { get; set; } = new CaclulateFieldFactory();

        public List<CalculateFieldBase> Calulators { get; set; }
    }
}
