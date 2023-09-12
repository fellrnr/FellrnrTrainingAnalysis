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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveDatabaseAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forceRecalculationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataSourcesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bootstrapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToStravaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.syncWithStravaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.errorLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.dataStreamDefinitionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showErrorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.findDataQualityIssuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rescanForDataQualityIssuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scanForDataQualityIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.graphTabPage = new System.Windows.Forms.TabPage();
            this.progressGraph1 = new FellrnrTrainingAnalysis.UI.ProgressGraph();
            this.summaryTabPage = new System.Windows.Forms.TabPage();
            this.summaryTextBox = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.activityReport = new System.Windows.Forms.TabPage();
            this.activityReport1 = new FellrnrTrainingAnalysis.ActivityReport();
            this.activityTreeTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.activityTree1 = new FellrnrTrainingAnalysis.UI.ActivityTree();
            this.activityFormsPlot = new ScottPlot.FormsPlot();
            this.goalsTabPage = new System.Windows.Forms.TabPage();
            this.goalsSplitContainer4 = new System.Windows.Forms.SplitContainer();
            this.goalsTextBox = new System.Windows.Forms.TextBox();
            this.goalsDataGridView = new System.Windows.Forms.DataGridView();
            this.fixDataQualityIssueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.graphTabPage.SuspendLayout();
            this.summaryTabPage.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.activityReport.SuspendLayout();
            this.activityTreeTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.goalsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.goalsSplitContainer4)).BeginInit();
            this.goalsSplitContainer4.Panel1.SuspendLayout();
            this.goalsSplitContainer4.Panel2.SuspendLayout();
            this.goalsSplitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.goalsDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.dataSourcesToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.configureToolStripMenuItem,
            this.showErrorsToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.filterToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1831, 33);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveDatabaseAsToolStripMenuItem,
            this.clearDatabaseToolStripMenuItem,
            this.forceRecalculationToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(102, 29);
            this.fileToolStripMenuItem.Text = "Database";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(267, 34);
            this.openToolStripMenuItem.Text = "Open Database...";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(267, 34);
            this.saveToolStripMenuItem.Text = "Save Database";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveDatabaseAsToolStripMenuItem
            // 
            this.saveDatabaseAsToolStripMenuItem.Name = "saveDatabaseAsToolStripMenuItem";
            this.saveDatabaseAsToolStripMenuItem.Size = new System.Drawing.Size(267, 34);
            this.saveDatabaseAsToolStripMenuItem.Text = "Save Database As...";
            this.saveDatabaseAsToolStripMenuItem.Click += new System.EventHandler(this.saveDatabaseAsToolStripMenuItem_Click);
            // 
            // clearDatabaseToolStripMenuItem
            // 
            this.clearDatabaseToolStripMenuItem.Name = "clearDatabaseToolStripMenuItem";
            this.clearDatabaseToolStripMenuItem.Size = new System.Drawing.Size(267, 34);
            this.clearDatabaseToolStripMenuItem.Text = "Clear Database";
            this.clearDatabaseToolStripMenuItem.Click += new System.EventHandler(this.clearDatabaseToolStripMenuItem_Click);
            // 
            // forceRecalculationToolStripMenuItem
            // 
            this.forceRecalculationToolStripMenuItem.Name = "forceRecalculationToolStripMenuItem";
            this.forceRecalculationToolStripMenuItem.Size = new System.Drawing.Size(267, 34);
            this.forceRecalculationToolStripMenuItem.Text = "Force Recalculation";
            this.forceRecalculationToolStripMenuItem.Click += new System.EventHandler(this.forceRecalculationToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(267, 34);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // dataSourcesToolStripMenuItem
            // 
            this.dataSourcesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bootstrapToolStripMenuItem,
            this.connectToStravaToolStripMenuItem,
            this.syncWithStravaToolStripMenuItem});
            this.dataSourcesToolStripMenuItem.Name = "dataSourcesToolStripMenuItem";
            this.dataSourcesToolStripMenuItem.Size = new System.Drawing.Size(132, 29);
            this.dataSourcesToolStripMenuItem.Text = "Data Sources";
            // 
            // bootstrapToolStripMenuItem
            // 
            this.bootstrapToolStripMenuItem.Name = "bootstrapToolStripMenuItem";
            this.bootstrapToolStripMenuItem.Size = new System.Drawing.Size(302, 34);
            this.bootstrapToolStripMenuItem.Text = "Load From Strava CSV...";
            this.bootstrapToolStripMenuItem.Click += new System.EventHandler(this.loadFromStravaCsvToolStripMenuItem_Click);
            // 
            // connectToStravaToolStripMenuItem
            // 
            this.connectToStravaToolStripMenuItem.Name = "connectToStravaToolStripMenuItem";
            this.connectToStravaToolStripMenuItem.Size = new System.Drawing.Size(302, 34);
            this.connectToStravaToolStripMenuItem.Text = "Connect to Strava...";
            this.connectToStravaToolStripMenuItem.Click += new System.EventHandler(this.connectToStravaToolStripMenuItem_Click);
            // 
            // syncWithStravaToolStripMenuItem
            // 
            this.syncWithStravaToolStripMenuItem.Name = "syncWithStravaToolStripMenuItem";
            this.syncWithStravaToolStripMenuItem.Size = new System.Drawing.Size(302, 34);
            this.syncWithStravaToolStripMenuItem.Text = "Sync with Strava";
            this.syncWithStravaToolStripMenuItem.Click += new System.EventHandler(this.syncWithStravaToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.logToolStripMenuItem,
            this.normalLogToolStripMenuItem,
            this.errorLogToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(65, 29);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // logToolStripMenuItem
            // 
            this.logToolStripMenuItem.Name = "logToolStripMenuItem";
            this.logToolStripMenuItem.Size = new System.Drawing.Size(220, 34);
            this.logToolStripMenuItem.Text = "Debug Log...";
            this.logToolStripMenuItem.Click += new System.EventHandler(this.debugLogToolStripMenuItem_Click);
            // 
            // normalLogToolStripMenuItem
            // 
            this.normalLogToolStripMenuItem.Name = "normalLogToolStripMenuItem";
            this.normalLogToolStripMenuItem.Size = new System.Drawing.Size(220, 34);
            this.normalLogToolStripMenuItem.Text = "Normal Log...";
            this.normalLogToolStripMenuItem.Click += new System.EventHandler(this.logToolStripMenuItem_Click);
            // 
            // errorLogToolStripMenuItem
            // 
            this.errorLogToolStripMenuItem.Name = "errorLogToolStripMenuItem";
            this.errorLogToolStripMenuItem.Size = new System.Drawing.Size(220, 34);
            this.errorLogToolStripMenuItem.Text = "Error Log...";
            this.errorLogToolStripMenuItem.Click += new System.EventHandler(this.errorLogToolStripMenuItem_Click);
            // 
            // configureToolStripMenuItem
            // 
            this.configureToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.optionsToolStripMenuItem1,
            this.dataStreamDefinitionsToolStripMenuItem});
            this.configureToolStripMenuItem.Name = "configureToolStripMenuItem";
            this.configureToolStripMenuItem.Size = new System.Drawing.Size(106, 29);
            this.configureToolStripMenuItem.Text = "Configure";
            // 
            // optionsToolStripMenuItem1
            // 
            this.optionsToolStripMenuItem1.Name = "optionsToolStripMenuItem1";
            this.optionsToolStripMenuItem1.Size = new System.Drawing.Size(390, 34);
            this.optionsToolStripMenuItem1.Text = "Options...";
            this.optionsToolStripMenuItem1.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // dataStreamDefinitionsToolStripMenuItem
            // 
            this.dataStreamDefinitionsToolStripMenuItem.Name = "dataStreamDefinitionsToolStripMenuItem";
            this.dataStreamDefinitionsToolStripMenuItem.Size = new System.Drawing.Size(390, 34);
            this.dataStreamDefinitionsToolStripMenuItem.Text = "Data Stream Graph Configuration...";
            this.dataStreamDefinitionsToolStripMenuItem.Click += new System.EventHandler(this.dataStreamGraphDefinitionsToolStripMenuItem_Click);
            // 
            // showErrorsToolStripMenuItem
            // 
            this.showErrorsToolStripMenuItem.Name = "showErrorsToolStripMenuItem";
            this.showErrorsToolStripMenuItem.Size = new System.Drawing.Size(135, 29);
            this.showErrorsToolStripMenuItem.Text = "Show Errors...";
            this.showErrorsToolStripMenuItem.Click += new System.EventHandler(this.showErrorsToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.findDataQualityIssuesToolStripMenuItem,
            this.rescanForDataQualityIssuesToolStripMenuItem,
            this.scanForDataQualityIssueToolStripMenuItem,
            this.fixDataQualityIssueToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(69, 29);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // findDataQualityIssuesToolStripMenuItem
            // 
            this.findDataQualityIssuesToolStripMenuItem.Name = "findDataQualityIssuesToolStripMenuItem";
            this.findDataQualityIssuesToolStripMenuItem.Size = new System.Drawing.Size(392, 34);
            this.findDataQualityIssuesToolStripMenuItem.Text = "Clear Data Quality Issues";
            this.findDataQualityIssuesToolStripMenuItem.Click += new System.EventHandler(this.clearDataQualityToolStripMenuItem_Click);
            // 
            // rescanForDataQualityIssuesToolStripMenuItem
            // 
            this.rescanForDataQualityIssuesToolStripMenuItem.Name = "rescanForDataQualityIssuesToolStripMenuItem";
            this.rescanForDataQualityIssuesToolStripMenuItem.Size = new System.Drawing.Size(392, 34);
            this.rescanForDataQualityIssuesToolStripMenuItem.Text = "Rescan For All Data Quality Issues...";
            this.rescanForDataQualityIssuesToolStripMenuItem.Click += new System.EventHandler(this.rescanForDataQualityIssuesToolStripMenuItem_Click);
            // 
            // scanForDataQualityIssueToolStripMenuItem
            // 
            this.scanForDataQualityIssueToolStripMenuItem.Name = "scanForDataQualityIssueToolStripMenuItem";
            this.scanForDataQualityIssueToolStripMenuItem.Size = new System.Drawing.Size(392, 34);
            this.scanForDataQualityIssueToolStripMenuItem.Text = "Scan For Data Quality Issue";
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(78, 29);
            this.filterToolStripMenuItem.Text = "Filter...";
            this.filterToolStripMenuItem.Click += new System.EventHandler(this.filterToolStripMenuItem_Click);
            // 
            // graphTabPage
            // 
            this.graphTabPage.Controls.Add(this.progressGraph1);
            this.graphTabPage.Location = new System.Drawing.Point(4, 34);
            this.graphTabPage.Name = "graphTabPage";
            this.graphTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.graphTabPage.Size = new System.Drawing.Size(192, 62);
            this.graphTabPage.TabIndex = 1;
            this.graphTabPage.Text = "Graphs";
            this.graphTabPage.UseVisualStyleBackColor = true;
            // 
            // progressGraph1
            // 
            this.progressGraph1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressGraph1.Location = new System.Drawing.Point(8, 6);
            this.progressGraph1.Name = "progressGraph1";
            this.progressGraph1.Size = new System.Drawing.Size(178, 53);
            this.progressGraph1.TabIndex = 0;
            // 
            // summaryTabPage
            // 
            this.summaryTabPage.Controls.Add(this.summaryTextBox);
            this.summaryTabPage.Location = new System.Drawing.Point(4, 34);
            this.summaryTabPage.Name = "summaryTabPage";
            this.summaryTabPage.Size = new System.Drawing.Size(192, 62);
            this.summaryTabPage.TabIndex = 2;
            this.summaryTabPage.Text = "Summary";
            this.summaryTabPage.UseVisualStyleBackColor = true;
            // 
            // summaryTextBox
            // 
            this.summaryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.summaryTextBox.Location = new System.Drawing.Point(8, 13);
            this.summaryTextBox.Multiline = true;
            this.summaryTextBox.Name = "summaryTextBox";
            this.summaryTextBox.ReadOnly = true;
            this.summaryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.summaryTextBox.Size = new System.Drawing.Size(70, 0);
            this.summaryTextBox.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.activityReport);
            this.tabControl1.Controls.Add(this.graphTabPage);
            this.tabControl1.Controls.Add(this.activityTreeTabPage);
            this.tabControl1.Controls.Add(this.summaryTabPage);
            this.tabControl1.Controls.Add(this.goalsTabPage);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 33);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1831, 1164);
            this.tabControl1.TabIndex = 1;
            // 
            // activityReport
            // 
            this.activityReport.Controls.Add(this.activityReport1);
            this.activityReport.Location = new System.Drawing.Point(4, 34);
            this.activityReport.Name = "activityReport";
            this.activityReport.Padding = new System.Windows.Forms.Padding(3);
            this.activityReport.Size = new System.Drawing.Size(1823, 1126);
            this.activityReport.TabIndex = 5;
            this.activityReport.Text = "Activity Report";
            this.activityReport.UseVisualStyleBackColor = true;
            // 
            // activityReport1
            // 
            this.activityReport1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activityReport1.Location = new System.Drawing.Point(0, 3);
            this.activityReport1.Name = "activityReport1";
            this.activityReport1.Size = new System.Drawing.Size(1820, 1123);
            this.activityReport1.TabIndex = 0;
            // 
            // activityTreeTabPage
            // 
            this.activityTreeTabPage.Controls.Add(this.splitContainer4);
            this.activityTreeTabPage.Location = new System.Drawing.Point(4, 34);
            this.activityTreeTabPage.Name = "activityTreeTabPage";
            this.activityTreeTabPage.Size = new System.Drawing.Size(192, 62);
            this.activityTreeTabPage.TabIndex = 4;
            this.activityTreeTabPage.Text = "Activity Tree";
            this.activityTreeTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.activityTree1);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.activityFormsPlot);
            this.splitContainer4.Size = new System.Drawing.Size(192, 62);
            this.splitContainer4.SplitterDistance = 31;
            this.splitContainer4.TabIndex = 0;
            // 
            // activityTree1
            // 
            this.activityTree1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activityTree1.Location = new System.Drawing.Point(8, 3);
            this.activityTree1.Name = "activityTree1";
            this.activityTree1.Size = new System.Drawing.Size(176, 25);
            this.activityTree1.TabIndex = 0;
            // 
            // activityFormsPlot
            // 
            this.activityFormsPlot.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activityFormsPlot.Location = new System.Drawing.Point(6, 5);
            this.activityFormsPlot.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.activityFormsPlot.Name = "activityFormsPlot";
            this.activityFormsPlot.Size = new System.Drawing.Size(175, 12);
            this.activityFormsPlot.TabIndex = 0;
            // 
            // goalsTabPage
            // 
            this.goalsTabPage.Controls.Add(this.goalsSplitContainer4);
            this.goalsTabPage.Location = new System.Drawing.Point(4, 34);
            this.goalsTabPage.Name = "goalsTabPage";
            this.goalsTabPage.Size = new System.Drawing.Size(192, 62);
            this.goalsTabPage.TabIndex = 3;
            this.goalsTabPage.Text = "Goals";
            this.goalsTabPage.UseVisualStyleBackColor = true;
            // 
            // goalsSplitContainer4
            // 
            this.goalsSplitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.goalsSplitContainer4.Location = new System.Drawing.Point(0, 0);
            this.goalsSplitContainer4.Name = "goalsSplitContainer4";
            this.goalsSplitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // goalsSplitContainer4.Panel1
            // 
            this.goalsSplitContainer4.Panel1.Controls.Add(this.goalsTextBox);
            // 
            // goalsSplitContainer4.Panel2
            // 
            this.goalsSplitContainer4.Panel2.Controls.Add(this.goalsDataGridView);
            this.goalsSplitContainer4.Size = new System.Drawing.Size(192, 62);
            this.goalsSplitContainer4.SplitterDistance = 31;
            this.goalsSplitContainer4.TabIndex = 0;
            // 
            // goalsTextBox
            // 
            this.goalsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.goalsTextBox.Location = new System.Drawing.Point(3, 13);
            this.goalsTextBox.Multiline = true;
            this.goalsTextBox.Name = "goalsTextBox";
            this.goalsTextBox.Size = new System.Drawing.Size(181, 14);
            this.goalsTextBox.TabIndex = 1;
            // 
            // goalsDataGridView
            // 
            this.goalsDataGridView.AllowUserToAddRows = false;
            this.goalsDataGridView.AllowUserToDeleteRows = false;
            this.goalsDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.goalsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.goalsDataGridView.Location = new System.Drawing.Point(3, 3);
            this.goalsDataGridView.Name = "goalsDataGridView";
            this.goalsDataGridView.ReadOnly = true;
            this.goalsDataGridView.RowHeadersVisible = false;
            this.goalsDataGridView.RowHeadersWidth = 62;
            this.goalsDataGridView.RowTemplate.Height = 33;
            this.goalsDataGridView.Size = new System.Drawing.Size(186, 21);
            this.goalsDataGridView.TabIndex = 2;
            // 
            // fixDataQualityIssueToolStripMenuItem
            // 
            this.fixDataQualityIssueToolStripMenuItem.Name = "fixDataQualityIssueToolStripMenuItem";
            this.fixDataQualityIssueToolStripMenuItem.Size = new System.Drawing.Size(392, 34);
            this.fixDataQualityIssueToolStripMenuItem.Text = "Fix Data Quality Issue";
            // 
            // FellrnrTrainingAnalysisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1831, 1197);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FellrnrTrainingAnalysisForm";
            this.Text = "Fellrnr Training Analytics";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FellrnrTrainingAnalysis_FormClosed);
            this.Load += new System.EventHandler(this.FellrnrTrainingAnalysisForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.graphTabPage.ResumeLayout(false);
            this.summaryTabPage.ResumeLayout(false);
            this.summaryTabPage.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.activityReport.ResumeLayout(false);
            this.activityTreeTabPage.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.goalsTabPage.ResumeLayout(false);
            this.goalsSplitContainer4.Panel1.ResumeLayout(false);
            this.goalsSplitContainer4.Panel1.PerformLayout();
            this.goalsSplitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.goalsSplitContainer4)).EndInit();
            this.goalsSplitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.goalsDataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}