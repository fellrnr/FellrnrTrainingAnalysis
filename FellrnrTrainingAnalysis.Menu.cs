using CsvHelper;
using FellrnrTrainingAnalysis.Action;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FellrnrTrainingAnalysis
{
    public partial class FellrnrTrainingAnalysisForm 
    {

        //Database
        #region Database
        private void clearDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Database = new Database();
            UpdateViews();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Database.SaveToMemoryPackFile();
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



        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitApp(false);
        }

        private void forceRecalculationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //this is now fast enough we don't need to ask
            //if (MessageBox.Show("Recalculate all?", "Sure?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                RecalculateAsync(forceActivities: true, forceHills: true, forceGoals: true);
            }
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
        private void integrityCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            Utils.Misc.IntegrityCheck(Database, sb);

            LargeTextDialogForm ltdf = new LargeTextDialogForm(sb.ToString());
            ltdf.ShowDialog();
        }

        #endregion
        #region Data Sources
        //Data sources
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
        private void connectToStravaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Action.StravaApi.Instance.Connect();
            MessageBox.Show("Connected to Strava");
            SetMenuAvailability();
        }

        private void syncWithStravaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SyncWithStrava(false, true);
        }


        private void stravaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SyncWithStrava(true, true);
        }

        private void syncWithStravaAndUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SyncWithStrava(true, true);
        }

        private void pollStravaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timerSyncStrava.Enabled = pollStravaToolStripMenuItem.Checked;
            Logging.Instance.Debug($"timerSyncStrava.Enabled set to {pollStravaToolStripMenuItem.Checked}");
        }

        private void timerSyncStrava_Tick(object sender, EventArgs e)
        {
            Logging.Instance.Debug($"timerSyncStrava_Tick called");
            SyncWithStrava(true, false);
        }


        private void SyncWithStrava(bool updateDescriptions, bool notify)
        {
            Application.UseWaitCursor = true;
            StravaApi.SyncedData synced = Action.StravaApi.Instance.SyncNewActivites(Database);

            Database.MasterRecalculate(forceActivities: false, forceHills: false, forceGoals: true);

            if (updateDescriptions)
            {
                foreach(Activity activity in synced.activities)
                {
                    if(activity.CheckSportType(Activity.ActivityTypeRun))
                    {
                        string? description = activity.UpdatedDescription();
                        if(description != null)
                        {
                            if (!Action.StravaApi.Instance.UpdateActivityDetails(activity, null, description))
                            {
                                Logging.Instance.Error("Update Description Failed (returned false) for " + activity);
                            }
                            activity.Description = description;
                        }
                    }
                }
            }

            Application.UseWaitCursor = false;

            UpdateViews();

            if (notify)
            {
                if (synced.remaining > 0)
                {
                    MessageBox.Show($"Synced {synced.synced} activities, with {synced.remaining} remaining");
                }
                else
                {
                    MessageBox.Show($"Synced {synced.synced} activities");
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
                            day.AddOrReplaceDatum(new TypedDatum<float>(Model.Day.TagWeight, true, weight));
                        }
                    }
                }
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



        //View
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
        #endregion
        #region View
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

        #endregion
        #region Configure

        //Configure
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog configForm = new OptionsDialog();
            configForm.ShowDialog();
            UpdateViews();//TODO: update this to be non-modal with a callback

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

        #endregion
        #region Show Errors
        //Show errors
        private void showErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string errorText = Logging.Instance.Error();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(errorText);
            largeTextDialogForm.ShowDialog();
            showErrorsToolStripMenuItem.Enabled = false;
        }
        #endregion
        #region Tools


        //Tools
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

        private void clearDataQualityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ClearDataQualityIssues();
            UpdateViews();
        }


        private void ClearDataQualityIssues()
        {
            if (Database == null || Database.CurrentAthlete == null) { return; }

            CurrentFilterBadData = null;

            foreach (KeyValuePair<DateTime, Model.Activity> kvp in Database.CurrentAthlete.ActivitiesByLocalDateTime)
            {
                Model.Activity activity = kvp.Value;
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

        private void exploreGlobalRelationshipsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActivityCorrelation activityCorrelation = new ActivityCorrelation(Database!, null, CurrentFilterActivities);
            activityCorrelation.Show();
        }

        private void experimentalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Athlete athlete = Database.CurrentAthlete;
            int count = 0;
            foreach (var kvp in athlete.Activities)
            {
                Model.Activity activity = kvp.Value as Model.Activity;

                if (activity.LocationStream != null && !activity.TimeSeries.ContainsKey("Altitude"))
                {
                    count++;
                }
            }
            MessageBox.Show($"activities with GPS but no altitude {count}");

            /*
            Athlete athlete = Database.CurrentAthlete;

            double sumTime = 0;
            double sumCount = 0;
            int countOne = 0;
            int countNon = 0;
            int countVOne = 0;
            int countVNon = 0;
            foreach (var kvp in athlete.Activities)
            {
                Activity activity = kvp.Value as Activity;
                foreach(var kvp2 in activity.TimeSeries)
                {
                    TimeSeriesBase timeSeriesBase = kvp2.Value as TimeSeriesBase;

                    if(timeSeriesBase != null && timeSeriesBase.IsValid() && !timeSeriesBase.IsVirtual())
                    {
                        TimeValueList? timeValueList = timeSeriesBase.GetData();
                        if(timeValueList != null)
                        {
                            uint lastTime = timeValueList.Times.Last();
                            int count = timeValueList.Length;
                            if (count == lastTime)
                                countOne++;
                            else
                                countNon++;
                            sumCount += count;
                            sumTime += lastTime;

                            for(int i=1; i < count; i++)
                            {
                                if (timeValueList.Times[i] == timeValueList.Times[i-1]+1)
                                    countVOne++;
                                else
                                    countVNon++;
                            }

                        }
                    }
                }
            }

            double avg = sumTime / sumCount;
            double percentOne = ((double)countVOne) / (double)(countVNon + countVOne);
            MessageBox.Show($"Average seconds per recording is {avg:f2}, #1 {countOne:N0}. #!1 {countNon:N0}, %1 sec {percentOne:P}, countVOne {countVOne:N0}, countVNon {countVNon:N0}, count {sumCount:N0}, sum time {sumTime:N0}");
            */
        }

        private void emailGoalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GoalsUI.SendGoals();
        }

        #endregion
        #region Filter

        //Filter

        private void clearFiltersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (activityFilterDialog != null)
                activityFilterDialog.Clear();

            DialogFilterActivities.Filters.Clear();
            UpdateViews(); //will call UpdateFilters which will clear the full filter list

        }

        private void filterToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (activityFilterDialog == null)
                activityFilterDialog = new ActivityFilterDialog();
            activityFilterDialog.UpdatedHandler += FilterChangedCallbackEventHandler;
            activityFilterDialog.Display(Database);
            activityFilterDialog.LoadFromFilterActivities(DialogFilterActivities);
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


        private void fixResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Activity> activities = CurrentFilterActivities.GetActivities(Database);
            List<string> keys = new List<string>();
            foreach (Activity activity in activities)
            {
                string pk = activity.PrimaryKey();
                keys.Add(pk);
            }

            string csv = string.Join(",", keys);

            DialogFilterActivities.Filters.Clear();
            DialogFilterActivities.Filters.Add(new FilterString(Activity.TagPrimarykey, FilterString.IN, csv));

            if (activityFilterDialog != null)
                activityFilterDialog.LoadFromFilterActivities(DialogFilterActivities);

        }

        private void toggleFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterActivities TmpFilterActivities = DialogFilterActivities;
            DialogFilterActivities = ToggleFilterActivities;
            ToggleFilterActivities = TmpFilterActivities;

            if(activityFilterDialog != null)
                activityFilterDialog.LoadFromFilterActivities(DialogFilterActivities);

            UpdateViews(); //will call UpdateFilters which will clear the full filter list
        }


        #endregion





        #region async


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


        private System.Diagnostics.Stopwatch timing = new System.Diagnostics.Stopwatch();

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
            timing.Stop(); 
            UpdateViews(); //has to be done in this thread
            ProgressDialog.Hide();
            if (e.Result != null)
            {
                int count = (int)e.Result;
                MessageBox.Show($"Loaded {count} activities from archive, took {timing.Elapsed}");
            }
            else
            {
                MessageBox.Show($"Recalculation complete, took {timing.Elapsed}");
            }

        }



        private void RecalculateAsync(bool forceActivities = false, bool forceHills = false, bool forceGoals = false)
        {
            timing.Reset(); timing.Start();
            ProgressDialog.Progress = 0;
            ProgressDialog.TaskName = "Recalculate";
            ProgressDialog.ShowMe();

            bool[] recalcs = new bool[3] { forceActivities, forceHills, forceGoals };

            recalculateBackgroundWorker1.RunWorkerAsync(recalcs);
        }

        #endregion


    }
}
