using FellrnrTrainingAnalysis.Action;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;
using Microsoft.VisualBasic;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System.Reflection;
using System.Text;

namespace FellrnrTrainingAnalysis
{
    public partial class ActivityReport : UserControl
    {
        public ActivityReport()
        {
            InitializeComponent();
            typeof(DataGridView).InvokeMember(
               "DoubleBuffered",
               BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
               null,
               activityDataGridView,
               new object[] { true });
        }



        Database? Database = null;
        FilterActivities? FilterActivities = null;
        int PageSize = 25;
        bool FirstTime = true;

        public delegate void UpdateViewsEventHandler();
        public event UpdateViewsEventHandler? UpdateViews;


        public void UpdateReport(Database database, FilterActivities filterActivities)
        {
            Logging.Instance.TraceEntry("UpdateReport");
            Database = database;
            FilterActivities = filterActivities;
            if (FirstTime)
                CreateRightClickMenus();
            FirstTime = false;
            UpdateReport();
            Logging.Instance.TraceLeave();
        }
        private void UpdateReport()
        {
            if (Database == null || FilterActivities == null) { return; }
            Logging.Instance.ResetAndStartTimer("UpdateReport-private");
            Logging.Instance.TraceEntry("UpdateReport");

            IgnoreSelectionChanged = true;
            activityDataGridView.Rows.Clear();
            IgnoreSelectionChanged = false;
            Logging.Instance.Log(string.Format("ActivityReport.UpdateReport rows.clear took {0}", Logging.Instance.GetAndResetTime("UpdateReport-private")));

            //Database.CurrentAthlete.
            //IReadOnlyCollection<string> activityFieldNames = Database.CurrentAthlete.ActivityFieldNames;
            List<ActivityDatumMetadata>? columnMetadata = ActivityDatumMetadata.GetDefinitions();
            if (columnMetadata == null)
            {
                Logging.Instance.TraceLeave();
                return;
            }
            activityDataGridView.ColumnCount = ActivityDatumMetadata.LastPositionInReport();
            if (activityDataGridView.ColumnCount == 0)
            {
                Logging.Instance.TraceLeave();
                return;
            }
            activityDataGridView.ColumnHeadersVisible = true;

            // Set the column header style.
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();

            columnHeaderStyle.BackColor = Color.Beige;
            columnHeaderStyle.Font = new Font("Verdana", 8, FontStyle.Regular);
            activityDataGridView.ColumnHeadersDefaultCellStyle = columnHeaderStyle;

            Logging.Instance.Log(string.Format("ActivityReport.UpdateReport setup took {0}", Logging.Instance.GetAndResetTime("UpdateReport-private")));
            // Set the column header names.
            //foreach (string s in activityFieldNames)
            foreach (ActivityDatumMetadata activityDatumMetadata in columnMetadata)
            {
                //ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(s);
                if (activityDatumMetadata != null && activityDatumMetadata.PositionInReport != null)
                {
                    int positionInReport = (int)activityDatumMetadata.PositionInReport;
                    DataGridViewColumn dataGridViewColumn = activityDataGridView.Columns[positionInReport];
                    dataGridViewColumn.Name = activityDatumMetadata.Title;

                    if (UI.DatumFormatter.RightJustify(activityDatumMetadata))
                        dataGridViewColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                    if (activityDatumMetadata.Invisible.HasValue && activityDatumMetadata.Invisible.Value) //most readable way of checking nullable bool? 
                        dataGridViewColumn.Visible = false;

                    if (activityDatumMetadata.ColumnSize != null)
                    {
                        dataGridViewColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        dataGridViewColumn.Width = (int)activityDatumMetadata.ColumnSize;
                    }
                    else
                    {
                        dataGridViewColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }

                    AddRightClicks(dataGridViewColumn);

                }
            }
            Logging.Instance.Log(string.Format("ActivityReport.UpdateReport set column metadata took {0}", Logging.Instance.GetAndResetTime("UpdateReport-private")));

            /*
            ReadOnlyDictionary<DateTime, Activity> activities = database.CurrentAthlete.ActivitiesByDateTime;
            IEnumerable<KeyValuePair<DateTime, Model.Activity>> enumerator = activities.Skip(Math.Max(0, activities.Count() - n));
            foreach (KeyValuePair<DateTime, Model.Activity> kvp in enumerator)
                */
            List<Activity> activities = FilterActivities.GetActivities(Database);
            Logging.Instance.Log(string.Format("ActivityReport.UpdateReport get activities took {0}", Logging.Instance.GetAndResetTime("UpdateReport-private")));
            labelTotalRows.Text = $"Total activities {activities.Count}";
            IEnumerable<Model.Activity> enumerator;
            if (PageSize < 0)
            {
                enumerator = activities;
            }
            else
            {
                enumerator = activities.Skip(Math.Max(0, activities.Count() - PageSize));
            }

            //List<DataGridViewRow> dataGridViewRows = new List<DataGridViewRow>();
            activityDataGridView.SuspendLayout();
            IgnoreSelectionChanged = true;
            foreach (Model.Activity activity in enumerator)
            {
                string[] row = new string[ActivityDatumMetadata.LastPositionInReport()];
                //foreach (string fieldname in activityFieldNames)
                foreach (ActivityDatumMetadata activityDatumMetadata in columnMetadata)
                {
                    //ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(fieldname);
                    if (activityDatumMetadata != null && activityDatumMetadata.PositionInReport != null)
                    {
                        int positionInReport = (int)activityDatumMetadata.PositionInReport;
                        string formated = "";
                        Extensible extensible = activity;

                        if (activityDatumMetadata.Level == ActivityDatumMetadata.LevelType.Day)
                        {
                            extensible = Database.CurrentAthlete.Days[activity.StartDateNoTimeLocal!.Value];
                        }
                        if (extensible.HasNamedDatum(activityDatumMetadata.Name))
                        {
                            //row[activityDatumMetadata.PositionInReport] = activity.GetNamedDatumForDisplay(fieldname);
                            formated = UI.DatumFormatter.FormatForGrid(extensible.GetNamedDatum(activityDatumMetadata.Name), activityDatumMetadata);

                        }
                        row[positionInReport] = formated;

                    }
                }
                activityDataGridView.Rows.Add(row);
                //dataGridViewRows.Add(new DataGridViewRow(row));
            }
            activityDataGridView.ResumeLayout();
            //activityDataGridView.Rows.AddRange(dataGridViewRows.ToArray());
            Logging.Instance.Log(string.Format("UpdateReport add rows took {0}", Logging.Instance.GetAndResetTime("UpdateReport-private")));
            if (activityDataGridView.Rows.Count > 0)
            {
                activityDataGridView.FirstDisplayedScrollingRowIndex = activityDataGridView.RowCount - 1; //this changes the selected row to be zero
                activityDataGridView.Rows[activityDataGridView.Rows.Count - 1].Selected = true;
            }

            IgnoreSelectionChanged = false;
            UpdateSelectedRow(); //now manually update
            Logging.Instance.TraceLeave();
        }

