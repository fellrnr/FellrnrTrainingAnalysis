using System;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using de.schumacher_bw.Strava.Endpoint;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using GMap.NET.MapProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualBasic.Logging;

//taken from https://github.com/ffes/fitlog2tcx/tree/master

namespace FellrnrTrainingAnalysis.Action
{
    public class SportTracks
    {
        public SportTracks() { }


        //CAUTION: The process to create new activities is to parse the Fitlog, verify, write a new Fitlog, manually import to Golden Cheetah, manually export to FIT files, then run the upload step.

        //Caution: fitlog and strava may be from different devices. The fitlog is "master", but doesn't have the name/description right

        //So:
        //we can't update an existing activity in a meaningful way (just name, description)
        //We can create a new activity with just time/distance but no elevation, etc. 
        //We can upload a new activity from a file (FIT, TCX and GPX)
        // read and manipulation FIT file? Ouch. Lots of work, lots of chance for really buggering things up
        // create new TCX file? Synthetic? 
        //   need to strip when pace very slow (not on treadmill)
        //   adjust pace for right distance
        //   fake elevation change? End up with very high elevations. 
        //We could mark an existing activity, embedding changes in the description
        //

        //Approach
        //
        // 1. Create missing activities. fitlog - parse - fitlog - gc - fit - strava? 
        // 2. Tag all descents with descent and grade
        // 3. Tag missmatched distance/time with override tags (only counts for goals, not even GAD, let alone other statistics)
        //
        // Accept the GPS error? Everyone has them, but the greenway was especially bad
        // 
        // 4. To adjust for wrong distance, we'd have to drop GPS data AND rework the distance feed


        //don't forget grade adjusted distance for treadmill descents



        public void FixFromFitlog(Database database, Athlete athlete)
        {
            AddFromFitlog(database, athlete);

        }

        public void UpdateFromFitlog(Database database, Athlete athlete, int limit)
        {
            int count = 0;
            foreach (SportTracksActivity act in AllActivitesParsed)
            {
                if (act.matchingActivity != null)
                {
                    Activity activity = act.matchingActivity;
                    string description = activity.Description;
                    float? distance = act.matchingActivity.GetNamedFloatDatum(Activity.TagDistance);
                    float? time = act.matchingActivity.GetNamedFloatDatum(Activity.TagElapsedTime);
                    string addOn = "";
                    string dtag = $"{Tags.START}{Activity.TagDistance}{Tags.MIDDLE}Override{Tags.MIDDLE}";
                    if (act._totalDistance != 0 && (distance == null || distance == 0 || !WithinMargin(distance, act._totalDistance, 500.0)) && !description.Contains(dtag))
                    {
                        addOn += $" {dtag}{act._totalDistance}{Tags.END} \n";
                    }

                    //the time in fitlog seems to be moving time
                    //string ttag = $"{Tags.START}{Activity.TagElapsedTime}{Tags.MIDDLE}Override{Tags.MIDDLE}";
                    //if (act._totalDuration != 0 && (time == null || time == 0 || !WithinMargin(time, act._totalDuration, 60.0)) && !description.Contains(ttag))
                    //{
                    //    addOn += $" {ttag}{act._totalDuration}{Tags.END} \n";
                    //}

                    string tmtag = $"{Tags.START}{Activity.TagTreadmillAngle}{Tags.MIDDLE}Override{Tags.MIDDLE}";
                    if (act._name.ToLower().Contains("descent") && !description.Contains(tmtag))
                    {
                        double defaultAngle = -10.0; //this seems to be our normal descent
                        double angle = act._treadmillAngle ?? defaultAngle;
                        addOn += $" {tmtag}{angle}{Tags.END} \n";
                    }
                    if (!string.IsNullOrEmpty(addOn))
                    {
                        Logging.Instance.Debug($"Update #{++count}, [{activity}], {distance}m, {time}s  description to add {addOn} to [{description}]");
                        if (count < limit)
                        {
                            if (!string.IsNullOrEmpty(description))
                                description += "\n";
                            description += addOn;
                            //if (!StravaApi.Instance.UpdateActivityDetails(activity, null, description))
                            //{
                            //    if (MessageBox.Show($"Update of {activity} failed, continue?", "oops", MessageBoxButtons.YesNoCancel) == DialogResult.No)
                            //        return;
                            //}
                        }
                    }
                }
            }
            MessageBox.Show($"Hit limit {limit} of {count}");
        }

