namespace FellrnrTrainingAnalysis.UI
{
    public partial class OptionsDialog : Form
    {
        public OptionsDialog()
        {
            InitializeComponent();
            propertyGrid1.SelectedObject = Utils.Options.Instance;
        }

        private void applyAndCloseToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void applyToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