        private List<Axis> CurrentAxis { get; set; } = new List<Axis>();

        private List<string> AxisNames { get; set; } = new List<string>();
        private List<Tuple<double, double>> YAxisMinMax { get; set; } = new List<Tuple<double, double>>();
        private int axisIndex = 0;
        private const int AXIS_OFFSET = 3;

        private Crosshair? MouseCrosshair { get; set; }

        //DataGridViewSelectedRowCollection? currentSelectedRowCollection = null;


        private bool IgnoreSelectionChanged = false;
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (IgnoreSelectionChanged)
                return;
            Logging.Instance.TraceEntry("dataGridView1_SelectionChanged");

            //hmmmm. I wrote all this optimisation code which never worked as there's no update to currentSelectedRowCollection. Commenting out for now.
            /*
            DataGridViewSelectedRowCollection dataGridViewSelectedRowCollection = activityDataGridView.SelectedRows;
            bool match = true;
            if (currentSelectedRowCollection != null &&
                dataGridViewSelectedRowCollection.Count == currentSelectedRowCollection.Count)
            {
                for (int i = 0; i < dataGridViewSelectedRowCollection.Count; i++)
                {
                    if (dataGridViewSelectedRowCollection[i] != currentSelectedRowCollection[i])
                    {
                        match = false; break;
                    }
                }
            }
            if(!match)
                UpdateSelectedRow();
            */

