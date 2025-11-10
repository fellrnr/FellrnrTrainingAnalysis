namespace FellrnrTrainingAnalysis.UI
{
    partial class LapDetails
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
            lapPanel = new TableLayoutPanel();
            toolTip1 = new ToolTip(components);
            SuspendLayout();
            // 
            // lapPanel
            // 
            lapPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lapPanel.AutoScroll = true;
            lapPanel.ColumnCount = 1;
            lapPanel.ColumnStyles.Add(new ColumnStyle());
            lapPanel.Location = new Point(3, 3);
            lapPanel.Name = "lapPanel";
            lapPanel.RowCount = 1;
            lapPanel.RowStyles.Add(new RowStyle());
            lapPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            lapPanel.Size = new Size(1184, 620);
            lapPanel.TabIndex = 0;
            // 
            // LapDetails
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lapPanel);
            Name = "LapDetails";
            Size = new Size(1187, 623);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel lapPanel;
        private ToolTip toolTip1;
    }
}
