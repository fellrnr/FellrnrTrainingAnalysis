using de.schumacher_bw.Strava;
using de.schumacher_bw.Strava.Model;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FellrnrTrainingAnalysis.Model.ActivityDatumMapping;
using static System.Formats.Asn1.AsnWriter;

namespace FellrnrTrainingAnalysis.Model
{

    //test with curl on cmd.exe (powershell curl is different)
    //curl -G https://www.strava.com/api/v3/athlete -H "Authorization: Bearer c64beaa9b84fe03bcc295e027bd39f8b1ef2a542"
    //bearer token is "Your Access Token" on the strava API page - check it's not expired
    public class StravaApi
    {
        public static StravaApi Instance { get; set; }

        private const string stravaAuthFilename = "stravaApi.json";
        private StravaApiV3Sharp StravaApiV3Sharp { get; set; }

        //private Scopes DesiredScope { get; } = Scopes.Read | Scopes.ReadAll | Scopes.ActivityWrite;
        private Scopes DesiredScope { get; } = Scopes.ActivityReadAll;

        private StravaApi()
        {
            string? serializedApi = System.IO.File.Exists(stravaAuthFilename) ? System.IO.File.ReadAllText(stravaAuthFilename) : null;

            // create an instance of the api and reload the local stored auth info
            StravaApiV3Sharp = new StravaApiV3Sharp(Utils.Options.Instance.ClientId, Utils.Options.Instance.ClientSecret, serializedApi);

            // add a delegate to the event of the refreshToken or authToken been updated
            StravaApiV3Sharp.SerializedObjectChanged += (s, e) => System.IO.File.WriteAllText(stravaAuthFilename, StravaApiV3Sharp.Serialize());
        }

        static StravaApi()
        {
            Instance = new StravaApi();
        }

        public bool IsConnected
        {
            get 
            {
                Scopes currentScope = StravaApiV3Sharp.Authentication.Scope;
                return (currentScope == DesiredScope); 
            }
        }

        public void Connect()
        {
            string callbackUrl = "http://localhost/doesNotExist/";
            StravaAuthorizeForm stravaAuthorizeForm = new StravaAuthorizeForm(callbackUrl);
            stravaAuthorizeForm.Navigate(StravaApiV3Sharp.Authentication.GetAuthUrl(new Uri(callbackUrl), DesiredScope).ToString());
            stravaAuthorizeForm.ShowDialog();
            if (!string.IsNullOrEmpty(stravaAuthorizeForm.FinalUrl))
            {
                StravaApiV3Sharp.Authentication.DoTokenExchange(new Uri(stravaAuthorizeForm.FinalUrl)); // do the token exchange with the stava api
                var athlete = StravaApiV3Sharp.Athletes.GetLoggedInAthlete();
                MessageBox.Show(String.Format("Hello {0}", athlete.Firstname));
            }
        }

        public int SyncNewActivites(Database database)
        {
            if (database == null || database.CurrentAthlete == null)
            {
                Logging.Instance.Debug(string.Format("Database or CurrentAthlete is null"));
                return 0;
            }
            DateTime? onlyAfter = null;
            if(database !=null && database.CurrentAthlete != null && database.CurrentAthlete.CalendarTree != null && database.CurrentAthlete.CalendarTree.Count > 0) 
            {
                onlyAfter = database.CurrentAthlete.GetLatestActivityDateTime(); //This will also refresh the last day's activities we have
                ////HACK: get the next activity
                //if(onlyAfter != null)
                //    onlyAfter = onlyAfter.Value.AddDays(1);
            }
            if (onlyAfter == null && Options.Instance.OnlyLoadAfter != null)
                onlyAfter = Options.Instance.OnlyLoadAfter;

            SummaryActivity[] newActivities = StravaApiV3Sharp.Activities.GetLoggedInAthleteActivities(null, onlyAfter);
            int counter = 0;
            foreach (SummaryActivity stravaActivity in newActivities)
            {
                if(stravaActivity.Id == null)
                {
                    Logging.Instance.Error(string.Format("Retrieved activity from strava with null id"));
                    //ActivityDataFromProperties(stravaActivity); //if there's no id, there's not much we can do with the activity data
                } 
                else
                {
                    DetailedActivity detailedActivity = StravaApiV3Sharp.Activities.GetActivityById((long)stravaActivity.Id);
                    Logging.Instance.Debug(string.Format("\r\n\r\n>>>DetailedActivity\r\n\r\n"));
                    Dictionary<string, Datum> activityDataFromProperties = ActivityDataFromProperties(detailedActivity);
                    Activity? activity = database!.CurrentAthlete!.AddOrUpdateActivity(activityDataFromProperties);

                    if(activity != null)
                    {
                        StreamSet streamSet = StravaApiV3Sharp.Streams.GetActivityStreams((long)stravaActivity.Id, (StreamTypes)Options.Instance.StravaStreamTypesToRetrieve);
                        AddDataStreams(activity, streamSet);

                        List<Uri>? photos = detailedActivity.Photos?.Primary?.Urls?.Values?.ToList(); //TODO: Photos from Strava API is only returning two resolutions of one photo
                        activity.PhotoUris= photos;

                    }
                    counter++;
                    if (counter >= 10)
                        return counter;

                }
            }
            return counter;
        }

