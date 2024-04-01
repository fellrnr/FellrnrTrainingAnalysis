using FellrnrTrainingAnalysis.Utils;
using MemoryPack;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class TimeValueList
    {
        [MemoryPackIgnore]
        public uint[] TimesX
        {
            get
            {
                uint[] retval = new uint[Length];
                for(uint i = 0; i < Length; i++) { retval[i] = i; }
                return retval;
            }
        }
        [MemoryPackInclude]
        public float[] Values { get; set; }

        [MemoryPackIgnore]
        public int Length { get { return Values.Length; } }

        public static TimeValueList TimeValueListFromTimed(uint[] times, float[] values)
        {
            List<float> to1sec = Utils.TimeSeriesUtils.InterpolateToOneSecond(times, values);
            TimeValueList result = new TimeValueList(to1sec);
            return result;
        }

        [MemoryPackConstructor]
        public TimeValueList(float[] values)
        {
            Values = values;
        }

        public TimeValueList(List<float> values) //convenience method
        {
            Values = values.ToArray();
        }


        public override string ToString()
        {
            return $"TimeValueList {Values.Length} count/seconds, total time {Misc.s2hms(Values.Length)}, avg value {Values.Average()}";
        }


        //calculate a simple delta, scaled by time. So change of 10 in a time of 1 is 10, a change of 10 in a time of 2 is 5
        //supports scaling and inverting the values
        public static TimeValueList? SimpleDeltas(TimeValueList data, float ScalingFactor, float? Numerator, float? Limit)
        {
            //uint[] elapsedTime = data.Times;
            float[] values = data.Values;
            float[] deltas = new float[data.Length];
            float lastValue = values[0];

            for (int i = 1; i < data.Length; i++) //note starting from one as we handle the first entry above
            {
                float deltasValue = (values[i] - lastValue); //one second times
                deltas[i] = deltasValue;

                //first value has no predecessor, so it has to be zero, but that creates some odd results, so copy the first delta back
                if (i == 1)
                    deltas[0] = deltasValue;
                if (Numerator != null && deltas[i] != 0)
                    deltas[i] = Numerator.Value / deltas[i];
                deltas[i] = deltas[i] * ScalingFactor;
                if (Limit != null && Math.Abs(deltas[i]) > Limit)
                    return null;
                lastValue = values[i];
            }
            TimeValueList newData = new TimeValueList(deltas);

            return newData;
        }

        //A more complex delta, using a time span for the change. Used for averaging over 60 seconds for instance. 
        public static TimeValueList? SpanDeltas(TimeValueList data, float scalingFactor, float? numerator, float? limit, float period, bool extraDebug)
        {
            //uint[] elapsedTime = data.Times;
            float[] values = data.Values;
            float[] deltas = new float[data.Length];
            float lastValue = values[0];
            //uint lastTime = 0;
            deltas[0] = 0; //first value has no predecessor, so it has to be zero
                           //List<uint> absoluteTimeStack = new List<uint>();
            //List<uint> incrementTimeStack = new List<uint>();
            List<float> deltaStack = new List<float>();

            float deltaSum = 0;
            //uint timeSum = 0;
            for (int i = 1; i < data.Length; i++) //note starting from one as we handle the first entry above
            {
                //uint currentTime = elapsedTime[i];
                //uint timeIncrement = 1; // currentTime - lastTime;
                float currentValue = values[i];
                float valueIncrement = currentValue - lastValue;

                //absoluteTimeStack.Add(currentTime);
                //incrementTimeStack.Add(timeIncrement);
                deltaStack.Add(valueIncrement);
                deltaSum += valueIncrement;
                //timeSum += timeIncrement;


                //if the time sum is less than the period, all the delta applies. For instance, in the first 2 seconds we climb 2 meters, then our climb rate is 2 meters/minute, not 2/60 meters/minute
                float timeProRata = deltaStack.Count > period ? deltaStack.Count / period : 1.0f;
                float currentDelta = deltaSum / timeProRata;
                if (numerator != null && currentDelta != 0)
                    currentDelta = numerator.Value / currentDelta;
                currentDelta = currentDelta * scalingFactor;
                deltas[i] = currentDelta;

                if (extraDebug)
                {
                    Logging.Instance.Log($"delta[{i}]: {currentDelta}, deltaSum {deltaSum}, timeProRata {timeProRata}, currentValue {currentValue}, valueIncrement {valueIncrement} ");
                }

                if (limit != null && Math.Abs(deltas[i]) > limit)
                    return null;

                //first value has no predecessor, so it has to be zero, but that creates some odd results, so copy the first delta back
                if (i == 1)
                    deltas[0] = currentDelta;

                //mop up
                while (deltaStack.Count > period)
                {
                    deltaSum -= deltaStack.First();
                    deltaStack.RemoveAt(0);
                }

                lastValue = currentValue;
            }
            TimeValueList newData = new TimeValueList(deltas);
            return newData;
        }

        public static TimeValueList? ExtractWindow(TimeValueList data, int start, int end = 0) //end of zero is to the finish
        {
            float[] newvalues;

            if (start > data.Values.Length || end > data.Values.Length)
                return null;

            if (end == 0)
                newvalues = data.Values[start..];
            else
                newvalues = data.Values[start..end];
            //List<float> newvalues = new List<float>();

            //uint lastTime = data.Times.Last();

            ////very occasionally, times don't start at zero. Huh. 
            //uint firstTime = data.Times.First();
            //start += firstTime;
            //end += firstTime;

            //if (start > lastTime)
            //    return null;

            //for (int i = 1; i < data.Length && (end == 0 || data.Times[i] <= end); i++)
            //{
            //    if (data.Times[i] >= start)
            //    {
            //        newvalues.Add(data.Values[i]);
            //    }
            //}
            //if (newvalues.Count == 0) { return null; }

            TimeValueList newData = new TimeValueList(newvalues.ToArray());

            return newData;
        }
    }
}
