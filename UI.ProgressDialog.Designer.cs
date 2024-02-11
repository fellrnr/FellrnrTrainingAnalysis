namespace FellrnrTrainingAnalysis.UI
{
    partial class ProgressDialog
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
            progressBar1 = new ProgressBar();
            progressLabel = new Label();
            SuspendLayout();
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 64);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(473, 34);
            progressBar1.TabIndex = 0;
            // 
            // progressLabel
            // 
            progressLabel.AutoSize = true;
            progressLabel.Location = new Point(11, 18);
            progressLabel.Name = "progressLabel";
            progressLabel.Size = new Size(59, 25);
            progressLabel.TabIndex = 1;
            progressLabel.Text = "label1";
            // 
            // ProgressDialog
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(497, 123);
            Controls.Add(progressLabel);
            Controls.Add(progressBar1);
            Name = "ProgressDialog";
            Text = "ProgressDialog";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ProgressBar progressBar1;
        private Label progressLabel;
    }
}