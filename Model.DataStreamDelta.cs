using System.Collections.ObjectModel;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]

    //A simple class to calculate the delta of another data stream, such as speed as the delta of distance
    public class DataStreamDelta : DataStreamEphemeral
    {
        public DataStreamDelta(string name, List<string> requiredFields, float scalingFactor, float? numerator = null) : base(name, requiredFields)
        {
            if (requiredFields.Count != 1) throw new ArgumentException("DataStreamDelta must have only one required field");
            ScalingFactor = scalingFactor;
            Numerator = numerator;
        }

        float ScalingFactor { get; set; }
        float? Numerator { get; set; }

        public const float MetersPerSecondToSecondsPerKilometer = 16.666666666667f * 60.0f;

        public override Tuple<uint[], float[]>? GetData(Activity parent)
        {
            ReadOnlyDictionary<string, IDataStream> timeSeries = parent.TimeSeries;
            string field = RequiredFields[0];
            IDataStream dataStream = timeSeries[field];
            Tuple<uint[], float[]>? data = dataStream.GetData(parent);
            if(data == null) { return null; }
            uint[] elapsedTime = data.Item1;
            float[] values = data.Item2;
            float[] deltas = new float[elapsedTime.Length];
            float lastValue = values[0];
            float lastTime = 0;
            deltas[0] = 0; //first value has no predecessor, so it has to be zero

            for (int i = 1; i < elapsedTime.Length; i++) //note starting from one as we handle the first entry above
            {
                float deltaTime = elapsedTime[i] - lastTime;
                deltas[i] = (values[i] - lastValue) / deltaTime;
                if(Numerator != null && deltas[i] != 0)
                    deltas[i] = Numerator.Value / deltas[i];
                deltas[i] = deltas[i] * ScalingFactor;
                lastValue = values[i];
                lastTime = elapsedTime[i];
            }
            Tuple<uint[], float[]> newData = new Tuple<uint[], float[]>(elapsedTime, deltas);
            return newData;
        }

        //TODO:Calculate averages and put them on activity. 
        public override void Recalculate(Activity parent, bool force) { return; }

    }
}
