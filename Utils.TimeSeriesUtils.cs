using pi.science.smoothing;
using pi.science.statistic;
using ScottPlot.Plottable;
using System.Diagnostics.Eventing.Reader;
using static pi.science.smoothing.PIMovingAverageSmoothing;


namespace FellrnrTrainingAnalysis.Utils
{
    public class TimeSeriesUtils
    {
        public enum SmoothingOptions { AverageWindow, SimpleExponential, InterpolateOnly, None };


        public static List<float> InterpolateToOneSecond(uint[] times, float[] values)
        {
            List<float> result = new List<float>();
            List<uint> sanity = new List<uint>();

            if (times.Length < 2)
                return new List<float>(values);

            if (times[0] != 0)
            {
                //fill in the first few values
                for (int i = 0; i < times[0]; i++)
                {
                    result.Add(values[0]);
                    sanity.Add((uint)i);
                }
            }

            result.Add(values[0]);
            sanity.Add(times[0]);
            for (int i = 1; i < times.Length; i++) //start from one
            {
                if (times[i] != times[i - 1] + 1)
                {
                    //gap to fill
                    uint timediff = times[i] - times[i - 1];
                    float valuediff = values[i] - values[i - 1];
                    float valuePerSec = (timediff != 0) ? valuediff / (float)timediff : (float)1;
                    for (int j = 1; j <= timediff; j++) //start from one
                    {
                        float newval = values[i - 1] + valuePerSec * j;
                        uint newtime = times[i - 1] + (uint)j;
                        result.Add(newval);
                        sanity.Add(newtime);
                    }
                }
                else
                {
                    result.Add(values[i]);
                    sanity.Add(times[i]);
                }
            }

            for (int i = 0; i < sanity.Count; i++)
            {
                //Logging.Instance.Debug($"{i} is t {sanity[i]} v {result[i]}");
                if (sanity[i] != i)
                {
                    Logging.Instance.Error($"Oops, time at {i} is {sanity[i]}");
                    break;
                }
            }

            return result;
        }


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
            double[] asDouble = Array.ConvertAll(rawData, x => (double)x);

            double[] smoothed = WindowSmoothed(asDouble, windowSize);

            float[] asFloat = Array.ConvertAll(rawData, x => (float)x);

            return asFloat;
            /*
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
            */
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






    }
}