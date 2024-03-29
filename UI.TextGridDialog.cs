using FellrnrTrainingAnalysis.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FellrnrTrainingAnalysis
{
    public partial class TextGridDialog : Form
    {
        public TextGridDialog(DataTable dt, bool freezeFirstColumn = true)
        {
            InitializeComponent();
            Table = dt;
            //dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            //dataGridView1.DefaultCellStyle.
            //dataGridView1.DataSource = new ArrayDataView(array, headers);
            dataGridView1.DataSource = dt;
            foreach (DataGridViewColumn c in dataGridView1.Columns)
            {
                c.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            }
            this.dataGridView1.Columns[0].Frozen = freezeFirstColumn;
        }

        DataTable Table { get; set; }


        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
        }

        private void dataGridView1_VisibleChanged(object sender, EventArgs e)
        {
            //for (int j = 1; j < dataGridView1.Rows.Count; j++) //start at one to skip the header
            //{
            //    DataGridViewRow displayrow = dataGridView1.Rows[j];
            //    DataRow datarow = Table.Rows[j - 1]; //no header in data table
            //    for (int i = 0; i < displayrow.Cells.Count; i++)
            //    {
            //        DataGridViewCell c = displayrow.Cells[i];
            //        object? o = datarow[i];
            //        if (o != null && o is LinearRegression)
            //        {
            //            LinearRegression lr = (LinearRegression)o;
            //            c.Style.BackColor = Utils.Misc.GetColorForValue(lr.RSquared, 1.0, 128, ColorMap: null);
            //        }
            //    }
            //}

        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            int rowid = e.RowIndex;
            int colid = e.ColumnIndex;
            if(rowid >= Table.Rows.Count) { return; } //extra row for adding data
            DataGridViewRow displayrow = dataGridView1.Rows[rowid];
            DataRow datarow = Table.Rows[rowid]; //no header in data table
            DataGridViewCell c = displayrow.Cells[colid];
            object? o = datarow[colid];
            if (o != null && o is LinearRegression)
            {
                LinearRegression lr = (LinearRegression)o;
                Color col = Utils.Misc.GetColorForValue(1.0f - lr.RSquared, 1.0, 255, ColorMap: null);
                if (e.CellStyle != null)
                    e.CellStyle.BackColor = col;
                //c.Style.BackColor = Utils.Misc.GetColorForValue(lr.RSquared, 1.0, 128, ColorMap: null);
            }
        }
    }
}