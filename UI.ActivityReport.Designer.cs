namespace FellrnrTrainingAnalysis.UI
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
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            splitContainerReportToTabs = new SplitContainer();
            splitContainerTopButtonsToReport = new SplitContainer();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label1 = new Label();
            pageSizeComboBox1 = new ComboBox();
            labelTotalRows = new Label();
            activityDataGridView = new DataGridView();
            tabControl2 = new TabControl();
            tabPageTimeGraph = new TabPage();
            activityTimeGraph1 = new ActivityTimeGraph();
            tabPageActivityTable = new TabPage();
            activityData1 = new ActivityData();
            tabPageActivityMap = new TabPage();
            activityMap1 = new ActivityMap();
            tabPagePowerDistributionCurve = new TabPage();
            powerDistributionCurveGraph1 = new PowerDistributionCurveGraph();
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
            tabControl2.SuspendLayout();
            tabPageTimeGraph.SuspendLayout();
            tabPageActivityTable.SuspendLayout();
            tabPageActivityMap.SuspendLayout();
            tabPagePowerDistributionCurve.SuspendLayout();
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
            splitContainerReportToTabs.Panel2.Controls.Add(tabControl2);
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
            pageSizeComboBox1.Items.AddRange(new object[] { "10", "15", "20", "25", "50", "100", "500", "All" });
            pageSizeComboBox1.Location = new Point(95, 3);
            pageSizeComboBox1.Name = "pageSizeComboBox1";
            pageSizeComboBox1.Size = new Size(182, 33);
            pageSizeComboBox1.TabIndex = 1;
            pageSizeComboBox1.Text = "20";
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
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = SystemColors.Control;
            dataGridViewCellStyle1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            activityDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            activityDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = SystemColors.Window;
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            activityDataGridView.DefaultCellStyle = dataGridViewCellStyle2;
            activityDataGridView.Location = new Point(3, 3);
            activityDataGridView.MultiSelect = false;
            activityDataGridView.Name = "activityDataGridView";
            activityDataGridView.ReadOnly = true;
            activityDataGridView.RowHeadersWidth = 62;
            activityDataGridView.RowTemplate.Height = 33;
            activityDataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            activityDataGridView.Size = new Size(1883, 553);
            activityDataGridView.TabIndex = 2;
            activityDataGridView.CellFormatting += activityDataGridView_CellFormatting;
            activityDataGridView.CellMouseEnter += activityDataGridView_CellMouseEnter;
            activityDataGridView.SelectionChanged += dataGridView1_SelectionChanged;
            // 
            // tabControl2
            // 
            tabControl2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControl2.Controls.Add(tabPageTimeGraph);
            tabControl2.Controls.Add(tabPageActivityTable);
            tabControl2.Controls.Add(tabPageActivityMap);
            tabControl2.Controls.Add(tabPagePowerDistributionCurve);
            tabControl2.Location = new Point(0, 0);
            tabControl2.Name = "tabControl2";
            tabControl2.SelectedIndex = 0;
            tabControl2.Size = new Size(1886, 683);
            tabControl2.TabIndex = 1;
            // 
            // tabPageTimeGraph
            // 
            tabPageTimeGraph.Controls.Add(activityTimeGraph1);
            tabPageTimeGraph.Location = new Point(4, 34);
            tabPageTimeGraph.Name = "tabPageTimeGraph";
            tabPageTimeGraph.Padding = new Padding(3);
            tabPageTimeGraph.Size = new Size(1878, 645);
            tabPageTimeGraph.TabIndex = 0;
            tabPageTimeGraph.Text = "Time Graph";
            tabPageTimeGraph.UseVisualStyleBackColor = true;
            // 
            // activityTimeGraph1
            // 
            activityTimeGraph1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            activityTimeGraph1.Location = new Point(0, 3);
            activityTimeGraph1.Name = "activityTimeGraph1";
            activityTimeGraph1.Size = new Size(1872, 636);
            activityTimeGraph1.TabIndex = 0;
            // 
            // tabPageActivityTable
            // 
            tabPageActivityTable.Controls.Add(activityData1);
            tabPageActivityTable.Location = new Point(4, 34);
            tabPageActivityTable.Name = "tabPageActivityTable";
            tabPageActivityTable.Padding = new Padding(3);
            tabPageActivityTable.Size = new Size(192, 62);
            tabPageActivityTable.TabIndex = 1;
            tabPageActivityTable.Text = "Activity Table";
            tabPageActivityTable.UseVisualStyleBackColor = true;
            // 
            // activityData1
            // 
            activityData1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            activityData1.Location = new Point(3, 3);
            activityData1.Name = "activityData1";
            activityData1.Size = new Size(172, 39);
            activityData1.TabIndex = 0;
            // 
            // tabPageActivityMap
            // 
            tabPageActivityMap.Controls.Add(activityMap1);
            tabPageActivityMap.Location = new Point(4, 34);
            tabPageActivityMap.Name = "tabPageActivityMap";
            tabPageActivityMap.Size = new Size(192, 62);
            tabPageActivityMap.TabIndex = 2;
            tabPageActivityMap.Text = "Activty Map";
            tabPageActivityMap.UseVisualStyleBackColor = true;
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
            // tabPagePowerDistributionCurve
            // 
            tabPagePowerDistributionCurve.Controls.Add(powerDistributionCurveGraph1);
            tabPagePowerDistributionCurve.Location = new Point(4, 34);
            tabPagePowerDistributionCurve.Name = "tabPagePowerDistributionCurve";
            tabPagePowerDistributionCurve.Size = new Size(1878, 645);
            tabPagePowerDistributionCurve.TabIndex = 3;
            tabPagePowerDistributionCurve.Text = "Power Distribution Curve";
            tabPagePowerDistributionCurve.UseVisualStyleBackColor = true;
            // 
            // powerDistributionCurveGraph1
            // 
            powerDistributionCurveGraph1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            powerDistributionCurveGraph1.Location = new Point(3, 3);
            powerDistributionCurveGraph1.Name = "powerDistributionCurveGraph1";
            powerDistributionCurveGraph1.Size = new Size(1872, 639);
            powerDistributionCurveGraph1.TabIndex = 0;
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
            tabControl2.ResumeLayout(false);
            tabPageTimeGraph.ResumeLayout(false);
            tabPageActivityTable.ResumeLayout(false);
            tabPageActivityMap.ResumeLayout(false);
            tabPagePowerDistributionCurve.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainerReportToTabs;
        private SplitContainer splitContainerTopButtonsToReport;
        private FlowLayoutPanel flowLayoutPanel1;
        private DataGridView activityDataGridView;
        private TabControl tabControl2;
        private TabPage tabPageTimeGraph;
        private TabPage tabPageActivityTable;
        private Label label1;
        private ComboBox pageSizeComboBox1;
        private Label labelTotalRows;
        private UI.ActivityData activityData1;
        private TabPage tabPageActivityMap;
        private UI.ActivityMap activityMap1;
        private TabPage tabPagePowerDistributionCurve;
        private ActivityTimeGraph activityTimeGraph1;
        private PowerDistributionCurveGraph powerDistributionCurveGraph1;
    }
}