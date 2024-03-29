using System;
using System.Collections.Generic;
using System.Windows.Forms;
using FellrnrTrainingAnalysis.Model;
using pi.science.regression;

namespace FellrnrTrainingAnalysis
{
    public partial class ActivityCorrelation : Form
    {
        public ActivityCorrelation(Database database, Activity? activity)
        {
            InitializeComponent();
            Database = database;
            Activity = activity;

            if (Activity != null)
            {
                IReadOnlyCollection<string> tsNames = Activity.TimeSeriesNames;

                tsXcomboBox.Items.AddRange(tsNames.ToArray());
                tsYcomboBox.Items.AddRange(tsNames.ToArray());
            }
            else
            {
                IReadOnlyCollection<string> tsNames = Database.CurrentAthlete.AllTimeSeriesNames;

                tsXcomboBox.Items.AddRange(tsNames.ToArray());
                tsYcomboBox.Items.AddRange(tsNames.ToArray());
            }
        }

        private Database Database { get; set; }
        private Activity? Activity { get; set; }

        private void buttonExecute_Click(object sender, EventArgs e)
        {
            string tsNameX = tsXcomboBox.Text;
            string tsNameY = tsYcomboBox.Text;
            if (Activity != null)
            {
                LinearRegression? regression = GetLinearRegresion(Activity, tsNameX, tsNameY, null);
                if (regression != null)
                {
                    slopeLabel.Text = $"Slope: {regression.Slope}";
                    r2label.Text = $"RSquared: {regression.RSquared}";
                    interceptYLabel.Text = $"YIntercept: {regression.YIntercept}";
                    TimeSeriesBase tsbx = Activity.TimeSeries[tsNameX];
                    TimeSeriesBase tsby = Activity.TimeSeries[tsNameY];
                    labelXDetails.Text = $"X:  {tsbx.ToStatisticsString()}";
                    labelYDetails.Text = $"Y:  {tsby.ToStatisticsString()}";
                }
                else
                {
                    slopeLabel.Text = r2label.Text = interceptYLabel.Text = "Error";
                }
            }
            else
            {
                LinearRegression? regression = null;
                int failures = 0;
                foreach (KeyValuePair<string, Activity> kvp in Database.CurrentAthlete.Activities)
                {
                    Activity activity = kvp.Value;
                    LinearRegression? next = GetLinearRegresion(activity, tsNameX, tsNameY, regression);
                    if (next != null)
                        regression = next;
                    else
                        failures++;
                }
                if (regression != null)
                {
                    slopeLabel.Text = $"Slope: {regression.Slope}";
                    r2label.Text = $"RSquared: {regression.RSquared}";
                    interceptYLabel.Text = $"YIntercept: {regression.YIntercept}";
                    labelXDetails.Text = $"X Details:  Stanard Deviation {regression.XStandardDeviation}";
                    labelYDetails.Text = $"Y Details:  Stanard Deviation {regression.YStandardDeviation}";
                    MessageBox.Show($"Complete, count {regression.Count}, failures {failures}");
                }
            }
        }

        private LinearRegression? GetLinearRegresion(Activity activityForAnalysis, string tsNameX, string tsNameY, LinearRegression? prior)
        {
            if (activityForAnalysis == null) { return null; }

            if (!activityForAnalysis.TimeSeries.ContainsKey(tsNameX))
                return null;

            if (!activityForAnalysis.TimeSeries.ContainsKey(tsNameY))
                return null;

            TimeSeriesBase tsbx = activityForAnalysis.TimeSeries[tsNameX];
            TimeSeriesBase tsby = activityForAnalysis.TimeSeries[tsNameY];

            if (tsbx.IsValid() && tsby.IsValid())
            {
                TimeValueList? tvlx = tsbx.GetData(0, false);
                TimeValueList? tvly = tsby.GetData(0, false);
                if (tvlx != null && tvly != null)
                {
                    AlignedTimeSeries? aligned = AlignedTimeSeries.Align(tvlx, tvly);

                    if (aligned != null)
                    {
                        LinearRegression? regression = LinearRegression.EvaluateLinearRegression(aligned, false, prior);
                        return regression;
                    }
                }
            }
            return null;
        }

    }
}
