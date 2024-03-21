using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Reflection;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ActivityData : UserControl
    {
        public ActivityData()
        {
            InitializeComponent();
            typeof(DataGridView).InvokeMember(
   "DoubleBuffered",
   BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
   null,
   tableLayoutPanel1,
   new object[] { true });

        }

        const string DATUM_POSTFIX = "(activity)";
        const string TIMESERIES_POSTFIX = "(time series)";
        const string DAY_POSTFIX = "(day)";

        private class Row
        {

            public Label Name;
            public Label Value;
            public Label Min;
            public Label Avg;
            public Label Max;
            public Label Notes;

            public Row(Label name, Label value, Label min, Label avg, Label max, Label notes)
            {
                Name = name;
                Value = value;
                Min = min;
                Avg = avg;
                Max = max;
                Notes = notes;
            }
        }

        private int RowCount = 0;
        private Dictionary<string, Row> Rows = new Dictionary<string, Row>();
        private bool AddedHeaders = false;
        private void Initialize(Model.Athlete athlete)
        {
            Logging.Instance.ResetAndStartTimer("ActivityData.Initialize");
            tableLayoutPanel1.SuspendLayout();
            IReadOnlyCollection<string> dataNames = athlete.ActivityFieldNames; //we want the fields for the activies, not the athlete

            AddRows(dataNames, DATUM_POSTFIX);

            IReadOnlyCollection<string> dayNames = athlete.DayFieldNames; //we want the fields for the activies, not the athlete

            AddRows(dayNames, DAY_POSTFIX);

            IReadOnlyCollection<string> timeSeriesNames = athlete.AllTimeSeriesNames; //we want the fields for the activies, not the athlete

            AddRows(timeSeriesNames, TIMESERIES_POSTFIX);

            AddedHeaders = true;

            tableLayoutPanel1.ResumeLayout(true);
            //tableLayoutPanel1.PerformLayout();
            Logging.Instance.Log(string.Format("ActivityData.Initialize took {0}", Logging.Instance.GetAndResetTime("ActivityData.Initialize")));
        }

        private void AddHeaderRow(string postfix)
        {
            int row = RowCount + 1; //zero is the header
            for (int i = 0; i < 6; i++)
            {
                string text = (i == 2 ? postfix : "*****");
                Label header = new Label { Text = text, Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true, BackColor = Color.AntiqueWhite };
                tableLayoutPanel1.Controls.Add(header, i, row);
            }
            RowCount++;
        }
        private void AddRows(IReadOnlyCollection<string> dataNames, string postfix)
        {
            if (!AddedHeaders)
                AddHeaderRow(postfix);
            IReadOnlyCollection<string> dataNamesSorted = dataNames.ToImmutableSortedSet();
            foreach (string fieldName in dataNamesSorted)
            {
                int row = RowCount + 1; //zero is the header
                if (!Rows.ContainsKey(fieldName + postfix))
                {
                    Label name = new Label { Text = fieldName, Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    tableLayoutPanel1.Controls.Add(name, 0, row);
                    Label value = new Label { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    tableLayoutPanel1.Controls.Add(value, 1, row);
                    Label min = new Label { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    tableLayoutPanel1.Controls.Add(min, 2, row);
                    Label avg = new Label { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    tableLayoutPanel1.Controls.Add(avg, 3, row);
                    Label max = new Label { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    tableLayoutPanel1.Controls.Add(max, 4, row);
                    Label notes = new Label { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    tableLayoutPanel1.Controls.Add(notes, 5, row);

                    Rows.Add(fieldName + postfix, new Row(name, value, min, avg, max, notes));
                }
                RowCount++;
            }
        }

        public void DisplayActivity(Athlete athlete, Model.Activity? activity)
        {
            Logging.Instance.TraceEntry("UI.ActivityData.DisplayActivity");
            tableLayoutPanel1.SuspendLayout();
            Initialize(athlete);
            foreach (KeyValuePair<string, Row> kvp in Rows)
            {
                Row row = kvp.Value;
                row.Value.Text = "";
                row.Min.Text = "N/A";
                row.Avg.Text = "N/A";
                row.Max.Text = "N/A";
                row.Notes.Text = "N/A";
            }

            if (activity == null)
            {
                Logging.Instance.TraceLeave();
                return;
            }

            DisplayData(activity, DATUM_POSTFIX);

            Model.Day day = athlete.Days[activity.StartDateNoTimeLocal!.Value];
            DisplayData(day, DAY_POSTFIX);

            DisplayActivityTimeSeries(activity);

            tableLayoutPanel1.ResumeLayout();
            Logging.Instance.TraceLeave();
        }

        private void DisplayActivityTimeSeries(Activity? activity)
        {
            Logging.Instance.TraceEntry("ActivityData.DisplayActivityTimeSeries");
            ReadOnlyDictionary<string, TimeSeriesBase> timeSeriesList = activity!.TimeSeries;

            foreach (KeyValuePair<string, TimeSeriesBase> kvp in timeSeriesList)
            {
                string fieldName = kvp.Key;
                string entryName = fieldName + TIMESERIES_POSTFIX;
                if (Rows.ContainsKey(entryName))
                {
                    TimeSeriesBase dataStream = kvp.Value;
                    TimeValueList? tuple = dataStream.GetData(forceCount: 0, forceJustMe: false);
                    if (tuple != null)
                    {
                        float[] values = tuple.Values;
                        Row row = Rows[entryName];
                        row.Value.Text = "";
                        row.Min.Text = values.Min().ToString();
                        row.Avg.Text = values.Average().ToString();
                        row.Max.Text = values.Max().ToString();
                        string last = tuple.Times.Count() > 0 ? $"{Utils.Misc.FormatTime(tuple.Times.Last())}" : "No Times";
                        row.Notes.Text = $"IsVirtual {dataStream.IsVirtual()}, t {last}";
                    }
                }
            }
            Logging.Instance.TraceLeave();
        }

        private void DisplayData(Extensible activity, string postfix)
        {
            Logging.Instance.TraceEntry("ActivityData.DisplayActivityDatum");
            IReadOnlyCollection<Datum> data = activity!.DataValues;

            foreach (Datum datum in data)
            {
                string fieldName = datum.Name;
                string entryName = fieldName + postfix;
                if (Rows.ContainsKey(entryName))
                {

                    Row row = Rows[entryName];
                    ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(fieldName);
                    if (activityDatumMetadata != null)
                    {
                        const int ArbitraryWrapLiength = 50;

                        string s = DatumFormatter.FormatForGrid(activity.GetNamedDatum(fieldName), activityDatumMetadata);
                        s = Utils.Misc.WordWrap(s, ArbitraryWrapLiength, " ".ToCharArray());
                        row.Value.Text = s;
                        row.Notes.Text = $"Recorded {datum.Recorded}";
                        //row.Value.BackColor = Color.White;
                    }
                    else
                    {
                        string? val = datum.DataAsString();
                        if (Utils.Options.Instance.DebugAddRawDataToGrids)
                        {
                            val += " [No Metadata]";
                        }
                        row.Value.Text = val;
                    }
                }
            }
            Logging.Instance.TraceLeave();
        }
    }
}