            UpdateSelectedRow();
            Logging.Instance.TraceLeave();
        }

        private void UpdateActivityDisplay()
        {
            Logging.Instance.TraceEntry("UpdateActivityDisplay");
            Model.Activity? activity = null;
            DataGridViewSelectedRowCollection dataGridViewSelectedRowCollection = activityDataGridView.SelectedRows;
            if (dataGridViewSelectedRowCollection.Count > 0)
            {
                DataGridViewRow row = dataGridViewSelectedRowCollection[0];
                activity = GetActivityForRow(row);
            }
            activityData1.DisplayActivity(Database!.CurrentAthlete, activity); //if we've got here, we have to have a database with an athlete
            activityMap1.DisplayActivity(activity, Database!.Hills);
            Logging.Instance.TraceLeave();
        }

        public void UpdateSelectedRow()
        {
            Logging.Instance.TraceEntry("UpdateSelectedRow");
            UpdateTimeSeriesGraph();
            UpdateActivityDisplay();
            Logging.Instance.TraceLeave();
        }


        private Activity? CurrentlyDisplayedActivity = null;
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
                    foreach (KeyValuePair<string, TimeSeriesBase> kvp in activity.TimeSeries)
                    {
                        if (kvp.Value.IsValid())
                            DisplayTimeSeries(activity, kvp);
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

        private Model.Activity? GetActivityForRow(DataGridViewRow row)
        {
            if (Database == null)
                return null;

            if (row != null)
            {
                var index = activityDataGridView.Columns[Activity.TagPrimarykey]?.Index;
                if (index != null && index != -1)
                {
                    string primarykey = (string)row.Cells[index.Value].Value;
                    if (primarykey.Contains(" "))
                        primarykey = primarykey.Substring(0, primarykey.IndexOf(" "));
                    if (Database.CurrentAthlete.Activities.ContainsKey(primarykey)) //should never happen unless we've turned on debugging to add extra data to the primary key column of the table. 
                    {
                        Model.Activity activity = Database.CurrentAthlete.Activities[primarykey];
                        return activity;
                    }
                }
            }
            return null;

        }

        const double MINPACE = 0.3; //0.3 is 55:30 min/km. Anything slower can be considered not moving to make the graph work, otherwise min/km values tend towards infinity
        private void DisplayTimeSeries(Model.Activity activity, KeyValuePair<string, TimeSeriesBase> kvp)
        {
            string timeSeriesName = kvp.Key;
            TimeSeriesBase timeSeriesBase = kvp.Value;

            TimeSeriesDefinition? dataStreamDefinition = TimeSeriesDefinition.FindTimeSeriesDefinition(timeSeriesName);
            if (dataStreamDefinition == null || !dataStreamDefinition.ShowReportGraph)
                return;

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
            double[] xArray;
            double[] yArraySmoothed;

            TimeValueList? dataStream = activityTimeSeriesdataStream.GetData(forceCount: 0, forceJustMe: false);
            if (dataStream == null)
            {
                return null;
            }
            xArray = Array.ConvertAll(dataStream.Times, x => (double)x);
            double[] yArrayRaw = Array.ConvertAll(dataStream.Values, x => (double)x);

            yArraySmoothed = Smooth(yArrayRaw, dataStreamDefinition);
            for (int i = 0; i < xArray.Length; i++)
            {
                double x = xArray[i];
                bool norm = double.IsNormal(x);
                if (x != 0 && !double.IsNormal(x))
                {
                    Logging.Instance.Log(string.Format("invalid X value {0} at offset {1} of {2}", xArray[i], i, timeSeriesName));
                    return null;
                }
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



        private void activityDataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            const int ArbitraryWrapLiength = 50;
            if (e == null || e.Value == null || e.Value is not string)
                return;
            string s = (string)e.Value;
            if (s.Length > ArbitraryWrapLiength)
            {
                DataGridViewCell cell = this.activityDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.ToolTipText = Utils.Misc.WordWrap(s, ArbitraryWrapLiength, " ".ToCharArray());
            }

        }
    }
}
