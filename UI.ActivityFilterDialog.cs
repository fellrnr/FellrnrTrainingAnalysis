﻿using FellrnrTrainingAnalysis.Model;

namespace FellrnrTrainingAnalysis
{
    public partial class ActivityFilterDialog : Form
    {
        public ActivityFilterDialog()
        {
            InitializeComponent();
        }

        private const string BadDataName = "BadData";
        private Dictionary<string, FilterRow> Filters = new Dictionary<string, FilterRow>();

        public void Display(Database database)
        {
            if (database == null || database.CurrentAthlete == null) { return; }

            IReadOnlyCollection<Tuple<String, Type>> metadata = database.CurrentAthlete.ActivityFieldMetaData;


            //put bad data at the top of the filters for now
            if (!Filters.ContainsKey(BadDataName))
            {
                FilterRow filterRow = FilterRowFactory.Create(tableLayoutPanel1, OnKeyPress);
                Filters.Add(BadDataName, filterRow);
            }

            tableLayoutPanel1.SuspendLayout();
            foreach (Tuple<String, Type> field in metadata)
            {
                if(!Filters.ContainsKey(field.Item1))
                {
                    FilterRow filterRow = FilterRowFactory.Create(tableLayoutPanel1, field, OnKeyPress);
                    Filters.Add(field.Item1, filterRow);
                }
            }


            //TODO: can you have a datum and time series with the same name? 
            IReadOnlyCollection<String> timeSeriesNames = database.CurrentAthlete.TimeSeriesNames;
            foreach(string name in timeSeriesNames)
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

            public static FilterRow Create(TableLayoutPanel tableLayoutPanel, Tuple<String, Type> tuple, KeyPressEventHandler onEnter)
            {
                string name = tuple.Item1;
                Type type = tuple.Item2;

                FilterRow filterRow;
                if (type == typeof(TypedDatum<float>))
                {
                    FloatFilter newFilter = new FloatFilter(tableLayoutPanel, name, CurrentRow, onEnter);

                    filterRow = newFilter;
                } 
                else if (type == typeof(TypedDatum<DateTime>))
                {
                    DateFilter newFilter = new DateFilter(tableLayoutPanel, name, CurrentRow, onEnter);

                    filterRow = newFilter;
                }
                else //string
                {
                    StringFilter newFilter = new StringFilter(tableLayoutPanel, name, CurrentRow, onEnter);

                    filterRow = newFilter;
                }

                CurrentRow++;

                return filterRow;
            }

            public static FilterRow Create(TableLayoutPanel tableLayoutPanel, string name, KeyPressEventHandler onEnter)
            {

                FilterRow filterRow;
                DataStreamFilter newFilter = new DataStreamFilter(tableLayoutPanel, name, CurrentRow, onEnter);

                filterRow = newFilter;

                CurrentRow++;

                return filterRow;
            }

            public static FilterRow Create(TableLayoutPanel tableLayoutPanel, KeyPressEventHandler onEnter)
            {

                FilterRow filterRow;
                BadDataFilter newFilter = new BadDataFilter(tableLayoutPanel, BadDataName, CurrentRow, onEnter);

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
            public string FilterValue() { return Filter.Text; }


            public abstract FilterBase? AsFilterBase();

            public abstract string ValueOne();
            public abstract string ValueTwo();


            public FilterRow(TableLayoutPanel tableLayoutPanel, string name, int row, string[] filterCommands, KeyPressEventHandler onEnter)
            {
                TableLayoutPanel = tableLayoutPanel;
                Row = row;
                FieldName = new Label { Text = name, Anchor = AnchorStyles.Left, AutoSize = true };
                Filter = new ComboBox { Dock = DockStyle.Fill };
                Filter.Items.AddRange(filterCommands);
                Filter.SelectedIndexChanged += Filter_SelectedIndexChanged;
                Filter.KeyPress += onEnter;
                tableLayoutPanel.Controls.Add(FieldName, 0, row);
                tableLayoutPanel.Controls.Add(Filter, 1, row);
            }

            protected abstract void Filter_SelectedIndexChanged(object? sender, EventArgs e);
        }
        private class StringFilter : FilterRow
        {
            TextBox? Value1;

            public override string ValueOne() { return Value1 != null ? Value1.Text : ""; }
            public override string ValueTwo() { return ""; }

            public StringFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnter) : base(tableLayoutPanel, name, row, FilterString.filterCommands, onEnter)
            {
            }

            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
                if(Value1 == null)
                {
                    Value1 = new TextBox { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    TableLayoutPanel.Controls.Add(Value1, 2, Row);
                }
                if (Filter.Text != "" && Filter.Text != "has")
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
                if(Filter.Text == "")
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

            public DateFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnter) : 
                base(tableLayoutPanel, name, row, FilterDateTime.FilterCommands, onEnter)
            {
            }
            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
                if (Value1 == null)
                {
                    Value1 = new DateTimePicker { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    TableLayoutPanel.Controls.Add(Value1, 2, Row);
                    Value1_text = new TextBox { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true, Visible = false };
                    TableLayoutPanel.Controls.Add(Value1_text, 2, Row);

                    Value2 = new DateTimePicker { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    TableLayoutPanel.Controls.Add(Value2, 3, Row);
                }

                //        public static readonly string[] FilterCommands = new string[] { "", "<", "<=", "=", ">=", ">", "between", "1M", "6M", "1Y" };

                if (Filter.Text != "" && Filter.Text != "1M" && Filter.Text != "6M" && Filter.Text != "1Y" && Filter.Text != "in")
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

        private class FloatFilter  : FilterRow
        {
            NumericUpDown? Value1;
            NumericUpDown? Value2;

            public override string ValueOne() { return Value1 != null ? Value1.Text : ""; }
            public override string ValueTwo() { return Value2 != null ? Value2.Text : ""; }
            public FloatFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnter) : 
                base(tableLayoutPanel, name, row, FilterFloat.FilterCommands, onEnter)
            {
            }
            protected override void Filter_SelectedIndexChanged(object? sender, EventArgs e)
            {
                if (Value1 == null)
                {
                    Value1 = new NumericUpDown { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    TableLayoutPanel.Controls.Add(Value1, 2, Row);
                    Value1.Minimum= decimal.MinValue; Value1.Maximum= decimal.MaxValue;
                    Value1.Width = 200;
                    Value2 = new NumericUpDown { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    Value2.Minimum = decimal.MinValue; Value1.Maximum = decimal.MaxValue;
                    Value2.Width = 200;
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

        private class DataStreamFilter : FilterRow
        {
            NumericUpDown? Value1;
            NumericUpDown? Value2;

            public override string ValueOne() { return Value1 != null ? Value1.Text : ""; }
            public override string ValueTwo() { return Value2 != null ? Value2.Text : ""; }
            public DataStreamFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnter) : 
                base(tableLayoutPanel, name, row, FilterDataStream.FilterCommands, onEnter)
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
                    Value2 = new NumericUpDown { Text = "", Anchor = AnchorStyles.Left | AnchorStyles.Top, AutoSize = true };
                    Value2.Minimum = decimal.MinValue; Value1.Maximum = decimal.MaxValue;
                    Value2.Width = 200;
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
                FilterDataStream filterDataStream = new FilterDataStream(FieldName.Text, Filter.Text, float1, float2);

                return filterDataStream;
            }
        }


        private class BadDataFilter : FilterRow
        {
            public override string ValueOne() { return ""; }
            public override string ValueTwo() { return ""; }

            public BadDataFilter(TableLayoutPanel tableLayoutPanel, string name, int row, KeyPressEventHandler onEnter) : 
                base(tableLayoutPanel, name, row, FilterBadData.filterCommands, onEnter)
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
        private FilterActivities GenerateFilterActivities()
        {
            FilterActivities aFilterActivities = new FilterActivities();

            foreach (KeyValuePair<string, FilterRow> kvp in Filters)
            { 
                FilterRow filterRow = kvp.Value;
                FilterBase? filterBase = filterRow.AsFilterBase();
                if(filterBase != null)
                {
                    aFilterActivities.Filters.Add(filterBase);
                }
            }

            return aFilterActivities;
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
    }
}