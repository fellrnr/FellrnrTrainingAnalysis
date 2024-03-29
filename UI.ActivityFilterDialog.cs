using FellrnrTrainingAnalysis.Model;
using System.Reflection;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ActivityFilterDialog : Form
    {
        public ActivityFilterDialog()
        {
            InitializeComponent();
            typeof(DataGridView).InvokeMember(
"DoubleBuffered",
BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
null,
tableLayoutPanel1,
new object[] { true });

        }

        private const string BadDataName = "BadData";
        private const string RelativeName = "Relative";
        private Dictionary<string, FilterRow> Filters = new Dictionary<string, FilterRow>();

        public void Display(Database database)
        {
            if (database == null || database.CurrentAthlete == null) { return; }

            IReadOnlyCollection<Tuple<String, Type>> metadata = database.CurrentAthlete.ActivityFieldMetaData;


            if (!Filters.ContainsKey(RelativeName))
            {
                IReadOnlyCollection<string> possibleFields = database.CurrentAthlete.ActivityFieldNames;
                FilterRow filterRow = FilterRowFactory.CreateRelative(tableLayoutPanel1, OnKeyPress, possibleFields);
                Filters.Add(RelativeName, filterRow);
            }

            //put bad data at the top of the filters for now
            if (!Filters.ContainsKey(BadDataName))
            {
                FilterRow filterRow = FilterRowFactory.CreateBadData(tableLayoutPanel1, OnKeyPress);
                Filters.Add(BadDataName, filterRow);
            }

            tableLayoutPanel1.SuspendLayout();
            foreach (Tuple<String, Type> field in metadata)
            {
                if (!Filters.ContainsKey(field.Item1))
                {
                    FilterRow filterRow = FilterRowFactory.Create(tableLayoutPanel1, field, OnKeyPress);
                    Filters.Add(field.Item1, filterRow);
                }
            }


            //TODO: can you have a datum and time series with the same name? 
            IReadOnlyCollection<String> timeSeriesNames = database.CurrentAthlete.AllTimeSeriesNames;
            foreach (string name in timeSeriesNames)
            {
                if (!Filters.ContainsKey(name))
                {
                    FilterRow filterRow = FilterRowFactory.Create(tableLayoutPanel1, name, OnKeyPress);
                    Filters.Add(name, filterRow);
                }
            }


            tableLayoutPanel1.ResumeLayout();

        }

        public delegate void FiltersUpdatedCallbackEventHandler(FilterActivities filterActivities);

        public event FiltersUpdatedCallbackEventHandler? UpdatedHandler;


        private class FilterRowFactory
        {
            private static int CurrentRow = 1;//zero is header row

            public static FilterRow Create(TableLayoutPanel tableLayoutPanel, Tuple<String, Type> tuple, KeyPressEventHandler onEnterHandler)
            {
                string name = tuple.Item1;
                Type type = tuple.Item2;

                FilterRow filterRow;
                if (type == typeof(TypedDatum<float>))
                {
                    FloatFilter newFilter = new FloatFilter(tableLayoutPanel, name, CurrentRow, onEnterHandler);

                    filterRow = newFilter;
                }
                else if (type == typeof(TypedDatum<DateTime>))
                {
                    DateFilter newFilter = new DateFilter(tableLayoutPanel, name, CurrentRow, onEnterHandler);

                    filterRow = newFilter;
                }
                else //string
                {
                    StringFilter newFilter = new StringFilter(tableLayoutPanel, name, CurrentRow, onEnterHandler);

                    filterRow = newFilter;
                }

                CurrentRow++;

                return filterRow;
            }

            public static FilterRow Create(TableLayoutPanel tableLayoutPanel, string name, KeyPressEventHandler onEnterHandler)
            {

                FilterRow filterRow;
                TimeSeriesFilter newFilter = new TimeSeriesFilter(tableLayoutPanel, name, CurrentRow, onEnterHandler);

                filterRow = newFilter;

                CurrentRow++;

                return filterRow;
            }

            public static FilterRow CreateBadData(TableLayoutPanel tableLayoutPanel, KeyPressEventHandler onEnterHandler)
            {

                FilterRow filterRow;
                BadDataFilter newFilter = new BadDataFilter(tableLayoutPanel, BadDataName, CurrentRow, onEnterHandler);

                filterRow = newFilter;

                CurrentRow++;

                return filterRow;
            }

            public static FilterRow CreateRelative(TableLayoutPanel tableLayoutPanel, KeyPressEventHandler onEnterHandler, IReadOnlyCollection<string> possibleFields)
            {

                FilterRow filterRow;
                RelativeFilter newFilter = new RelativeFilter(tableLayoutPanel, RelativeName, CurrentRow, onEnterHandler, possibleFields);

                filterRow = newFilter;

                CurrentRow++;

                return filterRow;
            }

        }

        private abstract class FilterRow
        {
            protected Label FieldName;
            protected ComboBox Filter;
            protected TableLayoutPanel TableLayoutPanel;
            protected int Row;
            protected KeyPressEventHandler OnEnterHandler;
            public string FilterValue() { return Filter.Text; }

            public void Clear()
            {
                if (!string.IsNullOrEmpty(Filter.Text))
                {
                    Filter.Text = string.Empty;
                }
            }
            public abstract FilterBase? AsFilterBase();

            public abstract string ValueOne();
            public abstract string ValueTwo();


            public FilterRow(TableLayoutPanel tableLayoutPanel, string name, int row, string[] filterCommands, KeyPressEventHandler onEnterHandler)
            {
                TableLayoutPanel = tableLayoutPanel;
                Row = row;
                FieldName = new Label { Text = name, Anchor = AnchorStyles.Left, AutoSize = true };
                Filter = new ComboBox { Dock = DockStyle.Fill };
                Filter.Items.AddRange(filterCommands);
                Filter.SelectedIndexChanged += Filter_SelectedIndexChanged;
                Filter.KeyPress += onEnterHandler;
                tableLayoutPanel.Controls.Add(FieldName, 0, row);
                tableLayoutPanel.Controls.Add(Filter, 1, row);
                OnEnterHandler = onEnterHandler;
            }

            protected abstract void Filter_SelectedIndexChanged(object? sender, EventArgs e);
        }
        private class StringFilter : FilterRow
        {
            TextBox? Value1;

            public override string ValueOne() { return Value1 != null ? Value1.Text : ""; }
            public override string ValueTwo() { return ""; }

            public StringFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnterHandler) : base(tableLayoutPanel, name, row, FilterString.filterCommands, onEnterHandler)
            {
            }

            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
                if (Value1 == null)
                {
                    Value1 = new TextBox { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    Value1.KeyPress += OnEnterHandler;
                    TableLayoutPanel.Controls.Add(Value1, 2, Row);
                }
                if (Filter.Text != "" && Filter.Text != "has" && Filter.Text != "missing")
                {
                    Value1!.Visible = true;
                }
                else
                {
                    Value1!.Visible = false;
                }

            }

            public override FilterBase? AsFilterBase()
            {
                if (Filter.Text == "")
                    return null;
                FilterString filterString = new FilterString(FieldName.Text, Filter.Text, Value1!.Text);
                return filterString;
            }

        }
        private class DateFilter : FilterRow
        {
            DateTimePicker? Value1;
            TextBox? Value1_text;
            DateTimePicker? Value2;
            public override string ValueOne() { return Value1 != null ? Value1.Text : ""; }
            public override string ValueTwo() { return Value2 != null ? Value2.Text : ""; }

            public DateFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnterHandler) :
                base(tableLayoutPanel, name, row, FilterDateTime.FilterCommands, onEnterHandler)
            {
            }
            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
                if (Value1 == null)
                {
                    Value1 = new DateTimePicker { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    Value1.KeyPress += OnEnterHandler;
                    TableLayoutPanel.Controls.Add(Value1, 2, Row);
                    Value1_text = new TextBox { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true, Visible = false };
                    Value1_text.KeyPress += OnEnterHandler;
                    TableLayoutPanel.Controls.Add(Value1_text, 2, Row);

                    Value2 = new DateTimePicker { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    Value2.KeyPress += OnEnterHandler;
                    TableLayoutPanel.Controls.Add(Value2, 3, Row);
                }

                //        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">", "between", "1M", "6M", "1Y" };

                if (Filter.Text != "" && Filter.Text != "1M" && Filter.Text != "6M" && Filter.Text != "1Y" && Filter.Text != "in" && Filter.Text != "has" && Filter.Text != "missing")
                {
                    Value1!.Visible = true;
                }
                else
                {
                    Value1!.Visible = false;
                }
                if (Filter.Text == "between")
                {
                    Value2!.Visible = true;
                }
                else
                {
                    Value2!.Visible = false;
                }
                if (Filter.Text == "in")
                {
                    Value1_text!.Visible = true;
                }
                else
                {
                    Value1_text!.Visible = false;
                }
            }
            public override FilterBase? AsFilterBase()
            {
                if (Filter.Text == "")
                    return null;

                DateTime? dateTime = (Value2 == null || !Value2.Visible ? null : Value2.Value);
                FilterDateTime filterDateTime = new FilterDateTime(FieldName.Text, Filter.Text, Value1!.Value, dateTime, Value1_text!.Text);

                return filterDateTime;
            }
        }

        private class FloatFilter : FilterRow
        {
            NumericUpDown? Value1;
            NumericUpDown? Value2;

            public override string ValueOne() { return Value1 != null ? Value1.Text : ""; }
            public override string ValueTwo() { return Value2 != null ? Value2.Text : ""; }
            public FloatFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnterHandler) :
                base(tableLayoutPanel, name, row, FilterFloat.FilterCommands, onEnterHandler)
            {
            }
            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
                if (Value1 == null)
                {
                    Value1 = new NumericUpDown { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    TableLayoutPanel.Controls.Add(Value1, 2, Row);
                    Value1.Minimum = decimal.MinValue; Value1.Maximum = decimal.MaxValue;
                    Value1.Width = 200;
                    Value1.KeyPress += OnEnterHandler;
                    Value2 = new NumericUpDown { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    Value2.Minimum = decimal.MinValue; Value2.Maximum = decimal.MaxValue;
                    Value2.Width = 200;
                    Value2.KeyPress += OnEnterHandler;
                    TableLayoutPanel.Controls.Add(Value2, 3, Row);
                }

                //        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">", "between", "has" };
                if (Filter.Text != "" && Filter.Text != "has" && Filter.Text != "missing")
                {
                    Value1!.Visible = true;
                }
                else
                {
                    Value1!.Visible = false;
                }
                if (Filter.Text == "between")
                {
                    Value2!.Visible = true;
                }
                else
                {
                    Value2!.Visible = false;
                }

            }
            public override FilterBase? AsFilterBase()
            {
                if (Filter.Text == "")
                    return null;

                float? float1 = (Value1 == null || !Value1.Visible ? null : (float)Value1.Value);
                float? float2 = (Value2 == null || !Value2.Visible ? null : (float)Value2.Value);
                FilterFloat filterFloat = new FilterFloat(FieldName.Text, Filter.Text, float1, float2);

                return filterFloat;
            }
        }

        private class TimeSeriesFilter : FilterRow
        {
            NumericUpDown? Value1;
            NumericUpDown? Value2;
            public override string ValueOne() { return Value1 != null ? Value1.Text : ""; }
            public override string ValueTwo() { return Value2 != null ? Value2.Text : ""; }
            public TimeSeriesFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnterHandler) :
                base(tableLayoutPanel, name, row, FilterTimeSeries.FilterCommands, onEnterHandler)
            {
            }
            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
                if (Value1 == null)
                {
                    Value1 = new NumericUpDown { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    TableLayoutPanel.Controls.Add(Value1, 2, Row);
                    Value1.Minimum = decimal.MinValue; Value1.Maximum = decimal.MaxValue;
                    Value1.Width = 200;
                    Value1.KeyPress += OnEnterHandler;
                    Value2 = new NumericUpDown { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    Value2.Minimum = decimal.MinValue; Value1.Maximum = decimal.MaxValue;
                    Value2.Width = 200;
                    Value2.KeyPress += OnEnterHandler;
                    TableLayoutPanel.Controls.Add(Value2, 3, Row);
                }

                //        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">", "between", "has" };
                if (Filter.Text != "" && Filter.Text != "has" && Filter.Text != "missing")
                {
                    Value1!.Visible = true;
                }
                else
                {
                    Value1!.Visible = false;
                }

                if (Filter.Text == "between")
                {
                    Value2!.Visible = true;
                }
                else
                {
                    Value2!.Visible = false;
                }

            }
            public override FilterBase? AsFilterBase()
            {
                if (Filter.Text == "")
                    return null;

                float? float1 = (Value1 == null || !Value1.Visible ? null : (float)Value1.Value);
                float? float2 = (Value2 == null || !Value2.Visible ? null : (float)Value2.Value);
                FilterTimeSeries filterTimeSeries = new FilterTimeSeries(FieldName.Text, Filter.Text, float1, float2);

                return filterTimeSeries;
            }
        }

        private class BadDataFilter : FilterRow
        {
            public override string ValueOne() { return ""; }
            public override string ValueTwo() { return ""; }

            public BadDataFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnterHandler) :
                base(tableLayoutPanel, name, row, FilterBadData.filterCommands, onEnterHandler)
            {
            }
            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
            }
            public override FilterBase? AsFilterBase()
            {
                if (Filter.Text == "")
                    return null;

                return new FilterBadData(Filter.Text);
            }
        }


        private class RelativeFilter : FilterRow
        {
            ComboBox? Value1;
            ComboBox? Value2;

            IReadOnlyCollection<string> PossibleFields;

            public override string ValueOne() { return Value1 != null ? Value1.Text : ""; }
            public override string ValueTwo() { return Value2 != null ? Value2.Text : ""; }

            public RelativeFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnterHandler, IReadOnlyCollection<string> possibleFields) : base(tableLayoutPanel, name, row, FilterRelative.FilterCommands, onEnterHandler)
            {
                PossibleFields = possibleFields;
            }

            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
                if (Value1 == null)
                {
                    Value1 = new ComboBox { Dock = DockStyle.Fill };
                    Value1.Items.AddRange(PossibleFields.ToArray()); ;
                    Value1.KeyPress += OnEnterHandler;

                    TableLayoutPanel.Controls.Add(Value1, 2, Row);

                    Value2 = new ComboBox { Dock = DockStyle.Fill };
                    Value2.Items.AddRange(PossibleFields.ToArray()); ;
                    Value2.KeyPress += OnEnterHandler;

                    TableLayoutPanel.Controls.Add(Value2, 3, Row);

                }
                if (Filter.Text != "")
                {
                    Value1!.Visible = true;
                    Value2!.Visible = true;
                }
                else
                {
                    Value1!.Visible = false;
                    Value2!.Visible = false;
                }

            }

            public override FilterBase? AsFilterBase()
            {
                if (Filter.Text == "")
                    return null;
                FilterRelative filterRelative = new FilterRelative(Value1!.Text, Filter.Text, Value2!.Text);
                return filterRelative;
            }

        }


        private FilterActivities GenerateFilterActivities()
        {
            FilterActivities aFilterActivities = new FilterActivities();

            foreach (KeyValuePair<string, FilterRow> kvp in Filters)
            {
                FilterRow filterRow = kvp.Value;
                FilterBase? filterBase = filterRow.AsFilterBase();
                if (filterBase != null)
                {
                    aFilterActivities.Filters.Add(filterBase);
                }
            }

            return aFilterActivities;
        }

        private void Clear()
        {
            foreach (KeyValuePair<string, FilterRow> kvp in Filters)
            {
                FilterRow filterRow = kvp.Value;
                filterRow.Clear();
            }
        }


        private void applyAndCloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterActivities filterActivities = GenerateFilterActivities();
            UpdatedHandler?.Invoke(filterActivities);
            this.Hide();
        }

        private void applyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FilterActivities filterActivities = GenerateFilterActivities();
            UpdatedHandler?.Invoke(filterActivities);
        }

        protected void OnKeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                FilterActivities filterActivities = GenerateFilterActivities();
                UpdatedHandler?.Invoke(filterActivities);
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void ActivityFilterDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true; // this cancels the close event.
        }

        private void clearAndCloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
            FilterActivities filterActivities = GenerateFilterActivities();
            UpdatedHandler?.Invoke(filterActivities);
            this.Hide();
        }
    }
}
