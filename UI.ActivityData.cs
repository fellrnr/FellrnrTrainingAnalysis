using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ActivityData : UserControl
    {
        public ActivityData()
        {
            InitializeComponent();
        }

        const string DATUM_POSTFIX = " (d)";
        const string TIMESERIES_POSTFIX = " (ts)";

        private class Row
        {

            public Label Name;
            public Label Value;
            public Label Min;
            public Label Avg;
            public Label Max;

            public Row(Label name, Label value, Label min, Label avg, Label max)
            {
                Name = name;
                Value = value;
                Min = min;
                Avg = avg;
                Max = max;
            }
        }

        private Dictionary<string, Row> Rows = new Dictionary<string, Row>();
        private void Initialize(Model.Athlete athlete)
        {
            Logging.Instance.StartResetTimer("ActivityData.Initialize");
            tableLayoutPanel1.SuspendLayout();
            IReadOnlyCollection<string> dataNames = athlete.ActivityFieldNames; //we want the fields for the activies, not the athlete

            AddRows(dataNames, DATUM_POSTFIX);

            IReadOnlyCollection<string> timeSeriesNames = athlete.AllTimeSeriesNames; //we want the fields for the activies, not the athlete

            AddRows(timeSeriesNames, TIMESERIES_POSTFIX);

            tableLayoutPanel1.ResumeLayout(true);
            //tableLayoutPanel1.PerformLayout();
            Logging.Instance.Log(string.Format("ActivityData.Initialize took {0}", Logging.Instance.GetAndResetTime("ActivityData.Initialize")));
        }

        private void AddRows(IReadOnlyCollection<string> dataNames, string postfix)
        {
            IReadOnlyCollection<string> dataNamesSorted = dataNames.ToImmutableSortedSet();

            foreach (string fieldName in dataNamesSorted)
            {
                int row = Rows.Count + 1; //zero is the header
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

                    Rows.Add(fieldName + postfix, new Row(name, value, min, avg, max));
                }
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
                row.Min.Text = "";
                row.Avg.Text = "";
                row.Max.Text = "";
            }

            if (activity == null)
            {
                Logging.Instance.TraceLeave();
                return;
            }

            DisplayActivityDatum(activity);

            DisplayActivityTimeSeries(activity);

            tableLayoutPanel1.ResumeLayout();
            Logging.Instance.TraceLeave();
        }

        private void DisplayActivityTimeSeries(Activity? activity)
        {
            Logging.Instance.TraceEntry("ActivityData.DisplayActivityTimeSeries");
            ReadOnlyDictionary<string, DataStreamBase> timeSeriesList = activity!.TimeSeries;

            foreach (KeyValuePair<string, DataStreamBase> kvp in timeSeriesList)
            {
                string fieldName = kvp.Key;
                string entryName = fieldName + TIMESERIES_POSTFIX;
                if (Rows.ContainsKey(entryName))
                {
                    DataStreamBase dataStream = kvp.Value;
                    Tuple<uint[], float[]>? tuple = dataStream.GetData();
                    if (tuple != null)
                    {
                        float[] values = tuple.Item2;
                        Row row = Rows[entryName];
                        row.Value.Text = "";
                        row.Min.Text = values.Min().ToString();
                        row.Avg.Text = values.Average().ToString();
                        row.Max.Text = values.Max().ToString();
                    }
                }
            }
            Logging.Instance.TraceLeave();
        }

        private void DisplayActivityDatum(Activity? activity)
        {
            Logging.Instance.TraceEntry("ActivityData.DisplayActivityDatum");
            IReadOnlyCollection<Datum> data = activity!.DataValues;

            foreach (Datum datum in data)
            {
                string fieldName = datum.Name;
                string entryName = fieldName + DATUM_POSTFIX;
                if (Rows.ContainsKey(entryName))
                {

                    Row row = Rows[entryName];
                    ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(fieldName);
                    if (activityDatumMetadata != null)
                    {
                        row.Value.Text = DatumFormatter.FormatForGrid(activity.GetNamedDatum(fieldName), activityDatumMetadata);
                        //row.Value.BackColor = Color.White;
                    }
                    else
                    {
                        string? val = datum.ToString();
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
