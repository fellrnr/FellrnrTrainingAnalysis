using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FellrnrTrainingAnalysis
{
    public partial class LargeTextDialogForm : Form
    {
        public LargeTextDialogForm(string text)
        {
            InitializeComponent();
            textBox1.Text = text;
        }
    }
}
