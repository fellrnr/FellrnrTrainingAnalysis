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
        //the id and date are used for display purposes
        private string stravaId = "";
        private DateTime? activityStart;
        private double rSquared;
        private double yIntercept;
        private double slope;
        private double xStandardDeviation;
        private double yStandardDeviation;
        private int count = 0;

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

        public double RSquared { get => rSquared; set => rSquared = value; }
        public double YIntercept { get => yIntercept; set => yIntercept = value; }
        public double Slope { get => slope; set => slope = value; }
        public double XStandardDeviation { get => xStandardDeviation; set => xStandardDeviation = value; }
        public double YStandardDeviation { get => yStandardDeviation; set => yStandardDeviation = value; }
        public int Count { get => count; set => count = value; }
        public string StravaId { get => stravaId; set => stravaId = value; }
        public DateTime? ActivityStart { get => activityStart; set => activityStart = value; }


        private void Evaluate(float[] arrayX, float[] arrayY, bool ignoreZerosX, bool ignoreZerosY)
        {
            //this isn't quite right; we really need the average of all time series, not doing it per list
            int length = arrayX.Length;
            Count += length;
            avgX = arrayX.Average();
            avgY = arrayX.Average();
            double sumSqDiffXThis = 0;
            double sumSqDiffYThis = 0;
            for (int i = 0; i < length; i++)
            {
                double x = arrayX[i];
                double y = arrayY[i];
                if ((!ignoreZerosX || x != 0) && (!ignoreZerosY || y != 0))
                {
                    sumCodeviates += x * y;
                    sumOfX += x;
                    sumOfY += y;
                    sumOfXSq += x * x;
                    sumOfYSq += y * y;

                    double diff;
                    diff = x - avgX;
                    sumSqDiffX += diff * diff;
                    sumSqDiffXThis += diff * diff;
                    diff = y - avgY;
                    sumSqDiffY += diff * diff;
                    sumSqDiffYThis += diff * diff;
                }
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
            if (sCo == 0 && ssX == 0)
            {
                Slope = 0;
                YIntercept = 0;
            }
            else
            {
                Slope = (float)(sCo / ssX);
                YIntercept = (float)(meanY - ((sCo / ssX) * meanX));
            }

            XStandardDeviation = (float)Math.Sqrt(sumSqDiffX / Count);
            YStandardDeviation = (float)Math.Sqrt(sumSqDiffY / Count);
            if (Options.Instance.DebugLinearRegression && XStandardDeviation > 1000)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
        }

        public static LinearRegression AverageLinearRegressionList(List<LinearRegression> regressions)
        {
            LinearRegression sum = new LinearRegression();

            foreach (LinearRegression lr in regressions)
            {
                sum.RSquared += lr.RSquared;
                sum.YIntercept += lr.YIntercept;
                sum.Slope += lr.Slope;
                sum.XStandardDeviation += lr.XStandardDeviation;
                sum.YStandardDeviation += lr.YStandardDeviation;
                sum.Count += lr.Count;
            }

            LinearRegression result = new LinearRegression();

            result.RSquared += sum.RSquared / regressions.Count;
            result.YIntercept += sum.YIntercept / regressions.Count;
            result.Slope += sum.Slope / regressions.Count;
            result.XStandardDeviation += sum.XStandardDeviation / regressions.Count;
            result.YStandardDeviation += sum.YStandardDeviation / regressions.Count;
            result.Count += sum.Count / regressions.Count;

            return result;
        }

        public static LinearRegression? EvaluateLinearRegression(AlignedTimeSeries alignedTimeSeries, bool primaryIsX, bool ignoreZerosX, bool ignoreZerosY)
        {

            float[] arrayX = primaryIsX ? alignedTimeSeries.Primary : alignedTimeSeries.Secondary;
            float[] arrayY = !primaryIsX ? alignedTimeSeries.Primary : alignedTimeSeries.Secondary;

            return EvaluateLinearRegression(arrayX, arrayY, ignoreZerosX, ignoreZerosY);
        }
        public static LinearRegression? EvaluateLinearRegression(float[] arrayX, float[] arrayY, bool ignoreZerosX, bool ignoreZerosY)
        {
            LinearRegression lr = new LinearRegression();

            lr.Evaluate(arrayX, arrayY, ignoreZerosX, ignoreZerosY);

            if (double.IsNaN(lr.RSquared))
            {
                Logging.Instance.Error("RSquared IsNaN");
                return null;
            }
            if (double.IsNaN(lr.YIntercept))
            {
                Logging.Instance.Error("YIntercept IsNaN");
                return null;
            }
            if (double.IsNaN(lr.Slope))
            {
                Logging.Instance.Error("Slope IsNaN");
                return null;
            }
            if (lr.RSquared > 1.001) //we can get rounding that bumps us just over 1
            {
                Logging.Instance.Error("R^2 > 1");
                return null;
            }

            return lr;
        }


    }

}
