using Dynastream.Fit;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.Action
{
    public class FitReader
    {

        public FitReader(FellrnrTrainingAnalysis.Model.Activity activity, string path)
        {
            Calculation = new CalculationData(path);
            Accumulation = new AccumulationData(activity);
            //Let's get the start time from the fit file, rather than the CSV
            Calculation.activityStartFallback = activity.StartDateTimeUTC;
        }

        public FitReader(FellrnrTrainingAnalysis.Model.Activity activity)
        {
            Calculation = new CalculationData();
            Accumulation = new AccumulationData(activity);
            Calculation.activityStartFallback = activity.StartDateTimeUTC;
        }

        //The things we know about the activity
        private class AccumulationData
        {
            public AccumulationData(FellrnrTrainingAnalysis.Model.Activity activity) { this.activity = activity; }
            public FellrnrTrainingAnalysis.Model.Activity activity;

            public Dictionary<string, KeyValuePair<List<uint>, List<float>>> timeSeriesSet = new Dictionary<string, KeyValuePair<List<uint>, List<float>>>();
            public Dictionary<string, uint> lastElapsedTime = new Dictionary<string, uint>();

            public List<uint> LocationTimes = new List<uint>();
            public List<float> LocationLats = new List<float>();
            public List<float> LocationLons = new List<float>();

            public static SortedDictionary<string, int> fieldCounts = new SortedDictionary<string, int>();
        }

        //put all the variables for ongoing calculations that don't get emitted in a seperate class for clarity
        private class CalculationData
        {
            public CalculationData(string path) { this.Path = path; }
            public CalculationData() { this.Path = ""; }
            public double totalStopped = 0;
            public System.DateTime lastStop;
            public System.DateTime? activityStartUTC = null;
            public System.DateTime? activityStartLocal = null;
            public System.DateTime? activityStartFallback = null;
            public bool IsRunning = true; //We seem to get a start message a few seconds after the recording starts, so lets assume we're already running
            public string Path;
            public bool debugMode = false;
            //TODO: debug specific FIT file, using public string debugFile = "";  and then when that file is processed, output debug data

            public System.Diagnostics.Stopwatch Overall = new System.Diagnostics.Stopwatch();
            public System.Diagnostics.Stopwatch Read = new System.Diagnostics.Stopwatch();
            public System.Diagnostics.Stopwatch Other = new System.Diagnostics.Stopwatch();
            public System.Diagnostics.Stopwatch InRecord = new System.Diagnostics.Stopwatch();
            public System.Diagnostics.Stopwatch Decompress = new System.Diagnostics.Stopwatch();
            public static int Processed = 0;
        }
        private CalculationData Calculation { get; }
        private AccumulationData Accumulation { get; }

        public static void ClearSummaryErrors()
        {
            AccumulationData.fieldCounts.Clear();
        }
        public static void SummaryErrors() //For optimization, only output the summary of the errors or the log file becomes too verbose
        {
            foreach (KeyValuePair<string, int> kvp in AccumulationData.fieldCounts)
                Logging.Instance.Debug(string.Format("Found key {0}, count {1}", kvp.Key, kvp.Value));
        }

        public void ReadFitFromStravaArchive(bool reload = false)
        {
            Calculation.Overall.Start();
            CalculationData.Processed++;

            if (Accumulation.activity.FileFullPath == null)
                return;

            string filepath = Accumulation.activity.FileFullPath;
            if (!System.IO.File.Exists(filepath))
            {
                throw new Exception("File " + filepath + " not found");
            }

            //using (FileStream fitSource = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (Stream fitSource = Misc.DecompressAndOpenFile(filepath))
            {
                Decode fitDecoder = CreateAndLinkDecoder();

                bool isFit = fitDecoder.IsFIT(fitSource);
                bool hasIntegrity = fitDecoder.CheckIntegrity(fitSource);
                // Process the file
                if (isFit && hasIntegrity)
                {
                    Calculation.Read.Start();
                    fitDecoder.Read(fitSource);
                    Calculation.Read.Stop();
                }
                else
                {
                    Logging.Instance.Error("FIT File Integrity Check Failed on " + filepath);
                }
                fitSource.Close();

            }

            Accumulation.activity.AddTimeSeriesSet(Accumulation.timeSeriesSet, reload);

            if (Accumulation.LocationTimes.Count > 0)
                Accumulation.activity.LocationStream = new LocationStream(Accumulation.LocationTimes.ToArray(), Accumulation.LocationLats.ToArray(), Accumulation.LocationLons.ToArray());

            if (Calculation.activityStartLocal != null)
            {
                Accumulation.activity.StartDateTimeLocal = Calculation.activityStartLocal;
            }
            else
            {
                Logging.Instance.Error($"No local time for activity {Accumulation.activity}, using UTC {Calculation.activityStartUTC}");
                Accumulation.activity.StartDateTimeLocal = Calculation.activityStartUTC; //last ditch fallback
            }
            Accumulation.activity.StartDateTimeUTC = Calculation.activityStartUTC;

            Calculation.Overall.Stop();



            FileInfo fi = new FileInfo(filepath);
            long size = fi.Length / 1024;
            double relativeTime = Calculation.Overall.Elapsed.TotalMicroseconds / fi.Length;
            if (Options.Instance.DebugFitPerformance)
                Logging.Instance.Log(String.Format("{4}: {5} Kb, {6:f1} us/Kb, {0} Finished in {3:f1}s, Read {7:f1}, Record {9:f1}, Other {8:f1}, reading {1} from {2} ",
                    System.DateTime.Now, filepath, Accumulation.activity.StartDateTimeLocal, Calculation.Overall.Elapsed.TotalSeconds, CalculationData.Processed, size, relativeTime,
                    Calculation.Read.Elapsed.TotalSeconds, Calculation.Other.Elapsed.TotalSeconds, Calculation.InRecord.Elapsed.TotalSeconds));
            if (Calculation.Overall.Elapsed.TotalSeconds > 10)
            {
                Logging.Instance.Error(String.Format(">Slow> {4}: {5} Kb, {6:f1} us/Kb, {0} Finished in {3:f1}s, Read {7:f1}, Record {9:f1}, Other {8:f1}, reading {1} from {2} ",
                    System.DateTime.Now, filepath, Accumulation.activity.StartDateTimeLocal, Calculation.Overall.Elapsed.TotalSeconds, CalculationData.Processed, size, relativeTime,
                    Calculation.Read.Elapsed.TotalSeconds, Calculation.Other.Elapsed.TotalSeconds, Calculation.InRecord.Elapsed.TotalSeconds));
            }
        }

        private void OnRecordMesgEvent(object sender, MesgEventArgs e)
        {
            if (Calculation.activityStartUTC == null) //this happens occasionally, where there's a couple of data points recorded before the start
            {
                if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                    Logging.Instance.Debug($"Received OnRecordMesgEvent before we have a start time, using fallback. {e.mesg.Num}, {e.mesg.Name}");
                Calculation.activityStartUTC = Calculation.activityStartFallback;
            }
            Calculation.InRecord.Start();

            if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                Logging.Instance.Debug(String.Format("RecordMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            RecordMesg myRecordMesg = (RecordMesg)e.mesg;


            var timestampUint = myRecordMesg.GetFieldValue(RecordMesg.FieldDefNum.Timestamp);
            Dynastream.Fit.DateTime dt = new Dynastream.Fit.DateTime((uint)timestampUint);
            System.DateTime recordTime = dt.GetDateTime();
            System.DateTime activityStart = (System.DateTime)Calculation.activityStartUTC!;
            uint elapsedTime = (uint)((recordTime - activityStart).TotalSeconds - Calculation.totalStopped);
            if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                Logging.Instance.Debug(String.Format("RecordMesgEvent: elapsedTime {0}, recordTime {1} activityStart {2}, totalStopped {3}",
                    elapsedTime, recordTime, Calculation.activityStartUTC, Calculation.totalStopped));


            foreach (var field in myRecordMesg.Fields)
            {
                if (field.Num == RecordMesg.FieldDefNum.Timestamp)
                    continue; //handled above

                ProcessFitRecordField(recordTime, elapsedTime, field);
            }

            foreach (var field in myRecordMesg.DeveloperFields)
            {
                ProcessFitRecordField(recordTime, elapsedTime, field);
            }

            //handle lat long specially
            ProcessFitPosition(myRecordMesg, elapsedTime);


            Calculation.InRecord.Stop();
        }

        //Convert Lat/Long 
        //
        //it seems Garmin stores its angular coordinates using a 32-bit integer, so that gives 2^32 possible values. We want to be
        //able to represent values up to 360° (or -180 to 180), so each degree represents 2^32 / 360 = 11930465.
        //
        //For the latitudes and longitudes, you can divide the numbers by 11930465 (2^32 / 360) to get values in decimal degrees. The
        //values seem to be stored in a signed 32-bit integer range, to represent the full range of geographic coordinate values possible.
        //
        //From https://gis.stackexchange.com/questions/371656/garmin-fit-coordinate-system

        private void ProcessFitPosition(RecordMesg myRecordMesg, uint elapsedTime)
        {
            int? latSemicircles = null;
            int? lonSemicircles = null;

            //note: Lat Long are Sint32 with the units "semicircles"

            foreach (var field in myRecordMesg.Fields)
            {
                if (field.Num == RecordMesg.FieldDefNum.PositionLat)
                    latSemicircles = (int)field.GetValue();
                if (field.Num == RecordMesg.FieldDefNum.PositionLong)
                    lonSemicircles = (int)field.GetValue();
            }
            if (latSemicircles == null || lonSemicircles == null)
                return;

            float lat = (float)latSemicircles / 11930465.0f;
            float lon = (float)lonSemicircles / 11930465.0f;

            Accumulation.LocationTimes.Add(elapsedTime);
            Accumulation.LocationLats.Add(lat);
            Accumulation.LocationLons.Add(lon);
        }

        private void ProcessFitRecordField(System.DateTime recordTime, uint elapsedTime, Field? field)
        {
            if (field == null || field!.GetName() == null)
            {
                return;
            }
            ProcessFitRecordField(recordTime, elapsedTime, field, field.Num); //Num is not on FieldBase, but duplicated on Field and DeveloperField
        }
        private void ProcessFitRecordField(System.DateTime recordTime, uint elapsedTime, DeveloperField? field)
        {
            if (field == null || field!.GetName() == null)
            {
                return;
            }
            ProcessFitRecordField(recordTime, elapsedTime, field, field.Num); //Num is not on FieldBase, but duplicated on Field and DeveloperField
        }

        private void ProcessFitRecordField(System.DateTime recordTime, uint elapsedTime, FieldBase? field, byte fieldNum)
        {
            string fieldName = field!.GetName().ToString();
            if (Options.Instance.DebugFitFields)
            {
                if (!AccumulationData.fieldCounts.ContainsKey(fieldName))
                    AccumulationData.fieldCounts.Add(fieldName, 0);
                AccumulationData.fieldCounts[fieldName]++;
            }

            if (fieldName.ToLower() == "unknown")
            {
                if (Options.Instance.ImportUnknownFitFields)
                {
                    fieldName = fieldName + "_" + fieldNum;
                }
                else
                {
                    return;
                }
            }
            ActivityDatumMapping? activityDatumMapping = ActivityDatumMapping.MapRecord(ActivityDatumMapping.DataSourceEnum.FitFile, ActivityDatumMapping.LevelType.TimeSeries, fieldName);
            if (activityDatumMapping == null || !activityDatumMapping.Import)
            {
                return;
            }
            fieldName = activityDatumMapping.InternalName;

            if (field.GetNumValues() == 1) //TODO: Handle fields with multiple values
            {

                var fieldValueObj = field.GetValue();
                if (fieldValueObj != null) //treat null as missing value
                {
                    //Can't use field.Type as it will return a different type
                    //see C:\coding\FitSDKRelease_21.94.00\cs\Dynastream\Fit\FieldBase.cs for details of the types
                    if (field.IsNumeric())
                    {
                        float value = GetFieldValueAsFloat(fieldValueObj);
                        AddTimeSeriesDatum(fieldName, elapsedTime, value);
                    }
                    else if (fieldName != null && fieldName != "ActivityType") //
                    {
                        //Activity at 04/11/2017 10:51:39 has a string value for field ActivityType (lots of incidents of it, but only one file)
                        Logging.Instance.Log(String.Format("Activity at {0} has a string value for field {1}", recordTime, fieldName)); //see if this ever happens
                    }
                }
            }
            else
            {
                Logging.Instance.Log(String.Format("Activity at {0} has multiple values for field {1}", recordTime, fieldName));
            }
        }

        private void AddTimeSeriesDatum(string name, uint elapsedTime, float data)
        {
            if (!Accumulation.timeSeriesSet.ContainsKey(name))
            {
                Accumulation.timeSeriesSet.Add(name, new KeyValuePair<List<uint>, List<float>>(new List<uint>(), new List<float>()));
            }

            List<uint> times = Accumulation.timeSeriesSet[name].Key;
            List<float> values = Accumulation.timeSeriesSet[name].Value;
            if (!Accumulation.lastElapsedTime.ContainsKey(name))
                Accumulation.lastElapsedTime.Add(name, uint.MaxValue);
            uint lastElapsedTime = Accumulation.lastElapsedTime[name];
            if (elapsedTime == lastElapsedTime && times.Count > 0) //optimized over checking the last entry based on performance data
            {
                times.RemoveAt(times.Count - 1);
                values.RemoveAt(values.Count - 1);
            }
            times.Add(elapsedTime);
            values.Add(data);
            Accumulation.lastElapsedTime[name] = elapsedTime;

        }

        private static float GetFieldValueAsFloat(object fieldValueObj)
        {
            float value;
            if (fieldValueObj is float)
            {
                value = (float)fieldValueObj;
            }
            else if (fieldValueObj is double)
            {
                double valueDouble = (float)fieldValueObj;
                value = (float)valueDouble;
            }
            else if (fieldValueObj is int)
            {
                int valueInt = (int)fieldValueObj;
                value = (float)valueInt;
            }
            else if (fieldValueObj is uint)
            {
                uint valueInt = (uint)fieldValueObj;
                value = (float)valueInt;
            }
            else if (fieldValueObj is ushort)
            {
                ushort valueInt = (ushort)fieldValueObj;
                value = (float)valueInt;
            }
            else if (fieldValueObj is byte)
            {
                byte valueInt = (byte)fieldValueObj;
                value = (float)valueInt;
            }
            else if (fieldValueObj is sbyte)
            {
                sbyte valueInt = (sbyte)fieldValueObj;
                value = (float)valueInt;
            }
            else
            {
                value = (float)fieldValueObj;
            }

            return value;
        }


        //TODO: We can look up the product details using the data in the FIT toolkit's profile.xlsx to find the manufacturer, and if it's Garmin, the model
        //TODO: There are C# constant definitions in FitSDKRelease_21.94.00\cs\Dynastream\Fit\Profile\Types but they're only "public const ushort ChestPressWithBand = 0;" with no string conversion
        void OnFileIDMesg(object sender, MesgEventArgs e)
        {
            Calculation.Other.Start();

            if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                Logging.Instance.Debug(String.Format("OnFileIDMesg: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            FileIdMesg myFileIdMesg = (FileIdMesg)e.mesg;
            var timestampUint = myFileIdMesg.GetFieldValue(FileIdMesg.FieldDefNum.TimeCreated);

            Dynastream.Fit.DateTime dt = new Dynastream.Fit.DateTime((uint)timestampUint);
            System.DateTime startTime = dt.GetDateTime();
            Calculation.lastStop = startTime;


            //we shouldn't use the file creation time as the start time
            //...but we have to, as some FIT files never have a start event. Doh. 
            Accumulation.activity.StartDateTimeUTC = startTime;

            /*
            if (Calculation.activityStart != null)
            {
                if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                    Logging.Instance.Debug(String.Format("Activity start time is {0} and FIT file is {1}",
                        Calculation.activityStart, startTime));
                if (Calculation.activityStart.Value.Subtract(startTime).TotalSeconds < 5) //ignore differences of less than 5 seconds (arbitrary) as FIT and strava often differ by 1 second
                {
                    Logging.Instance.Log(String.Format("Activity current start time is {0} and FIT file has {1}", 
                        Calculation.activityStart, startTime));
                }
            }
            else
            {
                if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                    Logging.Instance.Debug(String.Format("No activity start time, FIT file is {0}", startTime));
                Accumulation.activity.StartDateTimeLocal = startTime;
            }
            */
            Calculation.Other.Stop();
        }


        void OnSportMesgEvent(object sender, MesgEventArgs e)
        {
            Calculation.Other.Start();
            if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                Logging.Instance.Debug(String.Format("SportMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            SportMesg mySportMesg = (SportMesg)e.mesg;
            Sport? aSport = mySportMesg.GetSport();

            string fieldName = e.mesg.Name; //normally "Sport"

            ActivityDatumMapping? activityDatumMapping = ActivityDatumMapping.MapRecord(ActivityDatumMapping.DataSourceEnum.FitFile,
                ActivityDatumMapping.LevelType.Activity, fieldName);
            if (activityDatumMapping == null || !activityDatumMapping.Import)
            {
                return;
            }

            if (aSport != null)
            {
                string activitySportType = ((Sport)aSport).ToString();
                //mesg.name is "Sport", value it "Running" rather than "Run"
                Accumulation.activity.ImportDatum(activityDatumMapping.InternalName, ActivityDatumMapping.DataSourceEnum.FitFile, ActivityDatumMapping.LevelType.Activity, activitySportType);
            }
            Calculation.Other.Stop();
        }


        // We need to keep track of when the timer is stopped, otherwise we end up with odd gaps in the graphs. 
        // So if you start your watch, run for 5 min, stop it for a min, then run another 5 min, you want a 10 minute graph, not an 11 minute graph with a strange gap. 
        void OnEventMesgEvent(object sender, MesgEventArgs e)
        {
            Calculation.Other.Start();
            EventMesg myEventMesg = (EventMesg)e.mesg;

            if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                Logging.Instance.Debug(String.Format("EventMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name,
                    myEventMesg.GetEvent() != null ? myEventMesg.GetEvent().ToString() : "No event",
                    myEventMesg.GetEventType() != null ? myEventMesg.GetEventType().ToString() : "No event type"));


            if (myEventMesg.GetEvent() == null || myEventMesg.GetEvent() != Event.Timer)
            {
                return;
            }

            if (myEventMesg.GetEventType() != null)
            {
                if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                    Logging.Instance.Debug(String.Format(">>GetEventType is {0}", myEventMesg.GetEventType().ToString()));

                if (myEventMesg.GetEventType() == EventType.StopAll ||
                    myEventMesg.GetEventType() == EventType.Stop ||
                    myEventMesg.GetEventType() == EventType.StopDisable ||
                    myEventMesg.GetEventType() == EventType.StopDisableAll)
                {
                    if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                        Logging.Instance.Debug("Stop Timer Event");
                    if (Calculation.IsRunning)
                    {
                        Calculation.lastStop = myEventMesg.GetTimestamp().GetDateTime();
                        if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                            Logging.Instance.Debug(String.Format("Stop Timer while running, last stop is now {0}", Calculation.lastStop));
                    }
                    Calculation.IsRunning = false;
                }
                if (myEventMesg.GetEventType() == EventType.Start)
                {
                    if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                        Logging.Instance.Debug("Start Timer Event");
                    System.DateTime restartTime = myEventMesg.GetTimestamp().GetDateTime();

                    //the first start is the start of the activity (the activity event comes at the end of the file)
                    if (Calculation.activityStartUTC == null)
                        Calculation.activityStartUTC = restartTime;

                    if (!Calculation.IsRunning)
                    {

                        double stoppage = (restartTime - Calculation.lastStop).TotalSeconds;
                        if (stoppage > 0) { stoppage--; } //otherwise we end up with the next even on the same second
                        Calculation.totalStopped += stoppage;
                        if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                            Logging.Instance.Debug(String.Format("Stop Timer while stopped, ReStartTime {0}, stoppage {1}, totalStopped {2}", restartTime, stoppage, Calculation.totalStopped));
                    }
                    Calculation.IsRunning = true;
                }
            }
            Calculation.Other.Stop();
        }


        //This should be the start of the activity, giving us UTC and local times
        void OnActivityMesgEvent(object sender, MesgEventArgs e)
        {
            if (Options.Instance.DebugFitLoading) 
                Logging.Instance.Debug(String.Format("ActivityMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            ActivityMesg? myActivityMesg = (ActivityMesg)e.mesg;
            if (myActivityMesg != null)
            {
                Dynastream.Fit.DateTime dt = myActivityMesg.GetTimestamp();
                //System.DateTime startTime = dt.GetDateTime();
                //don't do this - it's the timestamp of the record that's at the end of the activity! Oops. 
                //Calculation.activityStartUTC = startTime; 

                uint? timestampUint = myActivityMesg.GetLocalTimestamp();
                if (timestampUint != null)
                {
                    Dynastream.Fit.DateTime ldt = new Dynastream.Fit.DateTime((uint)timestampUint!);
                    System.DateTime startTimeLocal = ldt.GetDateTime();
                    Calculation.activityStartLocal = startTimeLocal;

                    if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                        Logging.Instance.Debug($"Start time local {startTimeLocal}");
                }
                else
                {
                    if (Options.Instance.DebugFitLoading) //the string.format is expensive, so don't call it if not needed
                        Logging.Instance.Debug($"no local start time in activity");
                }

            }

        }


        #region DebugHandlers

        //This is the generic handler that gets called on every message.
        void DebugOnMesg(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("OnMesg: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            DebugLogEvent(e);
        }
        //TODO: process lap messages into the activity
        void DebugOnLapMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("LapMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            LapMesg myLapMesg = (LapMesg)e.mesg;
        }

        //TODO: handle HRV data into the activity
        void DebugOnHrvMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("HrvMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            HrvMesg myHrvMesg = (HrvMesg)e.mesg;
            //PrintProperties(myHrvMesg);
        }

        void DebugOnUserProfileMesg(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("OnUserProfileMesg: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
        }
        void DebugOnDeviceInfoMessage(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("OnDeviceInfoMessage: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
        }
        void DebugOnMonitoringMessage(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("OnMonitoringMessage: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
        }

        void DebugOnFileIdMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("FileIdMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            FileIdMesg myFileIdMesg = (FileIdMesg)e.mesg;
        }

        void DebugOnFileCreatorMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("FileCreatorMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            FileCreatorMesg myFileCreatorMesg = (FileCreatorMesg)e.mesg;
        }

        void DebugOnSoftwareMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("SoftwareMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            SoftwareMesg mySoftwareMesg = (SoftwareMesg)e.mesg;
        }

        void DebugOnSlaveDeviceMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("SlaveDeviceMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            SlaveDeviceMesg mySlaveDeviceMesg = (SlaveDeviceMesg)e.mesg;
        }

        void DebugOnCapabilitiesMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("CapabilitiesMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            CapabilitiesMesg myCapabilitiesMesg = (CapabilitiesMesg)e.mesg;
        }

        void DebugOnFileCapabilitiesMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("FileCapabilitiesMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            FileCapabilitiesMesg myFileCapabilitiesMesg = (FileCapabilitiesMesg)e.mesg;
        }

        void DebugOnMesgCapabilitiesMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("MesgCapabilitiesMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            MesgCapabilitiesMesg myMesgCapabilitiesMesg = (MesgCapabilitiesMesg)e.mesg;
        }

        void DebugOnFieldCapabilitiesMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("FieldCapabilitiesMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            FieldCapabilitiesMesg myFieldCapabilitiesMesg = (FieldCapabilitiesMesg)e.mesg;
        }

        void DebugOnDeviceSettingsMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("DeviceSettingsMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            DeviceSettingsMesg myDeviceSettingsMesg = (DeviceSettingsMesg)e.mesg;


            Logging.Instance.Debug(String.Format("GetActiveTimeZone {0}", myDeviceSettingsMesg.GetActiveTimeZone()));
            Logging.Instance.Debug(String.Format("GetActivityTrackerEnabled {0}", myDeviceSettingsMesg.GetActivityTrackerEnabled()));
            Logging.Instance.Debug(String.Format("GetAutoActivityDetect {0}", myDeviceSettingsMesg.GetAutoActivityDetect()));
            Logging.Instance.Debug(String.Format("GetAutoSyncFrequency {0}", myDeviceSettingsMesg.GetAutoSyncFrequency()));
            Logging.Instance.Debug(String.Format("GetAutosyncMinSteps {0}", myDeviceSettingsMesg.GetAutosyncMinSteps()));
            Logging.Instance.Debug(String.Format("GetAutosyncMinTime {0}", myDeviceSettingsMesg.GetAutosyncMinTime()));
            Logging.Instance.Debug(String.Format("GetBacklightMode {0}", myDeviceSettingsMesg.GetBacklightMode()));
            Logging.Instance.Debug(String.Format("GetBleAutoUploadEnabled {0}", myDeviceSettingsMesg.GetBleAutoUploadEnabled()));
            Logging.Instance.Debug(String.Format("GetClockTime {0}", myDeviceSettingsMesg.GetClockTime()));
            Logging.Instance.Debug(String.Format("GetDateMode {0}", myDeviceSettingsMesg.GetDateMode()));
            Logging.Instance.Debug(String.Format("GetDisplayOrientation {0}", myDeviceSettingsMesg.GetDisplayOrientation()));
            Logging.Instance.Debug(String.Format("GetLactateThresholdAutodetectEnabled {0}", myDeviceSettingsMesg.GetLactateThresholdAutodetectEnabled()));
            Logging.Instance.Debug(String.Format("GetMountingSide {0}", myDeviceSettingsMesg.GetMountingSide()));
            Logging.Instance.Debug(String.Format("GetMoveAlertEnabled {0}", myDeviceSettingsMesg.GetMoveAlertEnabled()));
            Logging.Instance.Debug(String.Format("GetNumberOfScreens {0}", myDeviceSettingsMesg.GetNumberOfScreens()));
            Logging.Instance.Debug(String.Format("GetNumDefaultPage {0}", myDeviceSettingsMesg.GetNumDefaultPage()));
            Logging.Instance.Debug(String.Format("GetNumPagesEnabled {0}", myDeviceSettingsMesg.GetNumPagesEnabled()));
            Logging.Instance.Debug(String.Format("GetNumTimeMode {0}", myDeviceSettingsMesg.GetNumTimeMode()));
            Logging.Instance.Debug(String.Format("GetNumTimeOffset {0}", myDeviceSettingsMesg.GetNumTimeOffset()));
            Logging.Instance.Debug(String.Format("GetNumTimeZoneOffset {0}", myDeviceSettingsMesg.GetNumTimeZoneOffset()));
            Logging.Instance.Debug(String.Format("GetSmartNotificationDisplayOrientation {0}", myDeviceSettingsMesg.GetSmartNotificationDisplayOrientation()));
            Logging.Instance.Debug(String.Format("GetTapInterface {0}", myDeviceSettingsMesg.GetTapInterface()));
        }

        void DebugOnUserProfileMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("UserProfileMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            UserProfileMesg myUserProfileMesg = (UserProfileMesg)e.mesg;
        }



        void DebugOnHrmProfileMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("HrmProfileMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            HrmProfileMesg myHrmProfileMesg = (HrmProfileMesg)e.mesg;
        }

        void DebugOnSdmProfileMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("SdmProfileMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            SdmProfileMesg mySdmProfileMesg = (SdmProfileMesg)e.mesg;
        }

        void DebugOnBikeProfileMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("BikeProfileMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            BikeProfileMesg myBikeProfileMesg = (BikeProfileMesg)e.mesg;
        }

        void DebugOnZonesTargetMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("ZonesTargetMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            ZonesTargetMesg myZonesTargetMesg = (ZonesTargetMesg)e.mesg;
        }


        void DebugOnHrZoneMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("HrZoneMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            HrZoneMesg myHrZoneMesg = (HrZoneMesg)e.mesg;
        }

        void DebugOnSpeedZoneMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("SpeedZoneMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            SpeedZoneMesg mySpeedZoneMesg = (SpeedZoneMesg)e.mesg;
        }

        void DebugOnCadenceZoneMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("CadenceZoneMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            CadenceZoneMesg myCadenceZoneMesg = (CadenceZoneMesg)e.mesg;
        }

        void DebugOnPowerZoneMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("PowerZoneMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            PowerZoneMesg myPowerZoneMesg = (PowerZoneMesg)e.mesg;
        }

        void DebugOnMetZoneMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("MetZoneMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            MetZoneMesg myMetZoneMesg = (MetZoneMesg)e.mesg;
        }

        void DebugOnGoalMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("GoalMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            GoalMesg myGoalMesg = (GoalMesg)e.mesg;
        }


        void DebugOnSessionMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("SessionMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            SessionMesg mySessionMesg = (SessionMesg)e.mesg;
        }


        void DebugOnLengthMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("LengthMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            LengthMesg myLengthMesg = (LengthMesg)e.mesg;
        }


        void DebugOnDeviceInfoMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("DeviceInfoMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            DeviceInfoMesg myDeviceInfoMesg = (DeviceInfoMesg)e.mesg;
        }


        void DebugOnCourseMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("CourseMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            CourseMesg myCourseMesg = (CourseMesg)e.mesg;
        }

        void DebugOnCoursePointMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("CoursePointMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            CoursePointMesg myCoursePointMesg = (CoursePointMesg)e.mesg;
        }

        void DebugOnWorkoutMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("WorkoutMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            WorkoutMesg myWorkoutMesg = (WorkoutMesg)e.mesg;
        }

        void DebugOnWorkoutStepMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("WorkoutStepMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            WorkoutStepMesg myWorkoutStepMesg = (WorkoutStepMesg)e.mesg;
        }

        void DebugOnScheduleMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("ScheduleMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            ScheduleMesg myScheduleMesg = (ScheduleMesg)e.mesg;
        }

        void DebugOnTotalsMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("TotalsMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            TotalsMesg myTotalsMesg = (TotalsMesg)e.mesg;
        }

        void DebugOnWeightScaleMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("WeightScaleMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            WeightScaleMesg myWeightScaleMesg = (WeightScaleMesg)e.mesg;
        }

        void DebugOnBloodPressureMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("BloodPressureMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            BloodPressureMesg myBloodPressureMesg = (BloodPressureMesg)e.mesg;
        }

        void DebugOnMonitoringInfoMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("MonitoringInfoMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            MonitoringInfoMesg myMonitoringInfoMesg = (MonitoringInfoMesg)e.mesg;
        }

        void DebugOnMonitoringMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("MonitoringMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            MonitoringMesg myMonitoringMesg = (MonitoringMesg)e.mesg;
        }

        void DebugOnPadMesgEvent(object sender, MesgEventArgs e)
        {
            Logging.Instance.Debug(String.Format("PadMesgEvent: Received {1} Mesg, it has global ID#{0}", e.mesg.Num, e.mesg.Name));
            PadMesg myPadMesg = (PadMesg)e.mesg;
        }

        #endregion
        private Decode CreateAndLinkDecoder()
        {
            Dynastream.Fit.Decode fitDecoder = new Dynastream.Fit.Decode();
            Dynastream.Fit.MesgBroadcaster mesgBroadcaster = new Dynastream.Fit.MesgBroadcaster();


            // Connect the Broadcaster to the event source (Decoder)
            fitDecoder.MesgEvent += mesgBroadcaster.OnMesg;
            fitDecoder.MesgDefinitionEvent += mesgBroadcaster.OnMesgDefinition;


            // Subscribe to message events of interest by connecting this class to the Broadcaster

            mesgBroadcaster.FileIdMesgEvent += OnFileIDMesg;
            mesgBroadcaster.RecordMesgEvent += OnRecordMesgEvent;
            mesgBroadcaster.EventMesgEvent += OnEventMesgEvent;
            mesgBroadcaster.SportMesgEvent += OnSportMesgEvent;
            mesgBroadcaster.ActivityMesgEvent += OnActivityMesgEvent;
            if (Options.Instance.DebugFitLoading && Options.Instance.DebugFitExtraDetails) //this will produce massive details and be very slow
            {

                //Debug only
                mesgBroadcaster.MesgEvent += new MesgEventHandler(DebugOnMesg);
                mesgBroadcaster.UserProfileMesgEvent += DebugOnUserProfileMesg;
                mesgBroadcaster.MonitoringMesgEvent += DebugOnMonitoringMessage;
                mesgBroadcaster.DeviceInfoMesgEvent += DebugOnDeviceInfoMessage;

                mesgBroadcaster.FileIdMesgEvent += DebugOnFileIdMesgEvent;
                mesgBroadcaster.FileCreatorMesgEvent += DebugOnFileCreatorMesgEvent;
                mesgBroadcaster.SoftwareMesgEvent += DebugOnSoftwareMesgEvent;
                mesgBroadcaster.SlaveDeviceMesgEvent += DebugOnSlaveDeviceMesgEvent;
                mesgBroadcaster.CapabilitiesMesgEvent += DebugOnCapabilitiesMesgEvent;
                mesgBroadcaster.FileCapabilitiesMesgEvent += DebugOnFileCapabilitiesMesgEvent;
                mesgBroadcaster.MesgCapabilitiesMesgEvent += DebugOnMesgCapabilitiesMesgEvent;
                mesgBroadcaster.FieldCapabilitiesMesgEvent += DebugOnFieldCapabilitiesMesgEvent;
                mesgBroadcaster.DeviceSettingsMesgEvent += DebugOnDeviceSettingsMesgEvent;
                mesgBroadcaster.UserProfileMesgEvent += DebugOnUserProfileMesgEvent;
                mesgBroadcaster.HrmProfileMesgEvent += DebugOnHrmProfileMesgEvent;
                mesgBroadcaster.SdmProfileMesgEvent += DebugOnSdmProfileMesgEvent;
                mesgBroadcaster.BikeProfileMesgEvent += DebugOnBikeProfileMesgEvent;
                mesgBroadcaster.ZonesTargetMesgEvent += DebugOnZonesTargetMesgEvent;
                mesgBroadcaster.HrZoneMesgEvent += DebugOnHrZoneMesgEvent;
                mesgBroadcaster.SpeedZoneMesgEvent += DebugOnSpeedZoneMesgEvent;
                mesgBroadcaster.CadenceZoneMesgEvent += DebugOnCadenceZoneMesgEvent;
                mesgBroadcaster.PowerZoneMesgEvent += DebugOnPowerZoneMesgEvent;
                mesgBroadcaster.MetZoneMesgEvent += DebugOnMetZoneMesgEvent;
                mesgBroadcaster.GoalMesgEvent += DebugOnGoalMesgEvent;
                mesgBroadcaster.SessionMesgEvent += DebugOnSessionMesgEvent;
                mesgBroadcaster.LapMesgEvent += DebugOnLapMesgEvent;
                mesgBroadcaster.LengthMesgEvent += DebugOnLengthMesgEvent;
                mesgBroadcaster.DeviceInfoMesgEvent += DebugOnDeviceInfoMesgEvent;
                mesgBroadcaster.HrvMesgEvent += DebugOnHrvMesgEvent;
                mesgBroadcaster.CourseMesgEvent += DebugOnCourseMesgEvent;
                mesgBroadcaster.CoursePointMesgEvent += DebugOnCoursePointMesgEvent;
                mesgBroadcaster.WorkoutMesgEvent += DebugOnWorkoutMesgEvent;
                mesgBroadcaster.WorkoutStepMesgEvent += DebugOnWorkoutStepMesgEvent;
                mesgBroadcaster.ScheduleMesgEvent += DebugOnScheduleMesgEvent;
                mesgBroadcaster.TotalsMesgEvent += DebugOnTotalsMesgEvent;
                mesgBroadcaster.WeightScaleMesgEvent += DebugOnWeightScaleMesgEvent;
                mesgBroadcaster.BloodPressureMesgEvent += DebugOnBloodPressureMesgEvent;
                mesgBroadcaster.MonitoringInfoMesgEvent += DebugOnMonitoringInfoMesgEvent;
                mesgBroadcaster.MonitoringMesgEvent += DebugOnMonitoringMesgEvent;
                mesgBroadcaster.PadMesgEvent += DebugOnPadMesgEvent;
            }

            return fitDecoder;
        }

        void DebugLogEvent(MesgEventArgs e)
        {
            Logging.Instance.Debug(string.Format(">>>> OnMesg (debug): Received Mesg with global ID# {0}, its name is {1}, class {2}", e.mesg.Num, e.mesg.Name, e.mesg.GetType()));
            int i = 0;
            foreach (Field field in e.mesg.Fields)
            {
                for (int j = 0; j < field.GetNumValues(); j++)
                {
                    String s = String.Format("Field {0} Index {1} (\"{2}\" Field #{4}) Value: {3} (raw value {5})",
                        i, j, field.GetName(), field.GetValue(j), field.Num, field.GetRawValue(j));
                    string k = "Field:" + i + ", value:" + j + ", name:" + field.GetName() + field.Num;
                    if (field.GetName() == "Timestamp")
                    {
                        string fieldValue = field.GetValue(j).ToString()!;
                        Dynastream.Fit.DateTime dt = new Dynastream.Fit.DateTime(uint.Parse(fieldValue));
                        Logging.Instance.Debug(string.Format("\ttimestamp {0}", dt.GetDateTime()));
                    }
                    else
                    {
                        //if (field.GetName() == "PositionLat")
                        //    MessageBox.Show("oops");
                        Logging.Instance.Debug(string.Format("\tField{0} Index{1} (\"{2}\" Field#{4}) Value: {3} (raw value {5})",
                        i,
                        j,
                        field.GetName(),
                        field.GetValue(j),
                        field.Num,
                        field.GetRawValue(j),
                        field.GetUnits()));
                    }
                }

                i++;
            }

            foreach (var field in e.mesg.DeveloperFields)
            {
                for (int j = 0; j < field.GetNumValues(); j++)
                {
                    String s = String.Format("DevField {0} Index {1} (\"{2}\" Field #{4}) Value: {3} (raw value {5})",
                        0, j, field.GetName(), field.GetValue(j), field.Num, field.GetRawValue(j));
                    string k = "Field:" + i + ", value:" + j + ", name:" + field.GetName() + field.Num;

                    Logging.Instance.Debug(string.Format("\tDeveloper{0} Field#{1} Index{2} (\"{3}\") Value: {4} (raw value {5})",
                        field.DeveloperDataIndex,
                        field.Num,
                        j,
                        field.Name,
                        field.GetValue(j),
                        field.GetRawValue(j)));
                }
            }

        }


    }
}