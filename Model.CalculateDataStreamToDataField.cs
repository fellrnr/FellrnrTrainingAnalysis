namespace FellrnrTrainingAnalysis.Model
{
    public class CalculateDataStreamToDataField : ICalculate
    {
        public CalculateDataStreamToDataField(string activityFieldname, Mode extractionMode, IDataStream ds) 
        {
            dataStream = ds;
            SourceStreamName = ds.Name;
            ExtractionMode = extractionMode;
            ActivityFieldname = activityFieldname;
        }

        public CalculateDataStreamToDataField(string activityFieldname, Mode extractionMode, string sourceStreamName)
        {
            SourceStreamName = sourceStreamName; 
            dataStream = null;
            ExtractionMode = extractionMode;
            ActivityFieldname = activityFieldname;
        }

        private string SourceStreamName;
        private IDataStream? dataStream = null;


        private IDataStream? DataStream(Activity parent)
        {
            if (dataStream != null)
                return dataStream;

            if (!parent.TimeSeries.ContainsKey(SourceStreamName))
                return null;

            return parent.TimeSeries[SourceStreamName];
        }

        public enum Mode { LastValue, Average, MinMax, Max }
        Mode ExtractionMode { get; set; }

        private Tuple<uint[], float[]>? GetUnderlyingDataStream(Activity parent)
        {
            return DataStream(parent) == null ? null : DataStream(parent)!.GetData(parent);
        }


        public string ActivityFieldname { get; set; }


        public void Recalculate(Activity parent, bool force)
        {
            if (DataStream(parent) == null)
                return;

            DataStream(parent)!.Recalculate(parent, force);


            if (parent.HasNamedDatum(ActivityFieldname) && !force)
                return;

            Tuple<uint[], float[]>? data = GetUnderlyingDataStream(parent);
            if (data == null) 
            {
                if (ExtractionMode == Mode.MinMax)
                {
                    parent.RemoveNamedDatum("Max " + ActivityFieldname);
                    parent.RemoveNamedDatum("Min " + ActivityFieldname);
                }
                else
                {
                    parent.RemoveNamedDatum(ActivityFieldname);
                }
                return; 
            }

            float value;
            if(ExtractionMode == Mode.LastValue)
            {
                value = data.Item2.Last();
                parent.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value));
            }
            else if (ExtractionMode == Mode.Average)
            {
                value = data.Item2.Average(); //TODO: Add average ignoring zeros
                parent.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value));
            }
            else if (ExtractionMode == Mode.Max)
            {
                value = data.Item2.Max(); 
                parent.AddOrReplaceDatum(new TypedDatum<float>(ActivityFieldname, false, value));
            }
            else if (ExtractionMode == Mode.MinMax)
            {
                value = data.Item2.Max();
                parent.AddOrReplaceDatum(new TypedDatum<float>("Max " + ActivityFieldname, false, value));
                value = data.Item2.Min();
                parent.AddOrReplaceDatum(new TypedDatum<float>("Min " + ActivityFieldname, false, value));
            }
        }



    }
}
