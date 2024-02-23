using System.Text;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;
using System.Collections.ObjectModel;
using CsvHelper;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using FellrnrTrainingAnalysis.Action;
using System.ComponentModel;
using de.schumacher_bw.Strava.Endpoint;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FellrnrTrainingAnalysis
{
    public partial class FellrnrTrainingAnalysisForm : Form
    {
        private Model.Database Database { get; set; }

        UI.Goals GoalsUI { get; set; }

        FilterActivities FilterActivities = new FilterActivities(); //the default filter

        private UI.ProgressDialog ProgressDialog = new UI.ProgressDialog();

        public FellrnrTrainingAnalysisForm(bool StravaSync, bool email, bool batch)
        {
            Logging.Instance.TraceEntry("FellrnrTrainingAnalysisForm");
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
            Logging.Instance.TraceLeave();
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


        public void CallbackEventHandler(FilterActivities filterActivities)
        {
            FilterActivities = filterActivities;
            UpdateViews(false);
        }

        private void UpdateViews(bool force)
        {
            Logging.Instance.TraceEntry("UpdateViews");
            //if(force) { progressBar1.Minimum = 0; progressBar1.Maximum = Database.CurrentAthlete.Activities.Count; }
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
            Logging.Instance.TraceLeave();
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
            Database.SaveToMemoryPackFile();
        }



        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitApp(false);
        }

        private void ExitApp(bool force)
        {
            Database.SaveToMemoryPackFile();
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


        //TODO: Load bin or memory map files doesn't work (does nothing)
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

                    Database = Database.LoadFromMemoryMapFile(filePath);
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
                saveFileDialog.Filter = "*.bin_mp|*.bin_mp|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = saveFileDialog.FileName;

                    Database.SaveToMemoryPack(filePath);
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

            foreach (KeyValuePair<DateTime, Activity> kvp in Database.CurrentAthlete.ActivitiesByLocalDateTime)
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

            //TODO: this doesn't add the bad data filter to the filter dialog
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

        private void openBinDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                openFileDialog.InitialDirectory = folder;
                openFileDialog.Filter = "*.bin|*.bin|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.ReadOnlyChecked = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    Database = Database.LoadFromBinaryFile(filePath);
                    //Database.LoadFromMemoryMapFile(filePath);
                }
            }

        }

        private void saveAsBinDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
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

                    Database.SaveToBinaryFile(filePath);
                }

            }
        }

        private class WeightData
        {
            public DateTime Date { get; set; }
            public float Recorded { get; set; }
        }
        private void loadWeightDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                openFileDialog.InitialDirectory = folder;
                openFileDialog.Filter = "*.csv|*.csv|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.ReadOnlyChecked = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;
                    using (var reader = new StreamReader(filePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        var records = csv.GetRecords<WeightData>();
                        foreach (var record in records)
                        {
                            DateTime date = record.Date;
                            float weight = record.Recorded;

                            Model.Day day = Database.CurrentAthlete.GetOrAddDay(date);
                            day.AddOrReplaceDatum(new TypedDatum<float>(Model.Day.WeightTag, true, weight));
                        }
                    }
                }
            }

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == activityTreeTabPage)
            {
                activityTree1.ShowNow(Database);

            }
        }

        SportTracks? SportTracksProcessor = null;

        private void verifyAgainstFitlogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SportTracksProcessor = new SportTracks();
            //string filePath = @"C:\Users\jfsav\OneDrive\Jonathan\FitLog From SportTracks\Jonathan-2015.fitlog";
            //fitlog.ReadFitlog(filePath);

            string filePath = @"C:\Users\jfsav\OneDrive\Jonathan\FitLog From SportTracks";
            SportTracksProcessor.ReadFitlogFolder(filePath);
            
            SportTracksProcessor.Verify(Database.CurrentAthlete);
            LargeTextDialogForm large = new LargeTextDialogForm(SportTracksProcessor.Results);
            large.ShowDialog();

            /*
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                //openFileDialog.InitialDirectory = folder;
                openFileDialog.Filter = "*.fitlog|*.fitlog|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.ReadOnlyChecked = true;
                //openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;
                    Fitlog fitlog = new Fitlog();
                    fitlog.ReadFitlog(filePath, Database.CurrentAthlete);
                    LargeTextDialogForm large = new LargeTextDialogForm(fitlog.Results);
                    large.ShowDialog();
                }
            }
            */

        }

        private void updateFromFitlogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(SportTracksProcessor != null)
            {
                SportTracksProcessor.FixFromFitlog(Database, Database.CurrentAthlete);
                SportTracksProcessor.UpdateFromFitlog(Database, Database.CurrentAthlete, 95);
            }
        }


        //Async Load Strava CSV
        //                                    _                     _    _____ _                           _____  _______      __
        //       /\                          | |                   | |  / ____| |                         / ____|/ ____\ \    / /
        //      /  \   ___ _   _ _ __   ___  | |     ___   __ _  __| | | (___ | |_ _ __ __ ___   ____ _  | |    | (___  \ \  / / 
        //     / /\ \ / __| | | | '_ \ / __| | |    / _ \ / _` |/ _` |  \___ \| __| '__/ _` \ \ / / _` | | |     \___ \  \ \/ /  
        //    / ____ \\__ \ |_| | | | | (__  | |___| (_) | (_| | (_| |  ____) | |_| | | (_| |\ V / (_| | | |____ ____) |  \  /   
        //   /_/    \_\___/\__, |_| |_|\___| |______\___/ \__,_|\__,_| |_____/ \__|_|  \__,_| \_/ \__,_|  \_____|_____/    \/    
        //                  __/ |                                                                                                
        //                 |___/                                                                                                 

        private void loadStravaCsvBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            string filePath = (string)e.Argument!;
            BackgroundWorker worker = (BackgroundWorker)sender!;

            Logging.Instance.TraceEntry("loadStravaCsvBackgroundWorker_DoWork");


            Action.StravaCsvImporter stravaCsvImporter = new Action.StravaCsvImporter();
            int count = stravaCsvImporter.LoadFromStravaArchive(filePath, Database, worker);

            Database.MasterRecalculate(true, worker);

            Logging.Instance.TraceLeave();

            e.Result = count;
        }

        private void loadStravaCsvBackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if(e.UserState != null && e.UserState is Misc.ProgressReport)
            {
                Misc.ProgressReport progress = (Misc.ProgressReport)e.UserState;
                ProgressDialog.TaskName = progress.TaskName;
                ProgressDialog.Maximum = progress.Maximum;
            }
            ProgressDialog.Progress = e.ProgressPercentage;
        }

        private void loadStravaCsvBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            UpdateViews(false); //has to be done in this thread
            ProgressDialog.Hide();
            if (e.Result != null)
            {
                int count = (int)e.Result;
                MessageBox.Show($"Loaded {count} activities from archive");
            }

        }

        private void LoadFromStravaCsv(string filePath)
        {
            ProgressDialog.Progress = 0;
            ProgressDialog.TaskName = "Load FIT Files";
            ProgressDialog.ShowMe();


            loadStravaCsvBackgroundWorker.RunWorkerAsync(filePath);

            //Logging.Instance.TraceEntry("LoadFromStravaCsv");

            //Action.StravaCsvImporter stravaCsvImporter = new Action.StravaCsvImporter();
            //int count = stravaCsvImporter.LoadFromStravaArchive(filePath, Database, ProgressDialog);

            //UpdateViews(true);

            //Logging.Instance.TraceLeave();

            //MessageBox.Show($"Loaded {count} activities from archive");
        }

    }

}
