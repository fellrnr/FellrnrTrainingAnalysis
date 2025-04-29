using CsvHelper;
using de.schumacher_bw.Strava.Endpoint;
using FellrnrTrainingAnalysis.Action;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace FellrnrTrainingAnalysis
{
    public partial class FellrnrTrainingAnalysisForm : Form
    {
        private Model.Database Database { get; set; }

        UI.Goals GoalsUI { get; set; }


        FilterString? CurrentTypeFilter = null;
        //dialog filters are added to the current filter
        FilterActivities DialogFilterActivities = new FilterActivities(); //the default filter
        FilterActivities ToggleFilterActivities = new FilterActivities(); //the default filter
        FilterBadData? CurrentFilterBadData = null;
        FilterActivities CurrentFilterActivities = new FilterActivities(); //the default filter
        ActivityFilterDialog? activityFilterDialog;

        private UI.ProgressDialog ProgressDialog = new UI.ProgressDialog();

        public FellrnrTrainingAnalysisForm(bool StravaSync, bool email, bool batch)
        {
            Logging.Instance.TraceEntry("FellrnrTrainingAnalysisForm");
            //TODO: consider using http://dockpanelsuite.com/
            InitializeComponent();
            Options.LoadConfig();

            FilterActivities? filter = FilterActivities.LoadFilters();
            if (filter != null) { DialogFilterActivities = filter; }

            showErrorsToolStripMenuItem.Enabled = false;
            Database = Database.LoadFromFile();
            GoalsUI = new UI.Goals(Database, goalsDataGridView, goalsTextBox);

            //moved some construction activities to load event to prevent errors with no room for display

            if (StravaSync)
            {
                Action.StravaApi.Instance.SyncNewActivites(Database);
            }
            if (email)
            {
                GoalsUI.SendGoals();
            }
            if (batch)
            {
                ExitApp(true);
            }

            activityReport1.UpdateViews += UpdateViewsEventHandler;

            timerSyncStrava.Interval = 20 * 60 * 1000; //20 min
            timerSyncStrava.Start();

            //Model.Goal.test();
            Logging.Instance.TraceLeave();
        }


        private void FellrnrTrainingAnalysisForm_Load(object sender, EventArgs e)
        {
            Logging.Instance.Log("Entering FellrnrTrainingAnalysisForm_Load");
            Logging.Instance.ResetAndStartTimer("FellrnrTrainingAnalysisForm_Load");
            SetMenuAvailability();

            UpdateViews();
            AddDataQualityMenus();
            Logging.Instance.Log(string.Format("FellrnrTrainingAnalysisForm_Load took {0}", Logging.Instance.GetAndResetTime("FellrnrTrainingAnalysisForm_Load")));
        }


        public void FilterChangedCallbackEventHandler(FilterActivities filterActivities)
        {
            DialogFilterActivities = filterActivities;
            DialogFilterActivities.SaveFilters();
            UpdateViews();
        }

        private void UpdateFilters()
        {
            CurrentFilterActivities.Filters.Clear();
            CurrentFilterActivities.Filters.AddRange(DialogFilterActivities.Filters);
            if (CurrentFilterBadData != null)
                CurrentFilterActivities.Filters.Add(CurrentFilterBadData);

            if (!string.IsNullOrEmpty(Options.Instance.OnlyShowActivityTypes) && !Options.Instance.OnlyShowActivityTypes.Contains("All"))
            {
                CurrentTypeFilter = new FilterString(Activity.TagType, "in", Options.Instance.OnlyShowActivityTypes);
                CurrentFilterActivities.Filters.Add(CurrentTypeFilter);
            }

        }

        private void UpdateViews(bool recalculate = true)
        {
            Logging.Instance.TraceEntry("UpdateViews");
            //if(force) { progressBar1.Minimum = 0; progressBar1.Maximum = Database.CurrentAthlete.Activities.Count; }

            if (recalculate)
                Database.MasterRecalculate(false, false, false); //forced recalculation done by our caller

            UpdateFilters();
            UpdateShowOnlyMenu();
            UpdateSummary();
            UpdateGoals();

            activityReport1.UpdateReport(Database, CurrentFilterActivities);
            activityTree1.Display(Database);

            progressGraph1.Display(Database, CurrentFilterActivities);

            overviewMap1.Display(Database, CurrentFilterActivities);

            if (Logging.Instance.HasErrors)
            {
                showErrorsToolStripMenuItem.Enabled = true;
                showErrorsToolStripMenuItem.ForeColor = Color.Red;
            }
            Logging.Instance.TraceLeave();
        }


        Dictionary<string, ToolStripMenuItem> showOnlyStripMenuItems = new Dictionary<string, ToolStripMenuItem>();

        private void UpdateShowOnlyMenu()
        {
            IReadOnlyCollection<String> types = Database.CurrentAthlete.AllActivityTypes;
            AddShowOnlyMenu("All");
            foreach (string s in types)
            {
                AddShowOnlyMenu(s);
            }
            showOnlyToolStripMenuItem.DropDownItems.AddRange(showOnlyStripMenuItems.Values.ToArray());
        }

        private void AddShowOnlyMenu(string type)
        {
            if (!showOnlyStripMenuItems.ContainsKey(type))
            {
                ToolStripMenuItem toolStripItem3 = new ToolStripMenuItem();
                toolStripItem3.Tag = type;
                toolStripItem3.Text = type;
                toolStripItem3.CheckOnClick = true;
                toolStripItem3.Click += showOnlyToolStripMenuItem_Click;
                showOnlyStripMenuItems.Add(type, toolStripItem3);
            }
        }

        private void UpdateGoals()
        {
            Logging.Instance.TraceEntry("UpdateGoals");
            GoalsUI.UpdateGoalsText();
            GoalsUI.UpdateGoalsGrid();
            Logging.Instance.TraceLeave();
        }
        private void UpdateSummary()
        {
            Logging.Instance.TraceEntry("UpdateSummary");
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("A total of {0} athletes loaded\r\n", Database.Athletes.Count));
            sb.Append(Database.CurrentAthlete);
            summaryTextBox.Text = sb.ToString();
            Logging.Instance.TraceLeave();
        }

        private void SetMenuAvailability()
        {
            /*
            if (StravaApi.Instance.IsConnected)
            {
                connectToStravaToolStripMenuItem.Enabled = false;
            }
            else
            {
                syncWithStravaToolStripMenuItem.Enabled = false;
            }

            */
        }



        private void FellrnrTrainingAnalysis_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logging.Instance.Close(); //must be last as other things write to logging
        }



        private void ExitApp(bool force)
        {
            Database.SaveToMemoryPackFile();
            Options.SaveConfig();
            DialogFilterActivities.SaveFilters();
            ActivityDatumMetadata.WriteToCsv();
            TimeSeriesDefinition.WriteToCsv();
            if (force)
            {
                Environment.Exit(0);
            }
            else
            {
                //app exit doesn't work if there's a dialog open.
                Environment.Exit(0);
                //Application.Exit();
            }
        }


        public void TimeSeriesEditEventHandler(TimeSeriesDefinitionEditor sender) //a callback from clients
        {
            List<Model.TimeSeriesDefinition>? definitions = sender.Definitions;
            TimeSeriesDefinition.SetDefinitions(definitions);
            activityReport1.UpdateSelectedRow(); //only update the grpah
        }

        public void UpdateViewsEventHandler() //a callback from clients
        {
            UpdateViews();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == activityTreeTabPage)
            {
                activityTree1.ShowNow(Database);

            }
        }

    }

}
