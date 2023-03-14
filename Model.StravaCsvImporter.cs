using CsvHelper;
using de.schumacher_bw.Strava.Endpoint;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FellrnrTrainingAnalysis.Model.ActivityDatumMapping;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FellrnrTrainingAnalysis.Model
{
    public class StravaCsvImporter
    {
        public StravaCsvImporter() { }

        public int LoadFromStravaArchive(string profileCsvPath, Database database)
        {
            LoadAthletesFromStravaArchive(profileCsvPath, database);

            int count = LoadActivitiesFromStravaArchive(profileCsvPath, database);
            
            return count;
        }

        //TODO make the load async with a progress bar
        private void LoadAthletesFromStravaArchive(string profileCsvPath, Database database)
        {
            List<Dictionary<string, Datum>> spreadsheetOfAthletes = ImportFromCSV(profileCsvPath, ActivityDatumMapping.LevelType.Athlete);
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
            List<Dictionary<string, Datum>> spreadsheetOfActivites = ImportFromCSV(activitiesCsvPath, ActivityDatumMapping.LevelType.Activity);
            List<Activity> activities = new List<Activity>();  
            foreach (Dictionary<string, Datum> activityRow in spreadsheetOfActivites)
            {
                DateTime expectedStartDateTime = Activity.ExpectedStartDateTime(activityRow);
                if (Utils.Options.Instance.OnlyLoadAfter != null && Utils.Options.Instance.OnlyLoadAfter >= expectedStartDateTime)
                {
                    Logging.Instance.Log(string.Format("Activity is at {0} and OnlyLoadAfter is {1}", expectedStartDateTime, Utils.Options.Instance.OnlyLoadAfter));
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
            Model.FitReader.ClearSummaryErrors(); //HACK diagnostics using static variables
            foreach (Activity activity in activities)
            {
                FitReader fitReader = new FitReader(activity, Path.GetDirectoryName(profileCsvPath)!); //high confidence this will not return null

                fitReader.ReadFitFromStravaArchive();
            }


            Model.FitReader.SummaryErrors();//HACK diagnostics using static variables
            Logging.Instance.Log(string.Format("\r\nCurrent athlete has {0} field names\r\n", database.CurrentAthlete.TimeSeriesNames.Count));
            foreach (string s in database.CurrentAthlete.TimeSeriesNames)
                Logging.Instance.Log(string.Format("Time Series Name[{0}]", s));
            Logging.Instance.Log("Load complete");
            return activities.Count;
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
                            ActivityDatumMapping? activityDatumMapping = ActivityDatumMapping.MapRecord(DataSourceEnum.StravaCSV, levelType, s);
                            if (activityDatumMapping != null && activityDatumMapping.Import && !row.ContainsKey(s))//Strava CSV has duplicate columns
                            {
                                if (activityDatumMapping.DataType == DataTypeEnum.String)
                                {
                                    ImportDatum(activityDatumMapping.InternalName, ActivityDatumMapping.DataSourceEnum.StravaCSV, levelType, csv[s]!, row);
                                }
                                if (activityDatumMapping.DataType == DataTypeEnum.Float)
                                {
                                    float floatValue;
                                    if (float.TryParse(csv[s], out floatValue))
                                    {
                                        floatValue *= activityDatumMapping.ScalingFactor;
                                        ImportDatum(activityDatumMapping.InternalName, ActivityDatumMapping.DataSourceEnum.StravaCSV, levelType, floatValue, row);
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
                                        ImportDatum(activityDatumMapping.InternalName, ActivityDatumMapping.DataSourceEnum.StravaCSV, levelType, DateTimeValue, row);
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