        //Easier to add streams directly rather than trying reflection 
        private void AddDataStreams(Activity activity, StreamSet streamSet)
        {
            int[] timeInt = streamSet.Time.Data;
            uint[] time = Array.ConvertAll(timeInt, x => (uint)x);

            if (time.Length == 0)
                return;

            uint previous = time[0];
            uint largestGap = 0;
            uint timerPauses = 0;
            int totalPauseCount= 0;
            for(int i=0; i<time.Length; i++)
            {
                uint t = time[i];
                uint gap = t - previous;
                if(gap > Options.Instance.StravaMaximumGap)
                {
                    //we have to guess where the timer pauses are, so if there's a big gap, we'll cut it out. Typically we see a data point every 1-2 seconds. 
                    timerPauses += gap - 1; //turn the gap into a one second gap
                    totalPauseCount++;
                }
                if (timerPauses > 0)
                    time[i] = t - timerPauses;
                if (gap > largestGap)
                {
                    largestGap = gap;
                }
                previous = t;

            }
            Logging.Instance.Debug(string.Format("       >>>> Total pause length {0}, total pauses {1}, largest time gap is {2} seconds", timerPauses, totalPauseCount, largestGap));


            //Time = 1,
            //Distance = 2,
            //Latlng = 4,
            //Altitude = 8,
            //VelocitySmooth = 16,
            //Heartrate = 32,
            //Cadence = 64,
            //Watts = 128,
            //Temp = 256,
            //Moving = 512,
            //GradeSmooth = 1024,

            if (streamSet.Heartrate != null)
                AddDataStream(activity, "Heartrate", time, streamSet.Heartrate.Data);
            if (streamSet.Distance != null)
                AddDataStream(activity, "Distance", time, streamSet.Distance.Data);
            //if (streamSet.Latlng != null)
            //  AddDataStream(activity, "Latlng", time, streamSet.Latlng.Data); //TODO: handle LatLong from stravaAPI
            if (streamSet.Altitude != null)
                AddDataStream(activity, "Altitude", time, streamSet.Altitude.Data);
            if (streamSet.VelocitySmooth != null)
                AddDataStream(activity, "VelocitySmooth", time, streamSet.VelocitySmooth.Data);
            if (streamSet.Cadence != null)
                AddDataStream(activity, "Cadence", time, streamSet.Cadence.Data);
            if (streamSet.Watts != null)
                AddDataStream(activity, "Watts", time, streamSet.Watts.Data);
            if (streamSet.Temp != null)
                AddDataStream(activity, "Temp", time, streamSet.Temp.Data);
            //if (streamSet.Moving != null)
            //  AddDataStream(activity, "Moving", time, streamSet.Moving.Data); //TODO: handle boolean moving from strava API
            if (streamSet.GradeSmooth != null)
                AddDataStream(activity, "GradeSmooth", time, streamSet.GradeSmooth.Data);

        }

