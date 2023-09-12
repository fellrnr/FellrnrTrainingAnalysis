namespace FellrnrTrainingAnalysis.Model
{
    public class CaclulateFactory
    {
        private CaclulateFactory()
        {
            //TODO: replace this with configuration driven dynamic load
            Calulators = new List<ICalculate>
            {
                new CalculateDataStreamToDataField("Avg Pace", CalculateDataStreamToDataField.Mode.Average, "Speed"),

                //TODO: removed ", limit: 120" from climb calculation to let data quality sort out the issues
                new CalculateDataStreamToDataField("Climb", CalculateDataStreamToDataField.Mode.MinMax, "Calc.Climb"), //meters per minute; don't do per second and scale up or we lose the intrinsic smoothing

                new CalculateDataStreamToDataField("Grade Adjusted Distance", CalculateDataStreamToDataField.Mode.LastValue, "Grade Adjusted Distance"),

                new CalculateDataStreamToDataField("Avg GAP", CalculateDataStreamToDataField.Mode.Average, "Grade Adjusted Distance"), //meters per second

                new CalculateDataStreamToDataField("Max HR", CalculateDataStreamToDataField.Mode.Max, "Heart Rate"),
            };
        }
            
        public static CaclulateFactory Instance { get; set; } = new CaclulateFactory();

        public List<ICalculate> Calulators { get; set; }
    }
}
