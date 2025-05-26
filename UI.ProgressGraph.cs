using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System.Collections.ObjectModel;
using System.Data;
using static FellrnrTrainingAnalysis.Model.TimeSeriesBase;

namespace FellrnrTrainingAnalysis.UI

{
    public partial class ProgressGraph : UserControl
    {
        public ProgressGraph()
        {
            InitializeComponent();
        }

        private Database? _database;
        private FilterActivities? _filterActivities;
        private Dictionary<string, GraphLineSelection> Selections = new Dictionary<string, GraphLineSelection>();
        private int Row = 1; //row zero is the headers
        private delegate void DoRefresh();
        private bool PauseUpdating = false;

        //TimeSeries are the form "TS.{operation}.{name}";

        private const string ACTIVITY_DOT = "Activity.";
        private const string DAY_DOT = "Day.";
        private const string TS_DOT = "TS.";
        private const string BAR = "Bar";
        private const string LINE = "Line";
        private const string SCATTER = "Scatter";

        //private bool IsTimeSeriesOperations(string s) { return s.StartsWith(TS_DOT); }
        //private string TimeSeriesFromOperation(string t) { return (t.LastIndexOf(".") != t.IndexOf(".")) ? t.Substring(t.LastIndexOf(".") + 1) : ""; }
        //private string Operation(string t) { return (t.LastIndexOf(".") != t.IndexOf(".")) ? t.Substring(t.IndexOf(".") + 1, t.LastIndexOf(".") - t.IndexOf(".") - 1) : ""; }

        public void Display(Database database, FilterActivities filterActivities)
        {
            Logging.Instance.TraceEntry("ProgressGraph.Display");
            _database = database;
            _filterActivities = filterActivities;
            if (database == null || database.CurrentAthlete == null || database.CurrentAthlete.ActivitiesByLocalDateTime.Count == 0 || _filterActivities == null) { return; }

            Athlete athlete = database.CurrentAthlete;
            DateTime earliest = athlete.ActivitiesByLocalDateTime.First().Key;
            DateTime latest = athlete.ActivitiesByLocalDateTime.Last().Key;
            dateTimePickerStart.Value = earliest;
            dateTimePickerEnd.Value = latest;
            CreateFilterRows(database, athlete);
            timePeriodComboBox.Text = "3M"; //let update callback set dates
            RefreshGraph();
            Logging.Instance.TraceLeave();
        }

        private void CreateFilterRows(Database database, Athlete athlete)
        {
            tableLayoutPanel1.SuspendLayout();

            //HACK: add a fake zero height row
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute));
            Row++;

            foreach (string name in athlete.ActivityFieldNames)
            {
                ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(name); //NB, look up without the prefix
                if (activityDatumMetadata != null &&
                    activityDatumMetadata.DisplayUnits != ActivityDatumMetadata.DisplayUnitsType.None &&
                    activityDatumMetadata.DisplayUnits != ActivityDatumMetadata.DisplayUnitsType.String)
                {
                    if (!Selections.ContainsKey(name))
                    {
                        Selections.Add(name, new GraphLineSelection(tableLayoutPanel1, name, GraphLineSelection.FieldTypeEnum.Activity, Row++, RefreshGraph));
                    }
                }
            }

            foreach (string name in athlete.DayFieldNames)
            {
                ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(name);
                if (activityDatumMetadata != null &&
                    activityDatumMetadata.DisplayUnits != ActivityDatumMetadata.DisplayUnitsType.None &&
                    activityDatumMetadata.DisplayUnits != ActivityDatumMetadata.DisplayUnitsType.String)
                {
                    if (!Selections.ContainsKey(name))
                    {
                        Selections.Add(name, new GraphLineSelection(tableLayoutPanel1, name, GraphLineSelection.FieldTypeEnum.Day, Row++, RefreshGraph));
                    }
                }
            }


