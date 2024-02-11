namespace FellrnrTrainingAnalysis.Model
{
    public class CaclulateFieldFactory
    {
        private CaclulateFieldFactory()
        {
            //TODO: replace this with configuration driven dynamic load
            Calulators = new List<CalculateFieldBase>
            {
                new CalculateDataFieldFromDataStreamSimple("Avg Pace", CalculateDataFieldFromDataStreamSimple.Mode.Average, "Speed"),

                //TODO: removed ", limit: 120" from climb calculation to let data quality sort out the issues
                //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new CalculateDataFieldFromDataStreamSimple("Calc.Climb", CalculateDataFieldFromDataStreamSimple.Mode.Max, "Max Climb"),

                new CalculateDataFieldFromDataStreamSimple("Calc.Climb", CalculateDataFieldFromDataStreamSimple.Mode.Min, "Min Climb"),
                new CalculateDataFieldFromDataStreamSimple("Grade Adjusted Distance", CalculateDataFieldFromDataStreamSimple.Mode.LastValue, "Grade Adjusted Distance"),

                new CalculateDataFieldFromDataStreamSimple("Avg GAP", CalculateDataFieldFromDataStreamSimple.Mode.Average, DataStreamFactory.GRADE_ADUJUSTED_PACE), //meters per second

                new CalculateDataFieldFromDataStreamSimple("Max HR", CalculateDataFieldFromDataStreamSimple.Mode.Max, "Heart Rate"),

                new CalculateDataFieldFromDataStreamAUC("TRIMP aerobic", false, 138, 180, "Heart Rate"), //hard code zone 4 as 138 and max as 180 as anythign above is bad data

                new CalculateDataFieldFromDataStreamAUC("TRIMP anaerobic", false, 250, null, "Power"), //hard code critical power as 250 

                new CalculateDataFieldFromDataStreamAUC("TRIMP downhill", true, 10, null, "Calc.Climb"), //hard code start of downhill as 10 meters/minute

                new CalculateDataFieldFromDataStreamThreashold("Percent Run", CalculateDataFieldFromDataStreamThreashold.Mode.AbovePercent, 75, "Cadence"), //cadence is both legs, so 75 = 150
            };
        }
            
        public static CaclulateFieldFactory Instance { get; set; } = new CaclulateFieldFactory();

        public List<CalculateFieldBase> Calulators { get; set; }
    }
}
