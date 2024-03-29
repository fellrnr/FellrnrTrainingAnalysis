using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class TimeSeriesRecorded : TimeSeriesBase
    {
        [MemoryPackConstructor]
        private TimeSeriesRecorded()
        {
            Data = new TimeValueList(new uint[0], new float[0]);
        }
        public TimeSeriesRecorded(string name, TimeValueList data, Activity parent) : base(name, parent)
        {
            Data = data;
        }


        //A "real" data stream is always valid. It's only the data streams computed on the fly that need other data streams to be valid.
        public override bool IsValid() { return true; }
        public override bool IsVirtual() { return false; }


        //time offset to Datum
        //TODO: we need the absolute time for a datum as well. If the timer is stopped, the elapsed time stops, but it might be useful to know the actual time. Solution: This will be a data stream of DateTime. 
        [MemoryPackInclude]
        private TimeValueList Data;

        //TODO: Potential optimisation - Share time between data streams 
        public override TimeValueList GetData(int forceCount, bool forceJustMe) { return Data; }


        //currently a data based data stream doesn't need to recalculate. This may change is we put averages and statistics on the activity
        public override bool Recalculate(int forceCount, bool forceJustMe) { LastForceCount = forceCount; return true; }



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

    }
}
