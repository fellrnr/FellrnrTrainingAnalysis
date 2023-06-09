﻿using BrightIdeasSoftware;
using FellrnrTrainingAnalysis.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ActivityList : UserControl
    {
        private const string DateTreeColumn = "Start Time";
        private const string ParentId = "par";
        private const string Id = "Id";
        private List<DataRow> lastRows = new List<DataRow>();

        public ActivityList()
        {
            InitializeComponent();
            dataTreeListView.KeyAspectName = Id;
            dataTreeListView.ParentKeyAspectName = ParentId;
            this.dataTreeListView.RootKeyValue = new Utils.DateTimeTree();
            //olvDataTree.RootKeyValue = 0u;
            dataTreeListView.EmptyListMsg = "Empty!";
            dataTreeListView.ShowKeyColumns = false; //have to hide key columns so we don't show parent, then duplicate the key column

        }

        public void Display(Database database)
        {
            DataTreeListView dataTreeListView_debug = dataTreeListView; //make this a local to simplify debugging



            lastRows.Clear();

            var myTable = new DataTable("Person");
            myTable.Clear();
            myTable.Columns.Add(Create(Id, typeof(Utils.DateTimeTree)));
            myTable.Columns.Add(Create(ParentId, typeof(Utils.DateTimeTree)));
            myTable.Columns.Add(Create(DateTreeColumn, typeof(Utils.DateTimeTree)));

            if (database.CurrentAthlete != null && 
                database.CurrentAthlete.CalendarTree != null && 
                database.CurrentAthlete.CalendarTree.Count > 0 && 
                database.CurrentAthlete.CalendarTree.First().Value.DataNames != null)
            {
                //gather the list of column names from the root calendar nodes
                List<string> masterDataNames = new List<string>();
                foreach (KeyValuePair<DateTime, CalendarNode> kvp in database.CurrentAthlete.CalendarTree)
                {
                    CalendarNode calendarNode = kvp.Value;
                    IReadOnlyCollection<string> dataNames = calendarNode.DataNames;
                    foreach (string s in dataNames)
                    {
                        if(!masterDataNames.Contains(s))
                            masterDataNames.Add(s);
                    }
                }
                foreach (string s in masterDataNames)
                {
                    myTable.Columns.Add(Create(s, typeof(string))); //For now, just create as string
                }
                foreach (KeyValuePair<DateTime, CalendarNode> kvp in database.CurrentAthlete.CalendarTree)
                {
                    CalendarNode calendarNode = kvp.Value;

                    bool lastChild = (database.CurrentAthlete.CalendarTree.Last().Value == calendarNode);

                    Add(myTable, calendarNode, new Utils.DateTimeTree(), masterDataNames, lastChild);
                }



                dataTreeListView.SuspendLayout();


                if (dataTreeListView.Columns.Count != myTable.Columns.Count)
                {
                    //dataTreeListView.Clear(); //tree list view doesn't come back from a clear
                    dataTreeListView.Reset();  //we have to do a reset if things change, like number of columns
                }

                dataTreeListView.DataSource = myTable;

                foreach (DataRow dataRow in lastRows)
                {
                    DataRowView drv = myTable.DefaultView[myTable.Rows.IndexOf(dataRow)];
                    dataTreeListView.Expand(drv);
                }

                dataTreeListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                dataTreeListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                dataTreeListView.ResumeLayout();
            }
        }


        private void Add(DataTable myTable, Extensible extensible, Utils.DateTimeTree parentId, IReadOnlyCollection<string> masterDataNames, bool isLast)
        {
            if (extensible is CalendarNode)
            {
                CalendarNode calendarNode = (CalendarNode)extensible;
                if (calendarNode.Children.Count == 1) //if we have only one child, skip this level
                {
                    Extensible e = calendarNode.Children.First().Value;
                    bool lastChild = true; //there's only one, so it must be last

                    Add(myTable, e, parentId, masterDataNames, lastChild); //pass our parent's id to our child

                    return;
                }
            }

            DataRow dataRow = myTable.NewRow();
            dataRow[Id] = extensible.Id;
            dataRow[ParentId] = parentId;
            dataRow[DateTreeColumn] = extensible.Id;
            foreach (Datum d in extensible.DataValues)
            {
                if (masterDataNames.Contains(d.Name))
                {
                    dataRow[d.Name] = d.ToString();
                }
            }

            myTable.Rows.Add(dataRow);
            if (isLast)
                lastRows.Add(dataRow);

            if (extensible is CalendarNode)
            {
                CalendarNode calendarNode = (CalendarNode)extensible;

                foreach (Extensible e in calendarNode.Children.Values)
                {
                    bool lastChild = (calendarNode.Children.Values.Last() == e);

                    Add(myTable, e, extensible.Id, masterDataNames, lastChild);
                }
            }
        }

        private DataColumn Create(string name, Type type)
        {
            DataColumn column = new DataColumn();
            column.DataType = type;
            column.AllowDBNull = true;
            column.Caption = name;
            column.ColumnName = name;

            return column;
        }
    }
}
