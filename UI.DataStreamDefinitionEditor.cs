using BrightIdeasSoftware;
using FellrnrTrainingAnalysis.Model;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class TimeSeriesDefinitionEditor : Form
    {
        public TimeSeriesDefinitionEditor(List<TimeSeriesDefinition> definitions)
        {
            InitializeComponent();

            Definitions = definitions;
            objectListView1.SuspendLayout();

            objectListView1.ShowGroups = false;
            objectListView1.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
            //Generator.GenerateColumns(objectListView1, definitions);
            Generator.GenerateColumns(this.objectListView1, typeof(TimeSeriesDefinition), true);
            objectListView1.SetObjects(definitions);
            objectListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            objectListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            objectListView1.ResumeLayout();


        }

        public List<TimeSeriesDefinition> Definitions { get; }

        public delegate void EditEventHandler(TimeSeriesDefinitionEditor sender);

        public event EditEventHandler? Edited;

        private void objectListView1_CellEditFinished(object sender, CellEditEventArgs e)
        {
            Edited?.Invoke(this);
        }

        private void objectListView1_SubItemChecking(object sender, SubItemCheckingEventArgs e) //TODO: Check box changes don't update the model
        {
            TimeSeriesDefinition row = (TimeSeriesDefinition)e.RowObject;
            row.ShowReportGraph = (e.NewValue == CheckState.Checked);
            //objectListView1.RefreshObject(e.RowObject);
            Edited?.Invoke(this);
        }



    }
}
