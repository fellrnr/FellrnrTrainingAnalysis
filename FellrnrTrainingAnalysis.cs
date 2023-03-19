using System.Text;
using FellrnrTrainingAnalysis.Model;
using ScottPlot;
using ScottPlot.Renderable;
using static FellrnrTrainingAnalysis.Utils.Utils;
using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis
{
    public partial class FellrnrTrainingAnalysisForm : Form
    {
        private Model.Database Database { get; set; }

        


        public FellrnrTrainingAnalysisForm(bool StravaSync, bool email, bool batch)
        {
            //TODO: consider using http://dockpanelsuite.com/
            //Utils.Config.Instance.OnlyLoadAfter = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0));
            //Utils.Config.Instance.OnlyLoadAfter = new DateTime(2022, 12,28);
            //Utils.Config.Instance.LogLevel = Utils.Config.Level.Debug;
            InitializeComponent();
            Utils.Options.LoadConfig();
            Logging.Instance.Log(string.Format("OnlyLoad After {0}", Utils.Options.Instance.OnlyLoadAfter));
            showErrorsToolStripMenuItem.Enabled = false;
            Database = Model.Database.LoadFromFile();
            GoalsUI = new UI.Goals(Database, goalsDataGridView, goalsTextBox);
            SetMenuAvailability();
            UpdateViews(false);

            if(StravaSync)
            {
                StravaApi.Instance.SyncNewActivites(Database);
            }
            if(email)
            {
                GoalsUI.SendGoals();
            }
            if(batch)
            {
                ExitApp(true);
            }
            //Model.Goal.test();
        }

        UI.Goals GoalsUI { get; set; }

        private void UpdateViews(bool force)
        {
            Database.Recalculate(force);
            UpdateSummary();
            UpdateGoals();
            UpdateReport();
            activityList1.Display(Database);

            if (Logging.Instance.HasErrors)
            {
                showErrorsToolStripMenuItem.Enabled = true;
                showErrorsToolStripMenuItem.ForeColor = Color.Red;
            }
        }

        private void UpdateGoals()
        {
            GoalsUI.UpdateGoalsText();
            GoalsUI.UpdateGoalsGrid();
        }
        private void UpdateSummary()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("A total of {0} athletes loaded\r\n", Database.Athletes.Count));
            sb.Append(Database.CurrentAthlete);
            summaryTextBox.Text = sb.ToString();
        }

        private void UpdateReport()
        {
            activityDataGridView.Rows.Clear();
            //Database.CurrentAthlete.
            IReadOnlyCollection<string> activityFieldNames = Database.CurrentAthlete.ActivityFieldNames;
            activityDataGridView.ColumnCount = ActivityDatumMetadata.LastPositionInReport();
            if (activityDataGridView.ColumnCount == 0)
                return;
            activityDataGridView.ColumnHeadersVisible = true;

            // Set the column header style.
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();

            columnHeaderStyle.BackColor = Color.Beige;
            columnHeaderStyle.Font = new Font("Verdana", 8, FontStyle.Regular);
            activityDataGridView.ColumnHeadersDefaultCellStyle = columnHeaderStyle;

            // Set the column header names.
            foreach (string s in activityFieldNames)
            {
                ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(s);
                if (activityDatumMetadata != null && activityDatumMetadata.PositionInReport != null)
                {
                    int positionInReport = (int)activityDatumMetadata.PositionInReport;
                    DataGridViewColumn dataGridViewColumn = activityDataGridView.Columns[positionInReport];
                    dataGridViewColumn.Name = activityDatumMetadata.Title;
                    
                    if(UI.DatumFormatter.RightJustify(activityDatumMetadata))
                        dataGridViewColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

                    if(activityDatumMetadata.Invisible.HasValue && activityDatumMetadata.Invisible.Value) //most readable way of checking nullable bool? 
                        dataGridViewColumn.Visible= false;

                    if (activityDatumMetadata.ColumnSize != null)
                    {
                        dataGridViewColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                        dataGridViewColumn.Width = (int)activityDatumMetadata.ColumnSize;
                    }
                    else
                    {
                        dataGridViewColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                }
            }

            foreach (KeyValuePair<string, Model.Activity> kvp in Database.CurrentAthlete.Activities)
            {
                Activity activity = kvp.Value;
                string[] row = new string[activityFieldNames.Count];
                foreach (string fieldname in activityFieldNames)
                {
                    ActivityDatumMetadata? activityDatumMetadata = ActivityDatumMetadata.FindMetadata(fieldname);
                    if (activityDatumMetadata != null && activityDatumMetadata.PositionInReport != null)
                    {
                        int positionInReport = (int)activityDatumMetadata.PositionInReport;
                        if (activity.HasNamedDatum(fieldname))
                        {
                            //row[activityDatumMetadata.PositionInReport] = activity.GetNamedDatumForDisplay(fieldname);
                            row[positionInReport] = UI.DatumFormatter.FormatForGrid(activity.GetNamedDatum(fieldname), activityDatumMetadata);
                        }
                        else
                        {
                            row[positionInReport] = "";
                        }
                    }
                }
                activityDataGridView.Rows.Add(row);
            }
            if (activityDataGridView.Rows.Count > 0)
            {
                activityDataGridView.FirstDisplayedScrollingRowIndex = activityDataGridView.RowCount - 1; //this changes the selected row to be zero
                activityDataGridView.Rows[activityDataGridView.Rows.Count - 1].Selected = true;
            }
        }

        private List<Axis> CurrentAxis { get; set; } = new List<Axis>();
        private int axisIndex = 0;
        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            UpdateDataStreamGraph();
        }

        private void UpdateDataStreamGraph()
        {
            DataGridViewSelectedRowCollection dataGridViewSelectedRowCollection = activityDataGridView.SelectedRows;
            formsPlot1.Plot.Clear();

            foreach (Axis axis in CurrentAxis) { formsPlot1.Plot.RemoveAxis(axis); }
            CurrentAxis.Clear();
            axisIndex = 0;

            if (dataGridViewSelectedRowCollection.Count > 0)
            {
                DataGridViewRow row = dataGridViewSelectedRowCollection[0];
                if (row != null)
                {
                    var index = activityDataGridView.Columns[Model.Activity.PrimarykeyTag]?.Index;
                    if (index != null && index != -1)
                    {
                        string primarykey = (string)row.Cells[index.Value].Value;
                        if (Database.CurrentAthlete.Activities.ContainsKey(primarykey)) //should never happen unless we've turned on debugging to add extra data to the primary key column of the table. 
                        {
                            Model.Activity activity = Database.CurrentAthlete.Activities[primarykey];
                            if (activity != null)
                            {
                                foreach (KeyValuePair<string, IDataStream> kvp in activity.TimeSeries)
                                {
                                    DisplayTimeSeries(activity, kvp);
                                }
                            }
                        }
                    }
                }
            }
            formsPlot1.Refresh();
        }

        const double MINPACE = 0.3; //0.3 is 55:30 min/km. Anything slower can be considered not moving to make the graph work, otherwise min/km values tend towards infinity
        private void DisplayTimeSeries(Model.Activity activity, KeyValuePair<string, IDataStream> kvp)
        {
            string timeSeriesName = kvp.Key;

            DataStreamDefinition? dataStreamDefinition = DataStreamDefinition.FindDataStreamDefinition(timeSeriesName);
            if (dataStreamDefinition == null || !dataStreamDefinition.Show)
                return;
            Model.IDataStream activityDataStreamdataStream = kvp.Value;
            Tuple<uint[], float[]>? dataStream = activityDataStreamdataStream.GetData(activity);
            if (dataStream == null)
            {
                return;
            }
            double[] xArray = Array.ConvertAll(dataStream.Item1, x => (double)x);
            double[] yArrayRaw = Array.ConvertAll(dataStream.Item2, x => (double)x);

            double[] yArraySmoothed = Smooth(yArrayRaw, dataStreamDefinition);

            for (int i = 0; i < xArray.Length; i++)
            {
                double x = xArray[i];
                bool norm = double.IsNormal(x);
                if (x != 0 && !double.IsNormal(x))
                {
                    Logging.Instance.Log(string.Format("invalid X value {0} at offset {1} of {2}", xArray[i], i, timeSeriesName));
                    return;
                }
            }
            for (int i = 0; i < yArraySmoothed.Length; i++)
            {
                double y = yArraySmoothed[i];
                if (y != 0 && !double.IsNormal(y))
                {
                    Logging.Instance.Log(string.Format("invalid Y value {0} at offset {1} of {2}", yArraySmoothed[i], i, timeSeriesName));
                    return;
                }
            }

            if (dataStreamDefinition.DisplayUnits == DataStreamDefinition.DisplayUnitsType.Pace && !Options.Instance.DebugDisableTimeAxis)
            {
                //negate the pace then negate the tick marks so faster is higher than slower
                //ignore paces below min to avoid tending towards infinity
                //divide 16.6 by pace to get min/km from meters/second
                //mulitply but 60*24 to go from minutes to fraction of a day, so it will display right
                yArraySmoothed = Array.ConvertAll(yArraySmoothed, x => ( x < MINPACE ? 0 : (16.666666667f/x) /(60.0*24.0)));
            }
            var scatterGraph = formsPlot1.Plot.AddScatter(xArray, yArraySmoothed);
            scatterGraph.MarkerShape = MarkerShape.none;
            scatterGraph.LineWidth = 2;

            //formsPlot1.Plot.YAxis.TickLabelFormat
            Axis yAxis;
            if (axisIndex == 0)
            {
                yAxis = formsPlot1.Plot.YAxis;
                scatterGraph.YAxisIndex = 0;

            }
            else
            {
                yAxis = formsPlot1.Plot.AddAxis(ScottPlot.Renderable.Edge.Left);
                yAxis.AxisIndex = axisIndex;
                scatterGraph.YAxisIndex = yAxis.AxisIndex;
                CurrentAxis.Add(yAxis);
            }
            if (dataStreamDefinition.DisplayUnits == DataStreamDefinition.DisplayUnitsType.Pace && !Options.Instance.DebugDisableTimeAxis)
            {
                //yAxis.DateTimeFormat(true);
                yAxis.TickLabelFormat("m:ss", dateTimeFormat: true);
                //yAxis.TickLabelNotation(invertSign: true);
            }
            yAxis.Label(dataStreamDefinition.DisplayTitle);
            yAxis.Color(scatterGraph.Color);
            axisIndex++;
            return;
        }


        private double[] Smooth(double[] input, DataStreamDefinition dataStreamDefinition)
        {
            double[] smoothedElevationChanges;
            if (dataStreamDefinition.Smoothing == DataStreamDefinition.SmoothingType.AverageWindow)
                smoothedElevationChanges = Smoothing.WindowSmoothed(input, dataStreamDefinition.SmoothingWindow);
            else if (dataStreamDefinition.Smoothing == DataStreamDefinition.SmoothingType.SimpleExponential)
                smoothedElevationChanges = Smoothing.SimpleExponentialSmoothed(input, dataStreamDefinition.SmoothingWindow);
            else
                smoothedElevationChanges = input;

            return smoothedElevationChanges;
        }

        private void SetMenuAvailability()
        {
            /*
            if (StravaApi.Instance.IsConnected)
            {
                connectToStravaToolStripMenuItem.Enabled = false;
            }
            else
            {
                syncWithStravaToolStripMenuItem.Enabled = false;
            }

            */
        }

        private void loadFromStravaCsvToolStripMenuItem_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "profile.csv|profile*.csv|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                //openFileDialog.RestoreDirectory = true;
                openFileDialog.ReadOnlyChecked = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    LoadFromStravaCsv(filePath);
                }
            }
        }

        private void LoadFromStravaCsv(string filePath)
        {
            System.Diagnostics.Stopwatch load = new System.Diagnostics.Stopwatch();
            load.Start();
            StravaCsvImporter stravaCsvImporter = new StravaCsvImporter();
            int count = stravaCsvImporter.LoadFromStravaArchive(filePath, Database);
            load.Stop();
            Logging.Instance.Log(string.Format("Load took {0}", load.Elapsed));
            UpdateViews(true);
            MessageBox.Show($"Loaded {count} activities from archive");
        }

        private void FellrnrTrainingAnalysis_FormClosed(object sender, FormClosedEventArgs e)
        {
            Logging.Instance.Close(); //must be last as other things write to logging
        }


        private void connectToStravaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StravaApi.Instance.Connect();
            MessageBox.Show("Connected to Strava");
            SetMenuAvailability();
        }

        private void syncWithStravaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.UseWaitCursor = true;
            int count = StravaApi.Instance.SyncNewActivites(Database);
            Application.UseWaitCursor = false;

            MessageBox.Show($"Synced {count} activities");
            UpdateViews(false);
        }

        private void clearDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Database = new Database();
            UpdateViews(false);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Database.SaveToFile();
        }



        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExitApp(false);
        }

        private void ExitApp(bool force)
        {
            Database.SaveToFile();
            Utils.Options.SaveConfig();
            ActivityDatumMetadata.WriteToCsv();
            DataStreamDefinition.WriteToCsv();
            if(force)
            {
                Environment.Exit(0);
            }
            else
            {
                Application.Exit();
            }
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog configForm = new OptionsDialog();
            configForm.ShowDialog();
            UpdateViews(false);

        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                openFileDialog.InitialDirectory = folder;
                openFileDialog.Filter = "*.bin|*.bin|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.ReadOnlyChecked = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileName;

                    Database.LoadFromFile(filePath);
                }
            }
        }

        private void saveDatabaseAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                string folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                saveFileDialog.InitialDirectory = folder;
                saveFileDialog.Filter = "*.bin|*.bin|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = saveFileDialog.FileName;

                    Database.SaveToFile(filePath);
                }

            }
        }

        private void forceRecalculationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateViews(true);
            MessageBox.Show("Recalculation complete");
        }

        private void showErrorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string errorText = Logging.Instance.Error();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(errorText);
            largeTextDialogForm.ShowDialog();
            Logging.Instance.HasErrors = false;
            Logging.Instance.Clear();
            showErrorsToolStripMenuItem.Enabled = false;
        }

        private void normalLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logText = Logging.Instance.Debug();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(logText);
            largeTextDialogForm.ShowDialog();
        }

        private void logToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string logText = Logging.Instance.Log();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(logText);
            largeTextDialogForm.ShowDialog();
        }

        private void errorLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string debugText = Logging.Instance.Error();
            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(debugText);
            largeTextDialogForm.ShowDialog();
        }

        private void dataStreamDefinitionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<Model.DataStreamDefinition>? definitions = Model.DataStreamDefinition.GetDefinitions();
            if (definitions != null)
            {
                UI.DataStreamDefinitionEditor dataStreamDefinitionEditor = new UI.DataStreamDefinitionEditor(definitions);
                dataStreamDefinitionEditor.Edited += EditEventHandler;

                dataStreamDefinitionEditor.Show();
            }
            else
            {
                MessageBox.Show("Oops, no definitions to edit");
            }
        }

        public void EditEventHandler(DataStreamDefinitionEditor sender)
        {
            List<Model.DataStreamDefinition>? definitions = sender.Definitions;
            Model.DataStreamDefinition.SetDefinitions(definitions);
            UpdateDataStreamGraph(); //only update the grpah
        }
    }

}
#region test
/*
 *         private void Test()
        {
            bool reload = true;

            if (reload)
            {
                //            Utils.Config.Instance.OnlyLoadAfter = DateTime.Now.Subtract(new TimeSpan(365, 0, 0, 0));
                //            Logging.Instance.Log(string.Format("OnlyLoad After {0}", Utils.Config.Instance.OnlyLoadAfter));

                Stopwatch load = new Stopwatch();
                load.Start();
                Database.LoadFromStravaArchive(@"C:\Users\jfsav\Downloads\A Strava 20221228 export_252540\profile.csv"); //HACK - force loading from csv
                load.Stop();
                Logging.Instance.Log(string.Format("Load took {0}", load.Elapsed));


                Stopwatch serialize = new Stopwatch();
                serialize.Start();
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream("Database.bin", FileMode.Create, FileAccess.Write, FileShare.None)) //TODO: variable binary filename
                {
#pragma warning disable SYSLIB0011
                    formatter.Serialize(stream, Database);
#pragma warning restore SYSLIB0011
                }
                serialize.Stop();
                Logging.Instance.Log(string.Format("Serialization took {0}", serialize.Elapsed));
            }
            else
            {
                Stopwatch deserialize = new Stopwatch();
                deserialize.Start();
                IFormatter formatter = new BinaryFormatter();
                using (Stream stream = new FileStream("Database.bin", FileMode.Open, FileAccess.Read, FileShare.None))
                {
#pragma warning disable SYSLIB0011
                    Object deserialized = formatter.Deserialize(stream);
                    if (deserialized != null && deserialized is Model.Database)
                    {
                        Database = (Model.Database)deserialized;
                    }
                    else
                    {
                        Logging.Instance.Error(string.Format("Derialization failed on {0}", "Database.bin"));
                    }
#pragma warning restore SYSLIB0011
                }
                deserialize.Stop();
                Logging.Instance.Log(string.Format("Derialization took {0}", deserialize.Elapsed));

            }
        }
*/
#endregion