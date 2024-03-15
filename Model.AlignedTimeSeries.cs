using FellrnrTrainingAnalysis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;

namespace FellrnrTrainingAnalysis.Model
{
    public class AlignedTimeSeries
    {

        public static AlignedTimeSeries? Align(TimeValueList primary, TimeValueList secondary)
        {
            Logging.Instance.ContinueAccumulator("Utils.TimeSeries.AlignedTimeSeries");

            uint[] ptimes = primary.Times;
            float[] pvalues = primary.Values;
            uint[] stimes = secondary.Times;
            float[] svalues = secondary.Values;


            AlignedTimeSeries? aligned = null;
            if (ptimes.Length == stimes.Length)
            {
                //let's assume they match
                aligned = new AlignedTimeSeries(ptimes, pvalues, svalues);
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

                    while (si < stimes.Length && pi < ptimes.Length && ptimes[pi] > stimes[si]) //we incremented pi above, so could be over
                    {
                        si++;
                    }

                    if (si < stimes.Length && pi < ptimes.Length && ptimes[pi] == stimes[si]) // pi < ptimes.Length && si < stimes.Length
                    {
                        //all good
                        newTimes.Add(ptimes[pi]);
                        newPrimary.Add(pvalues[pi]);
                        newSecondary.Add(svalues[si]);
                        if (si < stimes.Length)
                            si++;
                    }
                }

                if (newTimes.Count != newPrimary.Count || newTimes.Count != newSecondary.Count)
                {
                    throw new Exception($"AlignedTimeSeries times {newTimes.Count} and primary {newPrimary.Count} secondary {newSecondary.Count} don't match counts");
                }


                aligned = new AlignedTimeSeries(newTimes.ToArray(), newPrimary.ToArray(), newSecondary.ToArray());
            }

            Logging.Instance.PauseAccumulator("Utils.TimeSeries.AlignedTimeSeries");
            return aligned;
        }

        public AlignedTimeSeries(uint[] time, float[] primary, float[] secondary)
        {
            Time = time;
            Primary = primary;
            Secondary = secondary;
        }

        public int Length { get { return Time.Length; } }

        public uint[] Time { get; set; }
        public float[] Primary { get; set; }
        public float[] Secondary { get; set; }


        public string ToCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time,Primary,Secondary");
            for (int i = 0; i < Length; i++)
            {
                sb.AppendLine($"{Time[i]},{Primary[i]},{Secondary[i]}");
            }
            return sb.ToString();
        }

        public class LinearRegressionResults
        {
            public float RSquared;
            public float YIntercept;
            public float Slope;
        }


        public LinearRegressionResults? LinearRegression(bool primaryIsX)
        {
            LinearRegressionResults retval = new LinearRegressionResults();

            float sumOfX = 0;
            float sumOfY = 0;
            float sumOfXSq = 0;
            float sumOfYSq = 0;
            float ssX = 0;
            float ssY = 0;
            float sumCodeviates = 0;
            float sCo = 0;
            float count = this.Length;
            for (int i = 0; i < count; i++)
            {
                float p = this.Primary[i];
                float s = this.Secondary[i];
                float x = primaryIsX ? p : s;
                float y = !primaryIsX ? p : s;
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            float RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            float RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            float meanX = sumOfX / count;
            float meanY = sumOfY / count;
            float dblR = RNumerator / (float)Math.Sqrt(RDenom);

            retval.RSquared = dblR * dblR;
            retval.YIntercept = meanY - ((sCo / ssX) * meanX);
            retval.Slope = sCo / ssX;

            if (!float.IsNormal(retval.RSquared))
                return null;
            if (!float.IsNormal(retval.YIntercept))
                return null;
            if (!float.IsNormal(retval.Slope))
                return null;
            return retval;
        }

    }
}
