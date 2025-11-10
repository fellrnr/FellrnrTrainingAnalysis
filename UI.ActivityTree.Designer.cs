namespace FellrnrTrainingAnalysis.UI
{
    partial class ActivityTree
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
            components = new System.ComponentModel.Container();
            calendarTreeListView = new BrightIdeasSoftware.TreeListView();
            ((System.ComponentModel.ISupportInitialize)calendarTreeListView).BeginInit();
            SuspendLayout();
            // 
            // calendarTreeListView
            // 
            calendarTreeListView.AlternateRowBackColor = Color.FromArgb(192, 255, 255);
            calendarTreeListView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            calendarTreeListView.GridLines = true;
            calendarTreeListView.Location = new Point(3, 3);
            calendarTreeListView.Name = "calendarTreeListView";
            calendarTreeListView.ShowGroups = false;
            calendarTreeListView.Size = new Size(986, 631);
            calendarTreeListView.TabIndex = 0;
            calendarTreeListView.View = View.Details;
            calendarTreeListView.VirtualMode = true;
            calendarTreeListView.FormatRow += calendarTreeListView_FormatRow;
            calendarTreeListView.SelectionChanged += calendarTreeListView_SelectionChanged;
            // 
            // ActivityTree
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(calendarTreeListView);
            Name = "ActivityTree";
            Size = new Size(992, 637);
            ((System.ComponentModel.ISupportInitialize)calendarTreeListView).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.TreeListView calendarTreeListView;
    }
}
