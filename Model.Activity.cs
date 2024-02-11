using MemoryPack;
using System.Collections.ObjectModel;
using FellrnrTrainingAnalysis.Utils;
using System.IO;
using System.ComponentModel;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    [MemoryPackable(GenerateType.CircularReference)]
    public partial class Activity : Extensible
    {
        public Activity()
        {
        }

        public override string ToString()
        {
            return string.Format("Start {0} key {1} filename {2} name {3}", StartDateTimeLocal, PrimaryKey(), Filename, Name);
        }


        //KnownFields
        //    _  __                            ______ _      _     _     
        //   | |/ /                           |  ____(_)    | |   | |    
        //   | ' / _ __   _____      ___ __   | |__   _  ___| | __| |___ 
        //   |  < | '_ \ / _ \ \ /\ / / '_ \  |  __| | |/ _ \ |/ _` / __|
        //   | . \| | | | (_) \ V  V /| | | | | |    | |  __/ | (_| \__ \
        //   |_|\_\_| |_|\___/ \_/\_/ |_| |_| |_|    |_|\___|_|\__,_|___/
        //                                                               
        //                                                               


        //[MemoryPackOrder(0)]
        [MemoryPackIgnore]
        public DateTime? StartDateTimeLocal { 
            get { return GetNamedDateTimeDatum(StartDateAndTimeLocalTag); }
            set { if(value != null) AddOrReplaceDatum(new TypedDatum<DateTime>(StartDateAndTimeLocalTag, true, (DateTime)value)); } //used by fit reader
        }

        [MemoryPackIgnore]
        public DateTime? StartDateTimeUTC
        {
            get { return GetNamedDateTimeDatum(StartDateAndTimeUTCTag); }
            set { if (value != null) AddOrReplaceDatum(new TypedDatum<DateTime>(StartDateAndTimeUTCTag, true, (DateTime)value)); } //used by fit reader
        }


        [MemoryPackIgnore]
        public DateTime? StartDateNoTimeLocal { get { return StartDateTimeLocal == null ? null : ((DateTime)StartDateTimeLocal).Date; } }

        [MemoryPackOrder(1)]
        public LocationStream? LocationStream { get; set; } = null;

        public override Utils.DateTimeTree Id() { return new Utils.DateTimeTree(StartDateTimeLocal!.Value, DateTimeTree.DateTreeType.Time); } //HACK: to see if tree works


        [MemoryPackIgnore]
        public string? Filename { get { return GetNamedStringDatum(FilenameTag); } }
        [MemoryPackIgnore]
        public string? FileFullPath { get { return GetNamedStringDatum(FileFullPathTag); } set { if(value != null) AddOrReplaceDatum(new TypedDatum<string>(FileFullPathTag, true, value)); } }
        [MemoryPackIgnore]
        public string? ActivityType { get { return GetNamedStringDatum(ActivityTypeTag); } }

        [MemoryPackIgnore]
        public string Description { get { return GetNamedStringDatum(DescriptionTag) ?? ""; } set { AddOrReplaceDatum(new TypedDatum<string>(Activity.DescriptionTag, true, value)); } }

        [MemoryPackIgnore]
        public string Name { get { return GetNamedStringDatum(NameTag) ?? ""; } set { AddOrReplaceDatum(new TypedDatum<string>(Activity.NameTag, true, value)); } }
        //This needs to be static so callers can validate and find an activity given a dictionary of string/Datum by primary keys
        public static bool WillBeValid(Dictionary<string, Datum> activityData) { return activityData.ContainsKey(StravaActivityIDTag); } //was  && activityData.ContainsKey(StartDateAndTimeLocalTag);

        //This needs to be static so callers can find an activity given a dictionary of string/Datum by primary keys
        public static string ExpectedPrimaryKey(Dictionary<string, Datum> activityData) { return ((TypedDatum<string>)activityData[StravaActivityIDTag]).Data; }

        public string PrimaryKey() { return ((TypedDatum<string>)Data[StravaActivityIDTag]).Data; }


        //This needs to be static so callers can find the date to check if it's after the loading date. Has to be UTC as that's all we have from the Strava CSV file
        public static DateTime ExpectedStartDateTime(Dictionary<string, Datum> activityData) { return ((TypedDatum<DateTime>)activityData[StartDateAndTimeUTCTag]).Data; }

        //TimeSeries
        //    _______ _                   _____           _           
        //   |__   __(_)                 / ____|         (_)          
        //      | |   _ _ __ ___   ___  | (___   ___ _ __ _  ___  ___ 
        //      | |  | | '_ ` _ \ / _ \  \___ \ / _ \ '__| |/ _ \/ __|
        //      | |  | | | | | | |  __/  ____) |  __/ |  | |  __/\__ \
        //      |_|  |_|_| |_| |_|\___| |_____/ \___|_|  |_|\___||___/
        //                                                            
        //                                                            


        //set of time series, one for HR, speed, etc. 
        [MemoryPackInclude]
        [MemoryPackOrder(2)]
        private Dictionary<string, DataStreamBase> timeSeries = new Dictionary<string, DataStreamBase>();
        [MemoryPackInclude]
        [MemoryPackOrder(3)]
        private Dictionary<string, DataStreamBase>? removedTimeSeries;

        [MemoryPackIgnore]
        public ReadOnlyDictionary<string, DataStreamBase> TimeSeries { get { return timeSeries.AsReadOnly(); } }

        [MemoryPackIgnore]
        public List<String> TimeSeriesNames { get { return TimeSeries.Keys.ToList(); } }


        [MemoryPackIgnore]
        public Athlete? Parent { get { return parent_; } }
        //[MemoryPackInclude]
        [MemoryPackIgnore]
        private Athlete? parent_ = null;

        public void PostDeserialize(Athlete parent)
        {
            this.parent_ = parent;
            foreach (KeyValuePair<string, DataStreamBase> kvp in TimeSeries)
            {
                //kvp.Value.PostDeserialize(kvp.Key, this); //we had to repair names when memory pack didn't have a public setter
                kvp.Value.PostDeserialize(this);
            }

        }

        public void AddDataStreams(Dictionary<string, KeyValuePair<List<uint>, List<float>>> dataStreams, Activity activity)
        {
            foreach (KeyValuePair<string, KeyValuePair<List<uint>, List<float>>> kvp in dataStreams)
            {
                string name = kvp.Key;
                if (!TimeSeries.ContainsKey(name))
                {
                    KeyValuePair<List<uint>, List<float>> timesAndValues = kvp.Value;
                    List<uint> times = timesAndValues.Key;
                    List<float> values = timesAndValues.Value;
                    if (values.Min() == 0 && values.Max() == 0)
                    {
                        Logging.Instance.Log($"Not adding data stream {name} as it is all zeros {this.ToString()}");
                    }
                    else
                    {
                        AddDataStream(name, times.ToArray(), values.ToArray(), activity);
                    }
                }

            }
        }

        public void AddDataStream(string name, uint[] times, float[] values, Activity activity)
        {
            if (!timeSeries.ContainsKey(name))
            {
                DataStreamRecorded activityDataStream = new DataStreamRecorded(name, new Tuple<uint[], float[]>(times, values), activity);
                timeSeries.Add(name, activityDataStream);
            }
        }

        public void AddDataStream(DataStreamBase dataStream)
        {
            if (!timeSeries.ContainsKey(dataStream.Name))
            {
                timeSeries.Add(dataStream.Name, dataStream);
            }
            else
            {
                timeSeries[dataStream.Name] = dataStream;
            }
        }

        public void RemoveDataStream(string name)
        {
            if (timeSeries.ContainsKey(name))
            {
                if(removedTimeSeries == null)
                    removedTimeSeries = new Dictionary<string, DataStreamBase>();
                removedTimeSeries.Add(name, timeSeries[name]);
                timeSeries.Remove(name);
            }
        }

        //TODO: Add HRV data to activity data, not really a time series

        //TODO: Add Lap data, not a time series


        //a list of URIs to photos that were attached to the workout, typically from Strava
        [MemoryPackOrder(4)]
        public List<Uri>? PhotoUris { get; set; }

        public override void Recalculate(int forceCount, bool forceJustMe, BackgroundWorker? worker = null)
        {
            bool force = false;
            if(forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (force)
                base.Clean();

            //clear all existing virtual data streams - only an issue of they change names, but keeps things tidy
            List<string> toDelete = new List<string>();  
            foreach(KeyValuePair<string, DataStreamBase> kvp in TimeSeries)
            {
                if(kvp.Value.IsVirtual())
                    toDelete.Add(kvp.Key);
            }
            foreach(string s in toDelete) { timeSeries.Remove(s); }

            if (ProcessTags())
                force = true;

            List<DataStreamBase> dataStreams = DataStreamFactory.Instance.DataStreams(this);

            foreach (DataStreamBase dataStream in dataStreams)
            {
                if (dataStream.IsValid())
                {
                    try
                    {
                        dataStream.Recalculate(forceCount, forceJustMe); //if this object is forced, force dependent objects
                        this.AddDataStream(dataStream);
                    }
                    catch (Exception ex)
                    {
                        Logging.Instance.Error(string.Format("Failed to process activity {0} from {1} for data stream {2} due to {3}", 
                            this.PrimaryKey(), this.StartDateTimeLocal, dataStream.Name, ex));
                    }
                }
                //TODO: extra debug on recalculate
                //else 
                //{
                //    Logging.Instance.Debug($"activity.recalculate datastream {dataStream.Name} is not valid for activity {this}");
                //}
            }

            foreach(CalculateFieldBase calculate in CaclulateFieldFactory.Instance.Calulators)
            {
                calculate.Recalculate(this, force);
            }

        }


        private const string START = "⌗";
        private const string END = "֍";
        private const char MIDDLE = '༶';
        //start char is ⌗ U+2317
        //middle markers are ༶ (U+0F36)
        //end is ֍ (U+058D)
        //new TagActivities("Delete Altitude", "⌗Altitude༶Delete֍"),
        //new TagActivities("Replace Start of Altitude", "⌗Altitude༶CopyBack༶10֍"),
        //new TagActivities("Delete Power", "⌗Power༶Delete֍"),
        //new TagActivities("Cap Power CP", "⌗Power༶Cap༶100֍"),

        public bool ProcessTags()
        {
            TypedDatum<string>? descriptionDatum = (TypedDatum<string>?)this.GetNamedDatum(Activity.DescriptionTag);
            if (descriptionDatum == null || descriptionDatum.Data == null)
                return false;
            string description = descriptionDatum.Data;


            TypedDatum<string>? processedDatum = (TypedDatum<string>?)this.GetNamedDatum("Processed Tags");
            string processedTags = (processedDatum == null || processedDatum.Data == null) ? "" : processedDatum.Data;
            bool processedTagsChanged = false;

            while (description.Contains(START) && description.Contains(END))
            {
                int start = description.IndexOf(START, StringComparison.Ordinal);
                int end = description.IndexOf(END, StringComparison.Ordinal);
                int len = end - start;
                string tag = description.Substring(start + 1, len - 1);

                if(!processedTags.Contains(tag))
                {
                    Logging.Instance.Debug($"processedTags [{processedTags}] doesn't contain {tag}");
                    if(!ProcessTag(tag))
                    {
                        Logging.Instance.Debug($"ProcessTag failed");
                        return false;
                    }
                    processedTags += tag;
                    processedTagsChanged = true;
                }

                description = description.Substring(end + 1);
            }
            if (processedTagsChanged)
            {
                Logging.Instance.Debug($"processedTags is now {processedTags}");
                this.AddOrReplaceDatum(new TypedDatum<string>("Processed Tags", true, processedTags)); //set recorded to true as this isn't something we want to recreate all the time
                return true;
            }
            return false;
        }

        private bool ProcessTag(string tag)
        {
            Logging.Instance.Debug($"ProcessTag({tag})");
            bool retval = false;
            string[] strings = tag.Split(MIDDLE);
            string stream = strings[0];
            string command = strings[1];
            int amount = strings.Length > 2 ? int.Parse(strings[2]) : 0;

            if(command == "Delete")
            {
                Logging.Instance.Debug($"ProcessTag command: delete stream:{stream}");
                this.RemoveNamedDatum(stream);
                this.RemoveDataStream(stream);
                retval = true;
            }
            else if (command == "CopyBack")
            {
                Logging.Instance.Debug($"ProcessTag command: copyback stream:{stream}");
                if (!this.TimeSeries.ContainsKey(stream))
                {
                    Logging.Instance.Debug($"ProcessTag CopyBack missing {stream}");
                    return retval;
                }
                DataStreamBase dataStream = this.TimeSeries[stream];
                Tuple<uint[], float[]>? data = dataStream.GetData();
                if (data == null || data.Item1.Length < amount)
                {
                    Logging.Instance.Debug($"ProcessTag CopyBack {stream} is too short");
                    return retval;
                }
                float copyback = data.Item2[amount];

                for (int i = 0; i < amount; i++)
                {
                    data.Item2[i] = copyback;
                }
                Logging.Instance.Debug($"ProcessTag CopyBack {stream} Done");
                retval = true;
            }
            else if (command == "Cap")
            {
                Logging.Instance.Debug($"ProcessTag Cap {stream} to {amount}");
                DataStreamBase dataStream = this.TimeSeries[stream];
                Tuple<uint[], float[]>? data = dataStream.GetData();
                if (data == null)
                {
                    Logging.Instance.Debug($"ProcessTag Cap {stream} no data");
                    return retval;
                }
                for (int i = 0; i < data.Item2.Length; i++)
                {
                    if(data.Item2[i] > amount)
                        data.Item2[i] = amount;
                }
                Logging.Instance.Debug($"ProcessTag Cap {stream} Done");
                retval = true;
            }
            else
            {
                Logging.Instance.Debug($"ProcessTag unexpected command {command}");
                retval = false;
            }
            return retval;
        }
        public void RecalculateHills(List<Hill> hills, bool force, bool fullDebug)
        {
            if (LocationStream == null || LocationStream.Latitudes.Length == 0)
                return;
            if (Climbed != null && !force)
                return;
            if (Climbed == null || force)
                Climbed = new List<Hill>();

            int nochecked = 0;
            int nomatched = 0;

            //if (Options.Instance.LogLevel == Options.Level.Debug && force)
            //    Logging.Instance.StartTimer("hills");

            foreach (Hill hill in hills)
            {
                float minDistance = float.MaxValue;
                float nearestLat = 0;
                float nearestLon = 0;


                //if(hill.Number == 2503)

                //first optimization; is the hill within the bounds of the route?
                if (LocationStream.WithinBounds(hill.Latitude, hill.Longitude))
                {
                    if (fullDebug && Options.Instance.LogLevel == Options.Level.Debug)
                        Logging.Instance.Debug($"Hill {hill.ExtendedName} ({hill.Number}) within bounds");
                    nochecked++;

                    for (int i = 0; i < LocationStream.Latitudes.Length; i++)
                    {
                        float distance = hill.DistanceTo(LocationStream.Latitudes[i], LocationStream.Longitudes[i]);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestLat = LocationStream.Latitudes[i];
                            nearestLon = LocationStream.Longitudes[i];
                        }
                    }
                }
                else
                {
                    if (fullDebug && Options.Instance.LogLevel == Options.Level.Debug)
                        Logging.Instance.Debug($"Hill {hill.ExtendedName} ({hill.Number}) not within bounds");
                }
                if (fullDebug && Options.Instance.LogLevel == Options.Level.Debug && minDistance != float.MaxValue)
                    Logging.Instance.Debug($"Hill {hill.ExtendedName} ({hill.Number}) minDistance {minDistance}, hill {hill.Latitude}/{hill.Longitude}, nearest {nearestLat}/{nearestLon}");

                if (minDistance < Hill.CLOSE_ENOUGH)
                {
                    nomatched++;

                    if(!Climbed.Contains(hill))
                        Climbed.Add(hill);
                    if(!hill.Climbed.Contains(this))
                        hill.Climbed.Add(this);
                }
            }
            //if (Options.Instance.LogLevel == Options.Level.Debug && force)
            //    Logging.Instance.Debug($"Hill matching took {Logging.Instance.GetAndResetTime("hills")}, {nochecked} checked, {nomatched} matched");
        }

        private const string StravaActivityIDTag = "Strava ID";
        public const string PrimarykeyTag = StravaActivityIDTag;
        private const string StartDateAndTimeLocalTag = "Start DateTime Local"; //be explicit about the time part, as sometimes we only want the date component
        private const string StartDateAndTimeUTCTag = "Start DateTime UTC"; //be explicit about the time part, as sometimes we only want the date component
        private const string ActivityTypeTag = "Type";
        private const string FilenameTag = "Filename";
        private const string FileFullPathTag = "Filepath";
        public const string DescriptionTag = "Description";
        public const string NameTag = "Name";

        public const string DistanceTag = "Distance";
        public const string ElapsedTimeTag = "Elapsed Time";
        public const string MovingTimeTag = "Moving Time";
        /*
        public const string ActivityNameTag = "Activity Name";
        public const string ActivityDescriptionTag = "Activity Description";
        public const string MaxHeartRateTag = "Max Heart Rate";
        public const string RelativeEffortTag = "Relative Effort";
        public const string CommuteTag = "Commute";
        public const string ActivityGearTag = "Activity Gear";
        public const string AthleteWeightTag = "Athlete Weight";
        public const string BikeWeightTag = "Bike Weight";
        public const string ElapsedTimeTag = "Elapsed Time";
        public const string MaxSpeedTag = "Max Speed";
        public const string AverageSpeedTag = "Average Speed";
        public const string ElevationGainTag = "Elevation Gain";
        public const string ElevationLossTag = "Elevation Loss";
        public const string ElevationLowTag = "Elevation Low";
        public const string ElevationHighTag = "Elevation High";
        public const string MaxGradeTag = "Max Grade";
        public const string AverageGradeTag = "Average Grade";
        public const string AveragePositiveGradeTag = "Average Positive Grade";
        public const string AverageNegativeGradeTag = "Average Negative Grade";
        public const string MaxCadenceTag = "Max Cadence";
        public const string AverageCadenceTag = "Average Cadence";
        public const string AverageHeartRateTag = "Average Heart Rate";
        public const string MaxWattsTag = "Max Watts";
        public const string AverageWattsTag = "Average Watts";
        public const string CaloriesTag = "Calories";
        public const string MaxTemperatureTag = "Max Temperature";
        public const string AverageTemperatureTag = "Average Temperature";
        public const string TotalWorkTag = "Total Work";
        public const string NumberofRunsTag = "Number of Runs";
        public const string UphillTimeTag = "Uphill Time";
        public const string DownhillTimeTag = "Downhill Time";
        public const string OtherTimeTag = "Other Time";
        public const string PerceivedExertionTag = "Perceived Exertion";
        public const string TypeTag = "Type";
        public const string StartTimeTag = "Start Time";
        public const string WeightedAveragePowerTag = "Weighted Average Power";
        public const string PowerCountTag = "Power Count";
        public const string PreferPerceivedExertionTag = "Prefer Perceived Exertion";
        public const string PerceivedRelativeEffortTag = "Perceived Relative Effort";
        public const string TotalWeightLiftedTag = "Total Weight Lifted";
        public const string FromUploadTag = "From Upload";
        public const string GradeAdjustedDistanceTag = "Grade Adjusted Distance";
        public const string WeatherObservationTimeTag = "Weather Observation Time";
        public const string WeatherConditionTag = "Weather Condition";
        public const string WeatherTemperatureTag = "Weather Temperature";
        public const string ApparentTemperatureTag = "Apparent Temperature";
        public const string DewpointTag = "Dewpoint";
        public const string HumidityTag = "Humidity";
        public const string WeatherPressureTag = "Weather Pressure";
        public const string WindSpeedTag = "Wind Speed";
        public const string WindGustTag = "Wind Gust";
        public const string WindBearingTag = "Wind Bearing";
        public const string PrecipitationIntensityTag = "Precipitation Intensity";
        public const string SunriseTimeTag = "Sunrise Time";
        public const string SunsetTimeTag = "Sunset Time";
        public const string MoonPhaseTag = "Moon Phase";
        public const string BikeTag = "Bike";
        public const string GearTag = "Gear";
        public const string PrecipitationProbabilityTag = "Precipitation Probability";
        public const string PrecipitationTypeTag = "Precipitation Type";
        public const string CloudCoverTag = "Cloud Cover";
        public const string WeatherVisibilityTag = "Weather Visibility";
        public const string UVIndexTag = "UV Index";
        public const string WeatherOzoneTag = "Weather Ozone";
        public const string JumpCountTag = "Jump Count";
        public const string TotalGritTag = "Total Grit";
        public const string AvgFlowTag = "Avg Flow";
        public const string FlaggedTag = "Flagged";
        public const string AvgElapsedSpeedTag = "Avg Elapsed Speed";
        public const string DirtDistanceTag = "Dirt Distance";
        public const string NewlyExploredDistanceTag = "Newly Explored Distance";
        public const string NewlyExploredDirtDistanceTag = "Newly Explored Dirt Distance"; 
        */

        //transient data

        //let's hold on to these rather than querying every time
        //[NonSerialized] 
        [MemoryPackInclude]
        [MemoryPackOrder(5)]
        public List<string>? DataQualityIssues = null; //we don't persist data quality issues as it depends on the criteria applied, and it's quick to check each time

        [MemoryPackOrder(6)]
        public List<Hill>? Climbed { get; set; } = null; //an empty list means we've checked and there's no matches


        public bool CheckSportType(List<string> sportsToInclude)
        {
            string? activitySportType = this.ActivityType?.Trim(); //had spaces after sport
            //if (activity.StartDateNoTime == DateTime.Now.AddDays(-1).Date)
            //{
            //    MessageBox.Show(activitySportType);
            //}
            if (activitySportType == null)
                return false;
            if (!sportsToInclude.Contains(activitySportType))
                return false;
            return true;
        }

    }
}