        public void AddFromFitlog(Database database, Athlete athlete)
        {
            foreach(SportTracksActivity act in ActivitiesToUpload)
            {
                DateTime start = act._startTime;
                //2002_07_08_12_40_40.fit
                string filenameGuess = start.ToString("yyyy_MM_dd_HH_mm_ss") + ".fit";
                string folder = @"C:\Users\jfsav\OneDrive\Jonathan\Fellrnr Export Missing Workouts";
                string path = Path.Combine(folder, filenameGuess);
                if(File.Exists(path))
                {
                    Logging.Instance.Log($"Trying filename [{path}]");

                    FileInfo fileInfo = new FileInfo(path);

                    //activity has been added to athlete by upload
                    StravaApi.UploadResult result = StravaApi.Instance.UploadActivityFromFit(database, fileInfo, act._name, act._notes, act._treadmill);
                    if(result.Activity != null)
                    {
                        DialogResult dialogResult = MessageBox.Show($"Uploaded activity {result.Activity.PrimaryKey()} for {start}. {result.Usage}. Open or cancel?", "Continue, open, or quit?", MessageBoxButtons.YesNoCancel);
                        if (dialogResult != DialogResult.No) //open on cancel
                        {
                            StravaApi.OpenAsStravaWebPage(result.Activity);
                        }

                        if (dialogResult == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    else if(result.Error != null)
                    {
                        if(result.Error.ToLower().Contains("duplicate of "))
                        {
                            DialogResult dialogResult = MessageBox.Show($"We got a duplicate, error {result.Error}. Continue?", "Do More?", MessageBoxButtons.YesNo);
                            if (dialogResult == DialogResult.No)
                            {
                                return;
                            }
                        }
                        else
                        {
                            if(MessageBox.Show($"We got an error {result.Error}. Continue?", "Do More?", MessageBoxButtons.YesNo) == DialogResult.No)
                            {
                                return;
                            }
                        }

                    } 
                    else
                    {
                        if (MessageBox.Show($"No error, but no activity. Huh. Continue?", "Do More?", MessageBoxButtons.YesNo) == DialogResult.No)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    Logging.Instance.Log($"Can't find [{path}]");
                }
            }
        }

        public void ReadFitlogFolder(string folder)
        {
            
            string[] files = Directory.GetFiles(folder, "*.fitlog");

            foreach (string file in files)
            {
                ReadFitlog(file);
            }
        }

        public void ReadFitlog(string filename)
        {
            // Open the XML document and load it
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            // Move to the first node of DOM and get some attributes.
            XmlElement? root = doc.DocumentElement;

            if(root == null )
            {
                stringBuilderResults.AppendLine($"No XML found in {filename}");
                return;
            }


            StringBuilder stringBuilder = new StringBuilder();
            // Loop through child nodes
            foreach (XmlNode curNode in root.ChildNodes)
            {
                if (curNode.Name == "AthleteLog")
                {
                    // Loop through child nodes
                    foreach (XmlNode node in curNode.ChildNodes)
                    {
                        if (node.Name == "Activity")
                        {
                            SportTracksActivity act = new SportTracksActivity();
                            if(act.ParseXML(node, stringBuilder))
                                AllActivitesParsed.Add(act);
                        }
                    }
                }
            }
            stringBuilderResults.Append(stringBuilder.ToString());
        }

        private const string SportsTracksGUID = "SportTracks GUID";
        string FitLogPreamble = """
            <?xml version="1.0" encoding="utf-8"?>
            <FitnessWorkbook xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="http://www.zonefivesoftware.com/xmlschemas/FitnessLogbook/v3">
                <AthleteLog>
            """;

        string FitLogPostamble = """
             </AthleteLog>
            </FitnessWorkbook>
            """;



        string[] RunningCategories = { "Pace", "Intervals", "Easy", "Tempo", "Strides", "Long" };
        string[] ValidCategories = { "biking", "cycling", "running", "hiking", "walking", "swimming" };
        string[] GuidsToSkip = { "6f02d5d0-23c8-4399-9265-8b09fe147d79", "c97ba0d1-28c0-43e0-9e3f-2cda88d8e7bf", "a74b0685-580d-4d4d-b9b5-e9a2a483e1b3", "ccd47de4-641c-4205-9ac6-078f780f6b8d", "961a363b-5f60-4117-979c-8327c433e369", "d1f41890-05fe-4e05-b069-6456d95585f4", "a1795d94-9b10-4d9f-a4a3-6781e04d80ff", "56331c57-4d91-4dff-b0c5-07466eed8900", "fa61f4a9-e25b-4f07-a895-6bfc9b110027", "a9096001-6d2a-4567-80d2-249922f4da55", "b7cf3dba-7e05-45d5-a171-93210e89eaab", "9592f875-2823-4c1f-b56b-7b8e8914a7c8" }; //prior problems, such as activity in strava, but not exported into archive
        public void WriteMissingFitlog(StringBuilder stringBuilder)
        {
            //windows 11 protected folders blocks access to documents folder
            string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string AppDataSubFolder = "FellrnrTrainingData";
            string AppDataPath = Path.Combine(AppDataFolder, AppDataSubFolder);

            string fullpath = Path.Combine(AppDataPath, "MissingWorkouts.fitlog");
            //using (StreamWriter outputFile = new StreamWriter(fullpath))
            //using (Stream stream = new FileStream(fullpath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter stream = new StreamWriter(fullpath))
            using (XmlTextWriter writer = new XmlTextWriter(stream))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteRaw(FitLogPreamble);
                int count = 0;
                foreach (SportTracksActivity act in AllActivitesParsed)
                {
                    //filter out tiny activities, ignore problems
                    if (act.matchingActivity == null && act._xml != null)
                    {
                        XmlNode node = act._xml;
                        bool found = false;

                        string InnerText = $"\nRecreatedFromSportTracks {act._guid}\n ";
                        string sport = act._name;
                        if (RunningCategories.Contains(sport))
                        {
                            InnerText += $" SportTracksSportWas:{sport}\n";
                            sport = "Running";
                        }
                        if (act._totalDistance != act._lastDistance)
                        {
                            InnerText += $" {Tags.START}{Activity.TagDistance}{Tags.MIDDLE}Override{Tags.MIDDLE}{act._totalDistance}{Tags.END} \n";
                        }
                        if (act._totalDuration != act._lastTime && act._lastTime != 0) //only if they're different
                        {
                            InnerText += $" {Tags.START}{Activity.TagElapsedTime}{Tags.MIDDLE}Override{Tags.MIDDLE}{act._totalDuration}{Tags.END} \n";
                        }

                        if (act._name.ToLower().Contains("descent"))
                        {
                            double defaultAngle = -10.0; //this seems to be our normal descent
                            double angle = act._treadmillAngle ?? defaultAngle;
                            InnerText += $" {Tags.START}{Activity.TagTreadmillAngle}{Tags.MIDDLE}Override{Tags.MIDDLE}{defaultAngle}{Tags.END} \n";
                            act._treadmill = true;
                            act._treadmillAngle = angle;
                        }

                        if(act._notes.ToLower().StartsWith("treadmill") && !act._hasGps)
                        {
                            act._treadmill = true;
                        }

                        foreach (XmlNode curNode in node.ChildNodes)
                        {
                            if (curNode.Name == "Notes")
                            {
                                found = true;
                                curNode.InnerText += InnerText;
                            }
                            if (curNode.Name == "Category")
                            {
                                curNode.Attributes!["Name"]!.Value = sport;
                            }
                        }
                        if (!found)
                        {
                            if (node.OwnerDocument != null)
                            {
                                XmlDocument doc = node.OwnerDocument;
                                XmlNode notesNode = doc.CreateNode(XmlNodeType.Element, "Notes", "http://www.zonefivesoftware.com/xmlschemas/FitnessLogbook/v3");
                                notesNode.InnerText = " #RecreatedFromSportTracks";
                                node.AppendChild(notesNode);
                            }
                            else
                            {
                                stringBuilder.AppendLine("Oops, no XmlDocument");
                            }
                        }
                        act._notes += InnerText;
                        act._name = sport;
                        //Activity type is detected from <Activity Sport="*"> where ‘biking’, ‘running’, ‘hiking’, ‘walking’ and ‘swimming’ 
                        if (ValidCategories.Contains(sport.ToLower()) && !GuidsToSkip.Contains(act._guid))
                        {
                            act._xml.WriteTo(writer);
                            ActivitiesToUpload.Add(act);
                            count++;
                        }
                    }
                }
                writer.WriteRaw(FitLogPostamble);
                stringBuilder.AppendLine($"Wrote XML for {count} activities");
            }
        }

        public void Verify(Model.Athlete athlete)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach (SportTracksActivity act in AllActivitesParsed)
            {
                Verify(athlete, act, stringBuilder);
                if(act._startTime > lastInFitlog)
                    lastInFitlog = act._startTime;

                totalCount++;
            }

            foreach (SportTracksActivity act in AllActivitesParsed)
            {
                if(act.matchingActivity != null && act._guid != null)
                {
                    act.matchingActivity.AddOrReplaceDatum(new TypedDatum<string>(SportsTracksGUID, true, act._guid));
                }
            }


            WriteMissingFitlog(stringBuilder); //do this after verify or the activities won't be updated!

            stringBuilder.AppendLine($"Total {totalCount}");
            stringBuilder.AppendLine($"All good                 {goodCount}");
            stringBuilder.AppendLine($"To upload to strava      {ActivitiesToUpload.Count}");
            stringBuilder.AppendLine($"Missing from strava      {missingCount}");
            stringBuilder.AppendLine($"Missing with HR          {missingCountWHR}");
            stringBuilder.AppendLine($"Missing with distance    {missingCountWDis}");
            stringBuilder.AppendLine($"Missing with GPS         {missingCountWGps}");
            stringBuilder.AppendLine($"missingEmptyDay          {missingEmptyDay}");
            stringBuilder.AppendLine($"missingOnlyMatched       {missingOnlyMatched}");
            stringBuilder.AppendLine($"missingUnmatchedExist    {missingUnmatchedExist}");
            stringBuilder.AppendLine($"Distances no match any   {distanceDoesntMatchAtAll}");
            stringBuilder.AppendLine($"Matching last distance   {lastDistanceCount}");
            stringBuilder.AppendLine($"Matching last dist/HR    {badCountWHR}");
            stringBuilder.AppendLine($"Matching last dis/Dist   {badCountWDis}");
            stringBuilder.AppendLine($"Matching last dis/GPS    {badCountWGPS}");
            stringBuilder.AppendLine($"descriptionCount         {descriptionCount}");
            //stringBuilder.AppendLine($"nameCount                {nameCount}");
            stringBuilder.AppendLine($"durationCount            {durationCount}");
            stringBuilder.AppendLine($"No Distance in fitlog    {noDistanceFit}");
            stringBuilder.AppendLine($"Dist in fit, strava zero {noDistanceStrava}");
            stringBuilder.AppendLine($"Missing distance         {Math.Round(distanceMissing / 1000, 0)} Km");
            stringBuilder.AppendLine($"Extra distance           {Math.Round(distanceExtra / 1000, 0)} Km");
            stringBuilder.AppendLine($"Short distance           {Math.Round(distanceShort / 1000, 0)} Km");
            stringBuilder.AppendLine($"Short distance (>10%)    {Math.Round(distanceShort10pc / 1000, 0)} Km");
            stringBuilder.AppendLine();
            //stringBuilder.Append(stringBuilderDeclinesNoGPS.ToString());
            //stringBuilder.AppendLine();
            //stringBuilder.Append(stringBuilderDeclinesGPS.ToString());
            //stringBuilder.AppendLine();
            //stringBuilder.Append(nameProblems.ToString());
            if (UnmatchedDetails)
            {
                stringBuilder.AppendLine();
                stringBuilder.AppendLine("==========Unmatched Descents==========");
                stringBuilder.AppendLine();
                foreach (SportTracksActivity act in AllActivitesParsed)
                {
                    if (act._name.ToLower().Contains("descent") && act.matchingActivity == null)
                    {
                        stringBuilder.AppendLine($"Descent: {act._name}, {act._decline}, Laps {act._hasLaps}, GPS {act._hasGps}, HR {act._hasHR}, Dist {act._hasDistance}");
                    }
                }

                stringBuilder.AppendLine();
                stringBuilder.AppendLine("==========Matched Descents==========");
                stringBuilder.AppendLine();
                foreach (SportTracksActivity act in AllActivitesParsed)
                {
                    if (act._name.ToLower().Contains("descent") && act.matchingActivity != null)
                    {
                        stringBuilder.AppendLine($"Descent: {act._name}, {act._decline}, Laps {act._hasLaps}, GPS {act._hasGps}, HR {act._hasHR}, Dist {act._hasDistance}");
                    }
                }
                stringBuilder.AppendLine($"Last date in fitlog is {lastInFitlog}");
                int counter = 0;
                foreach (KeyValuePair<string, Activity> kvp in athlete.Activities)
                {
                    Activity activity = kvp.Value;
                    string stravaid = kvp.Key;
                    if (activity.StartDateTimeUTC < lastInFitlog && !MatchedStravaIds.Contains(stravaid))
                    {
                        bool foundSurroundingFitlog = false;
                        foreach (SportTracksActivity act in AllActivitesParsed)
                        {
                            DateTime endTime = act._startTime.AddSeconds(act._totalDuration);
                            if (activity.StartDateTimeUTC > act._startTime && activity.StartDateTimeUTC < endTime)
                            {
                                foundSurroundingFitlog = true;
                                stringBuilder.AppendLine($"Found strava activity within fitlog activity [{activity}] {activity.StartDateTimeUTC}/{activity.StartDateTimeLocal}, fitlog {act._startTime}, {act._guid} {act._lapDistance}, {act._lastDistance}, {act._totalDistance} {act._lapCountOver300 * 400}");
                            }
                        }
                        if (!foundSurroundingFitlog)
                        {
                            stringBuilder.AppendLine($"Found strava activity not in fitlog {activity} {activity.StartDateTimeUTC}/{activity.StartDateTimeLocal}");
                            counter++;
                        }
                    }
                }
                stringBuilder.AppendLine($"Strava not in fitlog is {counter}");
            }
            //counter = 0;
            //foreach (KeyValuePair<string, Activity> kvp in athlete.Activities)
            //{
            //    Activity activity = kvp.Value;
            //    string stravaid = kvp.Key;
            //    if (activity.StartDateTimeUTC == null)
            //    {
            //        stringBuilder.AppendLine($"Found strava activity with no start date UTC {activity} {activity.StartDateTimeUTC}/{activity.StartDateTimeLocal}");
            //        counter++;
            //    }
            //    else if (!athlete.ActivitiesByUTCDateTime.ContainsKey(activity.StartDateTimeUTC.Value))
            //    {
            //        stringBuilder.AppendLine($"Found strava activity not in the by UTC dictionary {activity} {activity.StartDateTimeUTC}/{activity.StartDateTimeLocal}");
            //        counter++;
            //    }
            //}
            //stringBuilder.AppendLine($"Strava no start time utc is {counter}");
            stringBuilderResults = stringBuilder;
        }
        DateTime lastInFitlog = DateTime.MinValue;
        List<SportTracksActivity> AllActivitesParsed = new List<SportTracksActivity>();
        List<SportTracksActivity> ActivitiesToUpload = new List<SportTracksActivity>();
        //StringBuilder stringBuilderDeclinesGPS = new StringBuilder();
        //StringBuilder stringBuilderDeclinesNoGPS = new StringBuilder();

        public List<string> MatchedStravaIds = new List<string>();
        public int goodCount = 0;
        public int missingCount = 0;
        public int missingCountWHR = 0;
        public int missingCountWDis = 0;
        public int missingCountWGps = 0;
        public int missingEmptyDay = 0;
        public int missingOnlyMatched = 0;
        public int missingUnmatchedExist = 0;

        public int distanceDoesntMatchAtAll = 0;
        public int noDistanceStrava = 0;
        public int noDistanceFit = 0;
        public int lastDistanceCount = 0;
        public int lastDistanceWithGpsCount = 0;
        public int totalCount = 0;
        public double distanceShort = 0;
        public double distanceShort10pc = 0;
        public double distanceMissing = 0;
        public double distanceExtra = 0;
        public int badCountWHR = 0;
        public int badCountWGPS = 0;
        public int badCountWDis = 0;

        public int descriptionCount = 0;
        //public int nameCount = 0;
        public int durationCount = 0;
        //public StringBuilder nameProblems = new StringBuilder();

        public bool UnmatchedDetails = false;

        private Activity? FindActivity(Model.Athlete athlete, SportTracksActivity act, StringBuilder stringBuilder)
        {
            if (act._guid == "ce91d28f-1ee6-4689-b05c-bde09d0444c9")
            {
                stringBuilder.AppendLine("asefd"); //make debugging easier
            }


            if (athlete.ActivitiesByUTCDateTime.ContainsKey(act._startTime))
            {
                //perfect hit
                Activity activity = athlete.ActivitiesByUTCDateTime[act._startTime];
                string filename = activity.Filename ?? "No File";
                stringBuilder.Append($"Found exact match for {act._startTime} {activity.PrimaryKey()} {filename} {act._name}: ");
                return activity;
            }

            DateTime daylightSavingsProblem = act._startTime.AddHours(-1);
            if (athlete.ActivitiesByUTCDateTime.ContainsKey(daylightSavingsProblem))
            {
                //perfect hit
                Activity activity = athlete.ActivitiesByUTCDateTime[daylightSavingsProblem];
                string filename = activity.Filename ?? "No File";
                stringBuilder.Append($"Found exact match for {act._startTime}/{daylightSavingsProblem} {activity.PrimaryKey()} {filename} {act._name}: ");
                return activity;
            }

            DateTime daylightSavingsProblemPlus = daylightSavingsProblem.AddSeconds(1);
            if (athlete.ActivitiesByUTCDateTime.ContainsKey(daylightSavingsProblemPlus))
            {
                //perfect hit
                Activity activity = athlete.ActivitiesByUTCDateTime[daylightSavingsProblemPlus];
                string filename = activity.Filename ?? "No File";
                stringBuilder.Append($"Found exact match for {act._startTime}/{daylightSavingsProblemPlus} {activity.PrimaryKey()} {filename} {act._name}: ");
                return activity;
            }



            for (int hour = -8; hour <= 8; hour++)
            {
                for (int second = -60; second <= 60; second++)
                {
                    DateTime date = act._startTime;
                    date = date.AddHours(hour).AddSeconds(second);

                    if (athlete.ActivitiesByUTCDateTime.ContainsKey(date))
                    {
                        //near hit
                        Activity activity = athlete.ActivitiesByUTCDateTime[date];
                        if (!MatchedStravaIds.Contains(activity.PrimaryKey())) //not one we've already matched
                        {
                            //fitlog and strava from different devices may be off by up to a minute or so (different start presses)
                            return activity;

                            //float? stravaDuration = activity.GetNamedFloatDatum(Activity.ElapsedTimeTag);

                            //if(WithinMargin(stravaDuration, act._totalDuration, 10.0))
                            //{
                            //    //good enough? Hopefully
                            //    stringBuilder.Append($"Found close match {act._startTime} as {date} {hour}::{second} {activity.PrimaryKey()} declared {s2hms(stravaDuration)}/{s2hms(act._totalDuration)}: ");
                            //    return activity;
                            //}

                            //if (WithinMargin(stravaDuration, act._lastTime, 10.0))
                            //{
                            //    //good enough? Hopefully
                            //    stringBuilder.Append($"Found close match {act._startTime} as {date} {hour}::{second} {activity.PrimaryKey()} last {s2hms(stravaDuration)}/{s2hms(act._lastTime)}: ");
                            //    return activity;
                            //}

                        }
                    }

                }
            }
            return null;
        }

        public void Verify(Model.Athlete athlete, SportTracksActivity act, StringBuilder stringBuilderAddTo)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Activity? activity = FindActivity(athlete, act, stringBuilder);
            bool allgood = true;
            bool showmsg = false;
            if (activity != null)
            {
                MatchedStravaIds.Add(activity.PrimaryKey());
                act.matchingActivity = activity;

                //no name in fitlog, just category
                //string? stravaName = activity.GetNamedStringDatum(Activity.NameTag);
                //if (stravaName == null)
                //{
                //    stringBuilder.Append($"Name Strava Null! Fitlog [{act._name}] ");
                //}
                //else if (stravaName != act._name && stravaName.Replace("Running: ", "") != act._name)
                //{
                //    stringBuilder.Append($"Name Strava [{stravaName}] Fitlog [{act._name}] ");
                //    nameProblems.AppendLine($"Name Strava [{stravaName}] Fitlog [{act._name}] ");
                //    nameCount++;
                //    //allgood = false;
                //}


                string? stravaDescription = activity.GetNamedStringDatum(Activity.TagDescription);
                if (stravaDescription == null && !string.IsNullOrEmpty(act._notes))
                {
                    stringBuilder.Append($"Description Strava Null! Fitlog [{act._notes}] ");
                }
                else if (stravaDescription != null && stravaDescription.ReplaceLineEndings() != act._notes.ReplaceLineEndings()) //some cr/lf differences
                {
                    stringBuilder.Append($"Description Strava [{stravaDescription}] Fitlog [{act._notes}] ");
                    descriptionCount++;
                    //allgood = false;
                }


                //moving or ElapsedTimeTag
                float? stravaDuration = activity.GetNamedFloatDatum(Activity.TagElapsedTime);
                if (!WithinMargin(stravaDuration, act._totalDuration, 60.0))
                {
                    if (WithinMargin(stravaDuration, act._lastTime, 60.0))
                    {
                        stringBuilder.Append($"Duration matches lastTime Strava {stravaDuration}/{s2hms(stravaDuration)} Fitlog {act._totalDuration}/{s2hms(act._totalDuration)} last {act._lastTime}/{s2hms(act._lastTime)} {stravaDuration-act._lastTime} ");
                    }
                    else
                    {
                        stringBuilder.Append($"Duration Strava {stravaDuration}/{s2hms(stravaDuration)} Fitlog {act._totalDuration}/{s2hms(act._totalDuration)} {stravaDuration - act._totalDuration} ");
                    }
                    durationCount++;
                    allgood = false;
                }


                float? stravaDistance = activity.GetNamedFloatDatum(Activity.TagDistance);
                if (act._totalDistance == 0)
                {
                    noDistanceFit++;
                }
                else if (stravaDistance == null || stravaDistance == 0)
                {
                    if (act._totalDistance != act._lastDistance)
                    {
                        stringBuilder.Append($"Distance Strava missing, Fitlog declared {act._totalDistance} Fitlog last track {act._lastDistance} ");
                    }
                    else
                    {
                        stringBuilder.Append($"Distance Strava missing, Fitlog declared/last {act._totalDistance} ");
                    }
                    distanceMissing += act._totalDistance;
                    noDistanceStrava++;
                }
                else
                {
                    if(!WithinMargin(stravaDistance, act._totalDistance, 500.0)) //500 meters
                    {
                        if(!WithinMargin(stravaDistance, act._lastDistance, 500.0))
                        {
                            stringBuilder.Append($"No distance matches Strava {stravaDistance} Fitlog declared {act._totalDistance} Fitlog last track {act._lastDistance} ");
                            distanceDoesntMatchAtAll++;
                            allgood = false;
                        }
                        else
                        {
                            stringBuilder.Append($"Distance Strava matches last track not declared {stravaDistance} Fitlog declared {act._totalDistance} Fitlog last track {act._lastDistance} ");
                            lastDistanceCount++;

                            if (act._hasHR)
                                badCountWHR++;
                            if (act._hasGps)
                                badCountWGPS++;
                            if (act._hasDistance)
                                badCountWDis++;

                        }
                        if (act._totalDistance > stravaDistance)
                        {
                            if (act._totalDistance > stravaDistance * 1.1)
                            {
                                distanceShort10pc += act._totalDistance;
                            }
                            distanceShort += act._totalDistance;
                        }
                        else
                        {
                            distanceExtra += act._totalDistance;
                        }
                    }
                }
            }
            else
            {
                allgood = false;
                if(UnmatchedDetails)
                    showmsg = true;
                missingCount++;
                if (act._hasHR)
                    missingCountWHR++;
                if (act._hasDistance)
                    missingCountWDis++;
                if (act._hasGps)
                    missingCountWGps++;

                distanceMissing += act._totalDistance;
                DateTime date = act._startTime.Date;
                if (athlete.Days.ContainsKey(date) && athlete.Days[date].Activities != null && athlete.Days[date].Activities!.Count() > 0)
                {
                    stringBuilder.AppendLine($"No activity for {act._startTime} {act._guid} {act._name} but found: ");
                    Model.Day day = athlete.Days[date];
                    bool allmatched = true;
                    foreach (Activity activitySearch in day.Activities!)
                    {
                        string stravaName = activitySearch.GetNamedStringDatum(Activity.TagName) ?? "NO NAME";

                        if (!MatchedStravaIds.Contains(activitySearch.PrimaryKey()))
                        {
                            float? stravaDuration = activitySearch.GetNamedFloatDatum(Activity.TagElapsedTime);

                            stringBuilder.AppendLine($"    strava {activitySearch.PrimaryKey()} {activitySearch.StartDateTimeLocal} {stravaName}");
                            allmatched = false;
                        }
                    }
                    if (allmatched)
                    {
                        stringBuilder.AppendLine("    only matching activities");
                        missingOnlyMatched++;
                    }
                    else
                    { 
                        missingUnmatchedExist++;
                    }
                }
                else
                {
                    missingEmptyDay++;
                    stringBuilder.Append($"No activities on day for {act._startTime} {act._guid} [{act._name}] declared {act._totalDistance} last track {act._lastDistance} ");
                }
            }

            if (showmsg) { stringBuilderAddTo.AppendLine(stringBuilder.ToString()); } 
            if(allgood) { goodCount++; }
            ExtractDecline(act);
        }