        private void AddDataStream(Activity activity, string name, uint[] time, int[] data)
        {
            if (data == null)
                return; 
            float[] values = Array.ConvertAll(data, x => (float)x);
            AddDataStream(activity, name, time, values);
        }
        private void AddDataStream(Activity activity, string name, uint[] time, float[] data)
        {
            ActivityDatumMapping? activityDatumMapping = ActivityDatumMapping.MapRecord(DataSourceEnum.StravaAPI, LevelType.DataStream, name);
            if (activityDatumMapping != null && activityDatumMapping.Import)
            {
                if (!activity.TimeSeries.ContainsKey(activityDatumMapping.InternalName) || Options.Instance.StravaApiOveridesData)
                {
                    activity.AddDataStream(activityDatumMapping.InternalName, time, data);
                }
            }


        }
        private Dictionary<string, Datum> ActivityDataFromProperties(object stravaActivity)
        {
            Dictionary<string, Datum> activityData = new Dictionary<string, Datum>();
            Type type = stravaActivity.GetType();
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            Logging.Instance.Debug(string.Format("Type is: {0}, underlying {1}", type.Name, underlyingType.Name ));
            PropertyInfo[] props = type.GetProperties();
            Logging.Instance.Debug(string.Format("Properties (N = {0}):", props.Length));
            foreach (var prop in props)
            {
                var underlyingPropertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if(prop == null)
                {
                    Logging.Instance.Debug(string.Format("   got a null property"));
                }
                else if (prop.GetIndexParameters().Length == 0)
                {
                    Logging.Instance.Debug(string.Format("   {0} ({1} underlying {3}): {2}", prop.Name, prop.PropertyType.Name, prop.GetValue(stravaActivity), underlyingPropertyType));

                    string fieldName = prop.Name;
                    object propertyValue = prop.GetValue(stravaActivity)!;
                    ActivityDatumMapping? activityDatumMapping = ActivityDatumMapping.MapRecord(DataSourceEnum.StravaAPI, LevelType.Activity, fieldName);
                    if (propertyValue == null)
                    {
                        Logging.Instance.Debug(string.Format("       propertyValue is null"));
                    }
                    else if (activityDatumMapping == null)
                    {
                        Logging.Instance.Debug(string.Format("       activityDataMapping is null"));
                    }
                    else if (!activityDatumMapping.Import)
                    {
                        Logging.Instance.Debug(string.Format("       import is false"));
                    }
                    else if (activityData.ContainsKey(fieldName))//ignore duplicate columns
                    {
                        Logging.Instance.Debug(string.Format("       duplicate column (ignored)"));
                    }
                    else
                    {
                        Logging.Instance.Debug(string.Format("   Mapping data type {0}, external name {1}, internal name {2}, import {3}",
                            activityDatumMapping.DataType, activityDatumMapping.ExternalName, activityDatumMapping.InternalName, activityDatumMapping.Import));

                        switch (activityDatumMapping.DataType)
                        {
                            case DataTypeEnum.String:
                                activityData.Add(activityDatumMapping.InternalName, new TypedDatum<string>(activityDatumMapping.InternalName, true, propertyValue.ToString()!));
                                break;
                            //case DataTypeEnum.LongInteger:
                            //    if (underlyingPropertyType == typeof(Int64))
                            //    {
                            //        activityData.Add(activityDatumMapping.InternalName, new TypedDatum<Int64>(activityDatumMapping.InternalName, true,(Int64)propertyValue));
                            //    }
                            //    break;
                            case DataTypeEnum.DateTime:
                                if(underlyingPropertyType == typeof(DateTime))
                                {
                                    activityData.Add(activityDatumMapping.InternalName, new TypedDatum<DateTime>(activityDatumMapping.InternalName, true, (DateTime)propertyValue));
                                }
                                break;
                            case DataTypeEnum.Float:
                                if (underlyingPropertyType == typeof(float))
                                {
                                    float floatValue = (float)propertyValue;
                                    floatValue *= activityDatumMapping.ScalingFactor;
                                    activityData.Add(activityDatumMapping.InternalName, new TypedDatum<float>(activityDatumMapping.InternalName, true, floatValue));
                                }
                                else if (underlyingPropertyType == typeof(double))
                                {
                                    double doubleValue = (double)propertyValue;
                                    doubleValue *= activityDatumMapping.ScalingFactor;
                                    activityData.Add(activityDatumMapping.InternalName, new TypedDatum<float>(activityDatumMapping.InternalName, true, (float)doubleValue));
                                }
                                break;
                            case DataTypeEnum.TimeSpan:
                                if (underlyingPropertyType == typeof(TimeSpan))
                                {
                                    TimeSpan timeSpan= (TimeSpan)propertyValue;
                                    float timeInSeconds = (float)timeSpan.TotalSeconds;
                                    activityData.Add(activityDatumMapping.InternalName, new TypedDatum<float>(activityDatumMapping.InternalName, true, timeInSeconds));
                                }
                                break;
                        }
                    }
                }
                else
                {
                    Logging.Instance.Debug(string.Format("   {0} ({1}): <Indexed>", prop.Name, prop.PropertyType.Name));
                }
            }
            return activityData;
        }
    }
}
