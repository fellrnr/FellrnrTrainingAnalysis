using FellrnrTrainingAnalysis.Model;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System.Data;

namespace FellrnrTrainingAnalysis.UI

{
    public partial class ProgressGraph : UserControl
    {
        public ProgressGraph()
        {
            InitializeComponent();
        }

        private Database? Database;
        private FilterActivities? FilterActivities;
        private Dictionary<string, FilterRow> Filters = new Dictionary<string, FilterRow>();
        private int Row = 1; //row zero is the headers
        private delegate void DoRefresh();

        private const string MIN = "Ts.Min ";
        private const string AVG = "Ts.Avg ";
        private const string MAX = "Ts.Max ";
        private int OP_LENGTH = MIN.Length; //not const to prevent compiler objection

        private string[] TimeSeriesOperations = { MIN, AVG, MAX };
        private bool IsTimeSeriesOperations(string s) { if(s.Length < OP_LENGTH) return false; return (TimeSeriesOperations.Any(s.Contains)); }
        private string TimeSeriesFromOperation(string s) { if (s.Length < OP_LENGTH) return ""; return s.Substring(OP_LENGTH); }
        private string Operation(string s) { if (s.Length < OP_LENGTH) return ""; return s.Substring(0, OP_LENGTH); }

        public void Display(Database database, FilterActivities filterActivities)
        {
            Database = database;
            FilterActivities = filterActivities;
            if (database == null || database.CurrentAthlete == null || database.CurrentAthlete.ActivitiesByDateTime.Count == 0 || FilterActivities == null) { return; }

            Athlete athlete = database.CurrentAthlete;
            DateTime earliest = athlete.ActivitiesByDateTime.First().Key;
            DateTime latest = athlete.ActivitiesByDateTime.Last().Key;
            dateTimePickerStart.Value = earliest;
            dateTimePickerEnd.Value = latest;
            CreateFilterRows(database, athlete);
            RefreshGraph();
        }

        private void CreateFilterRows(Database database, Athlete athlete)
        {
            tableLayoutPanel1.SuspendLayout();

            //HACK: add a fake zero height row
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute));
            Row++;

            foreach (string name in athlete.ActivityFieldNames)
            {
                ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(name);
                if (activityDatumMetadata != null && activityDatumMetadata.DisplayUnits != ActivityDatumMetadata.DisplayUnitsType.None)
                {
                    if (!IsTimeSeriesOperations(name)) //just in case a datum contains a time series (the "Ts." should prevent this)
                    {
                        if (!Filters.ContainsKey(name))
                        {
                            Filters.Add(name, new FilterRow(tableLayoutPanel1, name, Row++, RefreshGraph));
                        }
                    }
                }
            }

