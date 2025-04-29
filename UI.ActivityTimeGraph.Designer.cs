namespace FellrnrTrainingAnalysis.UI
{
    partial class ActivityTimeGraph
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
            splitContainer1 = new SplitContainer();
            positionLabel = new Label();
            formsPlot1 = new ScottPlot.FormsPlot();
            buttonDraw = new Button();
            objectListViewTimeSeries = new BrightIdeasSoftware.ObjectListView();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)objectListViewTimeSeries).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(positionLabel);
            splitContainer1.Panel1.Controls.Add(formsPlot1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(buttonDraw);
            splitContainer1.Panel2.Controls.Add(objectListViewTimeSeries);
            splitContainer1.Size = new Size(1134, 678);
            splitContainer1.SplitterDistance = 926;
            splitContainer1.TabIndex = 0;
            // 
            // positionLabel
            // 
            positionLabel.AutoSize = true;
            positionLabel.Location = new Point(6, 1);
            positionLabel.Name = "positionLabel";
            positionLabel.Size = new Size(79, 25);
            positionLabel.TabIndex = 2;
            positionLabel.Text = "Position:";
            // 
            // formsPlot1
            // 
            formsPlot1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            formsPlot1.Location = new Point(6, 31);
            formsPlot1.Margin = new Padding(6, 5, 6, 5);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(914, 642);
            formsPlot1.TabIndex = 1;
            formsPlot1.MouseMove += formsPlot1_MouseMove;
            // 
            // buttonDraw
            // 
            buttonDraw.Location = new Point(3, 3);
            buttonDraw.Name = "buttonDraw";
            buttonDraw.Size = new Size(112, 34);
            buttonDraw.TabIndex = 3;
            buttonDraw.Text = "Draw";
            buttonDraw.UseVisualStyleBackColor = true;
            buttonDraw.Click += buttonDraw_Click;
            // 
            // objectListViewTimeSeries
            // 
            objectListViewTimeSeries.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            objectListViewTimeSeries.Location = new Point(3, 55);
            objectListViewTimeSeries.Name = "objectListViewTimeSeries";
            objectListViewTimeSeries.ShowGroups = false;
            objectListViewTimeSeries.Size = new Size(198, 622);
            objectListViewTimeSeries.TabIndex = 2;
            objectListViewTimeSeries.View = View.Details;
            objectListViewTimeSeries.CellEditFinished += objectListViewTimeSeries_CellEditFinished;
            objectListViewTimeSeries.CellEditValidating += objectListViewTimeSeries_CellEditValidating;
            objectListViewTimeSeries.CellClick += objectListViewTimeSeries_CellClick;
            objectListViewTimeSeries.SubItemChecking += objectListViewTimeSeries_SubItemChecking;
            objectListViewTimeSeries.ItemsChanged += objectListViewTimeSeries_ItemsChanged;
            // 
            // ActivityTimeGraph
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(splitContainer1);
            Name = "ActivityTimeGraph";
            Size = new Size(1140, 681);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)objectListViewTimeSeries).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private BrightIdeasSoftware.ObjectListView objectListViewTimeSeries;
        private ScottPlot.FormsPlot formsPlot1;
        private Label positionLabel;
        private Button buttonDraw;
    }
}