            IReadOnlyCollection<String> timeSeriesNames = database.CurrentAthlete.AllTimeSeriesNames;
            foreach (string name in timeSeriesNames)
            {
                if (!Selections.ContainsKey(name))
                {
                    Selections.Add(name, new GraphLineSelection(tableLayoutPanel1, name, GraphLineSelection.FieldTypeEnum.TimeSeries, Row++, RefreshGraph));
                }

            }
            tableLayoutPanel1.ResumeLayout();
        }

        private List<Axis> CurrentAxis { get; set; } = new List<Axis>();
        private Dictionary<int, Axis> ReusedAxis { get; set; } = new Dictionary<int, Axis>();
        private int axisIndex = 0;

        private void RefreshGraph()
        {
            if(PauseUpdating) return;

            ScottPlot.Palettes.Category10 myPalette = new ScottPlot.Palettes.Category10();

            formsPlotProgress.Plot.Clear();
            foreach (Axis axis in CurrentAxis) { formsPlotProgress.Plot.RemoveAxis(axis); }
            CurrentAxis.Clear();
            ReusedAxis.Clear();
            axisIndex = 0;
            if (_database == null || _database.CurrentAthlete == null || _database.CurrentAthlete.ActivitiesByLocalDateTime.Count == 0 || _filterActivities == null) { return; }


            List<Activity> activities = _filterActivities.GetActivities(_database);
            ReadOnlyDictionary<DateTime, Model.Day> days = _database.CurrentAthlete.Days;

            foreach (KeyValuePair<string, GraphLineSelection> kvp in Selections)
            {
                GraphLineSelection filterRow = kvp.Value;
                string name = kvp.Key;
                if (!filterRow.IsChecked)
                    continue;

                List<DateTime> dateTimes = new List<DateTime>();
                List<double> values = new List<double>();

                //foreach (KeyValuePair<DateTime, Activity> kvp in Database.CurrentAthlete.ActivitiesByDateTime)
                if (filterRow.FieldType == GraphLineSelection.FieldTypeEnum.Day)
                {
                    foreach (KeyValuePair<DateTime, Model.Day> kvp2 in days)
                    {
                        Model.Day day = kvp2.Value;
                        DateTime dt = kvp2.Key;
                        if (dt >= dateTimePickerStart.Value && dt <= dateTimePickerEnd.Value)
                        {
                            //Activity activity = kvp.Value;
                            float? value = GetValue(filterRow, day);
                            if (value != null)
                            {
                                dateTimes.Add(dt);
                                values.Add((double)value);
                            }
                        }
                    }
                }
                else
                {
                    foreach (Activity activity in activities)
                    {
                        //DateTime dateTime = kvp.Key;
                        //Note we put activities on the timeline with time included so they're offset slightly, but days are for midnight
                        DateTime? startDateTime = activity.StartDateTimeLocal;
                        if (startDateTime != null && startDateTime >= dateTimePickerStart.Value && startDateTime <= dateTimePickerEnd.Value)
                        {
                            //Activity activity = kvp.Value;

                            float? value = GetValue(filterRow, activity);
                            if (value != null)
                            {
                                dateTimes.Add(startDateTime.Value);
                                values.Add((double)value);
                            }
                        }
                    }
                }
                if (values.Count == 0)
                    continue;
                double[] xArray = dateTimes.Select(x => x.ToOADate()).ToArray();
                double[] yArray = values.ToArray();
                if (filterRow.Smoothing > 0)
                    yArray = TimeSeriesUtils.WindowSmoothed(yArray, (int)filterRow.Smoothing);

                IPlottable plottable;
                if (filterRow.GraphStyle == BAR)
                {
                    BarPlot barPlot = formsPlotProgress.Plot.AddBar(yArray, xArray);
                    barPlot.Color = myPalette.GetColor(axisIndex);
                    barPlot.Label = filterRow.Name;
                    plottable = barPlot;
                }
                else if (filterRow.GraphStyle == SCATTER)
                {
                    ScatterPlot scatterPlot = formsPlotProgress.Plot.AddScatter(yArray, xArray);
                    scatterPlot.Color = myPalette.GetColor(axisIndex);
                    scatterPlot.Label = filterRow.Name;
                    plottable = scatterPlot;
                }
                else
                {
                    ScatterPlot scatterGraph = formsPlotProgress.Plot.AddScatter(xArray, yArray);
                    scatterGraph.MarkerShape = MarkerShape.filledCircle;
                    scatterGraph.MarkerSize = 15;
                    scatterGraph.LineWidth = 2;
                    scatterGraph.Smooth = true;
                    //scatterGraph.Color = myPalette.GetColor(axisIndex);
                    scatterGraph.Label = filterRow.Name;
                    plottable = scatterGraph;
                }


                formsPlotProgress.Plot.XAxis.DateTimeFormat(true);
                Axis yAxis;
                int forcedAxis = filterRow.Axis;
                if (forcedAxis > 0 && ReusedAxis.ContainsKey(forcedAxis))
                {
                    yAxis = ReusedAxis[forcedAxis];
                    yAxis.Label($"{yAxis.Label()}/{filterRow.Name}");
                }
                else
                {
                    if (axisIndex == 0)
                    {
                        yAxis = formsPlotProgress.Plot.YAxis;
                        plottable.YAxisIndex = 0;
                    }
                    else
                    {
                        yAxis = formsPlotProgress.Plot.AddAxis(Edge.Left);
                        //yAxis.AxisIndex = axisIndex + 4; //there are 4 default axises we have to skip
                        plottable.YAxisIndex = yAxis.AxisIndex;
                        CurrentAxis.Add(yAxis);
                    }
                    yAxis.Label(filterRow.Name);
                    //yAxis.Color(myPalette.GetColor(axisIndex));
                    axisIndex++;
                    if (forcedAxis >= 0)
                        ReusedAxis[forcedAxis] = yAxis;
                }

            }
            formsPlotProgress.Plot.Legend(true, Alignment.UpperCenter);

            formsPlotProgress.Refresh();
        }

        private float? GetValue(GraphLineSelection filterRow, Activity activity)
        {
            string name = filterRow.Name;
            if (filterRow.FieldType == GraphLineSelection.FieldTypeEnum.Activity)
            {
                if (activity.HasNamedDatum(name))
                {
                    Datum? datum = activity.GetNamedDatum(name);
                    if (datum != null && datum is TypedDatum<float>)
                    {
                        TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
                        return floatDatum.Data;
                    }
                }
            }
            else if (filterRow.FieldType == GraphLineSelection.FieldTypeEnum.TimeSeries)
            {
                if (activity.TimeSeries.ContainsKey(name))
                {
                    StaticsValue offset = TimeSeriesBase.StatisticsValueFromName(filterRow.Operation);
                    TimeSeriesBase dataStream = activity.TimeSeries[name];
                    float value = dataStream.Percentile(offset);
                    return value;
                }
            }
            return null;
        }

        private float? GetValue(GraphLineSelection filterRow, Model.Day day)
        {
            string name = filterRow.Name;
            if (day.HasNamedDatum(name))
            {
                Datum? datum = day.GetNamedDatum(name);
                if (datum != null && datum is TypedDatum<float>)
                {
                    TypedDatum<float> floatDatum = (TypedDatum<float>)datum;
                    return floatDatum.Data;
                }
            }
            return null;
        }
        private class GraphLineSelection
        {
            public enum FieldTypeEnum { Activity, Day, TimeSeries };
            public FieldTypeEnum FieldType { get; set; }
            public string Name { get; set; }

            public string Operation { get { if (OperationBox != null) return OperationBox.Text; else return ""; } }

            public string GraphStyle { get { if (GraphStyleBox != null) return GraphStyleBox.Text; else return ""; } }

            protected CheckBox FieldName;
            protected ComboBox? GraphStyleBox;
            protected ComboBox? OperationBox;
            protected NumericUpDown? SmoothingBox;
            protected NumericUpDown? AxisBox;
            protected int Row;
            private DoRefresh DoRefresh;
            private TableLayoutPanel TableLayoutPanel;
            public GraphLineSelection(TableLayoutPanel tableLayoutPanel, string name, FieldTypeEnum fieldType, int row, DoRefresh doRefresh)
            {
                Row = row;
                FieldType = fieldType;
                Name = name;
                TableLayoutPanel = tableLayoutPanel;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                DoRefresh = doRefresh;
                FieldName = new CheckBox { Text = name, Anchor = AnchorStyles.Left, AutoSize = true, Checked = false };
                FieldName.CheckedChanged += ChangedHandler;

                tableLayoutPanel.Controls.Add(FieldName, 0, row);
            }


            public void RestoreValues(GraphStore store)
            {
                CreateMissingControls();
                FieldName.Checked = store.IsChecked;
                if (SmoothingBox != null) SmoothingBox.Value = store.Smoothing;
                if (AxisBox != null) AxisBox.Value = store.Axis;
                if (OperationBox != null) OperationBox.Text = store.Operation;
                if (GraphStyleBox != null) GraphStyleBox.Text = store.Graphstyle;
            }

            public void Clear()
            {
                FieldName.Checked = false;
            }

            public bool IsChecked { get { return FieldName.Checked; } }
            public decimal Smoothing { get { return SmoothingBox == null ? 0 : SmoothingBox.Value; } }
            public int Axis { get { return AxisBox == null ? 0 : (int)AxisBox.Value; } }

            public class GraphStore
            {
                public string Name;
                public bool IsChecked;
                public decimal Smoothing;
                public int Axis;
                public string Operation;
                public string Graphstyle;

                public GraphStore(string name, bool isChecked, decimal smoothing, int axis, string operation, string graphstyle)
                {
                    Name = name;
                    IsChecked = isChecked;
                    Smoothing = smoothing;
                    Axis = axis;
                    Operation = operation;
                    Graphstyle = graphstyle;
                }
            }

            protected void ChangedHandler(object? sender, EventArgs e)
            {
                CreateMissingControls();
                DoRefresh.Invoke();
            }

            private void CreateMissingControls()
            {
                if (GraphStyleBox == null)
                {
                    if (FieldType == FieldTypeEnum.TimeSeries)
                    {
                        OperationBox = new ComboBox { Text = TimeSeriesBase.StaticsValueNames.Last(), Anchor = AnchorStyles.Left, AutoSize = true };
                        OperationBox.Items.AddRange(TimeSeriesBase.StaticsValueNames);
                        OperationBox.SelectedIndexChanged += ChangedHandler;
                        TableLayoutPanel.Controls.Add(OperationBox, 1, Row);
                    }
                    GraphStyleBox = new ComboBox { Text = LINE, Anchor = AnchorStyles.Left, AutoSize = true };
                    GraphStyleBox.Items.AddRange(new string[] { LINE, BAR, SCATTER });
                    GraphStyleBox.SelectedIndexChanged += ChangedHandler;
                    TableLayoutPanel.Controls.Add(GraphStyleBox, 2, Row);

                    SmoothingBox = new NumericUpDown { Value = 0, Anchor = AnchorStyles.Left, AutoSize = true, Increment = 1 };
                    SmoothingBox.ValueChanged += ChangedHandler;
                    TableLayoutPanel.Controls.Add(SmoothingBox, 3, Row);

                    AxisBox = new NumericUpDown { Minimum = -1, Value = -1, Anchor = AnchorStyles.Left, AutoSize = true, Increment = 1 };
                    AxisBox.ValueChanged += ChangedHandler;
                    TableLayoutPanel.Controls.Add(AxisBox, 4, Row);
                }
            }
        }

        //private Dictionary<string, CheckBox> TimeSeriesCheckBoxes = new Dictionary<string, CheckBox>();

        private void timePeriodComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime dateTime = DateTime.Now;
            switch (timePeriodComboBox.Text)
            {
                case "1W":
                    dateTime = dateTime.AddDays(-7); break;
                case "1M":
                    dateTime = dateTime.AddMonths(-1); break;
                case "3M":
                    dateTime = dateTime.AddMonths(-3); break;
                case "6M":
                    dateTime = dateTime.AddMonths(-6); break;
                case "1Y":
                    dateTime = dateTime.AddYears(-1); break;
                case "2Y":
                    dateTime = dateTime.AddYears(-2); break;
                case "3Y":
                    dateTime = dateTime.AddYears(-3); break;
                case "4Y":
                    dateTime = dateTime.AddYears(-4); break;
                case "5Y":
                    dateTime = dateTime.AddYears(-5); break;
                case "All":
                    if (_database == null) return;
                    Athlete athlete = _database.CurrentAthlete;
                    dateTime = athlete.ActivitiesByLocalDateTime.First().Key;
                    break;
                default:
                    break;
            }
            dateTimePickerStart.Value = dateTime;
        }

        private void dateTimePickerStart_ValueChanged(object sender, EventArgs e)
        {
            RefreshGraph();
        }

        private void dateTimePickerEnd_ValueChanged(object sender, EventArgs e)
        {
            RefreshGraph();
        }

        private Dictionary<string, List<GraphLineSelection.GraphStore>> Presets = new Dictionary<string, List<GraphLineSelection.GraphStore>>()
        {
            {
                "TSB", new List<GraphLineSelection.GraphStore> 
                {
                    new GraphLineSelection.GraphStore("CTL", true, 0, 2, "", LINE),
                    new GraphLineSelection.GraphStore("ATL", true, 0, 2, "", LINE),
                    new GraphLineSelection.GraphStore("TSB", true, 0, 2, "", LINE),
                }
            }
        };

        private void presetComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            PauseUpdating = true;
            //do this as a cheat for now, then add saving and restoring
            if (Presets.ContainsKey(presetComboBox.Text))
            {
                List<GraphLineSelection.GraphStore> graphStores = Presets[presetComboBox.Text];
                foreach (KeyValuePair<string, GraphLineSelection> kvp in Selections)
                {
                    kvp.Value.Clear();
                }
                foreach (GraphLineSelection.GraphStore store in graphStores)
                {
                    Selections[store.Name].RestoreValues(store);
                }
            }
            PauseUpdating = false;
            RefreshGraph();
        }
    }
}
