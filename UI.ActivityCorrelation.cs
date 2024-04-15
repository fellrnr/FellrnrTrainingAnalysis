using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using FellrnrTrainingAnalysis.Model;
using pi.science.regression;

namespace FellrnrTrainingAnalysis
{
    public partial class ActivityCorrelation : Form
    {

        FilterActivities? FilterActivities = null;

        public ActivityCorrelation(Database database, Activity? activity, FilterActivities? filterActivities)
        {
            InitializeComponent();
            Database = database;
            Activity = activity;
            FilterActivities = filterActivities;

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

        private int Failures = 0;


        private void buttonExecute_Click(object sender, EventArgs e)
        {

            string tsNameX = tsXcomboBox.Text;
            string tsNameY = tsYcomboBox.Text;
            if (Activity != null)
            {
                LinearRegression? regression = GetLinearRegresion(Activity, tsNameX, tsNameY);
                if (regression != null)
                {
                    slopeLabel.Text = $"Slope: {regression.Slope}";
                    r2label.Text = $"RSquared: {regression.RSquared}";
                    interceptYLabel.Text = $"YIntercept: {regression.YIntercept}";
                    TimeSeriesBase tsbx = Activity.TimeSeries[tsNameX];
                    TimeSeriesBase tsby = Activity.TimeSeries[tsNameY];
                    labelXDetails.Text = $"X:  {tsbx.ToStatisticsString()}";
                    labelYDetails.Text = $"Y:  {tsby.ToStatisticsString()}";
                    this.Text = $"Done";
                }
                else
                {
                    slopeLabel.Text = r2label.Text = interceptYLabel.Text = "Error";
                    this.Text = $"Error";
                }
                dataGridView1.DataSource = null;
            }
            else if (FilterActivities != null)
            {
                Failures = 0;
                List<LinearRegression> listLRs = new List<LinearRegression>();
                List<Activity> activities = FilterActivities.GetActivities(Database);

                foreach (Activity activity in activities)
                {

                    LinearRegression? next = GetLinearRegresion(activity, tsNameX, tsNameY);
                    if (next != null)
                    {
                        next.StravaId = activity.PrimaryKey();
                        next.ActivityStart = activity.StartDateTimeLocal;
                        if (next.RSquared > (double)numericUpDownMinR2.Value)
                        {
                            listLRs.Add(next);
                        }
                    }
                }
                LinearRegression regression = LinearRegression.AverageLinearRegressionList(listLRs);

                double[] slopes = listLRs.Select(x => x.Slope).ToArray();
                double sdSlope = Utils.Misc.StandardDeviation(slopes);

                double[] intercepts = listLRs.Select(x => x.Slope).ToArray();
                double sdIntercepts = Utils.Misc.StandardDeviation(intercepts);

                slopeLabel.Text = $"Slope: {regression.Slope:F3} SD: {sdSlope:F3}";
                r2label.Text = $"RSquared: {regression.RSquared:F3}";
                interceptYLabel.Text = $"YIntercept: {regression.YIntercept:F3} SD: {sdIntercepts:F3}";
                labelXDetails.Text = $"X Details:  Stanard Deviation {regression.XStandardDeviation:F3}";
                labelYDetails.Text = $"Y Details:  Stanard Deviation {regression.YStandardDeviation:F3}";
                this.Text = $"Processed {listLRs.Count} with {Failures} failures";

                DataTable dt = Utils.Misc.ConvertToDataTable(listLRs);
                dataGridView1.DataSource = dt;
            }
        }

        private LinearRegression? GetLinearRegresion(Activity activityForAnalysis, string tsNameX, string tsNameY)
        {
            if (activityForAnalysis == null) { return null; }

            if (!activityForAnalysis.TimeSeries.ContainsKey(tsNameX))
                return null;

            if (!activityForAnalysis.TimeSeries.ContainsKey(tsNameY))
                return null;

            TimeSeriesBase tsbx = activityForAnalysis.TimeSeries[tsNameX];
            TimeSeriesBase tsby = activityForAnalysis.TimeSeries[tsNameY];


            //All
            //Virtual
            //Recorded
            if (tsbx.IsVirtual() && comboBoxFilterX.Text == "Recorded")
                return null;
            if (!tsbx.IsVirtual() && comboBoxFilterX.Text == "Virtual")
                return null;
            if (tsby.IsVirtual() && comboBoxFilterY.Text == "Recorded")
                return null;
            if (!tsby.IsVirtual() && comboBoxFilterY.Text == "Virtual")
                return null;

            if (tsbx.IsValid() && tsby.IsValid())
            {
                TimeValueList? tvlx = tsbx.GetData(0, false);
                TimeValueList? tvly = tsby.GetData(0, false);
                if (tvlx != null && tvly != null)
                {
                    AlignedTimeSeries? aligned = AlignedTimeSeries.Align(tvlx, tvly);

                    if (aligned != null)
                    {
                        LinearRegression? regression = LinearRegression.EvaluateLinearRegression(aligned,
                                                                                                 primaryIsX: false,
                                                                                                 ignoreZerosX: checkBoxIgnore0X.Checked,
                                                                                                 ignoreZerosY: checkBoxIgnore0Y.Checked);
                        return regression;
                    }
                    else
                    {
                        Failures++;
                    }
                }
            }
            return null;
        }

    }
}