        private void ExtractDecline(SportTracksActivity act)
        {
            string notes = act._notes;
            //string key = "% decline";
            string key = "%";
            StringBuilder stringBuilder = new StringBuilder();
            while (notes.ToLower().Contains(key))
            {
                int keypos = notes.ToLower().IndexOf(key);
                string upto = notes.Substring(0, keypos);
                int lastspace = upto.LastIndexOf(' ');
                string number = upto.Substring(lastspace + 1);
                number = number.Replace("~", "");

                //int margin = 15;
                //int margin2 = margin * 2;
                //int offset = keypos - margin;
                //int start = Math.Max(0, offset);
                //int len = Math.Min(margin2, notes.Length - start);
                //string surrounding = notes.Substring(start, len);

                if (int.TryParse(number, out int percent))
                {
                    //Console.WriteLine(percent);
                    percent = Math.Abs(percent);
                    if (percent > 3 && percent < 20) //limits of treadmill descents
                    {
                        act._treadmillAngle = 0.0 - percent;
                        act._decline += $"Decline of {percent}% [{act._name}] has GPS {act._guid} {act._startTime} {act._notes} ";
                    }
                }
                notes = notes.Substring(keypos + key.Length);
            }
        }


        private StringBuilder stringBuilderResults = new StringBuilder();
        public string Results { get { return stringBuilderResults.ToString(); } }


