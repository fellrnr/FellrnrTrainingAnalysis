using FellrnrTrainingAnalysis.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ProgressDialog : Form, IProgress
    {
        public ProgressDialog()
        {
            InitializeComponent();
        }

        public string TaskName { get { return progressLabel.Text; } set { progressLabel.Text = value; } }
        public int Maximum { get { return progressBar1.Maximum; } set { progressBar1.Maximum = value; } }
        public int Progress { get { return progressBar1.Value; } set { progressBar1.Value = value; } }

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
