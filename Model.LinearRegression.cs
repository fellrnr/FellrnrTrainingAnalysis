using FellrnrTrainingAnalysis.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    public class LinearRegression
    {
        public float RSquared;
        public float YIntercept;
        public float Slope;
        public float XStandardDeviation;
        public float YStandardDeviation;
        public int Count = 0;

        public void Save(Extensible extensible, string name)
        {
            extensible.AddOrReplaceDatum(new TypedDatum<float>($"{name}-Slope", false, (float)Slope));
            extensible.AddOrReplaceDatum(new TypedDatum<float>($"{name}-YIntercept", false, (float)YIntercept));
            extensible.AddOrReplaceDatum(new TypedDatum<float>($"{name}-RSquared", false, (float)RSquared));
        }

        public override string ToString()
        {
            return $"R2 {RSquared:#,0.00} Slope {Slope:#,0.00} Y0 {YIntercept:#,0.00}";
        }

        double sumOfX = 0;
        double sumOfY = 0;
        double sumOfXSq = 0;
        double sumOfYSq = 0;
        double ssX;
        //double ssY;
        double sumCodeviates = 0;
        double sCo;

        //sd
        double sumSqDiffX = 0;
        double sumSqDiffY = 0;


        //working variables for debug
        double RNumerator;
        double RDenom;
        double meanX;
        double meanY;
        double dblR;
        double avgX;
        double avgY;

        private void Evaluate(AlignedTimeSeries alignedTimeSeries, bool primaryIsX)
        {
            int length = alignedTimeSeries.Length;
            Count += length;
            float[] arrayX = primaryIsX ? alignedTimeSeries.Primary : alignedTimeSeries.Secondary;
            float[] arrayY = !primaryIsX ? alignedTimeSeries.Primary : alignedTimeSeries.Secondary;

            //this isn't quite right; we really need the average of all time series, not doing it per list
            avgX = arrayX.Average();
            avgY = arrayX.Average();
            for (int i = 0; i < length; i++)
            {
                double x = arrayX[i];
                double y = arrayY[i];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;

                double diff;
                diff = x - avgX;
                sumSqDiffX += diff * diff;

                diff = y - avgY;
                sumSqDiffY += diff * diff;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / length);
            //ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            RNumerator = (length * sumCodeviates) - (sumOfX * sumOfY);
            RDenom = (length * sumOfXSq - (sumOfX * sumOfX)) * (length * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / length);

            meanX = sumOfX / length;
            meanY = sumOfY / length;

            if (RNumerator == 0 && RDenom == 0) //can happen when x is constant
            {
                RSquared = 0; 
            }
            else
            {
                dblR = RNumerator / (double)Math.Sqrt(RDenom);

                RSquared = (float)(dblR * dblR);
            }
            YIntercept = (float)(meanY - ((sCo / ssX) * meanX));
            if(sCo == 0 && ssX == 0)
            {
                Slope = 0;
            }
            else
            {
                Slope = (float)(sCo / ssX);
            }

            XStandardDeviation = (float)Math.Sqrt(sumSqDiffX / Count);
            YStandardDeviation = (float)Math.Sqrt(sumSqDiffY / Count);

        }

        public static LinearRegression? EvaluateLinearRegression(AlignedTimeSeries alignedTimeSeries, bool primaryIsX, LinearRegression? prior = null)
        {
            if(prior == null) { prior = new LinearRegression(); }
            

            prior.Evaluate(alignedTimeSeries, primaryIsX);

            if (double.IsNaN(prior.RSquared))
            {
                Logging.Instance.Error("RSquared IsNaN");
                return null;
            }
            if (double.IsNaN(prior.YIntercept))
            {
                Logging.Instance.Error("YIntercept IsNaN");
                return null;
            }
            if (double.IsNaN(prior.Slope))
            {
                Logging.Instance.Error("Slope IsNaN");
                return null;
            }
            if (prior.RSquared > 1.001) //we can get rounding that bumps us just over 1
            {
                Logging.Instance.Error("R^2 > 1");
                return null;
            }

            return prior;
        }


    }

}
