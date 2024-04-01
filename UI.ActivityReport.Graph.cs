using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis
{
    partial class ActivityReport
    {
        private List<Axis> CurrentAxis { get; set; } = new List<Axis>();

        private List<string> AxisNames { get; set; } = new List<string>();
        private List<Tuple<double, double>> YAxisMinMax { get; set; } = new List<Tuple<double, double>>();
        private int axisIndex = 0;
        private const int AXIS_OFFSET = 3;

        private Crosshair? MouseCrosshair { get; set; }

        const double MINPACE = 0.3; //0.3 is 55:30 min/km. Anything slower can be considered not moving to make the graph work, otherwise min/km values tend towards infinity


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

        private void UpdateTimeSeriesGraph()
        {
            Logging.Instance.TraceEntry("UpdateTimeSeriesGraph");
            if (Database == null)
                return;

            DataGridViewSelectedRowCollection dataGridViewSelectedRowCollection = activityDataGridView.SelectedRows;
            formsPlot1.Plot.Clear();

            //formsPlot1.Plot.GetSettings().Axes.Clear();
            foreach (Axis axis in CurrentAxis) { formsPlot1.Plot.RemoveAxis(axis); }
            CurrentAxis.Clear();
            AxisNames.Clear();
            YAxisMinMax.Clear();
            axisIndex = 0;

            if (dataGridViewSelectedRowCollection.Count > 0)
            {
                DataGridViewRow row = dataGridViewSelectedRowCollection[0];
                Model.Activity? activity = GetActivityForRow(row);
                if (activity != null)
                {
                    CurrentlyDisplayedActivity = activity;
                    CurrentlyDisplayedSeries.Clear();
                    foreach (KeyValuePair<string, TimeSeriesBase> kvp in activity.TimeSeries)
                    {
                        DisplayTimeSeries(activity, kvp.Value);
                    }

                    SetAxis();
                }
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
            if (dataStreamDefinition == null || !dataStreamDefinition.ShowReportGraph)
                return;

            if (!timeSeriesBase.IsValid())
                return;

            TimeValueList? dataStream = timeSeriesBase.GetData();
            if (dataStream == null)
            {
                return;
            }


            CurrentlyDisplayedSeries.Add(new CurrentlyDisplayed(timeSeriesBase, dataStream, dataStreamDefinition)); //cache for mouse movement

            double[] xArray, yArraySmoothed;
            Tuple<double[], double[]>? xyData = GetTimeSeriesForDisplay(activity, timeSeriesName, timeSeriesBase, dataStreamDefinition);
            if (xyData == null)
                return;
            xArray = xyData.Item1;
            yArraySmoothed = xyData.Item2;

            var scatterGraph = formsPlot1.Plot.AddScatter(xArray, yArraySmoothed, color: dataStreamDefinition.GetColor());
            scatterGraph.MarkerShape = MarkerShape.none;
            scatterGraph.LineWidth = 2;
            YAxisMinMax.Add(new Tuple<double, double>(yArraySmoothed.Min(), yArraySmoothed.Max()));


            formsPlot1.Plot.XAxis.TickLabelFormat(customTickFormatterForTime);
            Axis yAxis;
            int axisId;
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
            AxisNames.Add(dataStreamDefinition.DisplayTitle);

            if (dataStreamDefinition.DisplayUnits == TimeSeriesDefinition.DisplayUnitsType.Pace && !Options.Instance.DebugDisableTimeAxis)
            {
                yAxis.TickLabelFormat(customTickFormatterForPace);
            }
            else
            {
                yAxis.TickLabelFormat(null); //reset as axis are reused
            }
            yAxis.Label(dataStreamDefinition.DisplayTitle);
            yAxis.Color(scatterGraph.Color);
            axisIndex++;

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


            return;
        }

        private Tuple<double[], double[]>? GetTimeSeriesForDisplay(Activity activity, string timeSeriesName, TimeSeriesBase activityTimeSeriesdataStream, TimeSeriesDefinition dataStreamDefinition)
        {
            double[] yArraySmoothed;

            TimeValueList? dataStream = activityTimeSeriesdataStream.GetData(forceCount: 0, forceJustMe: false);
            if (dataStream == null)
            {
                return null;
            }
            //xArray = Array.ConvertAll(dataStream.Times, x => (double)x);
            double[] yArrayRaw = Array.ConvertAll(dataStream.Values, x => (double)x);

            yArraySmoothed = Smooth(yArrayRaw, dataStreamDefinition);

            double[] xArray = new double[dataStream.Length];
            for (int i = 0; i < dataStream.Length; i++)
            {
                xArray[i] = i;
            }
            for (int i = 0; i < yArraySmoothed.Length; i++)
            {
                double y = yArraySmoothed[i];
                if (y != 0 && !double.IsNormal(y))
                {
                    Logging.Instance.Log(string.Format("invalid Y value {0} at offset {1} of {2}", yArraySmoothed[i], i, timeSeriesName));
                    return null;
                }
            }

            return new Tuple<double[], double[]>(xArray, yArraySmoothed);
        }

        private void SetAxis()
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

        static string customTickFormatterForPace(double position)
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

        private void formsPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseCrosshair != null && CurrentlyDisplayedActivity != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                (double cx, double cy) = formsPlot1.GetMouseCoordinates();
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

                /*
                foreach (KeyValuePair<string, TimeSeriesBase> kvp in CurrentlyDisplayedActivity.TimeSeries)
                {
                    TimeSeriesDefinition? dataStreamDefinition = TimeSeriesDefinition.FindTimeSeriesDefinition(kvp.Key);
                    if (dataStreamDefinition != null && dataStreamDefinition.ShowReportGraph)
                    {
                        TimeSeriesBase dataStreamBase = kvp.Value;
                        TimeValueList? data = dataStreamBase.GetData(forceCount: 0, forceJustMe: false);
                        if (data != null)
                        {
                            uint[] times = data.Times;
                            int offset = Array.BinarySearch(times, time);
                            if (offset < 0)
                                offset = ~offset;
                            if (offset >= data.Values.Length)
                                offset = data.Values.Length - 1;

                            float value = data.Values[offset];
                            string representation = dataStreamDefinition.Format(value);
                            stringBuilder.Append($", {kvp.Key}: {representation}");
                        }
                    }
                }
                 */
                positionLabel.Text = stringBuilder.ToString();

            }
        }


    }
}
