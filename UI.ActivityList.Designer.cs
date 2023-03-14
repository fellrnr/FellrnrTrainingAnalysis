namespace FellrnrTrainingAnalysis.UI
{
    partial class ActivityList
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
            this.components = new System.ComponentModel.Container();
            this.dataTreeListView = new BrightIdeasSoftware.DataTreeListView();
            ((System.ComponentModel.ISupportInitialize)(this.dataTreeListView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataTreeListView
            // 
            this.dataTreeListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataTreeListView.DataSource = null;
            this.dataTreeListView.KeyAspectName = "num";
            this.dataTreeListView.Location = new System.Drawing.Point(3, 3);
            this.dataTreeListView.Name = "dataTreeListView";
            this.dataTreeListView.ParentKeyAspectName = "pid";
            this.dataTreeListView.RootKeyValueString = "";
            this.dataTreeListView.ShowGroups = false;
            this.dataTreeListView.Size = new System.Drawing.Size(986, 631);
            this.dataTreeListView.TabIndex = 2;
            this.dataTreeListView.UseCompatibleStateImageBehavior = false;
            this.dataTreeListView.View = System.Windows.Forms.View.Details;
            this.dataTreeListView.VirtualMode = true;
            // 
            // ActivityList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataTreeListView);
            this.Name = "ActivityList";
            this.Size = new System.Drawing.Size(992, 637);
            ((System.ComponentModel.ISupportInitialize)(this.dataTreeListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private BrightIdeasSoftware.DataTreeListView dataTreeListView;
    }
}
