using CsvHelper;
using System.Diagnostics.Contracts;
using System.Globalization;
using FellrnrTrainingAnalysis.Utils;
using static FellrnrTrainingAnalysis.Model.ActivityDatumMapping;
using FellrnrTrainingAnalysis.Model;

namespace FellrnrTrainingAnalysis.Action
{
    public class StravaCsvImporter
    {
        public StravaCsvImporter() { }


        int CountFitFiles = 0;
        int CountGpxFiles = 0;
        int CountBadFiles = 0;
        int CountActivitiesWithoutFilename = 0;

        public int LoadFromStravaArchive(string profileCsvPath, Database database)
        {
            LoadAthletesFromStravaArchive(profileCsvPath, database);

            int count = LoadActivitiesFromStravaArchive(profileCsvPath, database);
            
            return count;
        }

        //TODO make the load async with a progress bar
        private void LoadAthletesFromStravaArchive(string profileCsvPath, Database database)
        {
            List<Dictionary<string, Datum>> spreadsheetOfAthletes = ImportFromCSV(profileCsvPath, LevelType.Athlete);
            foreach (Dictionary<string, Datum> athleteRow in spreadsheetOfAthletes)
            {
                if (athleteRow.ContainsKey(Athlete.AthleteIdTag)) //TODO: Give error on missing primary kay
                {
                    Athlete athlete = database.FindOrCreateAthlete(athleteRow[Athlete.AthleteIdTag].ToString()!);
                    foreach (KeyValuePair<string, Datum> kvp in athleteRow)
                    {
                        athlete.AddOrReplaceDatum(kvp.Value);
                    }
                }
            }
            Contract.Ensures(database.Athletes.Count > 0);
        }
        private int LoadActivitiesFromStravaArchive(string profileCsvPath, Database database)
        {
            string activitiesCsvPath = Path.GetDirectoryName(profileCsvPath) + "\\activities.csv";


            //load the activities from the CSV
            List<Dictionary<string, Datum>> spreadsheetOfActivites = ImportFromCSV(activitiesCsvPath, LevelType.Activity);
            List<Activity> activities = new List<Activity>();  
            foreach (Dictionary<string, Datum> activityRow in spreadsheetOfActivites)
            {
                DateTime expectedStartDateTime = Activity.ExpectedStartDateTime(activityRow);
                if (Options.Instance.OnlyLoadAfter != null && Options.Instance.OnlyLoadAfter >= expectedStartDateTime)
                {
                    Logging.Instance.Log(string.Format("Activity is at {0} and OnlyLoadAfter is {1}", expectedStartDateTime, Options.Instance.OnlyLoadAfter));
                }
                else
                {
                    Activity? activity = database.CurrentAthlete.AddOrUpdateActivity(activityRow);
                    if (activity != null)
                    {
                        activities.Add(activity);
                    }
                }
            }

            //load the detailed activity data (from FIT, etc.)
            Logging.Instance.Log(string.Format("Total of {0} activities to process", activities.Count));
            FitReader.ClearSummaryErrors(); //HACK diagnostics using static variables
            foreach (Activity activity in activities)
            {
                LoadFromFile(activity, profileCsvPath);
            }


            if (CountActivitiesWithoutFilename > 0 || CountBadFiles > 0)
            {
                Logging.Instance.Error($"A total of {CountActivitiesWithoutFilename} activities without a filename");
                Logging.Instance.Error($"A total of {CountFitFiles} activities that are FIT files");
                Logging.Instance.Error($"A total of {CountGpxFiles} activities that are GPX files");
                Logging.Instance.Error($"A total of {CountBadFiles} activities that are not known file types");
            }

            FitReader.SummaryErrors();//HACK diagnostics using static variables
            Logging.Instance.Log(string.Format("\r\nCurrent athlete has {0} field names\r\n", database.CurrentAthlete.TimeSeriesNames.Count));
            foreach (string s in database.CurrentAthlete.TimeSeriesNames)
                Logging.Instance.Log(string.Format("Time Series Name[{0}]", s));
            Logging.Instance.Log("Load complete");
            return activities.Count;
        }

