using FellrnrTrainingAnalysis.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class LapDetails : UserControl
    {
        public LapDetails()
        {
            InitializeComponent();
        }

        public void DisplayActivity(Athlete athlete, Model.Activity? activity)
        {
            if (activity == null)
                return;

            lapPanel.SuspendLayout();
            lapPanel.Visible = false;
            lapPanel.Controls.Clear();
            lapPanel.RowCount = 0;
            lapPanel.ColumnCount = 0;

            //if(lapPanel.ColumnCount < 2)
            {
                int colnum = 0;
                AddColumn("Lap #", colnum++);
                AddColumn("Time", colnum++);
                AddColumn("Elapsed", colnum++);
                foreach (TimeSeriesBase tsb in activity.TimeSeries.Values)
                {
                    AddColumn(tsb.Name, colnum++);
                }
            }

            DateTime startTime = activity.StartDateTimeUTC ?? DateTime.Now;
            Dictionary<string, float> previous = new Dictionary<string, float>();
            foreach (Lap lap in activity.Laps)
            {
                int row = ++lapPanel.RowCount;
                lapPanel.Controls.Add(new Label { Text = row.ToString(), Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true }, 0, row);
                TimeSpan lapspan = lap.LapTime;
                lapPanel.Controls.Add(new Label { Text = lapspan.ToString(@"hh\:mm\:ss"), Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true }, 1, row);
                DateTime elapsed = startTime.Add(lap.ElapsedTime);
                lapPanel.Controls.Add(new Label { Text = elapsed.ToString(@"hh\:mm\:ss"), Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true }, 2, row);
                int colnum = 3;
                foreach (TimeSeriesBase tsb in activity.TimeSeries.Values)
                {
                    TimeValueList? data = tsb.GetData();


                    string valueText = "N/A";
                    string tooltip = tsb.Name;
                    if (data != null && TimeValueList.ExtractWindow(data, lap.StartSeconds, lap.EndSeconds) is TimeValueList sub && sub != null)
                    {
                        if (TimeSeriesDefinition.FindTimeSeriesDefinition(tsb.Name) is TimeSeriesDefinition definition && definition != null)
                        {
                            switch (definition.LapMath)
                            {
                                case TimeSeriesDefinition.LapMathType.Mean:
                                    valueText = sub.Percentile(TimeValueList.StaticsValue.Mean).ToString("F2");
                                    tooltip = sub.ToStatisticsString();
                                    break;
                                case TimeSeriesDefinition.LapMathType.Delta:
                                    float p;
                                    if (!previous.TryGetValue(tsb.Name, out p))
                                        p = 0;
                                    float last = sub.Values.LastOrDefault();
                                    float delta = last - p;
                                    valueText = delta.ToString("F2");
                                    previous[tsb.Name] = last;
                                    break;
                                case TimeSeriesDefinition.LapMathType.MinMax:
                                    valueText = sub.Percentile(TimeValueList.StaticsValue.Min).ToString("F2") + "/" + sub.Percentile(TimeValueList.StaticsValue.Max).ToString("F2");
                                    tooltip = sub.ToStatisticsString();
                                    break;
                            }
                        }
                    }
                    Label l = new Label { Text = valueText, Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    toolTip1.SetToolTip(l, tooltip);
                    lapPanel.Controls.Add(l, colnum++, row);
                }
            }
            lapPanel.ResumeLayout();
            lapPanel.Visible = true;
        }

        private void AddColumn(string text, int colnum)
        {
            lapPanel.Controls.Add(new Label { Text = text, Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true }, colnum, 0);
            lapPanel.ColumnCount++;
            lapPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }
    }
}
