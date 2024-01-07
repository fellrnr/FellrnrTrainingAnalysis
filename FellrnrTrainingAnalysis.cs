using System.Text;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;
using System.Collections.ObjectModel;

namespace FellrnrTrainingAnalysis
{
    public partial class FellrnrTrainingAnalysisForm : Form
    {
        private Model.Database Database { get; set; }




        public FellrnrTrainingAnalysisForm(bool StravaSync, bool email, bool batch)
        {
            Logging.Instance.Enter("FellrnrTrainingAnalysisForm");
            //TODO: consider using http://dockpanelsuite.com/
            InitializeComponent();
            Options.LoadConfig();
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

            //Model.Goal.test();
            Logging.Instance.Leave();
        }

        private void AddDataQualityMenus()
        {
            DataQuality dataQuality = new DataQuality();
            ReadOnlyCollection<DataQualityCheck> CheckList = dataQuality.CheckList;
            foreach (DataQualityCheck check in CheckList)
            {
                ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Tag = check;
                toolStripMenuItem.Text = check.Description;
                toolStripMenuItem.Click += checkQualityToolStripMenuItem_Click;
                scanForDataQualityIssueToolStripMenuItem.DropDownItems.Add(toolStripMenuItem);
            }
            foreach (DataQualityCheck check in CheckList)
            {
                ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem();
                toolStripMenuItem.Tag = check;
                toolStripMenuItem.Text = check.Description;
                toolStripMenuItem.Click += fixQualitToolStripMenuItem_Click;
                fixDataQualityIssueToolStripMenuItem.DropDownItems.Add(toolStripMenuItem);
            }
        }

        private void FellrnrTrainingAnalysisForm_Load(object sender, EventArgs e)
        {
            Logging.Instance.Log("Entering FellrnrTrainingAnalysisForm_Load");
            Logging.Instance.StartResetTimer("FellrnrTrainingAnalysisForm_Load");
            SetMenuAvailability();

            UpdateViews(false);
            AddDataQualityMenus();
            Logging.Instance.Log(string.Format("FellrnrTrainingAnalysisForm_Load took {0}", Logging.Instance.GetAndResetTime("FellrnrTrainingAnalysisForm_Load")));
        }


        UI.Goals GoalsUI { get; set; }

        FilterActivities FilterActivities = new FilterActivities(); //the default filter


        public void CallbackEventHandler(FilterActivities filterActivities)
        {
            FilterActivities = filterActivities;
            UpdateViews(false);
        }

        private void UpdateViews(bool force)
        {
            Logging.Instance.Enter("UpdateViews");
            Database.MasterRecalculate(force);

            UpdateSummary();
            UpdateGoals();

            activityReport1.UpdateReport(Database, FilterActivities);
            activityTree1.Display(Database);

            progressGraph1.Display(Database, FilterActivities);

            overviewMap1.Display(Database, FilterActivities);

            if (Logging.Instance.HasErrors)
            {
                showErrorsToolStripMenuItem.Enabled = true;
                showErrorsToolStripMenuItem.ForeColor = Color.Red;
            }
            Logging.Instance.Leave();
        }

        private void UpdateGoals()
        {
            Logging.Instance.Enter("UpdateGoals");
            GoalsUI.UpdateGoalsText();
            GoalsUI.UpdateGoalsGrid();
            Logging.Instance.Leave();
        }
        private void UpdateSummary()
        {
            Logging.Instance.Enter("UpdateSummary");
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("A total of {0} athletes loaded\r\n", Database.Athletes.Count));
            sb.Append(Database.CurrentAthlete);
            summaryTextBox.Text = sb.ToString();
            Logging.Instance.Leave();
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