        public class SportTracksActivity : MyBase
        {
            static string[] IgnoreGuids = { "cafee3c6-882e-45f1-b471-44e537907126" };
            static string[] IgnoreNames = { "Intermittent Hypoxia", "Aerobics", "My Activities" };

            public string? _guid;
            public double _totalDuration = 0;  // seconds
            public double _totalDistance = 0;  // meters
            public double _totalCalories = 0;
            public DateTime _startTime;
            public DateTime _trackStartTime;
            public bool _hasLaps = false;
            public bool _hasTracks = false;
            public bool _hasHR = false;
            public bool _hasCadence = false;
            public bool _hasGps = false;
            public bool _hasDistance = false;
            public string _name = "";
            public string _notes = "";
            public double _lastDistance = 0;
            public double _lastTime = 0;
            public double _lapDistance = 0;
            public int _lapCountOver300 = 0;
            public string _xmlAsText = "";
            public XmlNode? _xml;
            public Activity? matchingActivity = null;
            private List<Lap> _laps = new List<Lap>();
            public string _decline = "";
            public double? _treadmillAngle = null;
            public bool _treadmill = false;
            /*
     <Activity StartTime="2015-03-09T21:00:00Z" Id="5914f59b-fbfb-44ca-8331-d3dc90d30cdf">
       <Metadata Source="" Created="2015-03-09T21:53:31Z" Modified="2015-03-10T14:08:41Z" />
       <Duration TotalSeconds="2610" />
       <Distance TotalMeters="9334.2" />
       <Notes>Perfect timing! This is my last major treadmill descent before Umstead, and I watched the final episode of The Shield. It was great TV, up there with The Wire and Breaking Bad. It's kept me sane, or close to sane, though the long hours on the treadmill. I wonder if I can remember how to run on the flat? </Notes>
       <Category Id="3786fd69-c816-4044-adc2-28580af179f2" Name="Treadmill Descent" />
      </Activity>
            */

