using BrightIdeasSoftware;
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

namespace FellrnrTrainingAnalysis.UI
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
        int PageSize = 20;
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
                //activityDataGridView.Rows[activityDataGridView.Rows.Count - 1].Selected = true;
                SelectPreviouslySelectedRow();
            }

            IgnoreSelectionChanged = false;
            UpdateSelectedRow(); //now manually update
            Logging.Instance.TraceLeave();
        }



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

        private string? selectedPrimaryKey = null;
        private void SelectPreviouslySelectedRow()
        {
            if (selectedPrimaryKey == null)
            {
                activityDataGridView.Rows[activityDataGridView.Rows.Count - 1].Selected = true;
                return;
            }
            //activityDataGridView.Rows[activityDataGridView.Rows.Count - 1].Selected = true;
            var index = activityDataGridView.Columns[Activity.TagPrimarykey]?.Index;
            if (index != null && index != -1)
            {
                foreach (DataGridViewRow row in activityDataGridView.Rows)
                {
                    string primarykey = (string)row.Cells[index.Value].Value;
                    if (primarykey == selectedPrimaryKey)
                    {
                        row.Selected = true;
                        return; //only select one row
                    }
                }
            }
            //if we didn't select any row, do the last one
            activityDataGridView.Rows[activityDataGridView.Rows.Count - 1].Selected = true;
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
                if (activity != null)
                    selectedPrimaryKey = activity.PrimaryKey();
            }
            activityData1.DisplayActivity(Database!.CurrentAthlete, activity); //if we've got here, we have to have a database with an athlete
            activityMap1.DisplayActivity(activity, Database!.Hills);
            powerDistributionCurveGraph1.DisplayActivity(activity);

            Logging.Instance.TraceLeave();
        }

        public void UpdateSelectedRow()
        {
            Logging.Instance.TraceEntry("UpdateSelectedRow");

            if (Database == null)
                return;

            DataGridViewSelectedRowCollection dataGridViewSelectedRowCollection = activityDataGridView.SelectedRows;

            Model.Activity? activity = null;
            if (dataGridViewSelectedRowCollection.Count > 0)
            {
                DataGridViewRow row = dataGridViewSelectedRowCollection[0];
                activity = GetActivityForRow(row);
            }

            activityTimeGraph1.UpdateTimeSeriesGraph(activity);
            UpdateActivityDisplay();
            Logging.Instance.TraceLeave();
        }

        private string? GetActivityIdForRow(DataGridViewRow row)
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
                        primarykey = primarykey.Substring(0, primarykey.IndexOf(" ")); //only happens if we've added debug data to the primary key
                    return primarykey;
                }
            }
            return null;
        }

        private Model.Activity? GetActivityForRow(DataGridViewRow row)
        {
            string? primarykey = GetActivityIdForRow(row);

            if (primarykey == null)
                return null;

            if (Database!.CurrentAthlete.Activities.ContainsKey(primarykey)) //should never happen unless we've turned on debugging to add extra data to the primary key column of the table. 
            {
                Model.Activity activity = Database.CurrentAthlete.Activities[primarykey];
                return activity;
            }
            return null;

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
