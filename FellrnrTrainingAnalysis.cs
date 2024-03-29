using System.Text;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;
using System.Collections.ObjectModel;
using CsvHelper;
using System.Globalization;
using FellrnrTrainingAnalysis.Action;
using System.ComponentModel;
using de.schumacher_bw.Strava.Endpoint;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Windows.Forms;
using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;

namespace FellrnrTrainingAnalysis
{
    public partial class FellrnrTrainingAnalysisForm : Form
    {
        private Model.Database Database { get; set; }

        UI.Goals GoalsUI { get; set; }


        FilterString? CurrentTypeFilter = null;
        FilterActivities DialogFilterActivities = new FilterActivities(); //the default filter
        FilterBadData? CurrentFilterBadData = null;
        FilterActivities CurrentFilterActivities = new FilterActivities(); //the default filter

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

            if (!string.IsNullOrEmpty(Options.Instance.OnlyShowActivityTypes))
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
            AddShowOnlyMeny("All");
            foreach (string s in types)
            {
                AddShowOnlyMeny(s);
            }
            showOnlyToolStripMenuItem.DropDownItems.AddRange(showOnlyStripMenuItems.Values.ToArray());
        }

        private void AddShowOnlyMeny(string type)
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
            UpdateViews();
        }

        private void clearDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Database = new Database();
            UpdateViews();
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

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog configForm = new OptionsDialog();
            configForm.ShowDialog();
            UpdateViews();//TODO: update this to be non-modal with a callback

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

        private void showAccumulatedTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Logging.Instance.DumpAndResetAccumulators();
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
            List<Model.TimeSeriesDefinition>? definitions = TimeSeriesDefinition.GetDefinitions();
            if (definitions != null)
            {
                UI.TimeSeriesDefinitionEditor dataStreamDefinitionEditor = new UI.TimeSeriesDefinitionEditor(definitions);
                dataStreamDefinitionEditor.Edited += TimeSeriesEditEventHandler;

                dataStreamDefinitionEditor.ShowDialog(); //TODO: update this to be non-modal with a callback
                UpdateViews();
            }
            else
            {
                MessageBox.Show("Oops, no definitions to edit");
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



        private void clearDataQualityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDataQualityIssues();
            UpdateViews();
        }


        private void ClearDataQualityIssues()
        {
            if (Database == null || Database.CurrentAthlete == null) { return; }

            CurrentFilterBadData = null;

            foreach (KeyValuePair<DateTime, Activity> kvp in Database.CurrentAthlete.ActivitiesByLocalDateTime)
            {
                Activity activity = kvp.Value;
                activity.ClearDataQualityIssues();
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
            CurrentFilterBadData = new FilterBadData(FilterBadData.HasBadValueTag);
            UpdateViews();
        }

        private void fixQualitToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            if (sender == null) { return; }
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            if (toolStripMenuItem.Tag == null) { return; }
            DataQualityCheck dataQualityCheck = (DataQualityCheck)toolStripMenuItem.Tag;
            rescanForDataQualityIssues(dataQualityCheck, true);

            CurrentFilterBadData = new FilterBadData(FilterBadData.HasBadValueTag);
            UpdateViews();
        }

        ActivityFilterDialog? activityFilterDialog;

        private void rescanForDataQualityIssuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            rescanForDataQualityIssues();
        }

        private void rescanForDataQualityIssues(DataQualityCheck? dataQualityCheck = null, bool fix = false)
        {
            ClearDataQualityIssues();
            Utils.DataQuality dataQuality = new DataQuality();
            List<string> badStreams = dataQuality.FindBadTimeSeries(Database, dataQualityCheck, fix);

            UpdateViews();

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
            if (SportTracksProcessor != null)
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

            Database.MasterRecalculate(true, true, true, worker);

            Logging.Instance.TraceLeave();

            e.Result = count;
        }

        private void recalculateBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            bool[] recalcs = (bool[])e.Argument!;
            BackgroundWorker worker = (BackgroundWorker)sender!;

            Logging.Instance.TraceEntry("loadStravaCsvBackgroundWorker_DoWork");

            Database.MasterRecalculate(forceActivities: recalcs[0], forceHills: recalcs[1], forceGoals: recalcs[2], worker); 

            Logging.Instance.TraceLeave();

            e.Result = null;
        }


        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            if (e.UserState != null && e.UserState is Misc.ProgressReport)
            {
                Misc.ProgressReport progress = (Misc.ProgressReport)e.UserState;
                ProgressDialog.TaskName = progress.TaskName;
                ProgressDialog.Maximum = progress.Maximum;
            }
            if (e.ProgressPercentage < ProgressDialog.Maximum)
                ProgressDialog.Progress = e.ProgressPercentage;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            UpdateViews(); //has to be done in this thread
            ProgressDialog.Hide();
            if (e.Result != null)
            {
                int count = (int)e.Result;
                MessageBox.Show($"Loaded {count} activities from archive, took {Logging.Instance.GetAndStopTime("Async")}");
            }
            else
            {
                MessageBox.Show($"Recalculation complete, took {Logging.Instance.GetAndStopTime("Async")}");
            }

        }

        private void LoadFromStravaCsv(string filePath)
        {
            Logging.Instance.ResetAndStartTimer("Async");
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

        private void filterToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (activityFilterDialog == null)
                activityFilterDialog = new ActivityFilterDialog();
            activityFilterDialog.UpdatedHandler += FilterChangedCallbackEventHandler;
            activityFilterDialog.Display(Database);
            activityFilterDialog.Show();

        }

        private void showOnlyToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            List<string> includedTypes = new List<string>();
            foreach (KeyValuePair<string, ToolStripMenuItem> kvp in showOnlyStripMenuItems)
            {
                if (kvp.Value != null && kvp.Value.Checked)
                {
                    includedTypes.Add(kvp.Key);
                }
            }
            if (includedTypes.Count > 0)
            {
                string csv = string.Join(',', includedTypes);
                Options.Instance.OnlyShowActivityTypes = csv;
                UpdateViews();
            }
        }

        private void RecalculateAsync(bool forceActivities = false, bool forceHills = false, bool forceGoals = false)
        {
            Logging.Instance.ResetAndStartTimer("Async");
            ProgressDialog.Progress = 0;
            ProgressDialog.TaskName = "Recalculate";
            ProgressDialog.ShowMe();

            bool[] recalcs = new bool[3] { forceActivities, forceHills, forceGoals };

            recalculateBackgroundWorker1.RunWorkerAsync(recalcs);
        }
        private void forceRecalculationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecalculateAsync(forceActivities: true, forceHills: true, forceGoals: true);
        }


        private void recalculateHillsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecalculateAsync(forceActivities: false, forceHills: true, forceGoals: false);
        }


        private void recalculateGoalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RecalculateAsync(forceActivities: false, forceHills: false, forceGoals: true);
        }

        private void recalculateActivitiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //force goals is quick, so always do it
            RecalculateAsync(forceActivities: true, forceHills: false, forceGoals: true);
        }

        private void activityReport1_Load(object sender, EventArgs e)
        {

        }

        private void integrityCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            Utils.Misc.IntegrityCheck(Database, sb);

            LargeTextDialogForm ltdf = new LargeTextDialogForm(sb.ToString());
            ltdf.ShowDialog();
        }

    }

}