            public bool ParseXML(XmlNode node, StringBuilder stringBuilder)
            {
                if (node.Attributes == null) { stringBuilder.AppendLine("No Attributes"); return false; }
                if (node.Attributes["StartTime"] == null) { stringBuilder.AppendLine("No StartTime"); return false; }
                _startTime = DateTime.Parse(node.Attributes["StartTime"]!.Value);
                //_startTime = DateTime.SpecifyKind(_startTime, DateTimeKind.Unspecified);
                _startTime = _startTime.AddMilliseconds(-_startTime.Millisecond); //get rid of milliseconds or they won't match
                if (node.Attributes["Id"] == null) { stringBuilder.AppendLine("No Id"); return false; }
                //_guid = new Guid(node.Attributes["Id"]!.Value);
                _guid = node.Attributes["Id"]!.Value;
                _xmlAsText = node.OuterXml;
                _xml = node;
                // Loop through child nodes
                foreach (XmlNode curNode in node.ChildNodes)
                {
                    if (curNode.Attributes == null) { stringBuilder.AppendLine("No Attributes in curNode"); return false; }
                    switch (curNode.Name)
                    {
                        case "Notes":
                            if (curNode.InnerText == null) { stringBuilder.AppendLine("No Name Value"); return false; }
                            _notes = curNode.InnerText;
                            break;
                        case "Duration":
                            if (curNode.Attributes["TotalSeconds"] == null) { stringBuilder.AppendLine("No TotalSeconds"); return false; }
                            _totalDuration = ParseDoubleAttribute(curNode.Attributes["TotalSeconds"]!);
                            break;
                        case "Distance":
                            if (curNode.Attributes["TotalMeters"] == null) { stringBuilder.AppendLine("No TotalMeters"); return false; }
                            _totalDistance = ParseDoubleAttribute(curNode.Attributes["TotalMeters"]!);
                            break;
                        case "Calories":
                            if (curNode.Attributes["TotalCal"] == null) { stringBuilder.AppendLine("No TotalCal"); return false; }
                            _totalCalories = ParseDoubleAttribute(curNode.Attributes["TotalCal"]!);
                            break;
                        case "Category":
                            if (curNode.Attributes["Name"] == null) { stringBuilder.AppendLine("No Name"); return false; }
                            _name = curNode.Attributes!["Name"]!.Value;
                            break;
                        case "Track":
                            {
                                if (curNode.Attributes["StartTime"] == null) { stringBuilder.AppendLine("No StartTime"); return false; }
                                _trackStartTime = DateTime.Parse(curNode.Attributes["StartTime"]!.Value);
                                _hasTracks = true;

                                foreach (XmlNode tpNode in curNode.ChildNodes)
                                {
                                    if (tpNode.Attributes != null && tpNode.Attributes["dist"] != null)
                                    {
                                        _lastDistance = ParseDoubleAttribute(tpNode.Attributes["dist"]!);
                                    }
                                    if (tpNode.Attributes != null && tpNode.Attributes["tm"] != null)
                                    {
                                        _lastTime = ParseDoubleAttribute(tpNode.Attributes["tm"]!);
                                    }
                                    if (tpNode.Attributes != null && tpNode.Attributes["hr"] != null)
                                    {
                                        _hasHR = true;
                                    }
                                    if (tpNode.Attributes != null && tpNode.Attributes["cadence"] != null)
                                    {
                                        _hasCadence = true;
                                    }
                                    if (tpNode.Attributes != null && tpNode.Attributes["lat"] != null)
                                    {
                                        _hasGps = true;
                                    }
                                }
                                break;
                            }
                        case "Laps":
                            {
                                _hasLaps = true;
                                //foreach (XmlNode lapNode in curNode.ChildNodes)
                                //{
                                //    Lap lap = new Lap();
                                //    lap.ParseXML(lapNode);
                                //    _laps.Add(lap);
                                //    _lapDistance += lap._distance;
                                //    if (lap._distance > 300)
                                //        _lapCountOver300++;
                                //}
                                break;
                            }
                        case "DistanceMarkers":
                            {
                                _hasDistance = true;
                                //int markerCount = 1;
                                //foreach (XmlNode markerNode in curNode.ChildNodes)
                                //{
                                //    if (markerNode.Name == "Marker")
                                //    {
                                //        double distance = ParseDoubleAttribute(markerNode.Attributes["dist"]);
                                //        _laps[markerCount]._startDistance = distance;
                                //    }
                                //}
                                break;
                            }
                    }
                }
                //Debug.WriteLine("Distance: " + _totalDistance.ToString() + " m");



                //don't load crap
                if ((_totalDuration == 0 || _totalDuration > 900) &&
                    !IgnoreGuids.Contains(_guid) &&
                    !IgnoreNames.Contains(_name))
                {
                    return true;
                }
                return false;
            }
        }

