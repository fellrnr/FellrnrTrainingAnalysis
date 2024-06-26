﻿namespace FellrnrTrainingAnalysis.UI
{
    public partial class ProgressDialog : Form, IProgress
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        public string TaskName { get { return progressLabel.Text; } set { progressLabel.Text = value; } }
        public int Maximum { get { return progressBar1.Maximum; } set { progressBar1.Maximum = value; } }
        public int Progress { get { return progressBar1.Value; } set { progressBar1.Value = value; this.Text = $"Processed {value} of {Maximum}"; } }

        public void ShowMe()
        {
            this.Show();
        }

        public void HideMe()
        {
            this.Hide();
        }
    }
}
