using BrightIdeasSoftware;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ActivityTimeGraph : UserControl
    {
        public ActivityTimeGraph()
        {
            InitializeComponent();

            bool designMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
            if (!designMode)
                Definitions = TimeSeriesDefinition.GetDefinitions();

            CreateTimeSeriesPane();
        }

        private List<Axis> CurrentAxis { get; set; } = new List<Axis>();
        private Dictionary<int, Axis> ReusedAxis { get; set; } = new Dictionary<int, Axis>();

        private Dictionary<int, Tuple<double, double>> YAxisMinMax { get; set; } = new Dictionary<int, Tuple<double, double>>();
        private int axisIndex = 0;
        private const int AXIS_OFFSET = 3;

        private Crosshair? MouseCrosshair { get; set; }

        //0.3 is 55:30 min/km. Anything slower can be considered not moving to make the graph work, otherwise min/km values tend towards infinity
        const double MINPACE = 0.3;

        private List<TimeSeriesDefinition>? Definitions { get; }

        private TimeSeriesBase? XAxisTimeSeries = null;
        private TimeValueList? XAxisTimeValueList = null;
        private TimeSeriesDefinition? XAxisDataStreamDefinition = null;


        private void CreateTimeSeriesPane()
        {
            if (Definitions != null)
            {
                /**/
                objectListViewTimeSeries.SuspendLayout();

                objectListViewTimeSeries.ShowGroups = false;
                objectListViewTimeSeries.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
                //Generator.GenerateColumns(objectListView1, definitions);
                Generator.GenerateColumns(this.objectListViewTimeSeries, typeof(TimeSeriesDefinition), true);
                objectListViewTimeSeries.SetObjects(Definitions);
                objectListViewTimeSeries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                objectListViewTimeSeries.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                objectListViewTimeSeries.ResumeLayout();
                /**/
            }
        }
        //DataGridViewSelectedRowCollection? currentSelectedRowCollection = null;
        private Activity? CurrentlyDisplayedActivity = null;
        private List<CurrentlyDisplayed> CurrentlyDisplayedSeries = new List<CurrentlyDisplayed>();

        private class CurrentlyDisplayed
        {

            public TimeSeriesBase TimeSeriesBase;
            public TimeValueList TimeValueList;
            public TimeSeriesDefinition TimeSeriesDefinition;

            public CurrentlyDisplayed(TimeSeriesBase timeSeriesBase, TimeValueList timeValueList, TimeSeriesDefinition timeSeriesDefinition)
            {
                TimeSeriesBase = timeSeriesBase;
                TimeValueList = timeValueList;
                TimeSeriesDefinition = timeSeriesDefinition;
            }
        }



        public void UpdateTimeSeriesGraph(Model.Activity? activity)
        {
            Logging.Instance.TraceEntry("UpdateTimeSeriesGraph");

            formsPlot1.Plot.Clear();

            //formsPlot1.Plot.GetSettings().Axes.Clear();
            foreach (Axis axis in CurrentAxis) { formsPlot1.Plot.RemoveAxis(axis); }
            CurrentAxis.Clear();
            ReusedAxis.Clear();
            YAxisMinMax.Clear();
            axisIndex = 0;

            if (activity != null)
            {
                CurrentlyDisplayedActivity = activity;
                CurrentlyDisplayedSeries.Clear();

                XAxisTimeSeries = null;
                XAxisTimeValueList = null;
                XAxisDataStreamDefinition = null;
                foreach (KeyValuePair<string, TimeSeriesBase> kvp in activity.TimeSeries)
                {
                    string timeSeriesName = kvp.Key;
                    TimeSeriesDefinition? dataStreamDefinition = TimeSeriesDefinition.FindTimeSeriesDefinition(timeSeriesName);
                    if (dataStreamDefinition != null && dataStreamDefinition.ShowReportGraph && dataStreamDefinition.Axis < 0)
                    {
                        XAxisTimeSeries = kvp.Value;
                        XAxisTimeValueList = XAxisTimeSeries.GetData();
                        XAxisDataStreamDefinition = dataStreamDefinition;
                        break; //take the first one with negative axis id
                    }
                }


                foreach (KeyValuePair<string, TimeSeriesBase> kvp in activity.TimeSeries)
                {
                    DisplayTimeSeries(activity, kvp.Value);
                }

                SetYAxisLimits();
                SetXAxisLimits();

                MouseCrosshair = formsPlot1.Plot.AddCrosshair(10, 10);
                //MouseCrosshair.LineWidth = 2;
                MouseCrosshair.Color = Color.Red;
            }
            formsPlot1.Refresh();
            Logging.Instance.TraceLeave();
        }

        private void DisplayTimeSeries(Model.Activity activity, TimeSeriesBase timeSeriesBase)
        {
            string timeSeriesName = timeSeriesBase.Name;

            TimeSeriesDefinition? dataStreamDefinition = TimeSeriesDefinition.FindTimeSeriesDefinition(timeSeriesName);
            if (dataStreamDefinition == null || !dataStreamDefinition.ShowReportGraph || dataStreamDefinition.Axis < 0)
                return;

            if (!timeSeriesBase.IsValid())
                return;

            TimeValueList? dataStream = timeSeriesBase.GetData();
            if (dataStream == null)
                return;

            CurrentlyDisplayedSeries.Add(new CurrentlyDisplayed(timeSeriesBase, dataStream, dataStreamDefinition)); //cache for mouse movement


            double[]? xArray = GetXDataForDisplay(dataStream, dataStreamDefinition);
            if (xArray == null) return;

            double[]? yArray = GetYDataForDisplay(dataStream, dataStreamDefinition);
            if (yArray == null) return;


            if (XAxisTimeValueList == null || XAxisDataStreamDefinition == null)
            {

                var scatterGraph = formsPlot1.Plot.AddScatter(xArray, yArray, color: dataStreamDefinition.GetColor());
                SetLineStyle(dataStreamDefinition, scatterGraph);

                if (XAxisTimeValueList == null)
                    formsPlot1.Plot.XAxis.TickLabelFormat(customTickFormatterForTime);
                else
                    formsPlot1.Plot.XAxis.TickLabelFormat(null);

                int axisId = SetYAxis(timeSeriesBase, dataStreamDefinition, yArray, scatterGraph);

                SetHighlights(timeSeriesBase, yArray, axisId);
            }
            else
            {
                int limit = Math.Min(xArray.Length, yArray.Length);
                for (int i = 0; i < limit; i++)
                {
                    double di = (double)i;
                    double dlimit = (double)limit;
                    double colorFraction = di / dlimit;
                    Color c = ScottPlot.Drawing.Colormap.Turbo.GetColor(colorFraction, alpha: 0.5);
                    MarkerPlot markerPlot = formsPlot1.Plot.AddPoint(xArray[i], yArray[i], c);
                }
            }

            return;
        }

        private void SetHighlights(TimeSeriesBase timeSeriesBase, double[] yArraySmoothed, int axisId)
        {
            if (timeSeriesBase.Highlights != null)
            {
                foreach (Tuple<uint, uint> area in timeSeriesBase.Highlights)
                {
                    var rect = formsPlot1.Plot.AddRectangle((double)area.Item1, (double)area.Item2, yArraySmoothed.Min(), yArraySmoothed.Max());
                    rect.BorderColor = Color.Red;
                    rect.BorderLineWidth = 3;
                    rect.BorderLineStyle = LineStyle.Dot;
                    rect.Color = Color.FromArgb(50, Color.Yellow);
                    rect.YAxisIndex = axisId;
                }
            }
        }

        private int SetYAxis(TimeSeriesBase timeSeriesBase, TimeSeriesDefinition dataStreamDefinition, double[] yArraySmoothed, ScatterPlot scatterGraph)
        {
            CalculateYAxisLimits(dataStreamDefinition, yArraySmoothed);

            Axis yAxis;
            int axisId;
            int forcedAxis = dataStreamDefinition.Axis;
            string axisTags = timeSeriesBase.IsVirtual() ? " (V)" : "";
            string axisLabel = $"{dataStreamDefinition.DisplayTitle}{axisTags}";

            if (forcedAxis >= 0 && ReusedAxis.ContainsKey(forcedAxis))
            {
                yAxis = ReusedAxis[forcedAxis];
                yAxis.Label($"{yAxis.Label()}/{axisLabel}");
                axisId = yAxis.AxisIndex;
                scatterGraph.YAxisIndex = axisId;
            }
            else
            {

                if (axisIndex == 0)
                {
                    yAxis = formsPlot1.Plot.YAxis;
                    scatterGraph.YAxisIndex = 0;
                    axisId = 0;
                }
                else
                {
                    yAxis = formsPlot1.Plot.AddAxis(Edge.Left);
                    //yAxis.AxisIndex = axisIndex + AXIS_OFFSET; //there are four default indexes we have to skip

                    axisId = yAxis.AxisIndex;
                    scatterGraph.YAxisIndex = yAxis.AxisIndex;
                    CurrentAxis.Add(yAxis);
                }
                if (forcedAxis >= 0)
                    ReusedAxis[forcedAxis] = yAxis;

                if (dataStreamDefinition.DisplayUnits == TimeSeriesDefinition.DisplayUnitsType.Pace && !Options.Instance.DebugDisableTimeAxis)
                {
                    yAxis.TickLabelFormat(CustomTickFormatterForPace);
                }
                else
                {
                    yAxis.TickLabelFormat(null); //reset as axis are reused
                }
                yAxis.Label(axisLabel);
                yAxis.Color(scatterGraph.Color);
                axisIndex++;
            }

            return axisId;
        }

        private void CalculateYAxisLimits(TimeSeriesDefinition dataStreamDefinition, double[] yArraySmoothed)
        {
            double yMin = yArraySmoothed.Min();
            double yMax = yArraySmoothed.Max();
            if (dataStreamDefinition.MinYAxis != null)
                yMin = Math.Min(yMin, dataStreamDefinition.MinYAxis.Value);
            if (dataStreamDefinition.MaxYAxis != null)
                yMax = Math.Min(yMax, dataStreamDefinition.MaxYAxis.Value);

            if (YAxisMinMax.ContainsKey(axisIndex)) //have we reused this index
            {
                Tuple<double, double> prior = YAxisMinMax[axisIndex];
                double min = Math.Min(prior.Item1, yMin);
                double max = Math.Max(prior.Item2, yMax);
                YAxisMinMax[axisIndex] = new Tuple<double, double>(min, max);
            }
            else
            {
                YAxisMinMax.Add(axisIndex, new Tuple<double, double>(yMin, yMax));
            }
        }

        private void SetLineStyle(TimeSeriesDefinition dataStreamDefinition, ScatterPlot scatterGraph)
        {
            if (XAxisTimeValueList != null)
            {
                scatterGraph.LineStyle = LineStyle.None;
                scatterGraph.MarkerShape = MarkerShape.openCircle;
            }
            else
            {

                scatterGraph.MarkerShape = MarkerShape.none;
                scatterGraph.LineWidth = 2;
                if (dataStreamDefinition.LineStyle == "Dot")
                    scatterGraph.LineStyle = LineStyle.Dot;
                if (dataStreamDefinition.LineStyle == "DashDotDot")
                    scatterGraph.LineStyle = LineStyle.DashDotDot;
                if (dataStreamDefinition.LineStyle == "DashDot")
                    scatterGraph.LineStyle = LineStyle.DashDot;
                if (dataStreamDefinition.LineStyle == "Dash")
                    scatterGraph.LineStyle = LineStyle.Dash;
            }
        }

        private double[]? GetYDataForDisplay(TimeValueList timeValueList,
                                            TimeSeriesDefinition dataStreamDefinition)
        {
            double[] yArraySmoothed;

            double[] yArrayRaw = Array.ConvertAll(timeValueList.Values, x => (double)x);

            yArraySmoothed = Smooth(yArrayRaw, dataStreamDefinition);

            double[] xArray = new double[timeValueList.Length];
            for (int i = 0; i < timeValueList.Length; i++)
            {
                xArray[i] = i;
            }
            for (int i = 0; i < yArraySmoothed.Length; i++)
            {
                double y = yArraySmoothed[i];
                if (y != 0 && !double.IsNormal(y))
                {
                    Logging.Instance.Log(string.Format("invalid Y value {0} at offset {1} of {2}", yArraySmoothed[i], i, dataStreamDefinition.Name));
                    return null;
                }
            }

            return yArraySmoothed;
        }

        private double[]? GetXDataForDisplay(TimeValueList dataStream,
                                            TimeSeriesDefinition dataStreamDefinition)
        {
            if (XAxisTimeValueList == null || XAxisDataStreamDefinition == null)
            {

                double[] xArray = new double[dataStream.Length];
                for (int i = 0; i < dataStream.Length; i++)
                {
                    xArray[i] = i;
                }
                return xArray;
            }
            else
            {
                return GetYDataForDisplay(XAxisTimeValueList, XAxisDataStreamDefinition);
            }
        }

        private void SetXAxisLimits()
        {
            if (XAxisTimeValueList == null || XAxisDataStreamDefinition == null)
            {
                formsPlot1.Plot.XAxis.Dims.ResetLimits();
            }
            else
            {
                formsPlot1.Plot.XAxis.Dims.SetAxis(XAxisTimeValueList.Values.Min(), XAxisTimeValueList.Values.Max());
            }
        }

        private void SetYAxisLimits()
        {
            int offset = Options.Instance.SpaceOutOffset;
            double reduceMin = 1.0 - (offset / 100.0);
            double increaseMax = 1.0 + (offset / 100.0);
            for (int i = 0; i < axisIndex; i++)
            {
                Axis yAxis;
                if (i == 0)
                {
                    yAxis = formsPlot1.Plot.YAxis;
                }
                else
                {
                    yAxis = CurrentAxis[i - 1];
                }
                double min = YAxisMinMax[i].Item1 * reduceMin; // 0.95; //add some margins so they don't overlap
                double max = YAxisMinMax[i].Item2 * increaseMax; // 1.05;
                double diff = max - min;
                double newMin = min - diff * i;
                double newMax = min + diff * (axisIndex - i); //only one axis, then (min + diff * 1 == max)
                //formsPlot1.Plot.SetAxisLimits(yMax: newMax, yMin: newMin, yAxisIndex: i);
                //formsPlot1.Plot.SetAxisLimits(yMax: i+1, yMin: i, yAxisIndex: i);

                //yAxis.Dims.SetAxis(i, i + 1);
                if (Options.Instance.SpaceOutActivityGraphs)
                {
                    yAxis.Dims.SetAxis(newMin, newMax);
                }
                else
                {
                    yAxis.Dims.SetAxis(min, max);
                }
            }

        }

        static string CustomTickFormatterForPace(double position)
        {
            if (position == 0)
                return "-:--";

            double minPerKm = 16.666666667 / position * 60;
            double min = Math.Floor(minPerKm / 60.0);
            double sec = Math.Floor(minPerKm % 60);

            return $"{min}:{sec:00}";
        }

        static string customTickFormatterForTime(double position)
        {
            if (position < 0)
                return "";
            DateTime dateTime1 = new DateTime();
            DateTime dateTime2 = dateTime1.AddSeconds(position);

            return $"{dateTime2:H:mm:ss}";
        }

        private double[] Smooth(double[] input, TimeSeriesDefinition dataStreamDefinition)
        {
            double[] smoothedData;
            if (dataStreamDefinition.Smoothing == TimeSeriesDefinition.SmoothingType.AverageWindow)
                smoothedData = TimeSeriesUtils.WindowSmoothed(input, dataStreamDefinition.SmoothingWindow);
            else if (dataStreamDefinition.Smoothing == TimeSeriesDefinition.SmoothingType.SimpleExponential)
                smoothedData = TimeSeriesUtils.SimpleExponentialSmoothed(input, dataStreamDefinition.SmoothingWindow);
            else
                smoothedData = input;

            return smoothedData;
        }

        private void LockAxisForZoom(object? sender, MouseEventArgs e)
        {
            var limits = formsPlot1.Plot.GetAxisLimits();

            (double x, double y) = formsPlot1.GetMouseCoordinates();

            bool isOverXAxis = limits.YMin > y; //below the bottom of the Y axis
            bool isOverYAxis = limits.XMin > x; //to the left of the X axis

            //lock the axis from zooming if the mouse is over the other axis
            formsPlot1.Plot.YAxis.LockLimits(isOverXAxis);
            formsPlot1.Plot.XAxis.LockLimits(isOverYAxis);
            foreach (Axis axis in CurrentAxis) { axis.LockLimits(isOverXAxis); }

        }
        private void formsPlot1_MouseMove(object sender, MouseEventArgs e)
        {

            LockAxisForZoom(sender, e);

            (double cx, double cy) = formsPlot1.GetMouseCoordinates();


            if (MouseCrosshair != null && CurrentlyDisplayedActivity != null)
            {
                StringBuilder stringBuilder = new StringBuilder();


                MouseCrosshair.X = cx;
                MouseCrosshair.Y = cy;
                formsPlot1.Refresh(); //TODO: can we just refresh the cross hairs?

                if (cx < 0)
                {
                    positionLabel.Text = "N/A";
                    return; //happens when cursor moves too far left
                }
                int time = (int)cx;
                DateTime dateTime = new DateTime();
                dateTime = dateTime.AddSeconds(cx);
                stringBuilder.Append($"Position {dateTime.ToShortTimeString()}");
                foreach (var kvp in CurrentlyDisplayedSeries)
                {
                    TimeSeriesDefinition dataStreamDefinition = kvp.TimeSeriesDefinition;
                    TimeValueList data = kvp.TimeValueList;
                    string name = kvp.TimeSeriesBase.Name;
                    if (time < data.Values.Length - 1)
                    {
                        float value = data.Values[time];
                        string representation = dataStreamDefinition.Format(value);
                        stringBuilder.Append($", {name}: {representation}");
                    }
                    else
                    {
                        stringBuilder.Append($", {name}: Past End");
                    }
                }

                positionLabel.Text = stringBuilder.ToString();

            }
        }


        private void objectListViewTimeSeries_SubItemChecking(object sender, SubItemCheckingEventArgs e)
        {
            TimeSeriesDefinition row = (TimeSeriesDefinition)e.RowObject;
            row.ShowReportGraph = (e.NewValue == CheckState.Checked);
            //objectListView1.RefreshObject(e.RowObject);
            UpdateTimeSeriesGraph(CurrentlyDisplayedActivity);
        }

        private void objectListViewTimeSeries_CellEditFinished(object sender, CellEditEventArgs e)
        {
            UpdateTimeSeriesGraph(CurrentlyDisplayedActivity);
        }

        private void objectListViewTimeSeries_CellEditValidating(object sender, CellEditEventArgs e)
        {
            UpdateTimeSeriesGraph(CurrentlyDisplayedActivity);
        }

        private void objectListViewTimeSeries_ItemsChanged(object sender, ItemsChangedEventArgs e)
        {
        }

        private void buttonDraw_Click(object sender, EventArgs e)
        {
            UpdateTimeSeriesGraph(CurrentlyDisplayedActivity);
        }

        private void objectListViewTimeSeries_CellClick(object sender, CellClickEventArgs e)
        {
            UpdateTimeSeriesGraph(CurrentlyDisplayedActivity);
        }
    }
}