        public class Lap : MyBase
        {
            public DateTime _startTime;
            public double _duration = 0;
            public double _calories = 0;
            public double _startDistance = 0;
            public double _distance = 0;
            public void ParseXML(XmlNode node)
            {
                _startTime = DateTime.Parse(node.Attributes!["StartTime"]!.Value);
                _duration = ParseDoubleAttribute(node.Attributes["DurationSeconds"]!);

                foreach (XmlNode calNode in node.ChildNodes)
                {
                    if (calNode.Name == "Distance")
                    {
                        _distance = ParseDoubleAttribute(calNode.Attributes!["TotalMeters"]!);
                    }
                    if (calNode.Name == "Calories")
                    {
                        _calories = ParseDoubleAttribute(calNode.Attributes!["TotalCal"]!);
                    }
                }
            }
        }
        public class MyBase
        {
            protected double ParseDoubleAttribute(XmlAttribute attr)
            {
                if (attr == null)
                    return 0;
                return double.Parse(attr.Value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            protected long ParseLongAttribute(XmlAttribute attr)
            {
                if (attr == null)
                    return 0;
                return long.Parse(attr.Value);
            }

            protected void AddDateAttribute(XmlElement elem, string name, DateTime date)
            {
                elem.SetAttribute(name, date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"));
            }

            protected void AddDateTime(XmlDocument doc, XmlElement parent, string element, DateTime value)
            {
                string tmp = value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
                AddString(doc, parent, element, tmp);
            }

            protected void AddLong(XmlDocument doc, XmlElement parent, string element, long value)
            {
                string tmp = value.ToString();
                AddString(doc, parent, element, tmp);
            }

            protected void AddDouble(XmlDocument doc, XmlElement parent, string element, double value)
            {
                string tmp = value.ToString();
                AddString(doc, parent, element, tmp);
            }

            protected void AddString(XmlDocument doc, XmlElement parent, string element, string value)
            {
                XmlElement childNode = doc.CreateElement(element);
                if (value.Length > 0)
                    childNode.InnerText = value;
                parent.AppendChild(childNode);
            }
        }

        private string s2hms(double? secs)
        {
            if (secs == null) { return "null"; }
            TimeSpan t = TimeSpan.FromSeconds(secs!.Value);

            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);

            return answer;
        }

        private bool WithinMargin(float? test1, double test2, double margin)
        {
            float test1a = test1 != null ? test1.Value : 0;

            double upper = test1a + margin;
            double lower = test1a - margin;
            return (test2 <= upper && test2 >= lower);
        }

        /*

Details of activities found in strava, but not in fitlog

Deleted GPS Data from 8b60fbd2-2032-491a-8985-10c54802e381 (it's a treadmill run with junk GPS. Also clarified the percents that weren't the decents.

Last date in fitlog is 31/10/2017 10:10:41
DELETED - Found strava activity not in fitlog Start 21/08/2015 21:54:51 key 377082250 filename activities/425268105.fit.gz name Afternoon Run 21/08/2015 21:54:51/21/08/2015 21:54:51

RECREATED Found strava activity within fitlog activity Start 24/08/2015 13:16:07 key 377082445 filename activities/425268277.fit.gz name Morning Run 24/08/2015 13:16:07/24/08/2015 13:16:07, fitlog 24/08/2015 12:02:14, c8f3beae-2c92-46c9-a214-8b704f80cfaf

DELETED - Found strava activity not in fitlog Start 27/08/2015 14:03:59 key 379204094 filename activities/427499334.fit.gz name Morning Run 27/08/2015 14:03:59/27/08/2015 14:03:59

THIS IS A PAIR:
RECREATED Found strava activity within fitlog activity Start 02/09/2015 12:33:54 key 383255314 filename activities/431761752.fit.gz name Morning Run 02/09/2015 12:33:54/02/09/2015 12:33:54, fitlog 02/09/2015 12:08:30, d7e9e551-ec17-4c8b-8e40-a03088f5759c
RECREATED Found strava activity within fitlog activity Start 02/09/2015 13:06:18 key 383255397 filename activities/431761825.fit.gz name Morning Run 02/09/2015 13:06:18/02/09/2015 13:06:18, fitlog 02/09/2015 12:08:30, d7e9e551-ec17-4c8b-8e40-a03088f5759c

PAIR THAT OVERLAPS WITH ANOTHER STRAVA ACTIVITY (DOUBLE UPLOAD) DELETED
Found strava activity not in fitlog Start 07/09/2015 11:21:13 key 388449103 filename activities/437160902.fit.gz name Morning Run 07/09/2015 11:21:13/07/09/2015 11:21:13
Found strava activity not in fitlog Start 07/09/2015 12:40:18 key 388449225 filename activities/437161039.fit.gz name Morning Run 07/09/2015 12:40:18/07/09/2015 12:40:18

NO EXPLAINATION - NOTHING MATCHES, BUT LOOKS REAL
Found strava activity not in fitlog Start 12/09/2015 11:26:18 key 392147404 filename activities/441006309.fit.gz name Morning Run 12/09/2015 11:26:18/12/09/2015 11:26:18
Found strava activity not in fitlog Start 01/06/2016 09:11:37 key 594977958 filename activities/649705372.gpx.gz name Morning Run 01/06/2016 09:11:37/01/06/2016 09:11:37
Found strava activity not in fitlog Start 15/09/2016 15:45:37 key 720351482 filename activities/793684548.gpx.gz name Lunch Run 15/09/2016 15:45:37/15/09/2016 15:45:37

DUPLICATE - DELETED
Found strava activity not in fitlog Start 16/09/2015 20:57:14 key 393743665 filename activities/442651627.fit.gz name Afternoon Run 16/09/2015 20:57:14/16/09/2015 20:57:14

JUNK RUN - TINY DISTANCE/TIME
Found strava activity not in fitlog Start 04/12/2015 10:49:34 key 452834348 filename activities/503739117.fit.gz name Morning Run 04/12/2015 10:49:34/04/12/2015 10:49:34
Found strava activity not in fitlog Start 24/06/2016 20:52:18 key 623915804 filename activities/679871985.gpx.gz name Afternoon Run 24/06/2016 20:52:18/24/06/2016 20:52:18
Found strava activity not in fitlog Start 20/05/2016 13:09:51 key 582056502 filename activities/636372266.gpx.gz name Morning Run 20/05/2016 13:09:51/20/05/2016 13:09:51

DELETED AND RECREATED FROM FITLOG
Found strava activity within fitlog activity Start 18/12/2015 14:06:09 key 452834440 filename activities/503739197.fit.gz name Morning Run 18/12/2015 14:06:09/18/12/2015 14:06:09, fitlog 18/12/2015 13:21:41, e50ccc81-94c2-40bc-8fed-6ea785d63a14
Found strava activity not in fitlog Start 18/12/2015 16:15:03 key 452834488 filename activities/503739257.fit.gz name Lunch Run 18/12/2015 16:15:03/18/12/2015 16:15:03


NEAR MATCH, RECREATED FOR CLARITY
Found strava activity not in fitlog Start 21/05/2016 11:31:14 key 587569133 filename activities/642063374.gpx.gz name Morning Run 21/05/2016 11:31:14/21/05/2016 11:31:14
Found strava activity not in fitlog Start 27/05/2016 12:04:30 key 594977702 filename activities/649705104.gpx.gz name Morning Run 27/05/2016 12:04:30/27/05/2016 12:04:30
Found strava activity not in fitlog Start 28/05/2016 13:01:02 key 594977728 filename activities/649705145.gpx.gz name Morning Run 28/05/2016 13:01:02/28/05/2016 13:01:02
Found strava activity not in fitlog Start 20/09/2016 12:20:07 key 720351411 filename activities/793684461.gpx.gz name Morning Run 20/09/2016 12:20:07/20/09/2016 12:20:07

Strip GPS
Found strava activity not in fitlog Start 02/08/2015 11:50:46 key 2365367641 filename activities/2516060587.fit.gz name Running 02/08/2015 11:50:46/02/08/2015 11:50:46

TZ PROBLEMS - ACTUALLY ON THE SECOND - DELETED
Found strava activity not in fitlog Start 03/03/2015 14:50:00 key 7889933358 filename  name Treadmill Descent 03/03/2015 14:50:00/03/03/2015 14:50:00

Manually created activity, deleted
Found strava activity not in fitlog Start 02/11/2009 07:40:00 key 9703568178 filename  name Pacing City of Oaks marathon 02/11/2009 07:40:00/02/11/2009 07:40:00

Manually created, no original record
Found strava activity not in fitlog Start 31/03/2007 05:00:00 key 10583325862 filename  name Umstead 100 #race 31/03/2007 05:00:00/31/03/2007 05:00:00
Found strava activity not in fitlog Start 20/10/2002 05:00:00 key 10588813547 filename  name Chesapeake Bay Bridge Marathon #race 20/10/2002 05:00:00/20/10/2002 05:00:00
Found strava activity within fitlog activity Start 19/10/2003 13:40:00 key 10588843021 filename  name Chesapeake Bay Bridge Marathon 9th overall #race 19/10/2003 13:40:00/19/10/2003 13:40:00, fitlog 19/10/2003 12:00:02, fd869919-de5f-47b3-b608-88fea11786c0

        //note 23/07/2013 12:21:52 has GPS from the garage treadmill run
        //all other GPS runs are not treadmill descents

         */

    }
}
