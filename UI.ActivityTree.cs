using BrightIdeasSoftware;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using System.Collections;
using System.Data;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ActivityTree : UserControl
    {
        private const string DateTreeColumn = "Start Time";
        private const string ParentId = "par";
        private const string Id = "Id";
        private List<DataRow> lastRows = new List<DataRow>();

        public ActivityTree()
        {
            InitializeComponent();
            //calendarTreeListView.KeyAspectName = Id;
            //calendarTreeListView.ParentKeyAspectName = ParentId;
            //this.calendarTreeListView.RootKeyValue = new Utils.DateTimeTree();
            //olvDataTree.RootKeyValue = 0u;
            calendarTreeListView.EmptyListMsg = "Empty!";
            //            calendarTreeListView.ShowKeyColumns = false; //have to hide key columns so we don't show parent, then duplicate the key column




        }

        private void debug(string x)
        {
            Logging.Instance.Debug(x);
        }

        private bool HasBeenShown = false;
        public void ShowNow(Database database)
        {
            Display(database);
        }

        public void Display(Database database)
        {
            DisplayOnce(database);

            IReadOnlyCollection<CalendarNode> values = database.CurrentAthlete.CalendarTree.Values;
            calendarTreeListView.Roots = values;

            ExpandLast(database);
            //this helps a bit
            //foreach (OLVColumn c in calendarTreeListView.AllColumns) { c.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent); }
            AutosizeColumns();
        }

        private void ExpandLast(Database database)
        {
            if (database.CurrentAthlete.CalendarTree.Count == 0)
                return;

            //year
            calendarTreeListView.Expand(database.CurrentAthlete.CalendarTree.Last());

            //last 7 days
            


        }


        private void AutosizeColumns()
        {
            foreach (ColumnHeader col in calendarTreeListView.Columns)
            {
                //auto resize column width

                int colWidthBeforeAutoResize = col.Width;
                col.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
                int colWidthAfterAutoResizeByHeader = col.Width;
                col.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                int colWidthAfterAutoResizeByContent = col.Width;

                if (colWidthAfterAutoResizeByHeader > colWidthAfterAutoResizeByContent)
                    col.AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);

                //specific adjusts

                //first column
                if (col.Index == 0)
                    //we have to manually take care of tree structure, checkbox and image
                    col.Width += 16 + 16 + calendarTreeListView.SmallImageSize.Width;
                //last column
                else if (col.Index == calendarTreeListView.Columns.Count - 1)
                    //avoid "fill free space" bug
                    if (colWidthBeforeAutoResize > colWidthAfterAutoResizeByContent)
                        col.Width = colWidthBeforeAutoResize;
                    else
                        col.Width = colWidthAfterAutoResizeByContent;
            }
        }

        public void DisplayOnce(Database database)
        {
            if (HasBeenShown) { Logging.Instance.Debug("UI.ActivityTree.Display, !HasBeenShown, returning"); return; }
            HasBeenShown = true;

            Logging.Instance.TraceEntry("ActivityTree.Display");
            TreeListView calendarTreeListView_debug = calendarTreeListView; //make this a local to simplify debugging


            calendarTreeListView.CanExpandGetter = delegate (object x) {
                //debug("can expand? " + x.ToString());
                if (x is not CalendarNode)
                    return false;
                return ((CalendarNode)x).Children.Count() > 0;
            };

            calendarTreeListView.ChildrenGetter = delegate (object x) {
                //debug("get kids " + x.ToString());
                if (x is not CalendarNode)
                    return new ArrayList(); //shouldn't happen due to check above

                return ((CalendarNode)x).Children.Values;
            };

            OLVColumn dateColumn = new OLVColumn();
            dateColumn.Text = "Date";
            dateColumn.AspectGetter = delegate (object x) { return ((Extensible)x).Id().ToString(); };
            dateColumn.IsEditable = false;
            dateColumn.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            calendarTreeListView.AllColumns.Add(dateColumn);


            lastRows.Clear();

            if (database.CurrentAthlete != null &&
                database.CurrentAthlete.CalendarTree != null &&
                database.CurrentAthlete.CalendarTree.Count > 0 &&
                database.CurrentAthlete.CalendarTree.First().Value.DataNames != null)
            {
                Logging.Instance.TraceEntry("ActivityTree.Display-datatable");
                //gather the list of column names from the root calendar nodes
                List<string> masterDataNames = new List<string>();
                foreach (KeyValuePair<DateTime, CalendarNode> kvp in database.CurrentAthlete.CalendarTree)
                {
                    CalendarNode calendarNode = kvp.Value;
                    IReadOnlyCollection<string> dataNames = calendarNode.DataNames;
                    foreach (string s in dataNames)
                    {
                        if (!masterDataNames.Contains(s))
                            masterDataNames.Add(s);
                    }
                }

                //SortedDictionary<int, DataColumn> keyValuePairs = new SortedDictionary<int, DataColumn>();
                foreach (string s in masterDataNames)
                {
                    ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(s);
                    if (activityDatumMetadata != null && activityDatumMetadata.PositionInTree != null && !activityDatumMetadata.Invisible.GetValueOrDefault(false))
                    {
                        OLVColumn aNewColumn = new OLVColumn();
                        aNewColumn.Text = activityDatumMetadata.Title;
                        aNewColumn.AspectGetter = delegate (object x) { return DatumFormatter.FormatForGrid(((Extensible)x).GetNamedDatum(s), activityDatumMetadata); }; 
                        // ((Extensible)x).GetNamedDatumForDisplay(s); };
                        aNewColumn.IsEditable = false;
                        //aNewColumn.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                        //aNewColumn.MinimumWidth = 100;
                        // ... configure it and finally ...
                        calendarTreeListView.AllColumns.Add(aNewColumn);

                        //TODO: ignore position for now
                        //int positionInTree = (int)activityDatumMetadata.PositionInTree;
                        //keyValuePairs.Add(positionInTree, dataColumn);
                        //myTable.Columns[column.Name].SetOrdinal(positionInTree);
                    }
                }

                calendarTreeListView.RebuildColumns();
                //calendarTreeListView.Expand(values.Last());

                /*
                Logging.Instance.TraceLeave();
                Logging.Instance.TraceEntry("ActivityTree.Display-tree");
                foreach (KeyValuePair<DateTime, CalendarNode> kvp in database.CurrentAthlete.CalendarTree)
                {
                    CalendarNode calendarNode = kvp.Value;

                    bool lastChild = (database.CurrentAthlete.CalendarTree.Last().Value == calendarNode);

                    Add(myTable, calendarNode, new Utils.DateTimeTree(), masterDataNames, lastChild);
                }



                Logging.Instance.TraceLeave();
                Logging.Instance.TraceEntry("ActivityTree.Display-view");
                calendarTreeListView.SuspendLayout();


                if (calendarTreeListView.Columns.Count != myTable.Columns.Count)
                {
                    //calendarTreeListView.Clear(); //tree list view doesn't come back from a clear
                    calendarTreeListView.Reset();  //we have to do a reset if things change, like number of columns
                }


                calendarTreeListView.DataSource = myTable;
                foreach (OLVColumn column in calendarTreeListView.AllColumns)
                {
                    if (column.Name != null && column.Name != DateTreeColumn)
                    {
                        ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(column.Name);
                        if (activityDatumMetadata != null)
                        {
                            if (activityDatumMetadata.PositionInTree == null || activityDatumMetadata.Invisible.GetValueOrDefault(false))
                            {
                                column.IsVisible = false;
                            }
                            else
                            {
                                if (activityDatumMetadata.DisplayUnits != ActivityDatumMetadata.DisplayUnitsType.None && 
                                    activityDatumMetadata.DisplayUnits != ActivityDatumMetadata.DisplayUnitsType.String)
                                    column.TextAlign = HorizontalAlignment.Right;
                                column.IsVisible = true;
                                int positionInTree = (int)activityDatumMetadata.PositionInTree;
                                //myTable.Columns[column.Name].SetOrdinal(positionInTree);
                                if(activityDatumMetadata.ColumnSize != null)
                                    column.MaximumWidth = activityDatumMetadata.ColumnSize.Value;
                            }
                        }
                    }
                }
                calendarTreeListView.RebuildColumns();
                foreach (DataRow dataRow in lastRows)
                {
                    DataRowView drv = myTable.DefaultView[myTable.Rows.IndexOf(dataRow)];
                    calendarTreeListView.Expand(drv);
                }

                calendarTreeListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                //calendarTreeListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

                calendarTreeListView.ResumeLayout();
                Logging.Instance.TraceLeave();
                */
            }
            Logging.Instance.TraceLeave();

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
            dataRow[Id] = extensible.Id();
            dataRow[ParentId] = parentId;
            dataRow[DateTreeColumn] = extensible.Id();
            foreach (Datum d in extensible.DataValues)
            {
                if (masterDataNames.Contains(d.Name))
                {
                    ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(d.Name);
                    if (activityDatumMetadata != null && activityDatumMetadata.PositionInTree != null && !activityDatumMetadata.Invisible.GetValueOrDefault(false))
                        dataRow[d.Name] = DatumFormatter.FormatForGrid(d, activityDatumMetadata);

                    //dataRow[d.Name] = d.ToString();
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

                    Add(myTable, e, extensible.Id(), masterDataNames, lastChild);
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
