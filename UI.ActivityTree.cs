using BrightIdeasSoftware;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using ScottPlot.SnapLogic;
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
        private ActivityData? ActivityData;
        private Database? Database; //this is a placeholder, it will be set when we call ShowNow
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
        public void ShowNow(Database database, ActivityData ActivityData)
        {
            Display(database, ActivityData);
        }

        public void ResetColumnSize()
        {
            calendarTreeListView.ExpandAll();
            AutosizeColumns();
            //reset the column sizes
            foreach (OLVColumn c in calendarTreeListView.AllColumns)
            {
                if (c.Name != null)
                {
                    ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(c.Name);
                    if (activityDatumMetadata != null)
                        activityDatumMetadata.TreeColumnSize = c.Width; //save the current size for later
                }
            }
        }

        public void Display(Database database, ActivityData activityData)
        {
            ActivityData = activityData;
            Database = database;
            DisplayOnce(database);

            IReadOnlyCollection<CalendarNode> values = database.CurrentAthlete.CalendarTree.Values;
            calendarTreeListView.Roots = values;

            ExpandLast(database);
            //AutosizeColumns();

        }

        private void ExpandLast(Database database)
        {
            IReadOnlyCollection<CalendarNode> values = database.CurrentAthlete.CalendarTree.Values;
            if (values.Count == 0)
                return;

            //year
            CalendarNode calendarNode = values.Last();
            calendarTreeListView.Expand(calendarNode);


            //month
            IEnumerable<Extensible> extensibles = calendarNode.Children.Values;
            if (extensibles.Count() == 0)
                return;
            Extensible last = extensibles.Last();
            calendarTreeListView.Expand(last);
            //if we don't have a month, then we don't have any days, so we can skip this
            if (last == null)
                return;

            CalendarNode month = (CalendarNode)last;
            if (month == null)
            {
                Logging.Instance.Debug("No month for last calendar node, skipping expand");
                return;
            }
            //last 7 days
            DateTime now = DateTime.Now;
            DateTime date = now.Date;
            for (int i = 0; i < 7; i++)
            {
                //CalendarNode? day = database.CurrentAthlete.Days.GetValueOrDefault(date);
                CalendarNode? day = (CalendarNode?)month.Children.GetValueOrDefault(date);
                if (day != null)
                {
                    calendarTreeListView.Expand(day);
                    foreach (Activity activity in day.Activities)
                    {
                        calendarTreeListView.Expand(activity);
                    }
                }
                else
                {
                    //if we don't have a day, then we don't have any activities, so we can skip this
                    Logging.Instance.Debug($"No day for {date}, skipping expand");
                }
                date = date.AddDays(-1);
            }
        }


        private void AutosizeColumns()
        {
            foreach (ColumnHeader col in calendarTreeListView.Columns)
            {
                //auto resize column width
                if (col.Name != null && ActivityDatumMetadata.FindMetadata(col.Name) as ActivityDatumMetadata != null)
                {
                    ActivityDatumMetadata activityDatumMetadata = ActivityDatumMetadata.FindMetadata(col.Name)!;
                    if (activityDatumMetadata.TableColumnLimit != null)
                    {
                        col.Width = activityDatumMetadata.TableColumnLimit.Value;
                    }
                    else
                    {

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
            }
        }

        public void DisplayOnce(Database database)
        {
            if (HasBeenShown) { Logging.Instance.Debug("UI.ActivityTree.Display, !HasBeenShown, returning"); return; }
            HasBeenShown = true;

            Logging.Instance.TraceEntry("ActivityTree.Display");
            TreeListView calendarTreeListView_debug = calendarTreeListView; //make this a local to simplify debugging


            calendarTreeListView.CanExpandGetter = delegate (object x)
            {
                //debug("can expand? " + x.ToString());
                if (x is not CalendarNode)
                    return false;
                return ((CalendarNode)x).Children.Count() > 0;
            };

            calendarTreeListView.ChildrenGetter = delegate (object x)
            {
                //debug("get kids " + x.ToString());
                if (x is not CalendarNode)
                    return new ArrayList(); //shouldn't happen due to check above

                return ((CalendarNode)x).Children.Values;
            };

            OLVColumn dateColumn = new OLVColumn();
            dateColumn.Text = "Date";
            dateColumn.AspectGetter = delegate (object x) { return ((Extensible)x).Id().ToString(); };
            dateColumn.IsEditable = false;
            //this doesn't work:
            //dateColumn.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
            dateColumn.Width = 200; //wild guess
            calendarTreeListView.AllColumns.Add(dateColumn);


            lastRows.Clear();


            Logging.Instance.TraceEntry("ActivityTree.Display-datatable");
            SortedList<int, ActivityDatumMetadata> metadata = ActivityDatumMetadata.GetDefinitionsByTreePosition();
            //gather the list of column names from the root calendar nodes
            foreach (KeyValuePair<int, ActivityDatumMetadata> kvp in metadata)
            {

                ActivityDatumMetadata activityDatumMetadata = kvp.Value;
                if (activityDatumMetadata != null && activityDatumMetadata.PositionInTree != null && !activityDatumMetadata.Invisible.GetValueOrDefault(false))
                {
                    OLVColumn aNewColumn = new OLVColumn();
                    aNewColumn.Name = activityDatumMetadata.Name;
                    aNewColumn.Text = activityDatumMetadata.Title;
                    aNewColumn.AspectGetter = delegate (object x) { return DatumFormatter.FormatForGrid(((Extensible)x).GetNamedDatum(activityDatumMetadata.Name), activityDatumMetadata); };
                    // ((Extensible)x).GetNamedDatumForDisplay(s); };
                    aNewColumn.IsEditable = false;
                    //aNewColumn.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    //aNewColumn.MinimumWidth = 100;
                    // ... configure it and finally ...
                    calendarTreeListView.AllColumns.Add(aNewColumn);
                    if (activityDatumMetadata.TreeColumnSize != null)
                    {
                        aNewColumn.Width = activityDatumMetadata.TreeColumnSize.Value;
                    }
                    else
                    {
                        //if we don't have a size, then we will auto size it later
                        aNewColumn.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent);
                    }

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

        private void calendarTreeListView_FormatRow(object sender, FormatRowEventArgs e)
        {
            Extensible? eModel = e.Model as Extensible;
            if (eModel == null) return;
            if (eModel is CalendarNode)
            {
                CalendarNode cn = (CalendarNode)e.Model;

                if (cn.Id().Type == Utils.DateTimeTree.DateTreeType.Root)
                {
                    e.Item.BackColor = Color.LightGray; //root is light gray
                }
                else if (cn.Id().Type == Utils.DateTimeTree.DateTreeType.Year)
                {
                    e.Item.BackColor = Color.LightBlue; //year is light blue
                }
                else if (cn.Id().Type == Utils.DateTimeTree.DateTreeType.Month)
                {
                    e.Item.BackColor = Color.LightGreen; //month is light green
                }
                else if (cn.Id().Type == Utils.DateTimeTree.DateTreeType.Day)
                {
                    e.Item.BackColor = Color.LightYellow; //day is light yellow
                }
            }
            else
            {
                e.Item.BackColor = Color.White; //activity is white
            }
        }

        private void calendarTreeListView_SelectionChanged(object sender, EventArgs e)
        {
            OLVListItem item = calendarTreeListView.SelectedItem;
            if (item == null || item.RowObject == null)
            {
                return; //nothing selected
            }
            Extensible? extensible = item.RowObject as Extensible;
            if (extensible == null)
            {
                Logging.Instance.Debug("UI.ActivityTree.calendarTreeListView_SelectionChanged, extensible is null");
                return; //not an extensible
            }
            if (extensible is Activity activity)
            {
                if(ActivityData != null && Database != null)
                {
                    ActivityData.DisplayActivity(Database.CurrentAthlete, activity);
                }
                else
                {
                    Logging.Instance.Debug("UI.ActivityTree.calendarTreeListView_SelectionChanged, ActivityData is null");
                }

            }
        }
    }
}
