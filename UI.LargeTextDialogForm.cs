namespace FellrnrTrainingAnalysis.UI
{
    public partial class LargeTextDialogForm : Form
    {
        public LargeTextDialogForm(string text)
        {
            InitializeComponent();
            textBox1.Text = text;
        }

        public bool Cancelled { get; set; } = false;

        public string Value { get { return textBox1.Text; } }

        private void OkayButton_Click(object sender, EventArgs e)
        {
            Cancelled = false;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Cancelled = true;
            this.Close();
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }
    }
}
