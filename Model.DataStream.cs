namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class DataStream : IDataStream
    {
        public DataStream(string name, Tuple<uint[], float[]> data)
        {
            _data = data;
            Name = name;
        }


        //A "real" data stream is always valid. It's only the data streams computed on the fly that need other data streams to be valid.
        public bool IsValid(Activity Parent) { return true; }
        public bool IsVirtual() { return false; }


        //time offset to Datum
        //TODO: we need the absolute time for a datum as well. If the timer is stopped, the elapsed time stops, but it might be useful to know the actual time. Solution: This will be a data stream of DateTime. 

        private Tuple<uint[], float[]> _data;

        //TODO: Potential optimisation - Share time between data streams 
        public Tuple<uint[], float[]> GetData(Activity Parent) { return _data; }
        //public uint[] ElapsedTimes { get; private set; } = new uint[0];
        //public float[] Values { get; private set; } = new float[0];

        public string Name { get; }


        //currently a data based data stream doesn't need to recalculate. This may change is we put averages and statistics on the activity
        public void Recalculate(Activity parent, bool force) { return; }

        public override string ToString()
        {
            return string.Format("Data Stream Name {0}", Name);
        }


        /*
         * Time Series Name[PositionLat]
         * Time Series Name[PositionLong]
         * Time Series Name[Altitude]
         * Time Series Name[Distance]
         * Time Series Name[EnhancedAltitude]
         * Time Series Name[HeartRate]
         * Time Series Name[Cadence]
         * Time Series Name[Speed]
         * Time Series Name[EnhancedSpeed]
         * Time Series Name[VerticalOscillation]
         * Time Series Name[TotalHemoglobinConc]
         * Time Series Name[SaturatedHemoglobinPercent]
         * Time Series Name[VerticalRatio]
         * Time Series Name[StepLength]
         * Time Series Name[Temperature]
         * Time Series Name[FractionalCadence]
         * Time Series Name[StanceTimePercent]
         * Time Series Name[StanceTime]
         * Time Series Name[StanceTimeBalance]
         * Time Series Name[AccumulatedPower]
         * Time Series Name[Power]
         * Time Series Name[LeftRightBalance]
         * Time Series Name[Resistance]
         */

        //TODO: Maybe cache some calculated values, min, max, avg, std

        //Note, data have sources and origins, but a time series can be made of a mixture
    }
}