        private void loadFromStravaCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "profile.csv|profile*.csv|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                //openFileDialog.RestoreDirectory = true;
                openFileDialog.ReadOnlyChecked = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    LoadFromStravaCsv(filePath);
                }
            }
        }

        private void LoadFromStravaCsv(string filePath)
        {
            Logging.Instance.Enter("LoadFromStravaCsv");

            Action.StravaCsvImporter stravaCsvImporter = new Action.StravaCsvImporter();
            int count = stravaCsvImporter.LoadFromStravaArchive(filePath, Database);
            UpdateViews(true);

            Logging.Instance.Leave();

            MessageBox.Show($"Loaded {count} activities from archive");
        }

        private void FellrnrTrainingAnalysis_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logging.Instance.Close(); //must be last as other things write to logging
        }


        private void connectToStravaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Action.StravaApi.Instance.Connect();
            MessageBox.Show("Connected to Strava");
            SetMenuAvailability();
        }

        private void syncWithStravaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.UseWaitCursor = true;
            Tuple<int, int> count = Action.StravaApi.Instance.SyncNewActivites(Database);
            Application.UseWaitCursor = false;

            MessageBox.Show($"Synced {count.Item1} activities, with at least {count.Item2} remaining");
            UpdateViews(false);
        }

        private void clearDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Database = new Database();
            UpdateViews(false);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Database.SaveToFile();
        }



        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitApp(false);
        }

        private void ExitApp(bool force)
        {
            Database.SaveToFile();
            Options.SaveConfig();
            ActivityDatumMetadata.WriteToCsv();
            DataStreamDefinition.WriteToCsv();
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

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog configForm = new OptionsDialog();
            configForm.ShowDialog();
            UpdateViews(false);//TODO: update this to be non-modal with a callback

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                openFileDialog.InitialDirectory = folder;
                //openFileDialog.Filter = "*.bin|*.bin|All files (*.*)|*.*";
                openFileDialog.Filter = "*.bin_mp|*.bin_mp|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.ReadOnlyChecked = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    Database.LoadFromFile(filePath);
                    //Database.LoadFromMemoryMapFile(filePath);
                }
            }
        }

        private void saveDatabaseAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                saveFileDialog.InitialDirectory = folder;
                saveFileDialog.Filter = "*.bin|*.bin|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = saveFileDialog.FileName;

                    Database.SaveToFile(filePath);
                }

            }
        }

        private void forceRecalculationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateViews(true);
            MessageBox.Show("Recalculation complete");
        }

        private void showErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string errorText = Logging.Instance.Error();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(errorText);
            largeTextDialogForm.ShowDialog();
            showErrorsToolStripMenuItem.Enabled = false;
        }



        private void clearLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.Instance.HasErrors = false;
            Logging.Instance.Clear();
        }

        private void debugLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logText = Logging.Instance.Debug();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(logText);
            largeTextDialogForm.ShowDialog();
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logText = Logging.Instance.Log();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(logText);
            largeTextDialogForm.ShowDialog();
        }

        private void errorLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string debugText = Logging.Instance.Error();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(debugText);
            largeTextDialogForm.ShowDialog();
        }

        private void dataStreamGraphDefinitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Model.DataStreamDefinition>? definitions = DataStreamDefinition.GetDefinitions();
            if (definitions != null)
            {
                UI.DataStreamDefinitionEditor dataStreamDefinitionEditor = new UI.DataStreamDefinitionEditor(definitions);
                dataStreamDefinitionEditor.Edited += DataStreamEditEventHandler;

                dataStreamDefinitionEditor.ShowDialog(); //TODO: update this to be non-modal with a callback
                UpdateViews(false);
            }
            else
            {
                MessageBox.Show("Oops, no definitions to edit");
            }
        }




        public void DataStreamEditEventHandler(DataStreamDefinitionEditor sender) //a callback from clients
        {
            List<Model.DataStreamDefinition>? definitions = sender.Definitions;
            DataStreamDefinition.SetDefinitions(definitions);
            activityReport1.UpdateSelectedRow(); //only update the grpah
        }

        public void UpdateViewsEventHandler() //a callback from clients
        {
            UpdateViews(false);
        }



        private void clearDataQualityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDataQualityIssues();
            UpdateViews(false);
        }


        private FilterBadData aFilterBadData = new FilterBadData(FilterBadData.HasBadValueTag);
        private void ClearDataQualityIssues()
        {
            if (Database == null || Database.CurrentAthlete == null) { return; }

            FilterActivities.Filters.Remove(aFilterBadData);

            foreach (KeyValuePair<DateTime, Activity> kvp in Database.CurrentAthlete.ActivitiesByDateTime)
            {
                Activity activity = kvp.Value;

                if (activity.DataQualityIssues != null)
                    activity.DataQualityIssues.Clear();
            }
        }

        private void checkQualityToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender == null) { return; }
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            if (toolStripMenuItem.Tag == null) { return; }
            DataQualityCheck dataQualityCheck = (DataQualityCheck)toolStripMenuItem.Tag;
            rescanForDataQualityIssues(dataQualityCheck);

            FilterActivities.Filters.Clear();
            FilterActivities.Filters.Add(aFilterBadData);
            UpdateViews(false);
        }

        private void fixQualitToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender == null) { return; }
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            if (toolStripMenuItem.Tag == null) { return; }
            DataQualityCheck dataQualityCheck = (DataQualityCheck)toolStripMenuItem.Tag;
            rescanForDataQualityIssues(dataQualityCheck, true);

            FilterActivities.Filters.Clear();
            FilterActivities.Filters.Add(new FilterBadData(FilterBadData.HasBadValueTag));
            UpdateViews(false);
        }

        ActivityFilterDialog? activityFilterDialog;
        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (activityFilterDialog == null)
                activityFilterDialog = new ActivityFilterDialog();
            activityFilterDialog.UpdatedHandler += CallbackEventHandler;
            activityFilterDialog.Display(Database);
            activityFilterDialog.Show();
        }

        private void rescanForDataQualityIssuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rescanForDataQualityIssues();
        }

        private void rescanForDataQualityIssues(DataQualityCheck? dataQualityCheck = null, bool fix = false)
        {
            ClearDataQualityIssues();
            Utils.DataQuality dataQuality = new DataQuality();
            List<string> badStreams = dataQuality.FindBadDataStreams(Database, dataQualityCheck, fix);

            UpdateViews(false);

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("Found {0} bad data streams\r\n", badStreams.Count));
            foreach (string bad in badStreams)
            {
                stringBuilder.AppendLine(bad + "\n");
            }
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(stringBuilder.ToString());
            largeTextDialogForm.ShowDialog();
        }

        private void emailGoalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoalsUI.SendGoals();
        }
    }

}
