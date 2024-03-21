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
                        if(kvp.Value.IsValid())
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

            if(timeSeriesBase.Highlights != null)
            {
                foreach(Tuple<uint, uint> area in timeSeriesBase.Highlights)
                {
                    var rect = formsPlot1.Plot.AddRectangle()
                }
            }

            formsPlot1.Plot.XAxis.TickLabelFormat(customTickFormatterForTime);
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
                double min = YAxisMinMax[i].Item1 * 0.95; //add some margins so they don't overlap
                double max = YAxisMinMax[i].Item2 * 1.05;
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
                smoothedData = TimeSeries.WindowSmoothed(input, dataStreamDefinition.SmoothingWindow);
            else if (dataStreamDefinition.Smoothing == TimeSeriesDefinition.SmoothingType.SimpleExponential)
                smoothedData = TimeSeries.SimpleExponentialSmoothed(input, dataStreamDefinition.SmoothingWindow);
            else
                smoothedData = input;

            return smoothedData;
        }



        //Right Click
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
        List<ToolStripItem> rightClickMenuSubMenus = new List<ToolStripItem>();

        private void CreateRightClickMenus()
        {
            AddContextMenu("Open In Strava...", new EventHandler(toolStripItem1_Click_openStrava));
            AddContextMenu("Open ALL In Strava...", new EventHandler(toolStripItem1_Click_openAllStrava));
            AddContextMenu("Open File (system viewer)...", new EventHandler(toolStripItem1_Click_openFile));
            AddContextMenu("Copy File path", new EventHandler(toolStripItem1_Click_copyFitFile));
            AddContextMenu("Open In Garmin...", new EventHandler(toolStripItem1_Click_openGarmin));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Recalculate", new EventHandler(toolStripItem1_Click_recalculate));
            AddContextMenu("Recalculate Hills", new EventHandler(toolStripItem1_Click_recalculateHills));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Highlight", new EventHandler(toolStripItem1_Click_highlight));
            AddContextMenu("Edit Name", new EventHandler(toolStripItem1_Click_editName));
            AddContextMenu("Edit Description", new EventHandler(toolStripItem1_Click_editDescription));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Refresh From Strava", new EventHandler(toolStripItem1_Click_refresh));
            AddContextMenu("Refresh ALL From Strava", new EventHandler(toolStripItem1_Click_refreshAll));
            AddContextMenu("Reread FIT/GPX file", new EventHandler(toolStripItem1_Click_rereadDataFile));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Scan For Data Quality Issues...", new EventHandler(toolStripItem1_Click_findDataQuality));
            AddContextMenu("Show Data Quality Issues...", new EventHandler(toolStripItem1_Click_showDataQuality));
            AddContextMenu("Tag ALL In Strava As...", new EventHandler(toolStripItem1_Click_tagAllStravaAsInput));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddFixSubMenus("Fix This Activity", toolStripItem1_Click_tagStrava);
            AddFixSubMenus("Fix ALL Activities", toolStripItem1_Click_tagAllStrava);
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Write table to CSV...", new EventHandler(toolStripItem1_Click_writeCsv));
            AddContextMenu("Debug Activity...", new EventHandler(toolStripItem1_Click_debugActivity));
            AddContextMenu("Delete Activity...", new EventHandler(toolStripItem1_Click_deleteActivity));
        }

        private void AddContextMenu(string text, EventHandler eventHandler, TagActivities? tagActivities = null)
        {
            ToolStripMenuItem rightClickMenuItem = new ToolStripMenuItem();
            rightClickMenuItem.Text = text;
            rightClickMenuItem.Click += eventHandler;
            rightClickMenuItem.Tag = tagActivities;
            rightClickMenuSubMenus.Add(rightClickMenuItem);
        }



        //start char is ⌗ U+2317
        //middle markers are ༶ (U+0F36)
        //end is ֍ (U+058D)
        private const string ASKME = "ASKME";
        List<TagActivities> SpecialFixActivityTags = new List<TagActivities>() {
            new TagActivities("Replace Start of Altitude", "⌗Altitude༶CopyBack༶10֍"),
        };
        List<string> FixTimeSeriesCommands = new List<string>() { "Delete", "Cap" };
        List<string> FixDatumCommands = new List<string>() { "Override" };
        List<TagActivities> GetFixDatumTags(string command)
        {
            List<TagActivities> tags = new List<TagActivities>();
            if (Database != null)
            {
                foreach (string afn in Database!.CurrentAthlete.ActivityRecordedFieldNames)
                {
                    tags.Add(new TagActivities($"{command} {afn}...", $"⌗{afn}༶Override༶ASKME֍"));
                }
            }
            return tags;
        }
        List<TagActivities> GetFixTimeSeriesTags(string command)
        {
            List<TagActivities> tags = new List<TagActivities>();
            if (Database != null)
            {
                foreach (string tsn in Database!.CurrentAthlete.AllNonVirtualTimeSeriesNames)
                {
                    tags.Add(new TagActivities($"{command} {tsn}", $"⌗{tsn}༶{command}֍"));
                }
            }
            return tags;
        }

        private void AddFixSubMenus(string name, EventHandler eventHandler)
        {
            ToolStripMenuItem rightClickMenu = new ToolStripMenuItem(); //Fix All/Fix
            rightClickMenu.Text = name;
            rightClickMenuSubMenus.Add(rightClickMenu);

            AddFixSubSubMenus("Special", rightClickMenu, eventHandler, SpecialFixActivityTags);
            foreach (string command in FixTimeSeriesCommands)
            {
                List<TagActivities> tags = GetFixTimeSeriesTags(command);
                AddFixSubSubMenus(command, rightClickMenu, eventHandler, tags);
            }
            foreach (string command in FixDatumCommands)
            {
                List<TagActivities> tags = GetFixDatumTags(command);
                AddFixSubSubMenus(command, rightClickMenu, eventHandler, tags);
            }
        }

        private void AddFixSubSubMenus(string subSubMenuName, ToolStripMenuItem rightClickSubMenu, EventHandler eventHandler, List<TagActivities> tagActivities)
        {
            ToolStripMenuItem rightClickSubSubMenu = new ToolStripMenuItem(); //delete, cap, etc.
            rightClickSubSubMenu.Text = subSubMenuName;
            rightClickSubMenu.DropDownItems.Add(rightClickSubSubMenu);

            List<ToolStripMenuItem> toolStripSubMenuItems = new List<ToolStripMenuItem>();
            foreach (TagActivities t in tagActivities)
            {
                ToolStripMenuItem toolStripItem = new ToolStripMenuItem();
                toolStripItem.Text = t.Name;
                toolStripItem.Click += eventHandler;
                toolStripItem.Tag = t;
                toolStripSubMenuItems.Add(toolStripItem);
            }
            rightClickSubSubMenu.DropDownItems.AddRange(toolStripSubMenuItems.ToArray());
        }

        //private void AddFixSubMenusOLDXXXXXXXXXXXXXXXXXXX(string name, EventHandler eventHandler)
        //{
        //    ToolStripMenuItem rightClickMenuItem = new ToolStripMenuItem();
        //    rightClickMenuItem.Text = name;
        //    rightClickMenuSubMenus.Add(rightClickMenuItem);

        //    List<ToolStripMenuItem> toolStripSubMenuItems = new List<ToolStripMenuItem>();
        //    List<TagActivities> tagActivities = GetFixActivityTags();
        //    foreach (TagActivities t in tagActivities)
        //    {
        //        ToolStripMenuItem toolStripItem4 = new ToolStripMenuItem();
        //        toolStripItem4.Text = t.Name;
        //        toolStripItem4.Click += eventHandler;
        //        toolStripItem4.Tag = t;
        //        toolStripSubMenuItems.Add(toolStripItem4);
        //    }
        //    rightClickMenuItem.DropDownItems.AddRange(toolStripSubMenuItems.ToArray());
        //}

        private void AddRightClicks(DataGridViewColumn dataGridViewColumn)
        {
            dataGridViewColumn.ContextMenuStrip = strip;
            if (dataGridViewColumn.ContextMenuStrip.Items.Count > 0) { dataGridViewColumn.ContextMenuStrip.Items.Clear(); }
            dataGridViewColumn.ContextMenuStrip.Items.AddRange(rightClickMenuSubMenus.ToArray());
        }

        private Activity? GetActivity()
        {
            if (mouseLocation == null)
            {
                Logging.Instance.Log($"No mouse location for toolStripItem1_Click_recalculate");
                return null;
            }

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return null;
            }

            return activity;
        }

        // Change the cell's color.
        private void toolStripItem1_Click_highlight(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            activityDataGridView.Rows[mouseLocation.RowIndex].Cells[mouseLocation.ColumnIndex].Style.BackColor = Color.Red;
        }


        private void toolStripItem1_Click_editName(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(activity.Name);
            largeTextDialogForm.ShowDialog();
            if (largeTextDialogForm.Cancelled) return;

            string name = largeTextDialogForm.Value;

            if (!Action.StravaApi.Instance.UpdateActivityDetails(activity, name, null))
            {
                MessageBox.Show("Update Failed");
                return;
            }

            activity.Name = name;
            UpdateViews?.Invoke();
        }

        private void toolStripItem1_Click_editDescription(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(activity.Description);
            largeTextDialogForm.ShowDialog();
            if (largeTextDialogForm.Cancelled) return;


            string description = largeTextDialogForm.Value;
            if (!Action.StravaApi.Instance.UpdateActivityDetails(activity, null, description))
            {
                MessageBox.Show("Update Failed");
                return;
            }
            activity.Description = description;
            UpdateViews?.Invoke();
        }

        private void toolStripItem1_Click_recalculate(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            Logging.Instance.Log($"toolStripItem1_Click_recalculate activity {activity}");
            activity.Recalculate(true);

            Logging.Instance.Log($"toolStripItem1_Click_recalculate update views");
            UpdateViews?.Invoke();

            Logging.Instance.Log($"toolStripItem1_Click_recalculate done");
            MessageBox.Show("Done");
        }
        private void toolStripItem1_Click_recalculateHills(object? sender, EventArgs args)
        {
            if (mouseLocation == null || Database == null || Database.Hills == null)
                return;

            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            activity.RecalculateHills(Database.Hills, true, true);

            //don't muddy things with a recalculation
            //UpdateViews?.Invoke();

            MessageBox.Show("Done");
        }
        private void toolStripItem1_Click_refresh(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null || Database == null) return;

            StravaApi.Instance.RefreshActivity(Database, activity);

            Logging.Instance.Log($"toolStripItem1_Click_recalculate activity {activity}");
            activity.Recalculate(true);

            Logging.Instance.Log($"toolStripItem1_Click_recalculate update views");
            UpdateViews?.Invoke();

            Logging.Instance.Log($"toolStripItem1_Click_recalculate done");
            MessageBox.Show("Done");
        }

        private void toolStripItem1_Click_refreshAll(object? sender, EventArgs args)
        {

            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity == null)
                    return;

                StravaApi.Instance.RefreshActivity(Database!, activity);

                Logging.Instance.Log($"toolStripItem1_Click_recalculate activity {activity}");
                activity.Recalculate(true);
            }

            Logging.Instance.Log($"toolStripItem1_Click_recalculate update views");
            UpdateViews?.Invoke();

            Logging.Instance.Log($"toolStripItem1_Click_recalculate done");
            MessageBox.Show("Done");
        }


        private void toolStripItem1_Click_rereadDataFile(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            string? filepath = activity.FileFullPath;
            if (filepath == null)
            {
                MessageBox.Show("No file on activity");
                return;
            }

            if (filepath.ToLower().EndsWith(".fit") || filepath.ToLower().EndsWith(".fit.gz"))
            {
                FitReader fitReader = new FitReader(activity);
                try
                {
                    fitReader.ReadFitFromStravaArchive();
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Exception thrown reading FIT file {filepath}, {e}");
                    return;
                }
                MessageBox.Show("Completed FIT Reread");
            }
            else if (filepath.ToLower().EndsWith(".gpx") || filepath.ToLower().EndsWith(".gpx.gz"))
            {
                GpxProcessor gpxProcessor = new GpxProcessor(activity);

                gpxProcessor.ProcessGpx();

                MessageBox.Show("Completed GPX Reread");
            }
            else
            {
                MessageBox.Show("Activity file is not recognized type " + filepath);
            }
            UpdateViews?.Invoke();
        }

        private void toolStripItem1_Click_openGarmin(object? sender, EventArgs args)
        {

            //can't open activity directly as we don't have the garmin id (the fit file name doesn't match)
            //So search for activities by date
            //https://connect.garmin.com/modern/activities?activityType=running&startDate=2023-07-20&endDate=2023-07-20
            //https://connect.garmin.com/modern/activities?startDate=2023-07-20&endDate=2023-07-20

            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            DateTime? start = activity.StartDateNoTimeLocal;

            if (start == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            string date = string.Format("{0:D4}-{1:D2}-{2:d2}", start.Value.Year, start.Value.Month, start.Value.Day);
            string target = $"https://connect.garmin.com/modern/activities?startDate={date}&endDate={date}";
            Misc.RunCommand(target);
        }

        private void toolStripItem1_Click_openFile(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

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
            if (!filepath.ToLower().EndsWith(".fit"))
            {
                if (MessageBox.Show($"File is not fit, {filepath}", "Really?", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }
            }

            Misc.RunCommand(filepath);

        }

        private void toolStripItem1_Click_copyFitFile(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

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
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            StravaApi.OpenAsStravaWebPage(activity);

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
                Misc.RunCommand(target);
            }
        }

        private class TagActivities
        {
            public string Name;
            public string Tag;

            public TagActivities(string name, string tag)
            {
                Name = name;
                Tag = tag;
            }
        }

        private void toolStripItem1_Click_writeCsv(object? sender, EventArgs args)
        {

            if (activityDataGridView.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "Output.csv";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            int columnCount = activityDataGridView.Columns.Count;
                            string columnNames = "";
                            string[] outputCsv = new string[activityDataGridView.Rows.Count + 1];
                            for (int i = 0; i < columnCount; i++)
                            {
                                columnNames += activityDataGridView.Columns[i].HeaderText.ToString() + ",";
                            }
                            outputCsv[0] += columnNames;

                            for (int i = 1; (i - 1) < activityDataGridView.Rows.Count; i++)
                            {
                                for (int j = 0; j < columnCount; j++)
                                {
                                    outputCsv[i] += Utils.Misc.EscapeForCsv(activityDataGridView.Rows[i - 1].Cells[j].Value.ToString()!) + ",";
                                }
                            }

                            File.WriteAllLines(sfd.FileName, outputCsv, Encoding.UTF8);
                            MessageBox.Show("Data Exported Successfully", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }

        }
        private void toolStripItem1_Click_debugActivity(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Datums");
            foreach (Datum d in activity.DataValues)
            {
                sb.AppendLine(d.ToString());
            }

            sb.AppendLine("TimeSeries");
            foreach (KeyValuePair<string, TimeSeriesBase> kvp in activity.TimeSeries)
            {
                sb.AppendLine(kvp.Value.ToString());
            }

            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(sb.ToString());
            largeTextDialogForm.ShowDialog();
        }
        private void toolStripItem1_Click_deleteActivity(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (Database == null || activity == null) return;

            if (MessageBox.Show($"Really delete {activity}? (This doesn not remove it from Strava)", "Delete Activity", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            Database.CurrentAthlete.DeleteActivityBeforeRecalcualte(activity);

            MessageBox.Show("Deleted activity. Perform forced recalculation or restart now");
        }


        private void toolStripItem1_Click_tagStrava(object? sender, EventArgs args)
        {
            if (mouseLocation == null || sender == null)
                return;
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            TagActivities aTagActivities = (TagActivities)toolStripMenuItem.Tag;
            string tag = aTagActivities.Tag;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            TagStravaActivity(tag, activity);

            MessageBox.Show("Done");
        }

        private void toolStripItem1_Click_tagAllStrava(object? sender, EventArgs args)
        {
            if (sender == null) return;
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            TagActivities aTagActivities = (TagActivities)toolStripMenuItem.Tag;
            string tag = aTagActivities.Tag;

            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity != null)
                {
                    TagStravaActivity(tag, activity);
                }
            }
            MessageBox.Show("Done");
        }

        private void TagStravaActivity(string tag, Activity activity)
        {
            TypedDatum<string>? descriptionDatum = (TypedDatum<string>?)activity.GetNamedDatum(Activity.TagDescription);
            if (descriptionDatum == null)
                descriptionDatum = new TypedDatum<string>(Activity.TagDescription, true, ""); //make this recoreded as we need it to persist

            if (tag.Contains(ASKME)) //TODO: support overriding non-numeric values
            {
                string input = Interaction.InputBox("Enter numeric value");
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int askme))
                    return;
                tag = tag.Replace(ASKME, input);
            }

            string? description = descriptionDatum.Data;

            if (description != null && !description.Contains(tag))
            {
                description = description + tag;
                if (!Action.StravaApi.Instance.UpdateActivityDetails(activity, null, description))
                {
                    MessageBox.Show("Update Failed");
                    return;
                }
                descriptionDatum.Data = description;
                activity.AddOrReplaceDatum(descriptionDatum);

                Action.Tags tags = new FellrnrTrainingAnalysis.Action.Tags();
                tags.ProcessTags(activity, 0, true, true); //force and ask for debug

                activity.Recalculate(true);

                UpdateViews?.Invoke();
            }
        }

        private void toolStripItem1_Click_tagAllStravaAsInput(object? sender, EventArgs args)
        {
            string input = Interaction.InputBox("Enter tag to add, including hash");
            if (string.IsNullOrEmpty(input))
                return;
            string TAG = $" {input}";
            int success = 0, error = 0, already = 0, count = 0;
            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity == null)
                    return;

                string? name = activity.GetNamedStringDatum("Name");
                if (name != null && !name.Contains(TAG))
                {
                    name = name + TAG;
                    if (Action.StravaApi.Instance.UpdateActivityDetails(activity, name))
                        success++;
                    else
                        error++;
                }
                else
                {
                    already++;
                }
                if (++count >= 100)
                    break;
            }
            if (count >= 100)
                MessageBox.Show($"Updated {success} entries, with {error} failures, and {already} already tagged, hit rate limit so wait for 15 minutes");
            else
                MessageBox.Show($"Updated {success} entries, with {error} failures, and {already} already tagged");

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
            dataQuality.FindBadTimeSeries(activity);

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
            if (pageSizeComboBox1.Text == "All")
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

        private void formsPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseCrosshair != null && CurrentlyDisplayedActivity != null)
            {
                StringBuilder stringBuilder = new StringBuilder();

                (double cx, double cy) = formsPlot1.GetMouseCoordinates();
                MouseCrosshair.X = cx;
                MouseCrosshair.Y = cy;
                formsPlot1.Refresh();

                if (cx < 0)
                {
                    positionLabel.Text = "N/A";
                    return; //happens when cursor moves too far left
                }
                uint time = (uint)cx;
                DateTime dateTime = new DateTime();
                dateTime = dateTime.AddSeconds(cx);
                stringBuilder.Append($"Position {dateTime.ToShortTimeString()}");


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

                positionLabel.Text = stringBuilder.ToString();

            }
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
