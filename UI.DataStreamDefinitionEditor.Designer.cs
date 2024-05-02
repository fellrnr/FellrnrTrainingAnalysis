namespace FellrnrTrainingAnalysis.UI
{
    partial class TimeSeriesDefinitionEditor
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            objectListView1 = new BrightIdeasSoftware.ObjectListView();
            ((System.ComponentModel.ISupportInitialize)objectListView1).BeginInit();
            SuspendLayout();
            // 
            // objectListView1
            // 
            objectListView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            objectListView1.Location = new Point(9, 9);
            objectListView1.Name = "objectListView1";
            objectListView1.ShowGroups = false;
            objectListView1.Size = new Size(230, 360);
            objectListView1.TabIndex = 0;
            objectListView1.View = View.Details;
            objectListView1.CellEditFinished += objectListView1_CellEditFinished;
            objectListView1.SubItemChecking += objectListView1_SubItemChecking;
            // 
            // TimeSeriesDefinitionEditor
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(251, 381);
            Controls.Add(objectListView1);
            Name = "TimeSeriesDefinitionEditor";
            Text = "TimeSeriesDefinitionEditor";
            ((System.ComponentModel.ISupportInitialize)objectListView1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private BrightIdeasSoftware.ObjectListView objectListView1;
    }
}