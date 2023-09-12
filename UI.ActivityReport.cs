using FellrnrTrainingAnalysis.Model;
using ScottPlot.Renderable;
using ScottPlot;
using static FellrnrTrainingAnalysis.Utils.Utils;
using System.Text;
using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis
{
    public partial class ActivityReport : UserControl
    {
        public ActivityReport()
        {
            InitializeComponent();
            CreateRightClickMenus();
        }



        Database? Database = null;
        FilterActivities? FilterActivities = null;
        int PageSize = 25;

        public delegate void UpdateViewsEventHandler();
        public event UpdateViewsEventHandler? UpdateViews;


        public void UpdateReport(Database database, FilterActivities filterActivities)
        {
            Database = database;
            FilterActivities = filterActivities;
            UpdateReport();
        }
        private void UpdateReport()
        {
            if(Database == null || FilterActivities == null) { return; }

            activityDataGridView.Rows.Clear();
            //Database.CurrentAthlete.
            IReadOnlyCollection<string> activityFieldNames = Database.CurrentAthlete.ActivityFieldNames;
            activityDataGridView.ColumnCount = ActivityDatumMetadata.LastPositionInReport();
            if (activityDataGridView.ColumnCount == 0)
                return;
            activityDataGridView.ColumnHeadersVisible = true;

            // Set the column header style.
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();

            columnHeaderStyle.BackColor = Color.Beige;
            columnHeaderStyle.Font = new Font("Verdana", 8, FontStyle.Regular);
            activityDataGridView.ColumnHeadersDefaultCellStyle = columnHeaderStyle;

            // Set the column header names.
            foreach (string s in activityFieldNames)
            {
                ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(s);
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

            /*
            ReadOnlyDictionary<DateTime, Activity> activities = database.CurrentAthlete.ActivitiesByDateTime;
            IEnumerable<KeyValuePair<DateTime, Model.Activity>> enumerator = activities.Skip(Math.Max(0, activities.Count() - n));
            foreach (KeyValuePair<DateTime, Model.Activity> kvp in enumerator)
                */
            List<Activity> activities = FilterActivities.GetActivities(Database);
            labelTotalRows.Text= $"Total activities {activities.Count}";
            IEnumerable<Model.Activity> enumerator;
            if (PageSize < 0)
            {
                enumerator = activities;
            }
            else
            { 
                enumerator = activities.Skip(Math.Max(0, activities.Count() - PageSize));
            }
            foreach (Model.Activity activity in enumerator)
            {
                string[] row = new string[activityFieldNames.Count];
                foreach (string fieldname in activityFieldNames)
                {
                    ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(fieldname);
                    if (activityDatumMetadata != null && activityDatumMetadata.PositionInReport != null)
                    {
                        int positionInReport = (int)activityDatumMetadata.PositionInReport;
                        if (activity.HasNamedDatum(fieldname))
                        {
                            //row[activityDatumMetadata.PositionInReport] = activity.GetNamedDatumForDisplay(fieldname);
                            row[positionInReport] = UI.DatumFormatter.FormatForGrid(activity.GetNamedDatum(fieldname), activityDatumMetadata);
                        }
                        else
                        {
                            row[positionInReport] = "";
                        }
                    }
                }
                activityDataGridView.Rows.Add(row);
            }
            if (activityDataGridView.Rows.Count > 0)
            {
                activityDataGridView.FirstDisplayedScrollingRowIndex = activityDataGridView.RowCount - 1; //this changes the selected row to be zero
                activityDataGridView.Rows[activityDataGridView.Rows.Count - 1].Selected = true;
            }
        }

        private List<Axis> CurrentAxis { get; set; } = new List<Axis>();
        private List<Tuple<double, double>> YAxisMinMax { get; set; } = new List<Tuple<double, double>>();
        private int axisIndex = 0;
        private const int AXIS_OFFSET= 3;


        DataGridViewSelectedRowCollection? currentSelectedRowCollection = null;

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection dataGridViewSelectedRowCollection = activityDataGridView.SelectedRows;
            if (currentSelectedRowCollection != null && 
                dataGridViewSelectedRowCollection.Count == currentSelectedRowCollection.Count) 
            {
                bool match = true;
                for(int i=0;i < dataGridViewSelectedRowCollection.Count; i++)
                {
                    if (dataGridViewSelectedRowCollection[i] != currentSelectedRowCollection[i])
                    {
                        match = false; break;
                    }
                }
                if(match) 
                    return;

            }
            UpdateSelectedRow();
        }

        private void UpdateActivityDisplay()
        {
            Model.Activity? activity = null;
            DataGridViewSelectedRowCollection dataGridViewSelectedRowCollection = activityDataGridView.SelectedRows;
            if (dataGridViewSelectedRowCollection.Count > 0)
            {
                DataGridViewRow row = dataGridViewSelectedRowCollection[0];
                activity = GetActivityForRow(row);
            }
            activityData1.DisplayActivity(Database!.CurrentAthlete, activity); //if we've got here, we have to have a database with an athlete
            activityMap1.DisplayActivity(activity, Database!.Hills);
        }

        public void UpdateSelectedRow()
        {
            UpdateDataStreamGraph();
            UpdateActivityDisplay();
        }

        private void UpdateDataStreamGraph()
        {
            if(Database == null)
                return;

            DataGridViewSelectedRowCollection dataGridViewSelectedRowCollection = activityDataGridView.SelectedRows;
            formsPlot1.Plot.Clear();

            //formsPlot1.Plot.GetSettings().Axes.Clear();
            foreach (Axis axis in CurrentAxis) { formsPlot1.Plot.RemoveAxis(axis); }
            CurrentAxis.Clear();
            YAxisMinMax.Clear();
            axisIndex = 0;

            if (dataGridViewSelectedRowCollection.Count > 0)
            {
                DataGridViewRow row = dataGridViewSelectedRowCollection[0];
                Model.Activity? activity = GetActivityForRow(row);
                if (activity != null)
                {
                    foreach (KeyValuePair<string, IDataStream> kvp in activity.TimeSeries)
                    {
                        DisplayTimeSeries(activity, kvp);
                    }

                    SetAxis();
                }

            }
            formsPlot1.Refresh();
        }

        private Model.Activity? GetActivityForRow(DataGridViewRow row)
        {
            if (Database == null)
                return null;

            if (row != null)
            {
                var index = activityDataGridView.Columns[Activity.PrimarykeyTag]?.Index;
                if (index != null && index != -1)
                {
                    string primarykey = (string)row.Cells[index.Value].Value;
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
        private void DisplayTimeSeries(Model.Activity activity, KeyValuePair<string, IDataStream> kvp)
        {
            string timeSeriesName = kvp.Key;

            DataStreamDefinition? dataStreamDefinition;
            dataStreamDefinition = DataStreamDefinition.FindDataStreamDefinition(timeSeriesName);
            if (dataStreamDefinition == null || !dataStreamDefinition.ShowReportGraph)
                return;

            double[] xArray, yArraySmoothed;
            Tuple<double[], double[]>? xyData = GetDataStreamForDisplay(activity, kvp, dataStreamDefinition);
            if (xyData == null)
                return;
            xArray = xyData.Item1;
            yArraySmoothed = xyData.Item2;

            var scatterGraph = formsPlot1.Plot.AddScatter(xArray, yArraySmoothed);
            scatterGraph.MarkerShape = MarkerShape.none;
            scatterGraph.LineWidth = 2;
            YAxisMinMax.Add(new Tuple<double, double>(yArraySmoothed.Min(), yArraySmoothed.Max()));

            //formsPlot1.Plot.YAxis.TickLabelFormat
            Axis yAxis;
            if (axisIndex == 0)
            {
                yAxis = formsPlot1.Plot.YAxis;
                scatterGraph.YAxisIndex = 0;
            }
            else
            {
                yAxis = formsPlot1.Plot.AddAxis(Edge.Left);
                yAxis.AxisIndex = axisIndex + AXIS_OFFSET; //there are four default indexes we have to skip

                scatterGraph.YAxisIndex = yAxis.AxisIndex;
                CurrentAxis.Add(yAxis);
            }

            if (dataStreamDefinition.DisplayUnits == DataStreamDefinition.DisplayUnitsType.Pace && !Options.Instance.DebugDisableTimeAxis)
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
            return;
        }

        private Tuple<double[], double[]>? GetDataStreamForDisplay(Activity activity, KeyValuePair<string, IDataStream> kvp, DataStreamDefinition dataStreamDefinition)
        {
            string timeSeriesName = kvp.Key;
            double[] xArray;
            double[] yArraySmoothed;

            Model.IDataStream activityDataStreamdataStream = kvp.Value;
            Tuple<uint[], float[]>? dataStream = activityDataStreamdataStream.GetData(activity);
            if (dataStream == null)
            {
                return null;
            }
            xArray = Array.ConvertAll(dataStream.Item1, x => (double)x);
            double[] yArrayRaw = Array.ConvertAll(dataStream.Item2, x => (double)x);

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
            for(int i = 0; i < axisIndex; i++)
            {
                Axis yAxis;
                if (i == 0)
                {
                    yAxis = formsPlot1.Plot.YAxis;
                }
                else
                {
                    yAxis = CurrentAxis[i-1];
                }
                double min = YAxisMinMax[i].Item1 * 0.95; //add some margins so they don't overlap
                double max = YAxisMinMax[i].Item2 * 1.05;
                double diff = max - min;
                double newMin = min - diff * i;
                double newMax = min + diff * (axisIndex-i); //only one axis, then (min + diff * 1 == max)
                //formsPlot1.Plot.SetAxisLimits(yMax: newMax, yMin: newMin, yAxisIndex: i);
                //formsPlot1.Plot.SetAxisLimits(yMax: i+1, yMin: i, yAxisIndex: i);

                //yAxis.Dims.SetAxis(i, i + 1);
                yAxis.Dims.SetAxis(newMin, newMax);
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

        private double[] Smooth(double[] input, DataStreamDefinition dataStreamDefinition)
        {
            double[] smoothedElevationChanges;
            if (dataStreamDefinition.Smoothing == DataStreamDefinition.SmoothingType.AverageWindow)
                smoothedElevationChanges = TimeSeries.WindowSmoothed(input, dataStreamDefinition.SmoothingWindow);
            else if (dataStreamDefinition.Smoothing == DataStreamDefinition.SmoothingType.SimpleExponential)
                smoothedElevationChanges = TimeSeries.SimpleExponentialSmoothed(input, dataStreamDefinition.SmoothingWindow);
            else
                smoothedElevationChanges = input;

            return smoothedElevationChanges;
        }



        //    _____  _       _     _      _____ _ _      _    
        //   |  __ \(_)     | |   | |    / ____| (_)    | |   
        //   | |__) |_  __ _| |__ | |_  | |    | |_  ___| | __
        //   |  _  /| |/ _` | '_ \| __| | |    | | |/ __| |/ /
        //   | | \ \| | (_| | | | | |_  | |____| | | (__|   < 
        //   |_|  \_\_|\__, |_| |_|\__|  \_____|_|_|\___|_|\_\
        //              __/ |                                 
        //             |___/                                  

        private DataGridViewCellEventArgs? mouseLocation;
        ContextMenuStrip strip = new ContextMenuStrip();
        List<ToolStripMenuItem> toolStripMenuItems = new List<ToolStripMenuItem>();

        private void CreateRightClickMenus()
        {
            AddContextMenu("Highlight", new EventHandler(toolStripItem1_Click_highlight));
            AddContextMenu("Recalculate", new EventHandler(toolStripItem1_Click_recalculate));
            AddContextMenu("Recalculate Hills", new EventHandler(toolStripItem1_Click_recalculateHills));
            AddContextMenu("Reread FIT file", new EventHandler(toolStripItem1_Click_rereadFit));
            AddContextMenu("Open in Strava...", new EventHandler(toolStripItem1_Click_openStrava));
            AddContextMenu("Open File (system viewer)...", new EventHandler(toolStripItem1_Click_openFile));
            AddContextMenu("Copy File path", new EventHandler(toolStripItem1_Click_copyFitFile));
            AddContextMenu("Open in Garmin...", new EventHandler(toolStripItem1_Click_openGarmin));
            AddContextMenu("Scan For Data Quality Issues...", new EventHandler(toolStripItem1_Click_findDataQuality));
            AddContextMenu("Show Data Quality Issues...", new EventHandler(toolStripItem1_Click_showDataQuality));
            AddContextMenu("Open ALL in Strava...", new EventHandler(toolStripItem1_Click_openAllStrava));
            AddContextMenu("Tag ALL in Strava as #race", new EventHandler(toolStripItem1_Click_tagAllStravaAsRace));


        }

        private void AddContextMenu(string text, EventHandler eventHandler)
        {
            ToolStripMenuItem toolStripItem3 = new ToolStripMenuItem();
            toolStripItem3.Text = text;
            toolStripItem3.Click += eventHandler;
            toolStripMenuItems.Add(toolStripItem3);
        }

        private void AddRightClicks(DataGridViewColumn dataGridViewColumn)
        {
            dataGridViewColumn.ContextMenuStrip = strip;
            dataGridViewColumn.ContextMenuStrip.Items.AddRange(toolStripMenuItems.ToArray());
        }

        // Change the cell's color.
        private void toolStripItem1_Click_highlight(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            activityDataGridView.Rows[mouseLocation.RowIndex].Cells[mouseLocation.ColumnIndex].Style.BackColor = Color.Red;
        }

        private void toolStripItem1_Click_recalculate(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            activity.Recalculate(true);

            UpdateViews?.Invoke();

            MessageBox.Show("Done");
        }
        private void toolStripItem1_Click_recalculateHills(object? sender, EventArgs args)
        {
            if (mouseLocation == null || Database == null || Database.Hills == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            activity.RecalculateHills(Database.Hills, true, true);

            //don't muddy things with a recalculation
            //UpdateViews?.Invoke();

            MessageBox.Show("Done");
        }

        private void toolStripItem1_Click_rereadFit(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            string? filepath = activity.FileFullPath;
            if (filepath == null)
            {
                MessageBox.Show("No file on activity");
                return;
            }

            Action.FitReader fitReader = new Action.FitReader(activity);

            fitReader.ReadFitFromStravaArchive();

            MessageBox.Show("Compelted Reread");
        }

        private void toolStripItem1_Click_openGarmin(object? sender, EventArgs args)
        {

            //can't open activity directly as we don't have the garmin id (the fit file name doesn't match)
            //So search for activities by date
            //https://connect.garmin.com/modern/activities?activityType=running&startDate=2023-07-20&endDate=2023-07-20
            //https://connect.garmin.com/modern/activities?startDate=2023-07-20&endDate=2023-07-20

            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            DateTime? start = activity.StartDateNoTime;

            if(start == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            string date = string.Format("{0:D4}-{1:D2}-{2:d2}", start.Value.Year, start.Value.Month, start.Value.Day);
            string target = $"https://connect.garmin.com/modern/activities?startDate={date}&endDate={date}";
            RunCommand(target);
        }

        private void toolStripItem1_Click_openFile(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            string? filepath = activity.FileFullPath;
            if (filepath == null)
            {
                MessageBox.Show("No file on activity");
                return;
            }
            if (filepath.ToLower().EndsWith(".fit.gz"))
            {
                filepath = filepath.Remove(filepath.Length - 3);
            }
            if(!filepath.ToLower().EndsWith(".fit"))
            {
                if(MessageBox.Show($"File is not fit, {filepath}", "Really?", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }
            }

            RunCommand(filepath);

        }

        private void toolStripItem1_Click_copyFitFile(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            string? filepath = activity.FileFullPath;
            if (filepath == null)
            {
                MessageBox.Show("No file on activity");
                return;
            }
            if (filepath.ToLower().EndsWith(".fit.gz"))
            {
                filepath = filepath.Remove(filepath.Length - 3);
            }
            filepath = filepath.Replace('/', '\\');

            Clipboard.SetText(filepath);
        }

        private void toolStripItem1_Click_openStrava(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
                return;

            string key = activity.PrimaryKey();

            string target = "https://www.strava.com/activities/" + key;
            RunCommand(target);

        }

        private void toolStripItem1_Click_openAllStrava(object? sender, EventArgs args)
        {
            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity == null)
                    return;

                string key = activity.PrimaryKey();
                string target = "https://www.strava.com/activities/" + key;
                RunCommand(target);
            }
        }

        //TODO: generalize the update to strava beyond just tagging as race
        private void toolStripItem1_Click_tagAllStravaAsRace(object? sender, EventArgs args)
        {
            /*
            const string TAG = " #race";
            int success=0, error=0;
            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity == null)
                    return;

                string? name = activity.GetNamedStringDatum("Name");
                if(name != null && !name.Contains(TAG))
                {
                    name = name + TAG;
                    if(StravaApi.Instance.UpdateActivity(activity, name))
                        success++;
                    else 
                        error++;
                }
            }
            MessageBox.Show($"Updated {success} entries, with {error} failures");
            */
            MessageBox.Show($"not doing that again. Left in place for future expansion and generalization");

        }

        private static void RunCommand(string target)
        {
            try
            {
                //System.Diagnostics.Process.Start(target);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = target, UseShellExecute = true }); //use shell execute is false by default now
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void toolStripItem1_Click_showDataQuality(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            if (activity.DataQualityIssues == null || activity.DataQualityIssues.Count == 0)
            {
                MessageBox.Show("No data quality issues");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string s in activity.DataQualityIssues) { stringBuilder.AppendLine(s); }

            MessageBox.Show(stringBuilder.ToString());
        }
        private void toolStripItem1_Click_findDataQuality(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            DataQuality dataQuality = new DataQuality();
            dataQuality.FindBadDataStreams(activity);

            if (activity.DataQualityIssues == null || activity.DataQualityIssues.Count == 0)
            {
                MessageBox.Show("No data quality issues");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string s in activity.DataQualityIssues) { stringBuilder.AppendLine(s); }

            MessageBox.Show(stringBuilder.ToString());
        }

        private void activityDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            mouseLocation = e;
        }

        private void pageSizeComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(pageSizeComboBox1.Text == "All")
            {
                PageSize = -1;
            }
            else
            {
                if (!int.TryParse(pageSizeComboBox1.Text, out PageSize))
                    PageSize = 25;
            }
            UpdateReport();
        }
    }
}