        private void LoadFromFile(Activity activity, string profileCsvPath)
        {
            System.DateTime? activityDateTime = activity.StartDateTime;
            if (activityDateTime != null && Options.Instance.OnlyLoadAfter != null && Options.Instance.OnlyLoadAfter >= activityDateTime)
            {
                Logging.Instance.Log(string.Format("Activity is at {0} and OnlyLoadAfter is {1}", activityDateTime, Options.Instance.OnlyLoadAfter));
                return;
            }


            string? filenameFromDatum = activity.Filename;
            if (filenameFromDatum == null)
            {
                Logging.Instance.Debug("Activity does not have a filename");
                CountActivitiesWithoutFilename++;
                return;
            }

            string filepath;
            if (activity.FileFullPath == null)
            {
                string profilePath = Path.GetDirectoryName(profileCsvPath)!;
                string filepart = (string)filenameFromDatum;
                filepath = profilePath + '\\' + filepart;
                activity.FileFullPath = filepath;
            }
            else
            {
                filepath = activity.FileFullPath;
            }

            if (filepath.ToLower().EndsWith(".fit") || filepath.ToLower().EndsWith(".fit.gz"))
            {
                FitReader fitReader = new FitReader(activity);

                fitReader.ReadFitFromStravaArchive();
                CountFitFiles++;
            }
            else if (filepath.ToLower().EndsWith(".gpx") || filepath.ToLower().EndsWith(".gpx.gz"))
            {
                GpxProcessor gpxProcessor = new GpxProcessor(activity);

                gpxProcessor.ProcessGpx();
                CountGpxFiles++;
            }
            else
            { 
                if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                    Logging.Instance.Debug("Activity file is not FIT " + filepath);
                CountBadFiles++;
                return;
            }

        }


        public List<Dictionary<string, Datum>> ImportFromCSV(string csvPath, LevelType levelType)
        {
            List<Dictionary<string, Datum>> spreadsheet= new List<Dictionary<string, Datum>>();

            using (var reader = new StreamReader(File.OpenRead(csvPath)))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Read();
                csv.ReadHeader();
                string[]? headerRow = csv.HeaderRecord;
                if (headerRow == null)
                {
                    throw new Exception("CSV does not contain a header row:" + csvPath);
                }
                while (csv.Read())
                {
                    Dictionary<string, Datum> row = new Dictionary<string, Datum>();
                    spreadsheet.Add(row);
                    foreach (string s in headerRow)
                    {
                        if (!string.IsNullOrEmpty(csv[s]))
                        {
                            ActivityDatumMapping? activityDatumMapping = MapRecord(DataSourceEnum.StravaCSV, levelType, s);
                            if (activityDatumMapping != null && activityDatumMapping.Import && !row.ContainsKey(s))//Strava CSV has duplicate columns
                            {
                                if (activityDatumMapping.DataType == DataTypeEnum.String)
                                {
                                    ImportDatum(activityDatumMapping.InternalName, DataSourceEnum.StravaCSV, levelType, csv[s]!, row);
                                }
                                if (activityDatumMapping.DataType == DataTypeEnum.Float)
                                {
                                    float floatValue;
                                    if (float.TryParse(csv[s], out floatValue))
                                    {
                                        floatValue *= activityDatumMapping.ScalingFactor;
                                        ImportDatum(activityDatumMapping.InternalName, DataSourceEnum.StravaCSV, levelType, floatValue, row);
                                    }
                                }
                                //if (activityDatumMapping.DataType == DataTypeEnum.LongInteger)
                                //{
                                //    Int64 intValue;
                                //    if (Int64.TryParse(csv[s], out intValue))
                                //    {
                                //        ImportDatum(activityDatumMapping.InternalName, ActivityDatumMapping.DataSourceEnum.StravaCSV, levelType, intValue, row);
                                //    }
                                //}
                                if (activityDatumMapping.DataType == DataTypeEnum.DateTime)
                                {
                                    DateTime DateTimeValue;
                                    if (DateTime.TryParse(csv[s], out DateTimeValue))
                                    {
                                        ImportDatum(activityDatumMapping.InternalName, DataSourceEnum.StravaCSV, levelType, DateTimeValue, row);
                                    }
                                }
                            }
                        }
                    }
                }
                return spreadsheet;
            }
        }

        public void ImportDatum<T>(string name, ActivityDatumMapping.DataSourceEnum source, ActivityDatumMapping.LevelType level, T data, Dictionary<string, Datum> datumDictonary)
        {
            TypedDatum<T> datum = new TypedDatum<T>(name, true, data);
            if (!datumDictonary.ContainsKey(name))
            {
                datumDictonary.Add(name, datum);
            }
            datumDictonary[name] = datum;
        }


    }
}
