using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class TimeValueList
    {
        [MemoryPackInclude]
        public uint[] Times;
        [MemoryPackInclude]
        public float[] Values;

        [MemoryPackIgnore]
        public int Length { get {  return Times.Length; } }

        public TimeValueList(uint[] times, float[] values)
        {
            Times = times;
            Values = values;
            if(times.Length != values.Length) { throw new Exception($"TimesAndValues, counts don't match, times {Times.Length}, values {Values.Length}"); }
        }

        public override string ToString()
        {
            return $"TimeValueList {Times.Length} count, total time {Times.Last()}, avg value {Values.Average()}";
        }


        //calculate a simple delta, scaled by time. So change of 10 in a time of 1 is 10, a change of 10 in a time of 2 is 5
        //supports scaling and inverting the values
        public static TimeValueList? SimpleDeltas(TimeValueList data, float ScalingFactor, float? Numerator, float? Limit)
        {
            uint[] elapsedTime = data.Times;
            float[] values = data.Values;
            float[] deltas = new float[elapsedTime.Length];
            float lastValue = values[0];
            float lastTime = 0;


            for (int i = 1; i < elapsedTime.Length; i++) //note starting from one as we handle the first entry above
            {
                float deltaTime = elapsedTime[i] - lastTime;
                if (deltaTime == 0) //seems to happen for the last entry
                {
                    deltas[i] = 0;
                }
                else
                {
                    float deltasValue = (values[i] - lastValue) / deltaTime;
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
                    lastTime = elapsedTime[i];
                }
            }
            TimeValueList newData = new TimeValueList(elapsedTime, deltas);

            return newData;
        }

        //A more complex delta, using a time span for the change. Used for averaging over 60 seconds for instance. 
        public static TimeValueList? SpanDeltas(TimeValueList data, float scalingFactor, float? numerator, float? limit, float period, bool extraDebug)
        {
            uint[] elapsedTime = data.Times;
            float[] values = data.Values;
            float[] deltas = new float[elapsedTime.Length];
            float lastValue = values[0];
            uint lastTime = 0;
            deltas[0] = 0; //first value has no predecessor, so it has to be zero
                           //List<uint> absoluteTimeStack = new List<uint>();
            List<uint> incrementTimeStack = new List<uint>();
            List<float> deltaStack = new List<float>();

            float deltaSum = 0;
            uint timeSum = 0;
            for (int i = 1; i < elapsedTime.Length; i++) //note starting from one as we handle the first entry above
            {
                uint currentTime = elapsedTime[i];
                uint timeIncrement = currentTime - lastTime;
                float currentValue = values[i];
                float valueIncrement = currentValue - lastValue;

                //absoluteTimeStack.Add(currentTime);
                incrementTimeStack.Add(timeIncrement);
                deltaStack.Add(valueIncrement);
                deltaSum += valueIncrement;
                timeSum += timeIncrement;


                //if the time sum is less than the period, all the delta applies. For instance, in the first 2 seconds we climb 2 meters, then our climb rate is 2 meters/minute, not 2/60 meters/minute
                float timeProRata = timeSum > period ? timeSum / period : 1.0f;
                float currentDelta = deltaSum / timeProRata;
                if (numerator != null && currentDelta != 0)
                    currentDelta = numerator.Value / currentDelta;
                currentDelta = currentDelta * scalingFactor;
                deltas[i] = currentDelta;

                if (extraDebug)
                {
                    Logging.Instance.Log($"delta[{i}]: {currentDelta}, deltaSum {deltaSum}, timeProRata {timeProRata}, timeSum {timeSum}, currentValue {currentValue}, valueIncrement {valueIncrement} ");
                }

                if (limit != null && Math.Abs(deltas[i]) > limit)
                    return null;

                //first value has no predecessor, so it has to be zero, but that creates some odd results, so copy the first delta back
                if (i == 1)
                    deltas[0] = currentDelta;

                //mop up
                while (incrementTimeStack.Count > 0 && timeSum > period)
                {
                    deltaSum -= deltaStack.First();
                    timeSum -= incrementTimeStack.First();
                    incrementTimeStack.RemoveAt(0);
                    deltaStack.RemoveAt(0);
                }

                lastValue = currentValue;
                lastTime = currentTime;
            }
            TimeValueList newData = new TimeValueList(elapsedTime, deltas);
            return newData;
        }

        public static TimeValueList? ExtractWindow(TimeValueList data, uint start, uint end=0) //end of zero is to the finish
        {
            List<uint> newtimes = new List<uint>();
            List<float> newvalues = new List<float>();

            uint lastTime = data.Times.Last();

            //very occasionally, times don't start at zero. Huh. 
            uint firstTime = data.Times.First(); 
            start += firstTime;
            end += firstTime;

            if (start > lastTime)
                return null;

            for (int i = 1; i < data.Length && (end == 0 || data.Times[i] <= end); i++) 
            {
                if (data.Times[i] >= start)
                {
                    newtimes.Add(data.Times[i]);
                    newvalues.Add(data.Values[i]);
                }
            }
            if(newtimes.Count == 0) { return null; }

            TimeValueList newData = new TimeValueList(newtimes.ToArray(), newvalues.ToArray());

            return newData;
        }
    }
}