            IReadOnlyCollection<String> timeSeriesNames = database.CurrentAthlete.TimeSeriesNames;
            foreach (string name in timeSeriesNames)
            {
                foreach (string s in TimeSeriesOperations)
                {
                    string key = s + name;
                    if (!Filters.ContainsKey(key))
                    {
                        Filters.Add(key, new FilterRow(tableLayoutPanel1, key, Row++, RefreshGraph));
                    }
                }
            }
            tableLayoutPanel1.ResumeLayout();
        }

        private List<Axis> CurrentAxis { get; set; } = new List<Axis>();
        private int axisIndex = 0;

        private void RefreshGraph()
        {
            ScottPlot.Palettes.Category20 myPalette = new ScottPlot.Palettes.Category20();

            formsPlotProgress.Plot.Clear();
            foreach (Axis axis in CurrentAxis) { formsPlotProgress.Plot.RemoveAxis(axis); }
            CurrentAxis.Clear();
            axisIndex = 0;
            if (Database == null || Database.CurrentAthlete == null || Database.CurrentAthlete.ActivitiesByDateTime.Count == 0 || FilterActivities == null) { return; }

            foreach (KeyValuePair<string, FilterRow> kvp in Filters)
            {
                FilterRow filterRow = kvp.Value;
                string name = kvp.Key;
                if (!filterRow.IsChecked)
                    continue;

                List<DateTime> dateTimes = new List<DateTime>();
                List<double> values = new List<double>();

                List<Activity> activities = FilterActivities.GetActivities(Database);


                //foreach (KeyValuePair<DateTime, Activity> kvp in Database.CurrentAthlete.ActivitiesByDateTime)
                foreach(Activity activity in activities)
                {
                    //DateTime dateTime = kvp.Key;
                    DateTime? startDateTime = activity.StartDateTime;
                    if (startDateTime != null && startDateTime >= dateTimePickerStart.Value && startDateTime <= dateTimePickerEnd.Value)
                    {
                        //Activity activity = kvp.Value;

                        float? value = GetValue(name, activity);
                        if (value != null)
                        {
                            dateTimes.Add(startDateTime.Value);
                            values.Add((double)value);
                        }
                    }
                }
                double[] xArray = dateTimes.Select(x => x.ToOADate()).ToArray();
                double[] yArray = values.ToArray();
                IPlottable plottable;
                if (filterRow.IsBar)
                {
                    BarPlot barPlot = formsPlotProgress.Plot.AddBar(yArray, xArray);
                    barPlot.Color = myPalette.GetColor(axisIndex);
                    plottable = barPlot;
                }
                else
                {
                    ScatterPlot scatterGraph = formsPlotProgress.Plot.AddScatter(xArray, yArray);
                    scatterGraph.MarkerShape = MarkerShape.none;
                    scatterGraph.LineWidth = 2;
                    scatterGraph.Color = myPalette.GetColor(axisIndex);

                    plottable = scatterGraph;
                }

                formsPlotProgress.Plot.XAxis.DateTimeFormat(true);
                Axis yAxis;
                if (axisIndex == 0)
                {
                    yAxis = formsPlotProgress.Plot.YAxis;
                    plottable.YAxisIndex = 0;

                }
                else
                {
                    yAxis = formsPlotProgress.Plot.AddAxis(Edge.Left);
                    yAxis.AxisIndex = axisIndex + 4; //there are 4 default axises we have to skip
                    plottable.YAxisIndex = yAxis.AxisIndex;
                    CurrentAxis.Add(yAxis);
                }
                //yAxis.Label(dataStreamDefinition.DisplayTitle);
                yAxis.Color(myPalette.GetColor(axisIndex));
                axisIndex++;

            }
            formsPlotProgress.Refresh();
        }

        private float? GetValue(string name, Activity activity)
        {
            if (!IsTimeSeriesOperations(name)) //just in case a datum contains a time series (the "Ts." should prevent this)
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
            else
            {
                string tsName = TimeSeriesFromOperation(name);
                if (activity.TimeSeries.ContainsKey(tsName))
                {
                    IDataStream dataStream = activity.TimeSeries[tsName];
                    float value = 0;
                    if (dataStream != null && dataStream.GetData(activity) != null)
                    {
                        float[] valuesFromStream = dataStream.GetData(activity)!.Item2;
                        if (Operation(name) == MIN)
                        {
                            value = valuesFromStream.Min();
                        }
                        else if (Operation(name) == AVG)
                        {
                            value = valuesFromStream.Average();
                        }
                        else if (Operation(name) == MAX)
                        {
                            value = valuesFromStream.Max();
                        }

                        return value;
                    }
                }
            }
            return null;
        }

        private class FilterRow
        {
            protected CheckBox FieldName;
            protected CheckBox? BarGraph;
            protected NumericUpDown? SmoothingBox;
            protected int Row;
            private DoRefresh DoRefresh;
            private TableLayoutPanel TableLayoutPanel;
            public FilterRow(TableLayoutPanel tableLayoutPanel, string name, int row, DoRefresh doRefresh)
            {
                Row = row;
                TableLayoutPanel = tableLayoutPanel;
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                DoRefresh = doRefresh;
                FieldName = new CheckBox { Text = name, Anchor = AnchorStyles.Left, AutoSize = true, Checked = false };
                FieldName.CheckedChanged += ChangedHandler;

                tableLayoutPanel.Controls.Add(FieldName, 0, row);
            }

            public bool IsChecked { get { return FieldName.Checked; } }
            public bool IsBar { get { return BarGraph == null ? false : BarGraph.Checked; } }
            public decimal Smoothing { get { return SmoothingBox == null ? 0 : SmoothingBox.Value; } }

            protected void ChangedHandler(object? sender, EventArgs e)
            {
                if (BarGraph == null)
                {
                    BarGraph = new CheckBox { Text = "Bar?", Anchor = AnchorStyles.Left, AutoSize = true, Checked = false };
                    BarGraph.CheckedChanged += ChangedHandler;
                    SmoothingBox = new NumericUpDown { Value = 0, Anchor = AnchorStyles.Left, AutoSize = true, Increment = 1 };
                    SmoothingBox.ValueChanged += ChangedHandler;
                    TableLayoutPanel.Controls.Add(BarGraph, 1, Row);
                    TableLayoutPanel.Controls.Add(SmoothingBox, 2, Row);
                }
                DoRefresh.Invoke();
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
                    if (Database == null) return;
                    Athlete athlete = Database.CurrentAthlete;
                    dateTime = athlete.ActivitiesByDateTime.First().Key;
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

    }
}
