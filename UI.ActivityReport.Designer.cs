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
            splitContainerReportToTabs = new SplitContainer();
            splitContainerTopButtonsToReport = new SplitContainer();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label1 = new Label();
            pageSizeComboBox1 = new ComboBox();
            labelTotalRows = new Label();
            activityDataGridView = new DataGridView();
            splitContainerTabsToList = new SplitContainer();
            tabControl2 = new TabControl();
            tabPage1 = new TabPage();
            formsPlot1 = new ScottPlot.FormsPlot();
            tabPage2 = new TabPage();
            activityData1 = new UI.ActivityData();
            tabPage3 = new TabPage();
            activityMap1 = new UI.ActivityMap();
            listViewGraphOptions = new ListView();
            ((System.ComponentModel.ISupportInitialize)splitContainerReportToTabs).BeginInit();
            splitContainerReportToTabs.Panel1.SuspendLayout();
            splitContainerReportToTabs.Panel2.SuspendLayout();
            splitContainerReportToTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainerTopButtonsToReport).BeginInit();
            splitContainerTopButtonsToReport.Panel1.SuspendLayout();
            splitContainerTopButtonsToReport.Panel2.SuspendLayout();
            splitContainerTopButtonsToReport.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)activityDataGridView).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainerTabsToList).BeginInit();
            splitContainerTabsToList.Panel1.SuspendLayout();
            splitContainerTabsToList.Panel2.SuspendLayout();
            splitContainerTabsToList.SuspendLayout();
            tabControl2.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainerReportToTabs
            // 
            splitContainerReportToTabs.Dock = DockStyle.Fill;
            splitContainerReportToTabs.Location = new Point(0, 0);
            splitContainerReportToTabs.Name = "splitContainerReportToTabs";
            splitContainerReportToTabs.Orientation = Orientation.Horizontal;
            // 
            // splitContainerReportToTabs.Panel1
            // 
            splitContainerReportToTabs.Panel1.Controls.Add(splitContainerTopButtonsToReport);
            // 
            // splitContainerReportToTabs.Panel2
            // 
            splitContainerReportToTabs.Panel2.Controls.Add(splitContainerTabsToList);
            splitContainerReportToTabs.Size = new Size(1902, 1308);
            splitContainerReportToTabs.SplitterDistance = 618;
            splitContainerReportToTabs.TabIndex = 0;
            // 
            // splitContainerTopButtonsToReport
            // 
            splitContainerTopButtonsToReport.Dock = DockStyle.Fill;
            splitContainerTopButtonsToReport.Location = new Point(0, 0);
            splitContainerTopButtonsToReport.Name = "splitContainerTopButtonsToReport";
            splitContainerTopButtonsToReport.Orientation = Orientation.Horizontal;
            // 
            // splitContainerTopButtonsToReport.Panel1
            // 
            splitContainerTopButtonsToReport.Panel1.Controls.Add(flowLayoutPanel1);
            // 
            // splitContainerTopButtonsToReport.Panel2
            // 
            splitContainerTopButtonsToReport.Panel2.Controls.Add(activityDataGridView);
            splitContainerTopButtonsToReport.Size = new Size(1902, 618);
            splitContainerTopButtonsToReport.SplitterDistance = 51;
            splitContainerTopButtonsToReport.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(pageSizeComboBox1);
            flowLayoutPanel1.Controls.Add(labelTotalRows);
            flowLayoutPanel1.Location = new Point(3, 3);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(1896, 45);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(86, 25);
            label1.TabIndex = 0;
            label1.Text = "Page Size";
            // 
            // pageSizeComboBox1
            // 
            pageSizeComboBox1.FormattingEnabled = true;
            pageSizeComboBox1.Items.AddRange(new object[] { "25", "50", "100", "500", "All" });
            pageSizeComboBox1.Location = new Point(95, 3);
            pageSizeComboBox1.Name = "pageSizeComboBox1";
            pageSizeComboBox1.Size = new Size(182, 33);
            pageSizeComboBox1.TabIndex = 1;
            pageSizeComboBox1.Text = "25";
            pageSizeComboBox1.SelectedIndexChanged += pageSizeComboBox1_SelectedIndexChanged;
            // 
            // labelTotalRows
            // 
            labelTotalRows.AutoSize = true;
            labelTotalRows.Location = new Point(283, 0);
            labelTotalRows.Name = "labelTotalRows";
            labelTotalRows.Size = new Size(59, 25);
            labelTotalRows.TabIndex = 2;
            labelTotalRows.Text = "label2";
            // 
            // activityDataGridView
            // 
            activityDataGridView.AllowUserToAddRows = false;
            activityDataGridView.AllowUserToDeleteRows = false;
            activityDataGridView.AllowUserToOrderColumns = true;
            activityDataGridView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            activityDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            activityDataGridView.Location = new Point(3, 3);
            activityDataGridView.MultiSelect = false;
            activityDataGridView.Name = "activityDataGridView";
            activityDataGridView.ReadOnly = true;
            activityDataGridView.RowHeadersWidth = 62;
            activityDataGridView.RowTemplate.Height = 33;
            activityDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            activityDataGridView.Size = new Size(1883, 553);
            activityDataGridView.TabIndex = 2;
            activityDataGridView.CellMouseEnter += activityDataGridView_CellMouseEnter;
            activityDataGridView.SelectionChanged += dataGridView1_SelectionChanged;
            // 
            // splitContainerTabsToList
            // 
            splitContainerTabsToList.Dock = DockStyle.Fill;
            splitContainerTabsToList.Location = new Point(0, 0);
            splitContainerTabsToList.Name = "splitContainerTabsToList";
            // 
            // splitContainerTabsToList.Panel1
            // 
            splitContainerTabsToList.Panel1.Controls.Add(tabControl2);
            // 
            // splitContainerTabsToList.Panel2
            // 
            splitContainerTabsToList.Panel2.Controls.Add(listViewGraphOptions);
            splitContainerTabsToList.Size = new Size(1902, 686);
            splitContainerTabsToList.SplitterDistance = 1700;
            splitContainerTabsToList.TabIndex = 0;
            // 
            // tabControl2
            // 
            tabControl2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl2.Controls.Add(tabPage1);
            tabControl2.Controls.Add(tabPage2);
            tabControl2.Controls.Add(tabPage3);
            tabControl2.Location = new Point(6, -5);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new Size(1691, 688);
            tabControl2.TabIndex = 1;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(formsPlot1);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1683, 650);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Time Graph";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // formsPlot1
            // 
            formsPlot1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlot1.Location = new Point(0, 0);
            formsPlot1.Margin = new Padding(6, 5, 6, 5);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(1677, 645);
            formsPlot1.TabIndex = 0;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(activityData1);
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(192, 62);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Activity Data";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // activityData1
            // 
            activityData1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            activityData1.Location = new Point(3, 3);
            activityData1.Name = "activityData1";
            activityData1.Size = new Size(183, 53);
            activityData1.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(activityMap1);
            tabPage3.Location = new Point(4, 34);
            tabPage3.Name = "tabPage3";
            tabPage3.Size = new Size(192, 62);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Activty Map";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // activityMap1
            // 
            activityMap1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            activityMap1.BackColor = SystemColors.ActiveCaption;
            activityMap1.Location = new Point(3, 3);
            activityMap1.Name = "activityMap1";
            activityMap1.Size = new Size(186, 59);
            activityMap1.TabIndex = 0;
            // 
            // listViewGraphOptions
            // 
            listViewGraphOptions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewGraphOptions.Location = new Point(3, 3);
            listViewGraphOptions.Name = "listViewGraphOptions";
            listViewGraphOptions.Size = new Size(195, 680);
            listViewGraphOptions.TabIndex = 0;
            listViewGraphOptions.UseCompatibleStateImageBehavior = false;
            // 
            // ActivityReport
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainerReportToTabs);
            Name = "ActivityReport";
            Size = new Size(1902, 1308);
            splitContainerReportToTabs.Panel1.ResumeLayout(false);
            splitContainerReportToTabs.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerReportToTabs).EndInit();
            splitContainerReportToTabs.ResumeLayout(false);
            splitContainerTopButtonsToReport.Panel1.ResumeLayout(false);
            splitContainerTopButtonsToReport.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerTopButtonsToReport).EndInit();
            splitContainerTopButtonsToReport.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)activityDataGridView).EndInit();
            splitContainerTabsToList.Panel1.ResumeLayout(false);
            splitContainerTabsToList.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainerTabsToList).EndInit();
            splitContainerTabsToList.ResumeLayout(false);
            tabControl2.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            ResumeLayout(false);
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
