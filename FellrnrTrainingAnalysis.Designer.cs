namespace FellrnrTrainingAnalysis
{
    partial class FellrnrTrainingAnalysisForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FellrnrTrainingAnalysisForm));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            openToolStripMenuItem = new ToolStripMenuItem();
            saveToolStripMenuItem = new ToolStripMenuItem();
            saveDatabaseAsToolStripMenuItem = new ToolStripMenuItem();
            clearDatabaseToolStripMenuItem = new ToolStripMenuItem();
            forceRecalculationToolStripMenuItem = new ToolStripMenuItem();
            recalculateActivitiesToolStripMenuItem = new ToolStripMenuItem();
            recalculateGoalsToolStripMenuItem = new ToolStripMenuItem();
            recalculateHillsToolStripMenuItem = new ToolStripMenuItem();
            openBinDatabaseToolStripMenuItem = new ToolStripMenuItem();
            saveBinDatabaseToolStripMenuItem = new ToolStripMenuItem();
            validationToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            dataSourcesToolStripMenuItem = new ToolStripMenuItem();
            bootstrapToolStripMenuItem = new ToolStripMenuItem();
            connectToStravaToolStripMenuItem = new ToolStripMenuItem();
            syncWithStravaToolStripMenuItem = new ToolStripMenuItem();
            loadWeightDataToolStripMenuItem = new ToolStripMenuItem();
            verifyAgainstFitlogToolStripMenuItem = new ToolStripMenuItem();
            updateFromFitlogToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            logToolStripMenuItem = new ToolStripMenuItem();
            normalLogToolStripMenuItem = new ToolStripMenuItem();
            errorLogToolStripMenuItem = new ToolStripMenuItem();
            clearLogsToolStripMenuItem = new ToolStripMenuItem();
            showAccumulatedTimeToolStripMenuItem = new ToolStripMenuItem();
            configureToolStripMenuItem = new ToolStripMenuItem();
            optionsToolStripMenuItem1 = new ToolStripMenuItem();
            dataStreamDefinitionsToolStripMenuItem = new ToolStripMenuItem();
            showErrorsToolStripMenuItem = new ToolStripMenuItem();
            toolsToolStripMenuItem = new ToolStripMenuItem();
            findDataQualityIssuesToolStripMenuItem = new ToolStripMenuItem();
            rescanForDataQualityIssuesToolStripMenuItem = new ToolStripMenuItem();
            scanForDataQualityIssueToolStripMenuItem = new ToolStripMenuItem();
            fixDataQualityIssueToolStripMenuItem = new ToolStripMenuItem();
            emailGoalsToolStripMenuItem = new ToolStripMenuItem();
            exploreGlobalRelationshipsToolStripMenuItem = new ToolStripMenuItem();
            filterToolStripMenuItem = new ToolStripMenuItem();
            filterToolStripMenuItem1 = new ToolStripMenuItem();
            showOnlyToolStripMenuItem = new ToolStripMenuItem();
            stravaToolStripMenuItem = new ToolStripMenuItem();
            graphTabPage = new TabPage();
            progressGraph1 = new UI.ProgressGraph();
            summaryTabPage = new TabPage();
            summaryTextBox = new TextBox();
            tabControl1 = new TabControl();
            activityReport = new TabPage();
            activityReport1 = new ActivityReport();
            activityTreeTabPage = new TabPage();
            splitContainer4 = new SplitContainer();
            activityTree1 = new UI.ActivityTree();
            activityFormsPlot = new ScottPlot.FormsPlot();
            goalsTabPage = new TabPage();
            goalsSplitContainer4 = new SplitContainer();
            goalsTextBox = new TextBox();
            goalsDataGridView = new DataGridView();
            tabPage1 = new TabPage();
            overviewMap1 = new UI.OverviewMap();
            loadStravaCsvBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            recalculateBackgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            experimentalToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            graphTabPage.SuspendLayout();
            summaryTabPage.SuspendLayout();
            tabControl1.SuspendLayout();
            activityReport.SuspendLayout();
            activityTreeTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer4).BeginInit();
            splitContainer4.Panel1.SuspendLayout();
            splitContainer4.Panel2.SuspendLayout();
            splitContainer4.SuspendLayout();
            goalsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)goalsSplitContainer4).BeginInit();
            goalsSplitContainer4.Panel1.SuspendLayout();
            goalsSplitContainer4.Panel2.SuspendLayout();
            goalsSplitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)goalsDataGridView).BeginInit();
            tabPage1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(24, 24);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, dataSourcesToolStripMenuItem, viewToolStripMenuItem, configureToolStripMenuItem, showErrorsToolStripMenuItem, toolsToolStripMenuItem, filterToolStripMenuItem, stravaToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(1831, 39);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { openToolStripMenuItem, saveToolStripMenuItem, saveDatabaseAsToolStripMenuItem, clearDatabaseToolStripMenuItem, forceRecalculationToolStripMenuItem, recalculateActivitiesToolStripMenuItem, recalculateGoalsToolStripMenuItem, recalculateHillsToolStripMenuItem, openBinDatabaseToolStripMenuItem, saveBinDatabaseToolStripMenuItem, validationToolStripMenuItem, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(102, 35);
            fileToolStripMenuItem.Text = "Database";
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(296, 34);
            openToolStripMenuItem.Text = "Open Database...";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // saveToolStripMenuItem
            // 
            saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            saveToolStripMenuItem.Size = new Size(296, 34);
            saveToolStripMenuItem.Text = "Save Database";
            saveToolStripMenuItem.Click += saveToolStripMenuItem_Click;
            // 
            // saveDatabaseAsToolStripMenuItem
            // 
            saveDatabaseAsToolStripMenuItem.Name = "saveDatabaseAsToolStripMenuItem";
            saveDatabaseAsToolStripMenuItem.Size = new Size(296, 34);
            saveDatabaseAsToolStripMenuItem.Text = "Save Database As...";
            saveDatabaseAsToolStripMenuItem.Click += saveDatabaseAsToolStripMenuItem_Click;
            // 
            // clearDatabaseToolStripMenuItem
            // 
            clearDatabaseToolStripMenuItem.Name = "clearDatabaseToolStripMenuItem";
            clearDatabaseToolStripMenuItem.Size = new Size(296, 34);
            clearDatabaseToolStripMenuItem.Text = "Clear Database";
            clearDatabaseToolStripMenuItem.Click += clearDatabaseToolStripMenuItem_Click;
            // 
            // forceRecalculationToolStripMenuItem
            // 
            forceRecalculationToolStripMenuItem.Name = "forceRecalculationToolStripMenuItem";
            forceRecalculationToolStripMenuItem.Size = new Size(296, 34);
            forceRecalculationToolStripMenuItem.Text = "Recalculate All..";
            forceRecalculationToolStripMenuItem.Click += forceRecalculationToolStripMenuItem_Click;
            // 
            // recalculateActivitiesToolStripMenuItem
            // 
            recalculateActivitiesToolStripMenuItem.Name = "recalculateActivitiesToolStripMenuItem";
            recalculateActivitiesToolStripMenuItem.Size = new Size(296, 34);
            recalculateActivitiesToolStripMenuItem.Text = "Recalculate Activities...";
            recalculateActivitiesToolStripMenuItem.Click += recalculateActivitiesToolStripMenuItem_Click;
            // 
            // recalculateGoalsToolStripMenuItem
            // 
            recalculateGoalsToolStripMenuItem.Name = "recalculateGoalsToolStripMenuItem";
            recalculateGoalsToolStripMenuItem.Size = new Size(296, 34);
            recalculateGoalsToolStripMenuItem.Text = "Recalculate Goals...";
            recalculateGoalsToolStripMenuItem.Click += recalculateGoalsToolStripMenuItem_Click;
            // 
            // recalculateHillsToolStripMenuItem
            // 
            recalculateHillsToolStripMenuItem.Name = "recalculateHillsToolStripMenuItem";
            recalculateHillsToolStripMenuItem.Size = new Size(296, 34);
            recalculateHillsToolStripMenuItem.Text = "Recalculate Hills...";
            recalculateHillsToolStripMenuItem.Click += recalculateHillsToolStripMenuItem_Click;
            // 
            // openBinDatabaseToolStripMenuItem
            // 
            openBinDatabaseToolStripMenuItem.Name = "openBinDatabaseToolStripMenuItem";
            openBinDatabaseToolStripMenuItem.Size = new Size(296, 34);
            openBinDatabaseToolStripMenuItem.Text = "Open Bin Database...";
            openBinDatabaseToolStripMenuItem.Click += openBinDatabaseToolStripMenuItem_Click;
            // 
            // saveBinDatabaseToolStripMenuItem
            // 
            saveBinDatabaseToolStripMenuItem.Name = "saveBinDatabaseToolStripMenuItem";
            saveBinDatabaseToolStripMenuItem.Size = new Size(296, 34);
            saveBinDatabaseToolStripMenuItem.Text = "Save As Bin Database...";
            saveBinDatabaseToolStripMenuItem.Click += saveAsBinDatabaseToolStripMenuItem_Click;
            // 
            // validationToolStripMenuItem
            // 
            validationToolStripMenuItem.Name = "validationToolStripMenuItem";
            validationToolStripMenuItem.Size = new Size(296, 34);
            validationToolStripMenuItem.Text = "Integrity Check...";
            validationToolStripMenuItem.Click += integrityCheckToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(296, 34);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // dataSourcesToolStripMenuItem
            // 
            dataSourcesToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { bootstrapToolStripMenuItem, connectToStravaToolStripMenuItem, syncWithStravaToolStripMenuItem, loadWeightDataToolStripMenuItem, verifyAgainstFitlogToolStripMenuItem, updateFromFitlogToolStripMenuItem });
            dataSourcesToolStripMenuItem.Name = "dataSourcesToolStripMenuItem";
            dataSourcesToolStripMenuItem.Size = new Size(132, 35);
            dataSourcesToolStripMenuItem.Text = "Data Sources";
            // 
            // bootstrapToolStripMenuItem
            // 
            bootstrapToolStripMenuItem.Name = "bootstrapToolStripMenuItem";
            bootstrapToolStripMenuItem.Size = new Size(471, 58);
            bootstrapToolStripMenuItem.Text = "Load From Strava CSV...";
            bootstrapToolStripMenuItem.Click += loadFromStravaCsvToolStripMenuItem_Click;
            // 
            // connectToStravaToolStripMenuItem
            // 
            connectToStravaToolStripMenuItem.Image = (Image)resources.GetObject("connectToStravaToolStripMenuItem.Image");
            connectToStravaToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            connectToStravaToolStripMenuItem.Name = "connectToStravaToolStripMenuItem";
            connectToStravaToolStripMenuItem.Size = new Size(471, 58);
            connectToStravaToolStripMenuItem.Text = "Connect to Strava...";
            connectToStravaToolStripMenuItem.Click += connectToStravaToolStripMenuItem_Click;
            // 
            // syncWithStravaToolStripMenuItem
            // 
            syncWithStravaToolStripMenuItem.Name = "syncWithStravaToolStripMenuItem";
            syncWithStravaToolStripMenuItem.Size = new Size(471, 58);
            syncWithStravaToolStripMenuItem.Text = "Sync with Strava";
            syncWithStravaToolStripMenuItem.Click += syncWithStravaToolStripMenuItem_Click;
            // 
            // loadWeightDataToolStripMenuItem
            // 
            loadWeightDataToolStripMenuItem.Name = "loadWeightDataToolStripMenuItem";
            loadWeightDataToolStripMenuItem.Size = new Size(471, 58);
            loadWeightDataToolStripMenuItem.Text = "Load Weight Data...";
            loadWeightDataToolStripMenuItem.Click += loadWeightDataToolStripMenuItem_Click;
            // 
            // verifyAgainstFitlogToolStripMenuItem
            // 
            verifyAgainstFitlogToolStripMenuItem.Name = "verifyAgainstFitlogToolStripMenuItem";
            verifyAgainstFitlogToolStripMenuItem.Size = new Size(471, 58);
            verifyAgainstFitlogToolStripMenuItem.Text = "Verify against Fitlog...";
            verifyAgainstFitlogToolStripMenuItem.Click += verifyAgainstFitlogToolStripMenuItem_Click;
            // 
            // updateFromFitlogToolStripMenuItem
            // 
            updateFromFitlogToolStripMenuItem.Name = "updateFromFitlogToolStripMenuItem";
            updateFromFitlogToolStripMenuItem.Size = new Size(471, 58);
            updateFromFitlogToolStripMenuItem.Text = "Update from Fitlog...";
            updateFromFitlogToolStripMenuItem.Click += updateFromFitlogToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { logToolStripMenuItem, normalLogToolStripMenuItem, errorLogToolStripMenuItem, clearLogsToolStripMenuItem, showAccumulatedTimeToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(65, 35);
            viewToolStripMenuItem.Text = "View";
            // 
            // logToolStripMenuItem
            // 
            logToolStripMenuItem.Name = "logToolStripMenuItem";
            logToolStripMenuItem.Size = new Size(321, 34);
            logToolStripMenuItem.Text = "Debug Log...";
            logToolStripMenuItem.Click += debugLogToolStripMenuItem_Click;
            // 
            // normalLogToolStripMenuItem
            // 
            normalLogToolStripMenuItem.Name = "normalLogToolStripMenuItem";
            normalLogToolStripMenuItem.Size = new Size(321, 34);
            normalLogToolStripMenuItem.Text = "Normal Log...";
            normalLogToolStripMenuItem.Click += logToolStripMenuItem_Click;
            // 
            // errorLogToolStripMenuItem
            // 
            errorLogToolStripMenuItem.Name = "errorLogToolStripMenuItem";
            errorLogToolStripMenuItem.Size = new Size(321, 34);
            errorLogToolStripMenuItem.Text = "Error Log...";
            errorLogToolStripMenuItem.Click += errorLogToolStripMenuItem_Click;
            // 
            // clearLogsToolStripMenuItem
            // 
            clearLogsToolStripMenuItem.Name = "clearLogsToolStripMenuItem";
            clearLogsToolStripMenuItem.Size = new Size(321, 34);
            clearLogsToolStripMenuItem.Text = "Clear Logs";
            clearLogsToolStripMenuItem.Click += clearLogsToolStripMenuItem_Click;
            // 
            // showAccumulatedTimeToolStripMenuItem
            // 
            showAccumulatedTimeToolStripMenuItem.Name = "showAccumulatedTimeToolStripMenuItem";
            showAccumulatedTimeToolStripMenuItem.Size = new Size(321, 34);
            showAccumulatedTimeToolStripMenuItem.Text = "Show Accumulated Time...";
            showAccumulatedTimeToolStripMenuItem.Click += showAccumulatedTimeToolStripMenuItem_Click;
            // 
            // configureToolStripMenuItem
            // 
            configureToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { optionsToolStripMenuItem1, dataStreamDefinitionsToolStripMenuItem });
            configureToolStripMenuItem.Name = "configureToolStripMenuItem";
            configureToolStripMenuItem.Size = new Size(106, 35);
            configureToolStripMenuItem.Text = "Configure";
            // 
            // optionsToolStripMenuItem1
            // 
            optionsToolStripMenuItem1.Name = "optionsToolStripMenuItem1";
            optionsToolStripMenuItem1.Size = new Size(390, 34);
            optionsToolStripMenuItem1.Text = "Options...";
            optionsToolStripMenuItem1.Click += optionsToolStripMenuItem_Click;
            // 
            // dataStreamDefinitionsToolStripMenuItem
            // 
            dataStreamDefinitionsToolStripMenuItem.Name = "dataStreamDefinitionsToolStripMenuItem";
            dataStreamDefinitionsToolStripMenuItem.Size = new Size(390, 34);
            dataStreamDefinitionsToolStripMenuItem.Text = "Data Stream Graph Configuration...";
            dataStreamDefinitionsToolStripMenuItem.Click += dataStreamGraphDefinitionsToolStripMenuItem_Click;
            // 
            // showErrorsToolStripMenuItem
            // 
            showErrorsToolStripMenuItem.Name = "showErrorsToolStripMenuItem";
            showErrorsToolStripMenuItem.Size = new Size(135, 35);
            showErrorsToolStripMenuItem.Text = "Show Errors...";
            showErrorsToolStripMenuItem.Click += showErrorsToolStripMenuItem_Click;
            // 
            // toolsToolStripMenuItem
            // 
            toolsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { findDataQualityIssuesToolStripMenuItem, rescanForDataQualityIssuesToolStripMenuItem, scanForDataQualityIssueToolStripMenuItem, fixDataQualityIssueToolStripMenuItem, emailGoalsToolStripMenuItem, exploreGlobalRelationshipsToolStripMenuItem, experimentalToolStripMenuItem });
            toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            toolsToolStripMenuItem.Size = new Size(69, 35);
            toolsToolStripMenuItem.Text = "Tools";
            // 
            // findDataQualityIssuesToolStripMenuItem
            // 
            findDataQualityIssuesToolStripMenuItem.Name = "findDataQualityIssuesToolStripMenuItem";
            findDataQualityIssuesToolStripMenuItem.Size = new Size(392, 34);
            findDataQualityIssuesToolStripMenuItem.Text = "Clear Data Quality Issues";
            findDataQualityIssuesToolStripMenuItem.Click += clearDataQualityToolStripMenuItem_Click;
            // 
            // rescanForDataQualityIssuesToolStripMenuItem
            // 
            rescanForDataQualityIssuesToolStripMenuItem.Name = "rescanForDataQualityIssuesToolStripMenuItem";
            rescanForDataQualityIssuesToolStripMenuItem.Size = new Size(392, 34);
            rescanForDataQualityIssuesToolStripMenuItem.Text = "Rescan For All Data Quality Issues...";
            rescanForDataQualityIssuesToolStripMenuItem.Click += rescanForDataQualityIssuesToolStripMenuItem_Click;
            // 
            // scanForDataQualityIssueToolStripMenuItem
            // 
            scanForDataQualityIssueToolStripMenuItem.Name = "scanForDataQualityIssueToolStripMenuItem";
            scanForDataQualityIssueToolStripMenuItem.Size = new Size(392, 34);
            scanForDataQualityIssueToolStripMenuItem.Text = "Scan For Data Quality Issue";
            // 
            // fixDataQualityIssueToolStripMenuItem
            // 
            fixDataQualityIssueToolStripMenuItem.Name = "fixDataQualityIssueToolStripMenuItem";
            fixDataQualityIssueToolStripMenuItem.Size = new Size(392, 34);
            fixDataQualityIssueToolStripMenuItem.Text = "Fix Data Quality Issue";
            // 
            // emailGoalsToolStripMenuItem
            // 
            emailGoalsToolStripMenuItem.Name = "emailGoalsToolStripMenuItem";
            emailGoalsToolStripMenuItem.Size = new Size(392, 34);
            emailGoalsToolStripMenuItem.Text = "Email Goals";
            emailGoalsToolStripMenuItem.Click += emailGoalsToolStripMenuItem_Click;
            // 
            // exploreGlobalRelationshipsToolStripMenuItem
            // 
            exploreGlobalRelationshipsToolStripMenuItem.Name = "exploreGlobalRelationshipsToolStripMenuItem";
            exploreGlobalRelationshipsToolStripMenuItem.Size = new Size(392, 34);
            exploreGlobalRelationshipsToolStripMenuItem.Text = "Explore Global Relationships...";
            exploreGlobalRelationshipsToolStripMenuItem.Click += exploreGlobalRelationshipsToolStripMenuItem_Click;
            // 
            // filterToolStripMenuItem
            // 
            filterToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { filterToolStripMenuItem1, showOnlyToolStripMenuItem });
            filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            filterToolStripMenuItem.Size = new Size(66, 35);
            filterToolStripMenuItem.Text = "Filter";
            // 
            // filterToolStripMenuItem1
            // 
            filterToolStripMenuItem1.Name = "filterToolStripMenuItem1";
            filterToolStripMenuItem1.Size = new Size(200, 34);
            filterToolStripMenuItem1.Text = "Filter...";
            filterToolStripMenuItem1.Click += filterToolStripMenuItem1_Click;
            // 
            // showOnlyToolStripMenuItem
            // 
            showOnlyToolStripMenuItem.Name = "showOnlyToolStripMenuItem";
            showOnlyToolStripMenuItem.Size = new Size(200, 34);
            showOnlyToolStripMenuItem.Text = "Show Only";
            // 
            // stravaToolStripMenuItem
            // 
            stravaToolStripMenuItem.Image = (Image)resources.GetObject("stravaToolStripMenuItem.Image");
            stravaToolStripMenuItem.ImageScaling = ToolStripItemImageScaling.None;
            stravaToolStripMenuItem.Name = "stravaToolStripMenuItem";
            stravaToolStripMenuItem.Size = new Size(185, 35);
            // 
            // graphTabPage
            // 
            graphTabPage.Controls.Add(progressGraph1);
            graphTabPage.Location = new Point(4, 34);
            graphTabPage.Name = "graphTabPage";
            graphTabPage.Padding = new Padding(3);
            graphTabPage.Size = new Size(192, 62);
            graphTabPage.TabIndex = 1;
            graphTabPage.Text = "Graphs";
            graphTabPage.UseVisualStyleBackColor = true;
            // 
            // progressGraph1
            // 
            progressGraph1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressGraph1.Location = new Point(8, 6);
            progressGraph1.Name = "progressGraph1";
            progressGraph1.Size = new Size(178, 53);
            progressGraph1.TabIndex = 0;
            // 
            // summaryTabPage
            // 
            summaryTabPage.Controls.Add(summaryTextBox);
            summaryTabPage.Location = new Point(4, 34);
            summaryTabPage.Name = "summaryTabPage";
            summaryTabPage.Size = new Size(192, 62);
            summaryTabPage.TabIndex = 2;
            summaryTabPage.Text = "Summary";
            summaryTabPage.UseVisualStyleBackColor = true;
            // 
            // summaryTextBox
            // 
            summaryTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            summaryTextBox.Location = new Point(8, 13);
            summaryTextBox.Multiline = true;
            summaryTextBox.Name = "summaryTextBox";
            summaryTextBox.ReadOnly = true;
            summaryTextBox.ScrollBars = ScrollBars.Vertical;
            summaryTextBox.Size = new Size(70, 0);
            summaryTextBox.TabIndex = 0;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(activityReport);
            tabControl1.Controls.Add(graphTabPage);
            tabControl1.Controls.Add(activityTreeTabPage);
            tabControl1.Controls.Add(summaryTabPage);
            tabControl1.Controls.Add(goalsTabPage);
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 39);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1831, 1158);
            tabControl1.TabIndex = 1;
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
            // 
            // activityReport
            // 
            activityReport.Controls.Add(activityReport1);
            activityReport.Location = new Point(4, 34);
            activityReport.Name = "activityReport";
            activityReport.Padding = new Padding(3);
            activityReport.Size = new Size(1823, 1120);
            activityReport.TabIndex = 5;
            activityReport.Text = "Activity Report";
            activityReport.UseVisualStyleBackColor = true;
            // 
            // activityReport1
            // 
            activityReport1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            activityReport1.Location = new Point(0, 3);
            activityReport1.Name = "activityReport1";
            activityReport1.Size = new Size(1820, 1121);
            activityReport1.TabIndex = 0;
            activityReport1.Load += activityReport1_Load;
            // 
            // activityTreeTabPage
            // 
            activityTreeTabPage.Controls.Add(splitContainer4);
            activityTreeTabPage.Location = new Point(4, 34);
            activityTreeTabPage.Name = "activityTreeTabPage";
            activityTreeTabPage.Size = new Size(192, 62);
            activityTreeTabPage.TabIndex = 4;
            activityTreeTabPage.Text = "Activity Tree";
            activityTreeTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            splitContainer4.Dock = DockStyle.Fill;
            splitContainer4.Location = new Point(0, 0);
            splitContainer4.Name = "splitContainer4";
            splitContainer4.Orientation = Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            splitContainer4.Panel1.Controls.Add(activityTree1);
            // 
            // splitContainer4.Panel2
            // 
            splitContainer4.Panel2.Controls.Add(activityFormsPlot);
            splitContainer4.Size = new Size(192, 62);
            splitContainer4.SplitterDistance = 31;
            splitContainer4.TabIndex = 0;
            // 
            // activityTree1
            // 
            activityTree1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            activityTree1.Location = new Point(8, 3);
            activityTree1.Name = "activityTree1";
            activityTree1.Size = new Size(176, 25);
            activityTree1.TabIndex = 0;
            // 
            // activityFormsPlot
            // 
            activityFormsPlot.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            activityFormsPlot.Location = new Point(6, 5);
            activityFormsPlot.Margin = new Padding(6, 5, 6, 5);
            activityFormsPlot.Name = "activityFormsPlot";
            activityFormsPlot.Size = new Size(175, 12);
            activityFormsPlot.TabIndex = 0;
            // 
            // goalsTabPage
            // 
            goalsTabPage.Controls.Add(goalsSplitContainer4);
            goalsTabPage.Location = new Point(4, 34);
            goalsTabPage.Name = "goalsTabPage";
            goalsTabPage.Size = new Size(192, 62);
            goalsTabPage.TabIndex = 3;
            goalsTabPage.Text = "Goals";
            goalsTabPage.UseVisualStyleBackColor = true;
            // 
            // goalsSplitContainer4
            // 
            goalsSplitContainer4.Dock = DockStyle.Fill;
            goalsSplitContainer4.Location = new Point(0, 0);
            goalsSplitContainer4.Name = "goalsSplitContainer4";
            goalsSplitContainer4.Orientation = Orientation.Horizontal;
            // 
            // goalsSplitContainer4.Panel1
            // 
            goalsSplitContainer4.Panel1.Controls.Add(goalsTextBox);
            // 
            // goalsSplitContainer4.Panel2
            // 
            goalsSplitContainer4.Panel2.Controls.Add(goalsDataGridView);
            goalsSplitContainer4.Size = new Size(192, 62);
            goalsSplitContainer4.SplitterDistance = 31;
            goalsSplitContainer4.TabIndex = 0;
            // 
            // goalsTextBox
            // 
            goalsTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            goalsTextBox.Location = new Point(3, 13);
            goalsTextBox.Multiline = true;
            goalsTextBox.Name = "goalsTextBox";
            goalsTextBox.Size = new Size(181, 14);
            goalsTextBox.TabIndex = 1;
            // 
            // goalsDataGridView
            // 
            goalsDataGridView.AllowUserToAddRows = false;
            goalsDataGridView.AllowUserToDeleteRows = false;
            goalsDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            goalsDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            goalsDataGridView.Location = new Point(3, 3);
            goalsDataGridView.Name = "goalsDataGridView";
            goalsDataGridView.ReadOnly = true;
            goalsDataGridView.RowHeadersVisible = false;
            goalsDataGridView.RowHeadersWidth = 62;
            goalsDataGridView.RowTemplate.Height = 33;
            goalsDataGridView.Size = new Size(186, 21);
            goalsDataGridView.TabIndex = 2;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(overviewMap1);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Size = new Size(192, 62);
            tabPage1.TabIndex = 6;
            tabPage1.Text = "Overview Maps";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // overviewMap1
            // 
            overviewMap1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            overviewMap1.Location = new Point(3, 3);
            overviewMap1.Name = "overviewMap1";
            overviewMap1.Size = new Size(186, 56);
            overviewMap1.TabIndex = 0;
            // 
            // loadStravaCsvBackgroundWorker
            // 
            loadStravaCsvBackgroundWorker.WorkerReportsProgress = true;
            loadStravaCsvBackgroundWorker.DoWork += loadStravaCsvBackgroundWorker_DoWork;
            loadStravaCsvBackgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            loadStravaCsvBackgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            // 
            // recalculateBackgroundWorker1
            // 
            recalculateBackgroundWorker1.WorkerReportsProgress = true;
            recalculateBackgroundWorker1.DoWork += recalculateBackgroundWorker_DoWork;
            recalculateBackgroundWorker1.ProgressChanged += backgroundWorker_ProgressChanged;
            recalculateBackgroundWorker1.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            // 
            // experimentalToolStripMenuItem
            // 
            experimentalToolStripMenuItem.Name = "experimentalToolStripMenuItem";
            experimentalToolStripMenuItem.Size = new Size(392, 34);
            experimentalToolStripMenuItem.Text = "Experimental...";
            experimentalToolStripMenuItem.Click += experimentalToolStripMenuItem_Click;
            // 
            // FellrnrTrainingAnalysisForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1831, 1197);
            Controls.Add(tabControl1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "FellrnrTrainingAnalysisForm";
            Text = "Fellrnr Training Analytics";
            WindowState = FormWindowState.Maximized;
            FormClosed += FellrnrTrainingAnalysis_FormClosed;
            Load += FellrnrTrainingAnalysisForm_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            graphTabPage.ResumeLayout(false);
            summaryTabPage.ResumeLayout(false);
            summaryTabPage.PerformLayout();
            tabControl1.ResumeLayout(false);
            activityReport.ResumeLayout(false);
            activityTreeTabPage.ResumeLayout(false);
            splitContainer4.Panel1.ResumeLayout(false);
            splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer4).EndInit();
            splitContainer4.ResumeLayout(false);
            goalsTabPage.ResumeLayout(false);
            goalsSplitContainer4.Panel1.ResumeLayout(false);
            goalsSplitContainer4.Panel1.PerformLayout();
            goalsSplitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)goalsSplitContainer4).EndInit();
            goalsSplitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)goalsDataGridView).EndInit();
            tabPage1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripMenuItem clearDatabaseToolStripMenuItem;
        private ToolStripMenuItem dataSourcesToolStripMenuItem;
        private ToolStripMenuItem connectToStravaToolStripMenuItem;
        private ToolStripMenuItem syncWithStravaToolStripMenuItem;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem logToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private TabPage graphTabPage;
        private TabPage summaryTabPage;
        private TextBox summaryTextBox;
        private TabControl tabControl1;
        private ToolStripMenuItem saveDatabaseAsToolStripMenuItem;
        private TabPage goalsTabPage;
        private SplitContainer goalsSplitContainer4;
        private DataGridView goalsDataGridView;
        private TextBox goalsTextBox;
        private ToolStripMenuItem forceRecalculationToolStripMenuItem;
        private TabPage activityTreeTabPage;
        private SplitContainer splitContainer4;
        private UI.ActivityTree activityTree1;
        private ScottPlot.FormsPlot activityFormsPlot;
        private ToolStripMenuItem showErrorsToolStripMenuItem;
        private ToolStripMenuItem bootstrapToolStripMenuItem;
        private ToolStripMenuItem normalLogToolStripMenuItem;
        private ToolStripMenuItem errorLogToolStripMenuItem;
        private ToolStripMenuItem configureToolStripMenuItem;
        private ToolStripMenuItem optionsToolStripMenuItem1;
        private ToolStripMenuItem dataStreamDefinitionsToolStripMenuItem;
        private UI.ProgressGraph progressGraph1;
        private ToolStripMenuItem toolsToolStripMenuItem;
        private ToolStripMenuItem findDataQualityIssuesToolStripMenuItem;
        private TabPage activityReport;
        private ActivityReport activityReport1;
        private ToolStripMenuItem filterToolStripMenuItem;
        private ToolStripMenuItem rescanForDataQualityIssuesToolStripMenuItem;
        private ToolStripMenuItem scanForDataQualityIssueToolStripMenuItem;
        private ToolStripMenuItem fixDataQualityIssueToolStripMenuItem;
        private ToolStripMenuItem clearLogsToolStripMenuItem;
        private TabPage tabPage1;
        private UI.OverviewMap overviewMap1;
        private ToolStripMenuItem stravaToolStripMenuItem;
        private ToolStripMenuItem emailGoalsToolStripMenuItem;
        private ToolStripMenuItem openBinDatabaseToolStripMenuItem;
        private ToolStripMenuItem saveBinDatabaseToolStripMenuItem;
        private ToolStripMenuItem loadWeightDataToolStripMenuItem;
        private ToolStripMenuItem verifyAgainstFitlogToolStripMenuItem;
        private ToolStripMenuItem updateFromFitlogToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker loadStravaCsvBackgroundWorker;
        private ToolStripMenuItem showAccumulatedTimeToolStripMenuItem;
        private ToolStripMenuItem recalculateHillsToolStripMenuItem;
        private ToolStripMenuItem filterToolStripMenuItem1;
        private ToolStripMenuItem showOnlyToolStripMenuItem;
        private ToolStripMenuItem recalculateGoalsToolStripMenuItem;
        private ToolStripMenuItem recalculateActivitiesToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker recalculateBackgroundWorker1;
        private ToolStripMenuItem validationToolStripMenuItem;
        private ToolStripMenuItem exploreGlobalRelationshipsToolStripMenuItem;
        private ToolStripMenuItem experimentalToolStripMenuItem;
    }
}