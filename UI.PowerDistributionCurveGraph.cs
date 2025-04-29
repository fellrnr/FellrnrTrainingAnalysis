using FellrnrTrainingAnalysis.Action;
using FellrnrTrainingAnalysis.Model;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.Logging;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static FellrnrTrainingAnalysis.Model.PowerDistributionCurve;
using static ScottPlot.Plottable.PopulationPlot;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class PowerDistributionCurveGraph : UserControl
    {
        public PowerDistributionCurveGraph()
        {
            InitializeComponent();
        }

        public void DisplayActivity(Activity? activity)
        {
            var plt = formsPlot1.Plot;

            plt.Clear();

            DisplayActivityCurve(activity);

            DisplayBestCurve(activity);


            formsPlot1.Refresh();
        }

        private void DisplayBestCurve(Activity? activity)
        {
            if (activity == null) return;
            DateTime? start = activity.StartDateNoTimeLocal;
            if(start == null) return;
            if(activity.ParentAthlete == null) return;

            

            PowerDistributionCurve.BestCurve? bestCurve = activity.ParentAthlete.CalculateDistrubutionCurve(Activity.TagPowerDistributionCurve, start.Value, duration: 90);

            if(bestCurve == null) return;

            TimeValueList best = bestCurve.TimeValueList;




            //do some modelling...
            //make this configurable,but LR and 2 param envelope are nonsense

            //DisplayModel(best, new PdmFitLinearRegression(new PdmModel2Param()), "Linear Regression");

            DisplayModel(best, new PdmFitLeastSquares(new PdmModel2Param()), "2 Parm Least Squares");

            DisplayModel(best, new PdmFitLeastSquares(new PdmModel3Param(modelDecayForLeastSquares: false)), "3 Parm Least Squares, no decay");

            DisplayModel(best, new PdmFitLeastSquares(new PdmModel3Param(modelDecayForLeastSquares: true)), "3 Parm Least Squares, decay");

            DisplayModel(best, new PdmFitEnvelope(new PdmModel3Param()), "3 Parm Envelope");

            //DisplayModel(best, new PdmFitEnvelope(new PdmModel2Param()), "2 Parm Envelope");

            //display bests last to they're on top
            //DisplayPowerDistributionCurve(best, MarkerShape.filledDiamond, "Bests");
            DisplayPowerDistributionCurve(bestCurve, "Bests");


            var legend = formsPlot1.Plot.Legend();
            legend.Location = Alignment.UpperRight;
            legend.FontSize = 14;
        }

        private void DisplayModel(TimeValueList best, PdmFit fit, string name)
        {
            fit.DeriveCPParameters(best);

            PdmModel model = fit.Model;

            float[] modelValues = new float[best.Length];
            int[] times = PowerDistributionCurve.GetTimes(best);
            for (int i = 0; i < best.Length; i++)
            {
                modelValues[i] = (float)model.ModelledPowerAtTime(times[i]);
            }
            TimeValueList timeValueList = new TimeValueList(modelValues);

            string msg = $"{name}: Critical Power {model.CP:#,0}, W' {model.WPrime:#,0}";
            DisplayPowerDistributionCurve(timeValueList, MarkerShape.none, msg);

            if(fit.Model.MapTimeToValue != null)
            {
                foreach(var kvp in fit.Model.MapTimeToValue)
                {
                    int time = kvp.Key;
                    double logTime = Math.Log10(time);
                    double power = kvp.Value;
                    MarkerPlot markerPlot = formsPlot1.Plot.AddPoint(x: logTime, y: power, color: Color.Black, shape: MarkerShape.eks, size: 30);
                }
            }
        }

        private void DisplayActivityCurve(Activity? activity)
        {
            var plt = formsPlot1.Plot;

            TimeValueList? curve = GetCurveForActivity(activity);
            if (curve == null) return;

            DisplayPowerDistributionCurve(curve, MarkerShape.filledCircle, "Activity");

        }

        private void DisplayPowerDistributionCurve(TimeValueList curve, MarkerShape markerShape, string name)
        {
            var plt = formsPlot1.Plot;
            int[] times = PowerDistributionCurve.GetTimes(curve);
            double[] yArray = Array.ConvertAll(curve.Values, x => (double)x);
            //Scott Plot is only linear, so we have to pretent it's log
            double[] xArray = Array.ConvertAll(times, x => Math.Log10((double)x));
            //double[] xArray = Array.ConvertAll(computer.Curve.Times, x => (double)x);
            //double[] logXs = xArray.Select(x => Math.Log10(x)).ToArray();

            var scatter = plt.AddScatter(xArray, yArray, lineWidth: 2, markerSize: 10, markerShape: markerShape);
            scatter.Label = name;

            //static string logTickLabels(double x) => Math.Pow(10, x).ToString("N0");
            //plt.XAxis.TickLabelFormat(logTickLabels);

            plt.XAxis.MinorLogScale(true);
            plt.XAxis.MajorGrid(true, Color.FromArgb(80, Color.Black));
            plt.XAxis.MinorGrid(true, Color.FromArgb(20, Color.Black));
            plt.YAxis.MajorGrid(true, Color.FromArgb(80, Color.Black));

            //plt.XAxis.TickLabelFormat(customTickFormatterForTime);
            //plt.XAxis.DateTimeFormat(true);


            double[] logXs = TimePositions.Select(x => Math.Log10(x)).ToArray();
            plt.XAxis.ManualTickPositions(logXs, LabelPositions);
            //ManualTickPositions(double[] positions, string[] labels)

            //ScottPlot.Ticks.ManualTickCollection tc = new ScottPlot.Ticks.ManualTickCollection();
            //for (int i = 0; i < TimePositions.Length; i++)
            //    tc.AddMajor(TimePositions[i], LabelPositions[i]);

            //ScottPlot.Ticks.Tick[] ticks = tc.GetTicks();
            //plt.BottomAxis.SetTicks(ticks);
        }

        private void DisplayPowerDistributionCurve(PowerDistributionCurve.BestCurve bestCurve, string name)
        {
            var plt = formsPlot1.Plot;
            int[] times = PowerDistributionCurve.GetTimes(bestCurve.TimeValueList);
            //Scott Plot is only linear, so we have to pretent it's log
            double[] xArray = Array.ConvertAll(times, x => Math.Log10((double)x));


            double limit = 1;
            DateTime root = bestCurve.Root;
            foreach (var kvp in bestCurve.BestActivities)
            {
                TimeSpan ts = root - kvp.Value.StartDateNoTimeLocal!.Value;
                double offset = ts.TotalDays;
                limit = Math.Max(limit, offset);
            }

            foreach (var kvp in bestCurve.BestActivities)
            {
                double x = xArray[kvp.Key]; //key is offset into times, which have been mapped to log10 time


                TimeSpan ts = root - kvp.Value.StartDateNoTimeLocal!.Value;
                double offset = ts.TotalDays;
                double colorFraction = offset / limit;
                Color c = ScottPlot.Drawing.Colormap.Jet.GetColor(colorFraction, alpha: 0.5);
                MarkerPlot markerPlot = formsPlot1.Plot.AddPoint(x: x, y: bestCurve.TimeValueList.Values[kvp.Key], color: c, size: 10);
            }

            plt.XAxis.MinorLogScale(true);
            plt.XAxis.MajorGrid(true, Color.FromArgb(80, Color.Black));
            plt.XAxis.MinorGrid(true, Color.FromArgb(20, Color.Black));
            plt.YAxis.MajorGrid(true, Color.FromArgb(80, Color.Black));



            double[] logXs = TimePositions.Select(x => Math.Log10(x)).ToArray();
            plt.XAxis.ManualTickPositions(logXs, LabelPositions);
        }


        private TimeValueList? GetCurveForActivity(Activity? activity)
        {
            if (activity == null) { return null; }
            if (!activity.TimeSeries.ContainsKey(Activity.TagPowerDistributionCurve)) { return null; }
            TimeValueList? tvl = activity.TimeSeries[Activity.TagPowerDistributionCurve].GetData();
            if (tvl == null) { return null; }

            return tvl;
        }

        static double[] TimePositions = {
            1, 5, 15, 30,
            1*60, 2*60, 3*60, 5*60, 10*60, 20*60, 30*60,
            1*60*60, 2*60*60, 3*60*60, 5*60*60, 10*60*60, 20*60*60, 30*60*60,
            };

        static string[] LabelPositions = {
            "1s", "5s", "15s", "30s",
            "1m", "2m", "3m", "5m", "10m", "20m", "30m",
            "1h", "2h", "3h", "5h", "10h", "20h", "30h",
            };

        static string customTickFormatterForTime(double position)
        {
            if (position < 0)
                return "";
            DateTime dateTime1 = new DateTime();
            DateTime dateTime2 = dateTime1.AddSeconds(position);

            if (position < 60)
                return $"{dateTime2:ss}";
            if (position < 60*60)
                return $"{dateTime2:mm:ss}";
            return $"{dateTime2:H:mm:ss}";
        }
    }
}