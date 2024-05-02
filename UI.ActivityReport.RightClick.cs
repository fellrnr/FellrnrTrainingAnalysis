using FellrnrTrainingAnalysis.Action;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.UI;
using FellrnrTrainingAnalysis.Utils;
using Microsoft.VisualBasic;
using ScottPlot;
using ScottPlot.Plottable;
using ScottPlot.Renderable;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FellrnrTrainingAnalysis
{
    partial class ActivityReport
    {

        private DataGridViewCellEventArgs? mouseLocation;
        ContextMenuStrip strip = new ContextMenuStrip();
        List<ToolStripItem> rightClickMenuSubMenus = new List<ToolStripItem>();

        private void CreateRightClickMenus()
        {
            AddContextMenu("Open In Strava...", new EventHandler(toolStripItem1_Click_openStrava));
            AddContextMenu("Open ALL In Strava...", new EventHandler(toolStripItem1_Click_openAllStrava));
            AddContextMenu("Open File (system viewer)...", new EventHandler(toolStripItem1_Click_openFile));
            AddContextMenu("Copy File path", new EventHandler(toolStripItem1_Click_copyFitFile));
            AddContextMenu("Open In Garmin...", new EventHandler(toolStripItem1_Click_openGarmin));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Recalculate", new EventHandler(toolStripItem1_Click_recalculate));
            AddContextMenu("Recalculate Hills", new EventHandler(toolStripItem1_Click_recalculateHills));
            AddContextMenu("Reprocess Tags", new EventHandler(toolStripItem1_Click_reprocessTags));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Highlight", new EventHandler(toolStripItem1_Click_highlight));
            AddContextMenu("Edit Name", new EventHandler(toolStripItem1_Click_editName));
            AddContextMenu("Edit Description", new EventHandler(toolStripItem1_Click_editDescription));
            AddContextMenu("Update Description", new EventHandler(toolStripItem1_Click_updateDescription));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Refresh From Strava", new EventHandler(toolStripItem1_Click_refresh));
            AddContextMenu("Refresh ALL From Strava", new EventHandler(toolStripItem1_Click_refreshAll));
            AddContextMenu("Reread FIT/GPX file", new EventHandler(toolStripItem1_Click_rereadDataFile));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Show Relationship Combinations...", new EventHandler(toolStripItem1_Click_showRelationships));
            AddContextMenu("Explore Relationships...", new EventHandler(toolStripItem1_Click_exploreRelationships));
            AddContextMenu("Scan For Data Quality Issues...", new EventHandler(toolStripItem1_Click_findDataQuality));
            AddContextMenu("Show Data Quality Issues...", new EventHandler(toolStripItem1_Click_showDataQuality));
            AddContextMenu("Tag ALL In Strava As...", new EventHandler(toolStripItem1_Click_tagAllStravaAsInput));
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddFixSubMenus("Fix This Activity", toolStripItem1_Click_tagStrava);
            AddFixSubMenus("Fix ALL Activities", toolStripItem1_Click_tagAllStrava);
            AddContextMenu("Distance from GPS", toolStripItem1_Click_distanceGPS);
            AddContextMenu("Lookup altitude from location", toolStripItem1_Click_lookupLocation);
            AddContextMenu("Lookup ALL altitude from location", toolStripItem1_Click_lookupAllLocation);
            rightClickMenuSubMenus.Add(new ToolStripSeparator());
            AddContextMenu("Write table to CSV...", new EventHandler(toolStripItem1_Click_writeCsv));
            AddContextMenu("Debug Activity...", new EventHandler(toolStripItem1_Click_debugActivity));
            AddContextMenu("Delete Activity...", new EventHandler(toolStripItem1_Click_deleteActivity));
        }

        private void AddContextMenu(string text, EventHandler eventHandler, TagActivities? tagActivities = null)
        {
            ToolStripMenuItem rightClickMenuItem = new ToolStripMenuItem();
            rightClickMenuItem.Text = text;
            rightClickMenuItem.Click += eventHandler;
            rightClickMenuItem.Tag = tagActivities;
            rightClickMenuSubMenus.Add(rightClickMenuItem);
        }



        //start char is ⌗ U+2317
        //middle markers are ༶ (U+0F36)
        //end is ֍ (U+058D)
        private const string ASKME = "ASKME";
        List<TagActivities> SpecialFixActivityTags = new List<TagActivities>() {
            new TagActivities("Replace Start of Altitude", "⌗Altitude༶CopyBack༶10֍"),
            new TagActivities("Lookup Altitude using Google API", $"⌗{Activity.TagAltitude}༶Lookup֍"),
        };
        List<string> FixTimeSeriesCommands = new List<string>() { "Delete", "Cap" };
        List<string> FixDatumCommands = new List<string>() { "Override" };
        List<TagActivities> GetFixDatumTags(string command)
        {
            List<TagActivities> tags = new List<TagActivities>();
            if (Database != null)
            {
                foreach (string afn in Database!.CurrentAthlete.ActivityRecordedFieldNames)
                {
                    tags.Add(new TagActivities($"{command} {afn}...", $"⌗{afn}༶Override༶ASKME֍"));
                }
            }
            return tags;
        }
        List<TagActivities> GetFixTimeSeriesTags(string command)
        {
            List<TagActivities> tags = new List<TagActivities>();
            if (Database != null)
            {
                foreach (string tsn in Database!.CurrentAthlete.AllNonVirtualTimeSeriesNames)
                {
                    tags.Add(new TagActivities($"{command} {tsn}", $"⌗{tsn}༶{command}֍"));
                }
            }
            return tags;
        }

        private void AddFixSubMenus(string name, EventHandler eventHandler)
        {
            ToolStripMenuItem rightClickMenu = new ToolStripMenuItem(); //Fix All/Fix
            rightClickMenu.Text = name;
            rightClickMenuSubMenus.Add(rightClickMenu);

            AddFixSubSubMenus("Special", rightClickMenu, eventHandler, SpecialFixActivityTags);
            foreach (string command in FixTimeSeriesCommands)
            {
                List<TagActivities> tags = GetFixTimeSeriesTags(command);
                AddFixSubSubMenus(command, rightClickMenu, eventHandler, tags);
            }
            foreach (string command in FixDatumCommands)
            {
                List<TagActivities> tags = GetFixDatumTags(command);
                AddFixSubSubMenus(command, rightClickMenu, eventHandler, tags);
            }
        }

        private void AddFixSubSubMenus(string subSubMenuName, ToolStripMenuItem rightClickSubMenu, EventHandler eventHandler, List<TagActivities> tagActivities)
        {
            ToolStripMenuItem rightClickSubSubMenu = new ToolStripMenuItem(); //delete, cap, etc.
            rightClickSubSubMenu.Text = subSubMenuName;
            rightClickSubMenu.DropDownItems.Add(rightClickSubSubMenu);

            List<ToolStripMenuItem> toolStripSubMenuItems = new List<ToolStripMenuItem>();
            foreach (TagActivities t in tagActivities)
            {
                ToolStripMenuItem toolStripItem = new ToolStripMenuItem();
                toolStripItem.Text = t.Name;
                toolStripItem.Click += eventHandler;
                toolStripItem.Tag = t;
                toolStripSubMenuItems.Add(toolStripItem);
            }
            rightClickSubSubMenu.DropDownItems.AddRange(toolStripSubMenuItems.ToArray());
        }


        private void AddRightClicks(DataGridViewColumn dataGridViewColumn)
        {
            dataGridViewColumn.ContextMenuStrip = strip;
            if (dataGridViewColumn.ContextMenuStrip.Items.Count > 0) { dataGridViewColumn.ContextMenuStrip.Items.Clear(); }
            dataGridViewColumn.ContextMenuStrip.Items.AddRange(rightClickMenuSubMenus.ToArray());
        }

        private Activity? GetActivity()
        {
            if (mouseLocation == null)
            {
                Logging.Instance.Log($"No mouse location for toolStripItem1_Click_recalculate");
                return null;
            }

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return null;
            }

            return activity;
        }

        // Change the cell's color.
        private void toolStripItem1_Click_highlight(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            activityDataGridView.Rows[mouseLocation.RowIndex].Cells[mouseLocation.ColumnIndex].Style.BackColor = Color.Red;
        }


        private void toolStripItem1_Click_editName(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(activity.Name);
            largeTextDialogForm.ShowDialog();
            if (largeTextDialogForm.Cancelled) return;

            string name = largeTextDialogForm.Value;

            if (!Action.StravaApi.Instance.UpdateActivityDetails(activity, name, null))
            {
                MessageBox.Show("Update Failed");
                return;
            }

            activity.Name = name;
            UpdateViews?.Invoke();
        }

        private void toolStripItem1_Click_editDescription(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(activity.Description);
            largeTextDialogForm.ShowDialog();
            if (largeTextDialogForm.Cancelled) return;


            string description = largeTextDialogForm.Value;
            if (!Action.StravaApi.Instance.UpdateActivityDetails(activity, null, description))
            {
                MessageBox.Show("Update Failed");
                return;
            }
            activity.Description = description;
            UpdateViews?.Invoke();
        }

        private string AddDatum(Extensible extensible, string text, string name)
        {
            if (extensible.HasNamedDatum(name))
            {
                string formated = DatumFormatter.Format(extensible, name);
                return $"{Environment.NewLine}{text}{formated}";
            }
            else
            {
                return string.Empty;
            }
        }
        private void toolStripItem1_Click_updateDescription(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            string description = activity.Description;
            description += AddDatum(activity, "Grade Adjusted Distance: ", Activity.TagGradeAdjustedDistance);
            description += AddDatum(activity.Day, "Distance Rolling Year: ", "Σ🏃→ 1Y");
            description += AddDatum(activity.Day, "Elevation Rolling Year: ", "Σ🏃⬆ 1Y");
            description += AddDatum(activity.Day, "Grade Adjusted Distance Rolling Year: ", "Σ🏃📐 1Y");
            description += AddDatum(activity, "", Activity.TagClimbed);

            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(description);
            largeTextDialogForm.ShowDialog();
            if (largeTextDialogForm.Cancelled) return;


            description = largeTextDialogForm.Value;
            if (!Action.StravaApi.Instance.UpdateActivityDetails(activity, null, description))
            {
                MessageBox.Show("Update Failed");
                return;
            }
            activity.Description = description;
            UpdateViews?.Invoke();
        }

        private void toolStripItem1_Click_recalculate(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            Logging.Instance.Log($"toolStripItem1_Click_recalculate activity {activity}");
            activity.Recalculate(true);

            Logging.Instance.Log($"toolStripItem1_Click_recalculate update views");
            UpdateViews?.Invoke();

            Logging.Instance.Log($"toolStripItem1_Click_recalculate done");
            MessageBox.Show("Done");
        }
        private void toolStripItem1_Click_reprocessTags(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            activity.RemoveNamedDatum(Activity.TagProcessedTags);

            Logging.Instance.Log($"toolStripItem1_Click_recalculate activity {activity}");
            activity.Recalculate(true);

            Logging.Instance.Log($"toolStripItem1_Click_recalculate update views");
            UpdateViews?.Invoke();

            Logging.Instance.Log($"toolStripItem1_Click_recalculate done");
            MessageBox.Show("Done");
        }
        private void toolStripItem1_Click_recalculateHills(object? sender, EventArgs args)
        {
            if (mouseLocation == null || Database == null || Database.Hills == null)
                return;

            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            activity.RecalculateHills(Database.Hills, true, true);

            //don't muddy things with a recalculation
            //UpdateViews?.Invoke();

            MessageBox.Show("Done");
        }
        private void toolStripItem1_Click_refresh(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null || Database == null) return;

            StravaApi.Instance.RefreshActivity(Database, activity);

            Logging.Instance.Log($"toolStripItem1_Click_recalculate activity {activity}");
            activity.Recalculate(true);

            Logging.Instance.Log($"toolStripItem1_Click_recalculate update views");
            UpdateViews?.Invoke();

            Logging.Instance.Log($"toolStripItem1_Click_recalculate done");
            MessageBox.Show("Done");
        }

        private void toolStripItem1_Click_refreshAll(object? sender, EventArgs args)
        {

            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity == null)
                    return;

                StravaApi.Instance.RefreshActivity(Database!, activity);

                Logging.Instance.Log($"toolStripItem1_Click_recalculate activity {activity}");
                activity.Recalculate(true);
            }

            Logging.Instance.Log($"toolStripItem1_Click_recalculate update views");
            UpdateViews?.Invoke();

            Logging.Instance.Log($"toolStripItem1_Click_recalculate done");
            MessageBox.Show("Done");
        }


        private void toolStripItem1_Click_rereadDataFile(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            string? filepath = activity.FileFullPath;
            if (filepath == null)
            {
                MessageBox.Show("No file on activity");
                return;
            }

            if (filepath.ToLower().EndsWith(".fit") || filepath.ToLower().EndsWith(".fit.gz"))
            {
                FitReader fitReader = new FitReader(activity);
                try
                {
                    fitReader.ReadFitFromStravaArchive(reload: true);
                }
                catch (Exception e)
                {
                    MessageBox.Show($"Exception thrown reading FIT file {filepath}, {e}");
                    return;
                }
                MessageBox.Show("Completed FIT Reread");
            }
            else if (filepath.ToLower().EndsWith(".gpx") || filepath.ToLower().EndsWith(".gpx.gz"))
            {
                GpxProcessor gpxProcessor = new GpxProcessor(activity);

                gpxProcessor.ProcessGpx();

                MessageBox.Show("Completed GPX Reread");
            }
            else
            {
                MessageBox.Show("Activity file is not recognized type " + filepath);
            }
            UpdateViews?.Invoke();
        }

        private void toolStripItem1_Click_openGarmin(object? sender, EventArgs args)
        {

            //can't open activity directly as we don't have the garmin id (the fit file name doesn't match)
            //So search for activities by date
            //https://connect.garmin.com/modern/activities?activityType=running&startDate=2023-07-20&endDate=2023-07-20
            //https://connect.garmin.com/modern/activities?startDate=2023-07-20&endDate=2023-07-20

            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            DateTime? start = activity.StartDateNoTimeLocal;

            if (start == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            string date = string.Format("{0:D4}-{1:D2}-{2:d2}", start.Value.Year, start.Value.Month, start.Value.Day);
            string target = $"https://connect.garmin.com/modern/activities?startDate={date}&endDate={date}";
            Misc.RunCommand(target);
        }

        private void toolStripItem1_Click_openFile(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            string? filepath = activity.FileFullPath;
            if (filepath == null)
            {
                MessageBox.Show("No file on activity");
                return;
            }
            if (filepath.ToLower().EndsWith(".fit.gz"))
            {
                filepath = filepath.Remove(filepath.Length - 3);
            }
            if (!filepath.ToLower().EndsWith(".fit"))
            {
                if (MessageBox.Show($"File is not fit, {filepath}", "Really?", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }
            }

            Misc.RunCommand(filepath);

        }

        private void toolStripItem1_Click_copyFitFile(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            string? filepath = activity.FileFullPath;
            if (filepath == null)
            {
                MessageBox.Show("No file on activity");
                return;
            }
            if (filepath.ToLower().EndsWith(".fit.gz"))
            {
                filepath = filepath.Remove(filepath.Length - 3);
            }
            filepath = filepath.Replace('/', '\\');

            Clipboard.SetText(filepath);
        }

        private void toolStripItem1_Click_openStrava(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            StravaApi.OpenAsStravaWebPage(activity);

        }


        private void toolStripItem1_Click_openAllStrava(object? sender, EventArgs args)
        {
            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity == null)
                    return;

                string key = activity.PrimaryKey();
                string target = "https://www.strava.com/activities/" + key;
                Misc.RunCommand(target);
            }
        }

        private class TagActivities
        {
            public string Name;
            public string Tag;

            public TagActivities(string name, string tag)
            {
                Name = name;
                Tag = tag;
            }
        }

        private void toolStripItem1_Click_writeCsv(object? sender, EventArgs args)
        {

            if (activityDataGridView.Rows.Count > 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV (*.csv)|*.csv";
                sfd.FileName = "Output.csv";
                bool fileError = false;
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(sfd.FileName))
                    {
                        try
                        {
                            File.Delete(sfd.FileName);
                        }
                        catch (IOException ex)
                        {
                            fileError = true;
                            MessageBox.Show("It wasn't possible to write the data to the disk." + ex.Message);
                        }
                    }
                    if (!fileError)
                    {
                        try
                        {
                            int columnCount = activityDataGridView.Columns.Count;
                            string columnNames = "";
                            string[] outputCsv = new string[activityDataGridView.Rows.Count + 1];
                            for (int i = 0; i < columnCount; i++)
                            {
                                columnNames += activityDataGridView.Columns[i].HeaderText.ToString() + ",";
                            }
                            outputCsv[0] += columnNames;

                            for (int i = 1; (i - 1) < activityDataGridView.Rows.Count; i++)
                            {
                                for (int j = 0; j < columnCount; j++)
                                {
                                    outputCsv[i] += Utils.Misc.EscapeForCsv(activityDataGridView.Rows[i - 1].Cells[j].Value.ToString()!) + ",";
                                }
                            }

                            File.WriteAllLines(sfd.FileName, outputCsv, Encoding.UTF8);
                            MessageBox.Show("Data Exported Successfully", "Info");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error :" + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("No Record To Export !!!", "Info");
            }

        }
        private void toolStripItem1_Click_debugActivity(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (activity == null) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Datums");
            foreach (Datum d in activity.DataValues)
            {
                sb.AppendLine(d.ToString());
            }

            sb.AppendLine("TimeSeries");
            foreach (KeyValuePair<string, TimeSeriesBase> kvp in activity.TimeSeries)
            {
                sb.AppendLine(kvp.Value.ToString());
            }

            LargeTextDialogForm largeTextDialogForm = new LargeTextDialogForm(sb.ToString());
            largeTextDialogForm.ShowDialog();
        }
        private void toolStripItem1_Click_deleteActivity(object? sender, EventArgs args)
        {
            Model.Activity? activity = GetActivity();
            if (Database == null || activity == null) return;

            if (MessageBox.Show($"Really delete {activity}? (This doesn not remove it from Strava)", "Delete Activity", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            Database.CurrentAthlete.DeleteActivityBeforeRecalcualte(activity);

            MessageBox.Show("Deleted activity. Perform forced recalculation or restart now");
        }


        private void toolStripItem1_Click_tagStrava(object? sender, EventArgs args)
        {
            if (mouseLocation == null || sender == null)
                return;
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            TagActivities aTagActivities = (TagActivities)toolStripMenuItem.Tag;
            string tag = aTagActivities.Tag;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            TagStravaActivity(tag, activity);

            activity.Recalculate(true);

            UpdateViews?.Invoke();

            MessageBox.Show("Done");
        }

        private void toolStripItem1_Click_tagAllStrava(object? sender, EventArgs args)
        {
            if (sender == null) return;
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            TagActivities aTagActivities = (TagActivities)toolStripMenuItem.Tag;
            string tag = aTagActivities.Tag;

            List<Activity> aActivities = new List<Activity>();
            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity != null)
                {
                    TagStravaActivity(tag, activity);
                    aActivities.Add(activity);
                }
            }
            foreach (Activity activity in aActivities)
            {
                activity.Recalculate(true);
            }

            UpdateViews?.Invoke();

            MessageBox.Show("Done");
        }

        //this doesn't seem to help with data quality much, but left here in case it's useful at some point
        private void toolStripItem1_Click_distanceGPS(object? sender, EventArgs args)
        {
            if (mouseLocation == null || sender == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }


            if (activity.LocationStream != null && activity.LocationStream.Times != null)
            {
                float[] distances = activity.LocationStream.LocationToDistance();

                if (distances != null)
                {
                    float final = distances.Last();
                    if (activity.TimeSeries.ContainsKey(Activity.TagDistance))
                    {
                        List<float> to1sec = Utils.TimeSeriesUtils.InterpolateToOneSecond(activity.LocationStream.Times.ToArray(), distances);
                        TimeSeriesBase tsb = activity.TimeSeries[Activity.TagDistance];
                        TimeValueList? tvl = tsb.GetData();
                        if (tvl != null)
                        {
                            float[] existing = tvl.Values;

                            float[] diffs = new float[to1sec.Count];
                            for (int i = 0; i < to1sec.Count; i++)
                            {
                                float d = existing[i] - to1sec[i];
                                diffs[i] = d;
                            }

                        }
                    }

                    if (MessageBox.Show($"replace raw distance with that from location {final}?", "Continue?", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        activity.AddTimeSeries(Activity.TagDistance, activity.LocationStream.Times, distances);
                        MessageBox.Show("Done");
                    }
                }
                else
                {
                    MessageBox.Show("Failed to calculate distances");
                }
            }
            else
            {
                MessageBox.Show("No location stream or times for locations");
            }

        }
        private void TagStravaActivity(string tag, Activity activity)
        {
            if (tag.Contains(ASKME)) //TODO: support overriding non-numeric values
            {
                string input = Interaction.InputBox("Enter numeric value");
                if (string.IsNullOrEmpty(input) || !int.TryParse(input, out int askme))
                    return;
                tag = tag.Replace(ASKME, input);
            }

            Action.Tags tags = new FellrnrTrainingAnalysis.Action.Tags();
            tags.TagStravaActivity(tag, activity);
        }



        private void toolStripItem1_Click_tagAllStravaAsInput(object? sender, EventArgs args)
        {
            string input = Interaction.InputBox("Enter tag to add, including hash");
            if (string.IsNullOrEmpty(input))
                return;
            string TAG = $" {input}";
            int success = 0, error = 0, already = 0, count = 0;
            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity == null)
                    return;

                string? name = activity.GetNamedStringDatum("Name");
                if (name != null && !name.Contains(TAG))
                {
                    name = name + TAG;
                    if (Action.StravaApi.Instance.UpdateActivityDetails(activity, name))
                        success++;
                    else
                        error++;
                }
                else
                {
                    already++;
                }
                if (++count >= 100)
                    break;
            }
            if (count >= 100)
                MessageBox.Show($"Updated {success} entries, with {error} failures, and {already} already tagged, hit rate limit so wait for 15 minutes");
            else
                MessageBox.Show($"Updated {success} entries, with {error} failures, and {already} already tagged");

        }


        private async void toolStripItem1_Click_lookupAllLocation(object? sender, EventArgs args)
        {
            foreach (DataGridViewRow row in activityDataGridView.Rows)
            {
                Model.Activity? activity = GetActivityForRow(row);
                if (activity == null)
                    return;

                await LookupElevation(activity, false);
            }
            UpdateViews?.Invoke();
            MessageBox.Show("Done");
        }
        private async void toolStripItem1_Click_lookupLocation(object? sender, EventArgs args)
        {
            if (mouseLocation == null || sender == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            await LookupElevation(activity, true);
            UpdateViews?.Invoke();
            MessageBox.Show("Done");

        }

        private async Task LookupElevation(Activity activity, bool dialogs)
        {
            if (activity.LocationStream != null && activity.LocationStream.Times != null)
            {
                Action.Elevation elevation = new Elevation();

                Task<TimeSeriesBase?> task = elevation.GetElevation(activity.LocationStream, activity);
                TimeSeriesBase? result = await task;

                if (result != null)
                {
                    string stats;
                    if (activity.TimeSeries.ContainsKey(Activity.TagAltitude))
                    {
                        TimeSeriesBase original = activity.TimeSeries[Activity.TagAltitude];
                        stats =
                            $"Original/New: " +
                            $"min {original.Percentile(TimeSeriesBase.StaticsValue.Min):#,0.0}/{result.Percentile(TimeSeriesBase.StaticsValue.Min):#,0.0}, " +
                            $"mean {original.Percentile(TimeSeriesBase.StaticsValue.Mean):#,0.0}/{result.Percentile(TimeSeriesBase.StaticsValue.Mean):#,0.0}, " +
                            $"max {original.Percentile(TimeSeriesBase.StaticsValue.Max):#,0.0}/{result.Percentile(TimeSeriesBase.StaticsValue.Max):#,0.0}, " +
                            $"sd {original.Percentile(TimeSeriesBase.StaticsValue.StandardDeviation)}/{result.Percentile(TimeSeriesBase.StaticsValue.StandardDeviation)}";
                    }
                    else
                    {
                        stats = result.ToStatisticsString();
                    }
                    Logging.Instance.Log($"Replacing original elevation?{stats} on {activity} ");
                    bool doit = false;
                    if (!dialogs || MessageBox.Show($"replace original elevation?{Environment.NewLine}{stats}", "Continue?", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        doit = true;
                    }
                    if(doit)
                    { 
                        string tag = $"⌗{Activity.TagAltitude}༶Lookup֍";
                        Action.Tags tags = new FellrnrTrainingAnalysis.Action.Tags();
                        tags.TagStravaActivity(tag, activity, false);

                        activity.AddTimeSeries(result);
                        activity.Recalculate(true);
                    }
                }
                else
                {
                    if(dialogs)
                        MessageBox.Show("Failed to lookup elevation");
                    Logging.Instance.Log($"Failed to lookup elevation on {activity} ");
                }
            }
            else
            {
                if (dialogs)
                    MessageBox.Show("No location stream or times for locations");
                Logging.Instance.Log($"No location stream or times for locations on {activity} ");
            }
        }

        private void toolStripItem1_Click_showDataQuality(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            if (activity.DataQualityIssues == null || activity.DataQualityIssues.Count == 0)
            {
                MessageBox.Show("No data quality issues");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string s in activity.DataQualityIssues) { stringBuilder.AppendLine(s); }

            MessageBox.Show(stringBuilder.ToString());
        }

        private void toolStripItem1_Click_showRelationships(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            //StringBuilder stringBuilder = new StringBuilder();
            //SortedDictionary<float, string> results = new SortedDictionary<float, string>();
            List<string> done = new List<string>();
            //List<List<string>> grid = new List<List<string>>();
            //List<string> headerrow = new List<string>() { "Time Series" }; //top left corner
            //grid.Add(headerrow);
            DataTable dt = new DataTable("Relationships");
            dt.Columns.Add("Time Series", typeof(string));

            foreach (KeyValuePair<string, TimeSeriesBase> kvpX in activity.TimeSeries)
            {
                dt.Columns.Add(kvpX.Key, typeof(object));
            }

            foreach (KeyValuePair<string, TimeSeriesBase> kvpX in activity.TimeSeries)
            {
                TimeSeriesBase tsbx = kvpX.Value;
                //done.Add(tsbx.Name); uncomment to prevent all being added

                //List<string> resultrow = new List<string>() { tsbx.Name };
                DataRow workRow = dt.NewRow();
                workRow["Time Series"] = tsbx.Name;
                //dt.Rows.Add(rowno, tsbx.Name);    
                foreach (KeyValuePair<string, TimeSeriesBase> kvpY in activity.TimeSeries)
                {
                    TimeSeriesBase tsby = kvpY.Value;
                    object answer = "";
                    if(kvpX.Key != kvpY.Key && tsbx.IsValid() && tsby.IsValid() && !done.Contains(tsby.Name))
                    {
                        TimeValueList? tvlx = tsbx.GetData(0, false);
                        TimeValueList? tvly = tsby.GetData(0, false);
                        if (tvlx != null && tvly != null)
                        {
                            AlignedTimeSeries? aligned = AlignedTimeSeries.Align(tvlx, tvly);

                            if (aligned != null)
                            {
                                LinearRegression? regression = LinearRegression.EvaluateLinearRegression(aligned,
                                                                                                 primaryIsX: false,
                                                                                                 ignoreZerosX: true,
                                                                                                 ignoreZerosY: true);

                                if (regression != null)
                                {
                                    //float key = 1.0f - regression.RSquared;
                                    //while (results.ContainsKey(key))
                                    //    key += 0.0001f;
                                    answer = regression;
                                    //results.Add(key, $"{tsbx.Name}, {tsby.Name}, {regression}");//reverse order
                                }
                                else
                                {
                                    answer = "Error";
                                }
                            }
                        }
                    }
                    workRow[tsby.Name] = answer;
                    //resultrow.Add(answer);
                }
                dt.Rows.Add(workRow);
            }

            TextGridDialog textGridDialog = new TextGridDialog(dt);
            textGridDialog.Show();

        }
        private void toolStripItem1_Click_findDataQuality(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }

            DataQuality dataQuality = new DataQuality();
            dataQuality.FindBadTimeSeries(activity);

            if (activity.DataQualityIssues == null || activity.DataQualityIssues.Count == 0)
            {
                MessageBox.Show("No data quality issues");
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (string s in activity.DataQualityIssues) { stringBuilder.AppendLine(s); }

            MessageBox.Show(stringBuilder.ToString());
        }

        private void activityDataGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            mouseLocation = e;
        }

        private void pageSizeComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pageSizeComboBox1.Text == "All")
            {
                PageSize = -1;
            }
            else
            {
                if (!int.TryParse(pageSizeComboBox1.Text, out PageSize))
                    PageSize = 5;
            }
            UpdateReport();
        }

        
        private void toolStripItem1_Click_exploreRelationships(object? sender, EventArgs args)
        {
            if (mouseLocation == null)
                return;

            DataGridViewRow row = activityDataGridView.Rows[mouseLocation.RowIndex];
            Model.Activity? activity = GetActivityForRow(row);
            if (activity == null)
            {
                MessageBox.Show("No activity found");
                return;
            }
            ActivityCorrelation activityCorrelation = new ActivityCorrelation(Database!, activity, null);
            activityCorrelation.Show();
        }


    }
}
