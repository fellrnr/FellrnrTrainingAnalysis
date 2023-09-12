using FellrnrTrainingAnalysis.Model;
using System.Collections.Immutable;
using System.Collections.ObjectModel;

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
            IReadOnlyCollection<string> dataNames = athlete.ActivityFieldNames; //we want the fields for the activies, not the athlete

            AddRows(dataNames, DATUM_POSTFIX);

            IReadOnlyCollection<string> timeSeriesNames = athlete.TimeSeriesNames; //we want the fields for the activies, not the athlete

            AddRows(timeSeriesNames, TIMESERIES_POSTFIX);
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
                return;
            }

            IReadOnlyCollection<Datum> data = activity.DataValues;

            foreach(Datum datum in data)
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
                    }
                    else
                    {
                        row.Value.Text = datum.ToString();
                    }
                }
            }

            ReadOnlyDictionary<string, IDataStream> timeSeriesList = activity.TimeSeries;

            foreach (KeyValuePair<string, IDataStream> kvp in timeSeriesList)
            {
                string fieldName = kvp.Key;
                string entryName = fieldName + TIMESERIES_POSTFIX;
                if (Rows.ContainsKey(entryName))
                {
                    IDataStream dataStream = kvp.Value;
                    Tuple<uint[], float[]>? tuple = dataStream.GetData(activity);
                    if(tuple != null)
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

        }
    }
}
