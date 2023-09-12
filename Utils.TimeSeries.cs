using pi.science.smoothing;
using pi.science.statistic;

namespace FellrnrTrainingAnalysis.Utils
{
    public class Utils
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

                smoothInput.AddValues(rawData);
                PIMedianSmoothing medianSmoothing = new PIMedianSmoothing(smoothInput);
                medianSmoothing.SetWindowLength(windowSize);
                medianSmoothing.SetOuterValuesNull(false);
                medianSmoothing.Calc();

                PIVariable smoothOutput = medianSmoothing.GetOutputVariable();

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

            public static Tuple<uint[], float[]>? SpanDeltas(Tuple<uint[], float[]> data, float scalingFactor, float? numerator, float? limit, float period )
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


        }
    }
}
