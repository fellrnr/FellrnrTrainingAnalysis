using FellrnrTrainingAnalysis.Model;
using pi.science.smoothing;
using pi.science.statistic;
using static pi.science.smoothing.PIMovingAverageSmoothing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FellrnrTrainingAnalysis.Utils
{
    public class TimeSeries
    {
        public enum SmoothingOptions { AverageWindow, SimpleExponential, InterpolateOnly, None };

        public static Tuple<List<double>, List<double>> Interpolate(List<double> listX, List<double> listY)
        {
            if (listX.Count < 2)
            {
                return new Tuple<List<double>, List<double>>(listX, listY);
            }

            List<double> resultX = new List<double>();
            List<double> resultY = new List<double>();


            resultX.Add(listX.First());
            resultY.Add(listY.First());

            for (int i = 0; i < listX.Count - 1; i++) //Note: count - 1
            {
                double x1 = listX[i];
                double x2 = listX[i + 1];
                double y1 = listY[i];
                double y2 = listY[i + 1];

                double xi = (x2 - x1);
                double yi = (y2 - y1);

                //number of parts is xi as int
                double divider = Math.Floor(xi);

                // divide difference into n parts
                xi = xi / divider;
                yi = yi / divider;

                // set new temp vars equal to first point
                double xf = x1;
                double yf = y1;


                // increment temp vars by difference-division parts
                for (int j = 0; j < divider; j++)
                {
                    xf += xi;
                    yf += yi;

                    resultX.Add(xf);
                    resultY.Add(yf);
                }
                resultX.Add(x2);
                resultY.Add(y2);

                /* for debug - I had a GPX that 120m drop in 45cm distance
                for(int k = 0; k < resultY.Count-1; k++)
                {
                    double diff = resultY[k + 1] - resultY[k];
                    diff = Math.Abs(diff);
                    if (diff > 10)
                        Console.WriteLine(diff);
                }
                */
            }


            Tuple<List<double>, List<double>> retval = new Tuple<List<double>, List<double>>(resultX, resultY);
            return retval;
        }

        public static double[] WindowSmoothed(double[] rawData, int windowSize)
        {
            if (windowSize == 0)
                return rawData;


            PIVariable smoothInput = new PIVariable();
            smoothInput.AddMoreValues(rawData[0], windowSize);
            smoothInput.AddValues(rawData);
            smoothInput.AddMoreValues(rawData[rawData.Length - 1], windowSize);

            PIMovingAverageSmoothing MA = new PIMovingAverageSmoothing(smoothInput);
            MA.SetCalculationType(CalculationType.SIMPLE_CENTERED);
            MA.SetWindowLength(windowSize);
            MA.Calc();

            PIVariable smoothOutput = MA.GetOutputVariable();
            smoothOutput.DeleteFirst(windowSize);
            smoothOutput.DeleteLast(windowSize);

            double[] smoothed = new double[rawData.Length];

            for (int i = 0; i < rawData.Length; i++)
            {
                double? val = smoothOutput[i];
                if (val is null)
                    val = 0;
                smoothed[i] = (double)val;
            }

            return smoothed;
        }


        public static double[] WindowSmoothedXXX(double[] rawData, int windowSize)
        {
            if (windowSize == 0)
                return rawData;

            double[] smoothed = new double[rawData.Length];
            double[] buffer = new double[windowSize];


            int bufferIndex = 0;
            double sum = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                double nextInput = rawData[i];
                sum = sum - buffer[bufferIndex] + nextInput;

                // overwrite the old value with the new one
                buffer[bufferIndex] = nextInput;

                // increment the buffer index 
                bufferIndex = (bufferIndex + 1) % windowSize;

                // calculate the average
                smoothed[i] = ((double)sum) / windowSize;
            }

            return smoothed;
        }

        public static double[] SimpleExponentialSmoothed(double[] rawData, int windowSize)
        {
            if (windowSize == 0)
                return rawData;
            double[] smoothed = new double[rawData.Length];


            int smoothCounter = 0;
            double smoothedValue = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                double nextInput = rawData[i];
                if (smoothCounter < windowSize)
                {
                    smoothCounter++;
                }

                smoothedValue = (smoothedValue * (smoothCounter - 1.0) / smoothCounter) + nextInput / smoothCounter;
                // calculate the average
                smoothed[i] = smoothedValue;
            }

            return smoothed;
        }
        public static float[] WindowSmoothed(float[] rawData, int windowSize)
        {
            float[] smoothed = new float[rawData.Length];
            float[] buffer = new float[windowSize];


            int bufferIndex = 0;
            float sum = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                float nextInput = rawData[i];
                sum = sum - buffer[bufferIndex] + nextInput;

                // overwrite the old value with the new one
                buffer[bufferIndex] = nextInput;

                // increment the buffer index 
                bufferIndex = (bufferIndex + 1) % windowSize;

                // calculate the average
                smoothed[i] = ((float)sum) / windowSize;
            }

            return smoothed;
        }

        public static float[] SimpleExponentialSmoothed(float[] rawData, int windowSize)
        {
            float[] smoothed = new float[rawData.Length];


            int smoothCounter = 0;
            float smoothedValue = 0;
            for (int i = 0; i < rawData.Length; i++)
            {
                float nextInput = rawData[i];
                if (smoothCounter < windowSize)
                {
                    smoothCounter++;
                }

                smoothedValue = (smoothedValue * (smoothCounter - 1.0f) / smoothCounter) + nextInput / smoothCounter;
                // calculate the average
                smoothed[i] = smoothedValue;
            }

            return smoothed;
        }

        //calculate a simple delta, scaled by time. So change of 10 in a time of 1 is 10, a change of 10 in a time of 2 is 5
        //supports scaling and inverting the values
        public static Tuple<uint[], float[]>? SimpleDeltas(Tuple<uint[], float[]> data, float ScalingFactor, float? Numerator, float? Limit)
        {
            uint[] elapsedTime = data.Item1;
            float[] values = data.Item2;
            float[] deltas = new float[elapsedTime.Length];
            float lastValue = values[0];
            float lastTime = 0;


            for (int i = 1; i < elapsedTime.Length; i++) //note starting from one as we handle the first entry above
            {
                float deltaTime = elapsedTime[i] - lastTime;
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
            Tuple<uint[], float[]> newData = new Tuple<uint[], float[]>(elapsedTime, deltas);
            return newData;
        }

        //A more complex delta, using a time span for the change. Used for averaging over 60 seconds for instance. 
        public static Tuple<uint[], float[]>? SpanDeltas(Tuple<uint[], float[]> data, float scalingFactor, float? numerator, float? limit, float period, bool extraDebug)
        {
            uint[] elapsedTime = data.Item1;
            float[] values = data.Item2;
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

                if(extraDebug)
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
            Tuple<uint[], float[]> newData = new Tuple<uint[], float[]>(elapsedTime, deltas);
            return newData;
        }

        public static float Percentile(List<float> sortedSequence, float percentile)
        {
            //Array.Sort(sortedSequence);
            int N = sortedSequence.Count;
            float n = (N - 1) * (percentile / 100) + 1;
            // Another method: double n = (N + 1) * excelPercentile;
            if (n == 1d) return sortedSequence[0];
            else if (n == N) return sortedSequence[N - 1];
            else
            {
                int k = (int)n;
                float d = n - k;
                return sortedSequence[k - 1] + d * (sortedSequence[k] - sortedSequence[k - 1]);
            }
        }

        public static AlignedTimeSeries? Align(Tuple<uint[], float[]> primary, Tuple<uint[], float[]> secondary)
        {

            uint[] ptimes = primary.Item1;
            float[] pvalues = primary.Item2;
            uint[] stimes = secondary.Item1;
            float[] svalues = secondary.Item2;

            if (ptimes.Length == stimes.Length)
            {
                //let's assume they match
                AlignedTimeSeries aligned = new AlignedTimeSeries(ptimes, pvalues, svalues);
                return aligned;
            }
            else
            {
                List<uint> newTimes = new List<uint>();
                List<float> newPrimary = new List<float>();
                List<float> newSecondary = new List<float>();

                int si = 0;
                for (int pi = 0; pi < ptimes.Length && si < stimes.Length; pi++)
                {
                    while (pi < ptimes.Length && ptimes[pi] < stimes[si])
                    {
                        pi++;
                    }

                    while (si < stimes.Length && ptimes[pi] > stimes[si])
                    {
                        si++;
                    }

                    if (ptimes[pi] == stimes[si])
                    {
                        //all good
                        newTimes.Add(ptimes[pi]);
                        newPrimary.Add(pvalues[pi]);
                        newSecondary.Add(svalues[pi]);
                        if (si < stimes.Length)
                            si++;
                    }
                }
                AlignedTimeSeries aligned = new AlignedTimeSeries(newTimes.ToArray(), newPrimary.ToArray(), newSecondary.ToArray());
                return aligned;
            }
        }

        public class AlignedTimeSeries
        {
            public AlignedTimeSeries(uint[] time, float[] primary, float[] secondary)
            {
                Time = time;
                Primary = primary;
                Secondary = secondary;
            }

            public uint[] Time { get; set; }
            public float[] Primary { get; set; }
            public float[] Secondary { get; set; }
        }



        public static AlignedTimeLocationSeries? Align(LocationStream primary, DataStreamBase secondary)
        {
            if(primary.Times == null)
                return null; //can't aling without time

            Tuple<uint[], float[]>? sdata = secondary.GetData();
            if(sdata == null) return null;

            uint[] ptimes = primary.Times;
            float[] latvalues = primary.Latitudes;
            float[] lonvalues = primary.Longitudes;
            uint[] stimes = sdata.Item1;
            float[] svalues = sdata.Item2;

            if (ptimes.Length == stimes.Length)
            {
                //let's assume they match
                AlignedTimeLocationSeries aligned = new AlignedTimeLocationSeries(ptimes, latvalues, lonvalues, svalues);
                return aligned;
            }
            else
            {
                List<uint> newTimes = new List<uint>();
                List<float> newLats = new List<float>();
                List<float> newLons = new List<float>();
                List<float> newSecondary = new List<float>();



                int si = 0;
                for (int pi = 0; pi < ptimes.Length && si < stimes.Length; pi++)
                {
                    while (pi < ptimes.Length && ptimes[pi] < stimes[si])
                    {
                        pi++;
                    }

                    while (si < stimes.Length && ptimes[pi] > stimes[si])
                    {
                        si++;
                    }

                    if (ptimes[pi] == stimes[si])
                    {
                        //all good
                        newTimes.Add(ptimes[pi]);
                        newLats.Add(latvalues[pi]);
                        newLons.Add(lonvalues[pi]);
                        newSecondary.Add(svalues[pi]);
                        if (si < stimes.Length)
                            si++;
                    }
                }
                AlignedTimeLocationSeries aligned = new AlignedTimeLocationSeries(newTimes.ToArray(), newLats.ToArray(), newLons.ToArray(), newSecondary.ToArray());
                return aligned;
            }
        }

        public class AlignedTimeLocationSeries
        {
            public AlignedTimeLocationSeries(uint[] time, float[] lats, float[] lons, float[] secondary)
            {
                Time = time;
                Lats = lats;
                Lons = lons;
                Secondary = secondary;
            }

            public uint[] Time { get; set; }
            public float[] Lats { get; set; }
            public float[] Lons { get; set; }
            public float[] Secondary { get; set; }
        }
 
    
    }
}