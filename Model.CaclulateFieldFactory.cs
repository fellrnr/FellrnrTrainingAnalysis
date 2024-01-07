namespace FellrnrTrainingAnalysis.Model
{
    public class CaclulateFieldFactory
    {
        private CaclulateFieldFactory()
        {
            //TODO: replace this with configuration driven dynamic load
            Calulators = new List<ICalculateField>
            {
                new CalculateDataFieldFromDataStreamSimple("Avg Pace", CalculateDataFieldFromDataStreamSimple.Mode.Average, "Speed"),

                //TODO: removed ", limit: 120" from climb calculation to let data quality sort out the issues
                //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new CalculateDataFieldFromDataStreamSimple("Climb", CalculateDataFieldFromDataStreamSimple.Mode.Max, "Max Climb"),

                new CalculateDataFieldFromDataStreamSimple("Climb", CalculateDataFieldFromDataStreamSimple.Mode.Min, "Min Climb"),
                new CalculateDataFieldFromDataStreamSimple("Grade Adjusted Distance", CalculateDataFieldFromDataStreamSimple.Mode.LastValue, "Grade Adjusted Distance"),

                new CalculateDataFieldFromDataStreamSimple("Avg GAP", CalculateDataFieldFromDataStreamSimple.Mode.Average, "Grade Adjusted Distance"), //meters per second

                new CalculateDataFieldFromDataStreamSimple("Max HR", CalculateDataFieldFromDataStreamSimple.Mode.Max, "Heart Rate"),

                new CalculateDataFieldFromDataStreamAUC("TRIMP aerobic", false, 138, 180, "Heart Rate"), //hard code zone 4 as 138 and max as 180 as anythign above is bad data

                new CalculateDataFieldFromDataStreamAUC("TRIMP anaerobic", false, 250, null, "Power"), //hard code critical power as 250 

                new CalculateDataFieldFromDataStreamAUC("TRIMP downhill", true, 20, null, "Calc.Climb"), //hard code start of downhill as 20 meters/minute
            };
        }
            
        public static CaclulateFieldFactory Instance { get; set; } = new CaclulateFieldFactory();

        public List<ICalculateField> Calulators { get; set; }
    }
}
