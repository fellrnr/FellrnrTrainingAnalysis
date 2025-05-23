﻿namespace FellrnrTrainingAnalysis.UI
{
    partial class ProgressGraph
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
            flowLayoutPanel1 = new FlowLayoutPanel();
            label1 = new Label();
            timePeriodComboBox = new ComboBox();
            label2 = new Label();
            dateTimePickerStart = new DateTimePicker();
            label3 = new Label();
            dateTimePickerEnd = new DateTimePicker();
            splitContainer1 = new SplitContainer();
            formsPlotProgress = new ScottPlot.FormsPlot();
            tableLayoutPanel1 = new TableLayoutPanel();
            operationLabelCol1 = new Label();
            dataLabelCol0 = new Label();
            typeLabelCol2 = new Label();
            smoothLabelCol3 = new Label();
            axisLabelCol4 = new Label();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.Controls.Add(timePeriodComboBox);
            flowLayoutPanel1.Controls.Add(label2);
            flowLayoutPanel1.Controls.Add(dateTimePickerStart);
            flowLayoutPanel1.Controls.Add(label3);
            flowLayoutPanel1.Controls.Add(dateTimePickerEnd);
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(2288, 73);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(105, 25);
            label1.TabIndex = 0;
            label1.Text = "Time Period";
            // 
            // timePeriodComboBox
            // 
            timePeriodComboBox.FormattingEnabled = true;
            timePeriodComboBox.Items.AddRange(new object[] { "1W", "1M", "3M", "6M", "1Y", "2Y", "3Y", "4Y", "5Y", "All" });
            timePeriodComboBox.Location = new Point(114, 3);
            timePeriodComboBox.Name = "timePeriodComboBox";
            timePeriodComboBox.Size = new Size(182, 33);
            timePeriodComboBox.TabIndex = 1;
            timePeriodComboBox.SelectedIndexChanged += timePeriodComboBox_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(302, 0);
            label2.Name = "label2";
            label2.Size = new Size(90, 25);
            label2.TabIndex = 2;
            label2.Text = "Start Date";
            // 
            // dateTimePickerStart
            // 
            dateTimePickerStart.Location = new Point(398, 3);
            dateTimePickerStart.Name = "dateTimePickerStart";
            dateTimePickerStart.Size = new Size(191, 31);
            dateTimePickerStart.TabIndex = 3;
            dateTimePickerStart.ValueChanged += dateTimePickerStart_ValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(595, 0);
            label3.Name = "label3";
            label3.Size = new Size(84, 25);
            label3.TabIndex = 4;
            label3.Text = "End Date";
            // 
            // dateTimePickerEnd
            // 
            dateTimePickerEnd.Location = new Point(685, 3);
            dateTimePickerEnd.Name = "dateTimePickerEnd";
            dateTimePickerEnd.Size = new Size(194, 31);
            dateTimePickerEnd.TabIndex = 5;
            dateTimePickerEnd.ValueChanged += dateTimePickerEnd_ValueChanged;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(3, 79);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(formsPlotProgress);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tableLayoutPanel1);
            splitContainer1.Size = new Size(1028, 600);
            splitContainer1.SplitterDistance = 641;
            splitContainer1.TabIndex = 1;
            // 
            // formsPlotProgress
            // 
            formsPlotProgress.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlotProgress.Location = new Point(0, 0);
            formsPlotProgress.Margin = new Padding(6, 5, 6, 5);
            formsPlotProgress.Name = "formsPlotProgress";
            formsPlotProgress.Size = new Size(635, 595);
            formsPlotProgress.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tableLayoutPanel1.AutoScroll = true;
            tableLayoutPanel1.ColumnCount = 5;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle());
            tableLayoutPanel1.Controls.Add(dataLabelCol0, 0, 0);
            tableLayoutPanel1.Controls.Add(axisLabelCol4, 4, 0);
            tableLayoutPanel1.Controls.Add(operationLabelCol1, 1, 0);
            tableLayoutPanel1.Controls.Add(typeLabelCol2, 2, 0);
            tableLayoutPanel1.Controls.Add(smoothLabelCol3, 3, 0);
            tableLayoutPanel1.Location = new Point(3, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle());
            tableLayoutPanel1.Size = new Size(377, 597);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // operationLabelCol1
            // 
            operationLabelCol1.AutoSize = true;
            operationLabelCol1.Location = new Point(58, 0);
            operationLabelCol1.Name = "operationLabelCol1";
            operationLabelCol1.Size = new Size(92, 25);
            operationLabelCol1.TabIndex = 3;
            operationLabelCol1.Text = "Operation";
            // 
            // dataLabelCol0
            // 
            dataLabelCol0.AutoSize = true;
            dataLabelCol0.Location = new Point(3, 0);
            dataLabelCol0.Name = "dataLabelCol0";
            dataLabelCol0.Size = new Size(49, 25);
            dataLabelCol0.TabIndex = 0;
            dataLabelCol0.Text = "Data";
            // 
            // typeLabelCol2
            // 
            typeLabelCol2.AutoSize = true;
            typeLabelCol2.Location = new Point(156, 0);
            typeLabelCol2.Name = "typeLabelCol2";
            typeLabelCol2.Size = new Size(49, 25);
            typeLabelCol2.TabIndex = 1;
            typeLabelCol2.Text = "Type";
            // 
            // smoothLabelCol3
            // 
            smoothLabelCol3.AutoSize = true;
            smoothLabelCol3.Location = new Point(211, 0);
            smoothLabelCol3.Name = "smoothLabelCol3";
            smoothLabelCol3.Size = new Size(76, 25);
            smoothLabelCol3.TabIndex = 2;
            smoothLabelCol3.Text = "Smooth";
            // 
            // axisLabelCol4
            // 
            axisLabelCol4.AutoSize = true;
            axisLabelCol4.Location = new Point(293, 0);
            axisLabelCol4.Name = "axisLabelCol4";
            axisLabelCol4.Size = new Size(44, 25);
            axisLabelCol4.TabIndex = 4;
            axisLabelCol4.Text = "Axis";
            // 
            // ProgressGraph
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Controls.Add(flowLayoutPanel1);
            Name = "ProgressGraph";
            Size = new Size(1034, 679);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private Label label1;
        private ComboBox timePeriodComboBox;
        private Label label2;
        private DateTimePicker dateTimePickerStart;
        private Label label3;
        private DateTimePicker dateTimePickerEnd;
        private SplitContainer splitContainer1;
        private ScottPlot.FormsPlot formsPlotProgress;
        private TableLayoutPanel tableLayoutPanel1;
        private Label dataLabelCol0;
        private Label typeLabelCol2;
        private Label smoothLabelCol3;
        private Label operationLabelCol1;
        private Label axisLabelCol4;
    }
}
