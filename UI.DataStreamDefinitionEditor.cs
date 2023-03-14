using BrightIdeasSoftware;
using FellrnrTrainingAnalysis.Model;
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
    public partial class DataStreamDefinitionEditor : Form
    {
        public DataStreamDefinitionEditor(List<DataStreamDefinition> definitions)
        {
            InitializeComponent();

            Definitions= definitions;
            objectListView1.SuspendLayout();

            objectListView1.ShowGroups= false;
            objectListView1.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;

            //Generator.GenerateColumns(objectListView1, definitions);
            Generator.GenerateColumns(this.objectListView1, typeof(DataStreamDefinition), true);
            objectListView1.SetObjects(definitions);
            objectListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            objectListView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            objectListView1.ResumeLayout();
        }

        public List<DataStreamDefinition> Definitions { get; }

        public delegate void EditEventHandler(DataStreamDefinitionEditor sender);

        public event EditEventHandler? Edited;

        private void objectListView1_CellEditFinished(object sender, CellEditEventArgs e)
        {
            Edited?.Invoke(this);
        }

        private void objectListView1_SubItemChecking(object sender, SubItemCheckingEventArgs e) //TODO: Check box changes don't update the model
        {
            Edited?.Invoke(this);
        }

    }
}
