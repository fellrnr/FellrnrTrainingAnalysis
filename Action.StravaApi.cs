using de.schumacher_bw.Strava;
using de.schumacher_bw.Strava.Endpoint;
using de.schumacher_bw.Strava.Model;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using System.Reflection;
using static FellrnrTrainingAnalysis.Model.ActivityDatumMapping;

namespace FellrnrTrainingAnalysis.Action
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
        private Scopes DesiredScope { get; } = Scopes.ActivityReadAll | Scopes.ActivityWrite;

        private StravaApi()
        {
            string? serializedApi = File.Exists(stravaAuthFilename) ? File.ReadAllText(stravaAuthFilename) : null;

            // create an instance of the api and reload the local stored auth info
            StravaApiV3Sharp = new StravaApiV3Sharp(Options.Instance.ClientId, Options.Instance.ClientSecret, serializedApi);

            // add a delegate to the event of the refreshToken or authToken been updated
            StravaApiV3Sharp.SerializedObjectChanged += (s, e) => File.WriteAllText(stravaAuthFilename, StravaApiV3Sharp.Serialize());
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
            UI.StravaAuthorizeForm stravaAuthorizeForm = new UI.StravaAuthorizeForm(callbackUrl);
            stravaAuthorizeForm.Navigate(StravaApiV3Sharp.Authentication.GetAuthUrl(new Uri(callbackUrl), DesiredScope).ToString());
            stravaAuthorizeForm.ShowDialog();
            if (!string.IsNullOrEmpty(stravaAuthorizeForm.FinalUrl))
            {
                StravaApiV3Sharp.Authentication.DoTokenExchange(new Uri(stravaAuthorizeForm.FinalUrl)); // do the token exchange with the stava api
                var athlete = StravaApiV3Sharp.Athletes.GetLoggedInAthlete();
                MessageBox.Show(String.Format("Hello {0}", athlete.Firstname));
            }
        }

        //returns count processed and count left
        public Tuple<int, int> SyncNewActivites(Database database)
        {
            if (database == null || database.CurrentAthlete == null)
            {
                Logging.Instance.Debug(string.Format("Database or CurrentAthlete is null"));
                return new Tuple<int,int>(0,0);
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
            if(newActivities == null)
            {
                return new Tuple<int, int>(-1, -1);
            }
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
                    GetActivityFromStrava(database!, stravaActivity.Id.Value);
                    counter++;
                    if (counter >= 10)
                        return new Tuple<int, int>(counter, newActivities.Length - counter);

                }
            }
            return new Tuple<int, int>(counter, 0);
        }

        public void RefreshActivity(Database database, Activity activity)
        {
            long key = long.Parse(activity.PrimaryKey());
            GetActivityFromStrava(database, key);
        }

        private Activity? GetActivityFromStrava(Database database, long stravaId)
        {
            DetailedActivity detailedActivity = StravaApiV3Sharp.Activities.GetActivityById(stravaId);
            if(Options.Instance.DebugStravaAPI)
                Logging.Instance.Debug(string.Format("\r\n\r\n>>>DetailedActivity\r\n\r\n"));
            Dictionary<string, Datum> activityDataFromProperties = ActivityDataFromProperties(detailedActivity);
            Activity? activity = database!.CurrentAthlete!.InitialAddOrUpdateActivity(activityDataFromProperties);


            if (activity != null)
            {
                database!.CurrentAthlete!.FinalizeAdd(activity);

                //we had a null error on this call that went away the next morning. The activity had zeros in power, and seemed to be related to the power stream
                //https://www.strava.com/activities/9398565924
                //also see "Testing Strava API.docx" in OneDrive 
                StreamSet streamSet = StravaApiV3Sharp.Streams.GetActivityStreams(stravaId, (StreamTypes)Options.Instance.StravaStreamTypesToRetrieve); ;
                //StreamTypes.Time | StreamTypes.Distance | StreamTypes.Cadence | StreamTypes.Temp);
                //StreamTypes.Time | StreamTypes.Distance | StreamTypes.Cadence | StreamTypes.VelocitySmooth | StreamTypes.Watts | StreamTypes.Temp);
                //StreamTypes.Time | StreamTypes.Distance | StreamTypes.Latlng | StreamTypes.Altitude | StreamTypes.Cadence | StreamTypes.VelocitySmooth | StreamTypes.Watts | StreamTypes.Temp );
                //(StreamTypes)Options.Instance.StravaStreamTypesToRetrieve); ;
                AddTimeSeriess(activity, streamSet);

                List<Uri>? photos = detailedActivity.Photos?.Primary?.Urls?.Values?.ToList(); //TODO: Photos from Strava API is only returning two resolutions of one photo
                activity.PhotoUris = photos;
            }
            return activity;
        }

        public bool UpdateActivityDetails(Activity activity, string? name = null, string? description = null)
        {

            //public DetailedActivity UpdateActivityById(long id, UpdatableActivity data = null)

            UpdatableActivity updatableActivity = new UpdatableActivity();
            //if these are null, then they won't update the Strava record
            if (description != null)
            {
                updatableActivity.Description = description;
            }
            if (name != null)
            {
                updatableActivity.Name = name;
            }
            long stravaId;
            if(long.TryParse(activity.PrimaryKey(), out stravaId))
            {
                try
                {
                    DetailedActivity detailedActivity = StravaApiV3Sharp.Activities.UpdateActivityById(stravaId, updatableActivity);


                    if (name != null)
                    {
                        if (detailedActivity.Name != updatableActivity.Name)
                            return false;
                        TypedDatum<string> typedDatum = new TypedDatum<string>("Name", true, name);
                        activity.AddOrReplaceDatum(typedDatum);
                    }
                    if (description != null)
                    {
                        if (detailedActivity.Description != updatableActivity.Description)
                            return false;
                        TypedDatum<string> typedDatum = new TypedDatum<string>(Activity.TagDescription, true, description);
                        activity.AddOrReplaceDatum(typedDatum);
                    }
                    return true;
                }
                catch (Exception) { return false; }

            }
            return false;
        }

        public class UploadResult
        {
            public string? Error;
            public Activity? Activity;
            public string Usage = "";
        }
        public UploadResult UploadActivityFromFit(Database database, FileInfo file, string? name, string? description, bool? trainer)
        {
            Upload upload = StravaApiV3Sharp.Uploads.CreateUpload(file, DataType.Fit, name, description, trainer, false);
            UploadResult result = new UploadResult();

            result.Usage = $"Used {StravaApiV3Sharp.Limit15Minutes.Usage}/{StravaApiV3Sharp.Limit15Minutes.Limit} & {StravaApiV3Sharp.LimitDaily.Usage}/{StravaApiV3Sharp.LimitDaily.Limit}";

            if (upload.Error != null)
            {
                Logging.Instance.Error($"Upload failed on initial send with {upload.Error}, {upload.Status}");
                result.Error = upload.Error;
                return result;
            }

            if (upload.Id == null)
            {
                Logging.Instance.Error($"Upload with no id, {upload.Status}");
                result.Error = "No Id returned";
                return result;
            }
            long uploadId = upload.Id.Value;
            int sleep = 5 * 1000;
            int timeout = 10;
            int counter = 0;
            while( counter < timeout && upload.Error == null && upload.ActivityId == null)
            {
                Thread.Sleep(sleep);
                Logging.Instance.Debug($"Polling upload status {counter}");
                upload = StravaApiV3Sharp.Uploads.GetUploadById(uploadId);
                counter++;
            }

            if (upload.Error != null)
            {
                Logging.Instance.Error($"Upload failed on polling with {upload.Error}, {upload.Status}");
                result.Error = upload.Error;
                return result;
            }
            if (upload.ActivityId == null)
            {
                if (counter == timeout)
                {
                    Logging.Instance.Error($"Upload failed with timeout, {upload.Status}");
                    result.Error = "Timeout occured";
                }
                else
                {
                    Logging.Instance.Error($"Upload failed with no activity id, {upload.Status}");
                    result.Error = "No Activity Id returned";
                }
                return result;
            }

            Activity? activity = GetActivityFromStrava(database, upload.ActivityId.Value);
            if (activity == null)
            {
                Logging.Instance.Error($"Upload failed with no activity id, {upload.Status}");
                result.Error = "No Activity Id returned";
                return result;
            }
            else
            {
                Logging.Instance.Log($"Upload of activity id {upload.ActivityId} successful {upload.Status}");
                result.Error = null;
                result.Activity = activity;
                return result;
            }
        }

        //Easier to add streams directly rather than trying reflection 
        private void AddTimeSeriess(Activity activity, StreamSet streamSet)
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
                AddTimeSeries(activity, "Heartrate", time, streamSet.Heartrate.Data);
            if (streamSet.Distance != null)
                AddTimeSeries(activity, "Distance", time, streamSet.Distance.Data);
            if (streamSet.Latlng != null)
                AddTimeSeries(activity, time, streamSet.Latlng.Data); 
            if (streamSet.Altitude != null)
                AddTimeSeries(activity, "Altitude", time, streamSet.Altitude.Data);
            if (streamSet.VelocitySmooth != null)
                AddTimeSeries(activity, "VelocitySmooth", time, streamSet.VelocitySmooth.Data);
            if (streamSet.Cadence != null)
                AddTimeSeries(activity, "Cadence", time, streamSet.Cadence.Data);
            if (streamSet.Watts != null)
                AddTimeSeries(activity, "Watts", time, streamSet.Watts.Data);
            if (streamSet.Temp != null)
                AddTimeSeries(activity, "Temp", time, streamSet.Temp.Data);
            //if (streamSet.Moving != null)
            //  AddTimeSeries(activity, "Moving", time, streamSet.Moving.Data); //TODO: handle boolean moving from strava API
            if (streamSet.GradeSmooth != null)
                AddTimeSeries(activity, "GradeSmooth", time, streamSet.GradeSmooth.Data);

        }

        private void AddTimeSeries(Activity activity, string name, uint[] time, int[] data)
        {
            if (data == null)
                return; 
            float[] values = Array.ConvertAll(data, x => (float)x);
            if (values.Min() == 0 && values.Max() == 0)
            {
                Logging.Instance.Log($"Not adding data stream {name} as it is all zeros {activity.ToString()}");
            }
            else
            {
                AddTimeSeries(activity, name, time, values);
            }
        }
        private void AddTimeSeries(Activity activity, string name, uint[] time, float[] data)
        {
            ActivityDatumMapping? activityDatumMapping = MapRecord(DataSourceEnum.StravaAPI, LevelType.TimeSeries, name);
            if (activityDatumMapping != null && activityDatumMapping.Import)
            {
                if (!activity.TimeSeries.ContainsKey(activityDatumMapping.InternalName) || Options.Instance.StravaApiOveridesData)
                {
                    activity.AddTimeSeries(activityDatumMapping.InternalName, time, data);
                }
            }
        }

        private void AddTimeSeries(Activity activity, uint[] time, LatLng[] data)
        {
            List<uint> LocationTimes = new List<uint>();
            List<float> LocationLats = new List<float>();
            List<float> LocationLons = new List<float>();

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i].Lat != null && data[i].Lng != null)
                {
                    LocationTimes.Add(time[i]);
                    LocationLats.Add((float)data[i].Lat!);
                    LocationLons.Add((float)data[i].Lng!);
                }
            }

            LocationStream locationStream = new LocationStream(LocationTimes.ToArray(), LocationLats.ToArray(), LocationLons.ToArray());
            activity.LocationStream = locationStream;
        }

        private Dictionary<string, Datum> ActivityDataFromProperties(object stravaActivity)
        {
            Dictionary<string, Datum> activityData = new Dictionary<string, Datum>();
            Type type = stravaActivity.GetType();
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (Options.Instance.DebugStravaAPI)
                Logging.Instance.Debug(string.Format("Type is: {0}, underlying {1}", type.Name, underlyingType.Name ));
            PropertyInfo[] props = type.GetProperties();
            if (Options.Instance.DebugStravaAPI)
                Logging.Instance.Debug(string.Format("Properties (N = {0}):", props.Length));
            foreach (var prop in props)
            {
                var underlyingPropertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if(prop == null)
                {
                    if (Options.Instance.DebugStravaAPI)
                        Logging.Instance.Debug(string.Format("   got a null property"));
                }
                else if (prop.GetIndexParameters().Length == 0)
                {
                    if (Options.Instance.DebugStravaAPI)
                        Logging.Instance.Debug(string.Format("   {0} ({1} underlying {3}): {2}", prop.Name, prop.PropertyType.Name, prop.GetValue(stravaActivity), underlyingPropertyType));

                    string fieldName = prop.Name;
                    object propertyValue = prop.GetValue(stravaActivity)!;
                    ActivityDatumMapping? activityDatumMapping = MapRecord(DataSourceEnum.StravaAPI, LevelType.Activity, fieldName);
                    if (propertyValue == null)
                    {
                        if (Options.Instance.DebugStravaAPI) Logging.Instance.Debug(string.Format("       propertyValue is null"));
                    }
                    else if (activityDatumMapping == null)
                    {
                        if (Options.Instance.DebugStravaAPI) Logging.Instance.Debug(string.Format("       activityDataMapping is null"));
                    }
                    else if (!activityDatumMapping.Import)
                    {
                        if (Options.Instance.DebugStravaAPI) Logging.Instance.Debug(string.Format("       import is false"));
                    }
                    else if (activityData.ContainsKey(fieldName))//ignore duplicate columns
                    {
                        if (Options.Instance.DebugStravaAPI) Logging.Instance.Debug(string.Format("       duplicate column (ignored)"));
                    }
                    else
                    {
                        if (Options.Instance.DebugStravaAPI) 
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
                            case DataTypeEnum.WorkoutFlags:
                                //RunRace = 1,
                                //RunLongRun = 2,
                                //RunTraining = 3,
                                //RideRace = 11,
                                //RideTraining = 12
                                if (underlyingPropertyType == typeof(WorkoutType))
                                {
                                    WorkoutType workoutType = (WorkoutType)propertyValue;
                                    string flags = "";
                                    if (workoutType == WorkoutType.RunLongRun)
                                        flags = "Long Run";
                                    else if (workoutType == WorkoutType.RunRace)
                                        flags = "Race";
                                    else if (workoutType == WorkoutType.RunTraining)
                                        flags = "Training";
                                    else if (workoutType == WorkoutType.RideRace)
                                        flags = "Race";
                                    else if (workoutType == WorkoutType.RideTraining)
                                        flags = "Training";

                                    activityData.Add(activityDatumMapping.InternalName, new TypedDatum<string>(activityDatumMapping.InternalName, true, flags));
                                }
                                break;
                        }
                    }
                }
                else
                {
                    if (Options.Instance.DebugStravaAPI) Logging.Instance.Debug(string.Format("   {0} ({1}): <Indexed>", prop.Name, prop.PropertyType.Name));
                }
            }
            return activityData;
        }

        public static void OpenAsStravaWebPage(Activity activity)
        {
            string key = activity.PrimaryKey();

            string target = "https://www.strava.com/activities/" + key;
            Misc.RunCommand(target);
        }

    }
}
