﻿using MemoryPack;
using System.Formats.Asn1;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    [MemoryPackUnion(0, typeof(DataStream))]
    [MemoryPackUnion(1, typeof(DataStreamDelta))]
    [MemoryPackUnion(2, typeof(DataStreamEphemeral))]
    [MemoryPackUnion(3, typeof(DataStreamCalculated))]
    [MemoryPackUnion(4, typeof(DataStreamGradeAdjustedDistance))] 
    public abstract partial class DataStreamBase
    {
        [MemoryPackConstructor]
        protected DataStreamBase()  //for use by memory pack deserialization only
        {
        }

        public DataStreamBase(string name, Activity parent_)
        {
            Name = name;
            this.parent_ = parent_;
        }

        //need parent in case we need to get other data to calculate this stream
        public abstract Tuple<uint[], float[]>? GetData();

        public abstract bool IsValid();

        public abstract bool IsVirtual();

        [MemoryPackInclude]
        public string Name { get; }

        public abstract void Recalculate(bool force);

        public void PostDeserialize(Activity parent)
        {
            parent_ = parent;
        }

        [MemoryPackIgnore]
        protected Activity Parent { get { return parent_; } }
        //[MemoryPackInclude]
        [MemoryPackIgnore]
        private Activity parent_;

        public override string ToString()
        {
            return string.Format("Data Stream Name {0}", Name);
        }
        //percentiles - min, 0.03, 5, 32, 50, 68, 95, 99.7, max
        public enum StaticsValue { Min, SD3Low, SD2Low, SD1Low, Median, SD1High, SD2High, SD3High, Max, StandardDeviation, Mean }
        private float[]? _percentiles;
        public float Percentile(StaticsValue staticsValue)
        {
            if (_percentiles == null)
            {
                Tuple<uint[], float[]>? data = GetData();
                if (data == null || data.Item2.Length == 0)
                    return float.MinValue;

                List<float> sorted = data.Item2.ToList();
                sorted.Sort();
                _percentiles = new float[Enum.GetNames(typeof(StaticsValue)).Length];
                _percentiles[(int)StaticsValue.Min] = sorted[0];
                _percentiles[(int)StaticsValue.Max] = sorted[sorted.Count - 1];
                _percentiles[(int)StaticsValue.SD3Low] = Utils.TimeSeries.Percentile(sorted, 0.03f);
                _percentiles[(int)StaticsValue.SD2Low] = Utils.TimeSeries.Percentile(sorted, 5f);
                _percentiles[(int)StaticsValue.SD1Low] = Utils.TimeSeries.Percentile(sorted, 32f);
                _percentiles[(int)StaticsValue.SD1Low] = Utils.TimeSeries.Percentile(sorted, 32f);
                _percentiles[(int)StaticsValue.Median] = Utils.TimeSeries.Percentile(sorted, 50f);
                _percentiles[(int)StaticsValue.SD1High] = Utils.TimeSeries.Percentile(sorted, 68f);
                _percentiles[(int)StaticsValue.SD2High] = Utils.TimeSeries.Percentile(sorted, 95f);
                _percentiles[(int)StaticsValue.SD3High] = Utils.TimeSeries.Percentile(sorted, 99.7f);

                float average = sorted.Average();
                _percentiles[(int)StaticsValue.Mean] = average;

                float sum = 0;
                foreach (float f in sorted)
                {
                    float diff = f - average;
                    sum += diff * diff;
                }
                float sd = (float)Math.Sqrt(sum);
                _percentiles[(int)StaticsValue.StandardDeviation] = sd;
            }
            return _percentiles[(int)staticsValue];
        }

    }
}