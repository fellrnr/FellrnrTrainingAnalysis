namespace FellrnrTrainingAnalysis
{
    partial class ActivityReport
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainerReportToTabs = new System.Windows.Forms.SplitContainer();
            this.splitContainerTopButtonsToReport = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.pageSizeComboBox1 = new System.Windows.Forms.ComboBox();
            this.labelTotalRows = new System.Windows.Forms.Label();
            this.activityDataGridView = new System.Windows.Forms.DataGridView();
            this.splitContainerTabsToList = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.formsPlot1 = new ScottPlot.FormsPlot();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.activityData1 = new FellrnrTrainingAnalysis.UI.ActivityData();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.activityMap1 = new FellrnrTrainingAnalysis.UI.ActivityMap();
            this.listViewGraphOptions = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerReportToTabs)).BeginInit();
            this.splitContainerReportToTabs.Panel1.SuspendLayout();
            this.splitContainerReportToTabs.Panel2.SuspendLayout();
            this.splitContainerReportToTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTopButtonsToReport)).BeginInit();
            this.splitContainerTopButtonsToReport.Panel1.SuspendLayout();
            this.splitContainerTopButtonsToReport.Panel2.SuspendLayout();
            this.splitContainerTopButtonsToReport.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.activityDataGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTabsToList)).BeginInit();
            this.splitContainerTabsToList.Panel1.SuspendLayout();
            this.splitContainerTabsToList.Panel2.SuspendLayout();
            this.splitContainerTabsToList.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerReportToTabs
            // 
            this.splitContainerReportToTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerReportToTabs.Location = new System.Drawing.Point(0, 0);
            this.splitContainerReportToTabs.Name = "splitContainerReportToTabs";
            this.splitContainerReportToTabs.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerReportToTabs.Panel1
            // 
            this.splitContainerReportToTabs.Panel1.Controls.Add(this.splitContainerTopButtonsToReport);
            // 
            // splitContainerReportToTabs.Panel2
            // 
            this.splitContainerReportToTabs.Panel2.Controls.Add(this.splitContainerTabsToList);
            this.splitContainerReportToTabs.Size = new System.Drawing.Size(1902, 1308);
            this.splitContainerReportToTabs.SplitterDistance = 724;
            this.splitContainerReportToTabs.TabIndex = 0;
            // 
            // splitContainerTopButtonsToReport
            // 
            this.splitContainerTopButtonsToReport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerTopButtonsToReport.Location = new System.Drawing.Point(0, 0);
            this.splitContainerTopButtonsToReport.Name = "splitContainerTopButtonsToReport";
            this.splitContainerTopButtonsToReport.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerTopButtonsToReport.Panel1
            // 
            this.splitContainerTopButtonsToReport.Panel1.Controls.Add(this.flowLayoutPanel1);
            // 
            // splitContainerTopButtonsToReport.Panel2
            // 
            this.splitContainerTopButtonsToReport.Panel2.Controls.Add(this.activityDataGridView);
            this.splitContainerTopButtonsToReport.Size = new System.Drawing.Size(1902, 724);
            this.splitContainerTopButtonsToReport.SplitterDistance = 60;
            this.splitContainerTopButtonsToReport.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.Controls.Add(this.label1);
            this.flowLayoutPanel1.Controls.Add(this.pageSizeComboBox1);
            this.flowLayoutPanel1.Controls.Add(this.labelTotalRows);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(1896, 54);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Page Size";
            // 
            // pageSizeComboBox1
            // 
            this.pageSizeComboBox1.FormattingEnabled = true;
            this.pageSizeComboBox1.Items.AddRange(new object[] {
            "25",
            "50",
            "100",
            "500",
            "All"});
            this.pageSizeComboBox1.Location = new System.Drawing.Point(95, 3);
            this.pageSizeComboBox1.Name = "pageSizeComboBox1";
            this.pageSizeComboBox1.Size = new System.Drawing.Size(182, 33);
            this.pageSizeComboBox1.TabIndex = 1;
            this.pageSizeComboBox1.Text = "25";
            this.pageSizeComboBox1.SelectedIndexChanged += new System.EventHandler(this.pageSizeComboBox1_SelectedIndexChanged);
            // 
            // labelTotalRows
            // 
            this.labelTotalRows.AutoSize = true;
            this.labelTotalRows.Location = new System.Drawing.Point(283, 0);
            this.labelTotalRows.Name = "labelTotalRows";
            this.labelTotalRows.Size = new System.Drawing.Size(59, 25);
            this.labelTotalRows.TabIndex = 2;
            this.labelTotalRows.Text = "label2";
            // 
            // activityDataGridView
            // 
            this.activityDataGridView.AllowUserToAddRows = false;
            this.activityDataGridView.AllowUserToDeleteRows = false;
            this.activityDataGridView.AllowUserToOrderColumns = true;
            this.activityDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activityDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.activityDataGridView.Location = new System.Drawing.Point(3, 3);
            this.activityDataGridView.MultiSelect = false;
            this.activityDataGridView.Name = "activityDataGridView";
            this.activityDataGridView.ReadOnly = true;
            this.activityDataGridView.RowHeadersWidth = 62;
            this.activityDataGridView.RowTemplate.Height = 33;
            this.activityDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.activityDataGridView.Size = new System.Drawing.Size(1883, 650);
            this.activityDataGridView.TabIndex = 2;
            this.activityDataGridView.CellMouseEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.activityDataGridView_CellMouseEnter);
            this.activityDataGridView.SelectionChanged += new System.EventHandler(this.dataGridView1_SelectionChanged);
            // 
            // splitContainerTabsToList
            // 
            this.splitContainerTabsToList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerTabsToList.Location = new System.Drawing.Point(0, 0);
            this.splitContainerTabsToList.Name = "splitContainerTabsToList";
            // 
            // splitContainerTabsToList.Panel1
            // 
            this.splitContainerTabsToList.Panel1.Controls.Add(this.tabControl2);
            // 
            // splitContainerTabsToList.Panel2
            // 
            this.splitContainerTabsToList.Panel2.Controls.Add(this.listViewGraphOptions);
            this.splitContainerTabsToList.Size = new System.Drawing.Size(1902, 580);
            this.splitContainerTabsToList.SplitterDistance = 1700;
            this.splitContainerTabsToList.TabIndex = 0;
            // 
            // tabControl2
            // 
            this.tabControl2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl2.Controls.Add(this.tabPage1);
            this.tabControl2.Controls.Add(this.tabPage2);
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Location = new System.Drawing.Point(6, -5);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(1691, 582);
            this.tabControl2.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.formsPlot1);
            this.tabPage1.Location = new System.Drawing.Point(4, 34);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1683, 544);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Time Graph";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // formsPlot1
            // 
            this.formsPlot1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.formsPlot1.Location = new System.Drawing.Point(0, 0);
            this.formsPlot1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.formsPlot1.Name = "formsPlot1";
            this.formsPlot1.Size = new System.Drawing.Size(1677, 539);
            this.formsPlot1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.activityData1);
            this.tabPage2.Location = new System.Drawing.Point(4, 34);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 62);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Activity Data";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // activityData1
            // 
            this.activityData1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activityData1.Location = new System.Drawing.Point(3, 3);
            this.activityData1.Name = "activityData1";
            this.activityData1.Size = new System.Drawing.Size(183, 53);
            this.activityData1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.activityMap1);
            this.tabPage3.Location = new System.Drawing.Point(4, 34);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1683, 544);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Activty Map";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // activityMap1
            // 
            this.activityMap1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.activityMap1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.activityMap1.Location = new System.Drawing.Point(3, 3);
            this.activityMap1.Name = "activityMap1";
            this.activityMap1.Size = new System.Drawing.Size(1677, 541);
            this.activityMap1.TabIndex = 0;
            // 
            // listViewGraphOptions
            // 
            this.listViewGraphOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewGraphOptions.Location = new System.Drawing.Point(3, 3);
            this.listViewGraphOptions.Name = "listViewGraphOptions";
            this.listViewGraphOptions.Size = new System.Drawing.Size(195, 574);
            this.listViewGraphOptions.TabIndex = 0;
            this.listViewGraphOptions.UseCompatibleStateImageBehavior = false;
            // 
            // ActivityReport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainerReportToTabs);
            this.Name = "ActivityReport";
            this.Size = new System.Drawing.Size(1902, 1308);
            this.splitContainerReportToTabs.Panel1.ResumeLayout(false);
            this.splitContainerReportToTabs.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerReportToTabs)).EndInit();
            this.splitContainerReportToTabs.ResumeLayout(false);
            this.splitContainerTopButtonsToReport.Panel1.ResumeLayout(false);
            this.splitContainerTopButtonsToReport.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTopButtonsToReport)).EndInit();
            this.splitContainerTopButtonsToReport.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.activityDataGridView)).EndInit();
            this.splitContainerTabsToList.Panel1.ResumeLayout(false);
            this.splitContainerTabsToList.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerTabsToList)).EndInit();
            this.splitContainerTabsToList.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private SplitContainer splitContainerReportToTabs;
        private SplitContainer splitContainerTabsToList;
        private ListView listViewGraphOptions;
        private SplitContainer splitContainerTopButtonsToReport;
        private FlowLayoutPanel flowLayoutPanel1;
        private DataGridView activityDataGridView;
        private TabControl tabControl2;
        private TabPage tabPage1;
        private ScottPlot.FormsPlot formsPlot1;
        private TabPage tabPage2;
        private Label label1;
        private ComboBox pageSizeComboBox1;
        private Label labelTotalRows;
        private UI.ActivityData activityData1;
        private TabPage tabPage3;
        private UI.ActivityMap activityMap1;
    }
}
