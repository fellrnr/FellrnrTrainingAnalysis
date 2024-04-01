using FellrnrTrainingAnalysis.Utils;
using MemoryPack;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    [MemoryPackable(GenerateType.CircularReference)]
    public partial class Activity : Extensible
    {
        [MemoryPackConstructor]
        public Activity()
        {
        }
        public Activity(Athlete parent)
        {
            PostDeserialize(parent);
        }

        public override string ToString()
        {
            return string.Format("Activity, Start {0} key {1} filename {2} name {3}", StartDateTimeLocal, PrimaryKey(), Filename, Name);
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
        public DateTime? StartDateTimeLocal
        {
            get { return GetNamedDateTimeDatum(TagStartDateAndTimeLocal); }
            set { if (value != null) AddOrReplaceDatum(new TypedDatum<DateTime>(TagStartDateAndTimeLocal, true, (DateTime)value)); } //used by fit reader
        }

        [MemoryPackIgnore]
        public DateTime? StartDateTimeUTC
        {
            get { return GetNamedDateTimeDatum(TagStartDateAndTimeUTC); }
            set { if (value != null) AddOrReplaceDatum(new TypedDatum<DateTime>(TagStartDateAndTimeUTC, true, (DateTime)value)); } //used by fit reader
        }


        [MemoryPackIgnore]
        public DateTime? StartDateNoTimeLocal { get { return StartDateTimeLocal == null ? null : ((DateTime)StartDateTimeLocal).Date; } }

        [MemoryPackOrder(1)]
        public LocationStream? LocationStream { get; set; } = null;

        public override Utils.DateTimeTree Id() { return new Utils.DateTimeTree(StartDateTimeLocal!.Value, DateTimeTree.DateTreeType.Time); } //HACK: to see if tree works


        [MemoryPackIgnore]
        public string? Filename { get { return GetNamedStringDatum(FilenameTag); } }
        [MemoryPackIgnore]
        public string? FileFullPath { get { return GetNamedStringDatum(TagFileFullPath); } set { if (value != null) AddOrReplaceDatum(new TypedDatum<string>(TagFileFullPath, true, value)); } }
        [MemoryPackIgnore]
        public string? ActivityType { get { return GetNamedStringDatum(TagActivityType); } }

        [MemoryPackIgnore]
        public string Description { get { return GetNamedStringDatum(TagDescription) ?? ""; } set { AddOrReplaceDatum(new TypedDatum<string>(Activity.TagDescription, true, value)); } }

        [MemoryPackIgnore]
        public string Name { get { return GetNamedStringDatum(TagName) ?? ""; } set { AddOrReplaceDatum(new TypedDatum<string>(Activity.TagName, true, value)); } }
        //This needs to be static so callers can validate and find an activity given a dictionary of string/Datum by primary keys
        public static bool WillBeValid(Dictionary<string, Datum> activityData) { return activityData.ContainsKey(TagStravaActivityID); } //was  && activityData.ContainsKey(StartDateAndTimeLocalTag);

        //This needs to be static so callers can find an activity given a dictionary of string/Datum by primary keys
        public static string ExpectedPrimaryKey(Dictionary<string, Datum> activityData) { return ((TypedDatum<string>)activityData[TagStravaActivityID]).Data; }

        public string PrimaryKey() { return ((TypedDatum<string>)Data[TagStravaActivityID]).Data; }


        //This needs to be static so callers can find the date to check if it's after the loading date. Has to be UTC as that's all we have from the Strava CSV file
        public static DateTime EstimatedStartDateTime(Dictionary<string, Datum> activityData) { return ((TypedDatum<DateTime>)activityData[TagStartDateAndTimeUTC]).Data; }

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
        private Dictionary<string, TimeSeriesBase> timeSeries = new Dictionary<string, TimeSeriesBase>();
        [MemoryPackInclude]
        [MemoryPackOrder(3)]
        private Dictionary<string, TimeSeriesBase>? removedTimeSeries;

        [MemoryPackIgnore]
        public ReadOnlyDictionary<string, TimeSeriesBase> TimeSeries { get { return timeSeries.AsReadOnly(); } }

        [MemoryPackIgnore]
        public List<String> TimeSeriesNames { get { return TimeSeries.Keys.ToList(); } }


        [MemoryPackIgnore]
        public Athlete? ParentAthlete { get { return parent_; } } //set using the PostDeserialize cleanup
        //[MemoryPackInclude]
        [MemoryPackIgnore]
        private Athlete? parent_ = null;

        [MemoryPackIgnore]

        public Model.Day Day { get { return parent_!.Days[this.StartDateNoTimeLocal!.Value]; } }


        public void PostDeserialize(Athlete parent)
        {
            this.parent_ = parent;
            foreach (KeyValuePair<string, TimeSeriesBase> kvp in TimeSeries)
            {
                //kvp.Value.PostDeserialize(kvp.Key, this); //we had to repair names when memory pack didn't have a public setter
                kvp.Value.PostDeserialize(this);
            }

        }

        public void PreSerialize(Athlete parent)
        {
            this.parent_ = parent;
            foreach (KeyValuePair<string, TimeSeriesBase> kvp in TimeSeries)
            {
                //kvp.Value.PostDeserialize(kvp.Key, this); //we had to repair names when memory pack didn't have a public setter
                kvp.Value.PreSerialize();
            }

        }

        public void AddTimeSeriesSet(Dictionary<string, KeyValuePair<List<uint>, List<float>>> timeSeriesSet)
        {
            foreach (KeyValuePair<string, KeyValuePair<List<uint>, List<float>>> kvp in timeSeriesSet)
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
                        AddTimeSeries(name, times.ToArray(), values.ToArray());
                    }
                }

            }
        }

        public void AddTimeSeries(string name, uint[] times, float[] values)
        {
            TimeValueList aTimeValueList = TimeValueList.TimeValueListFromTimed(times, values);
            TimeSeriesRecorded activityTimeSeries = new TimeSeriesRecorded(name, aTimeValueList, this);
            if (!timeSeries.ContainsKey(name))
            {
                timeSeries.Add(name, activityTimeSeries);
            }
            else
            {
                timeSeries[name] = activityTimeSeries;
            }
        }

        public void AddTimeSeries(TimeSeriesBase dataStream)
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

        public void RemoveTimeSeries(string name)
        {
            if (timeSeries.ContainsKey(name) && !TimeSeries[name].IsVirtual())
            {
                if (removedTimeSeries == null)
                    removedTimeSeries = new Dictionary<string, TimeSeriesBase>();
                removedTimeSeries.Add(name, timeSeries[name]);
                timeSeries.Remove(name);
            }
        }

        //TODO: Add HRV data to activity data, not really a time series

        //TODO: Add Lap data, not a time series


        //a list of URIs to photos that were attached to the workout, typically from Strava
        [MemoryPackOrder(4)]
        public List<Uri>? PhotoUris { get; set; }


        [MemoryPackIgnore]
        public static int CurrentRecalculateProgress { get; set; } //ugly, but the alternatives are worse

        public override void Recalculate(int forceCount, bool forceJustMe, BackgroundWorker? worker = null)
        {
            Logging.Instance.ContinueAccumulator("Activity.Recalculate");

            if (worker != null)
                worker.ReportProgress(++CurrentRecalculateProgress);

            if (parent_ != null && CurrentRecalculateProgress > parent_.Activities.Count)
                MessageBox.Show("Huh");

            bool force = false;
            if (forceCount > LastForceCount || forceJustMe) { LastForceCount = forceCount; force = true; }

            if (force)
            {
                Logging.Instance.ContinueAccumulator("Activity.Recalculate(clean)");
                base.Clean();
                List<string> toDelete = new List<string>();
                foreach (KeyValuePair<string, TimeSeriesBase> kvp in TimeSeries)
                {
                    if (kvp.Value.IsVirtual())
                        toDelete.Add(kvp.Key);
                }

                foreach (string s in toDelete) { timeSeries.Remove(s); }
                Logging.Instance.PauseAccumulator("Activity.Recalculate(clean)");
            }

            //process the action tags first, as they may change or remove the recorded time series
            Action.Tags tags = new Action.Tags();
            if (tags.ProcessTags(this, forceCount: forceCount, forceJustMe: forceJustMe, force: force))
                forceJustMe = true;

            foreach (CalculateFieldBase calculate in CaclulateFieldFactory.Instance.PreTimeSeriesCalulators)
            {
                calculate.Recalculate(this, forceCount, forceJustMe);
            }

            if (force)
            {

                List<TimeSeriesBase> ephemeralTimeSeries = TimeSeriesFactory.Instance.TimeSeries(this);

                Logging.Instance.ContinueAccumulator("Activity.Recalculate(time series)");
                foreach (TimeSeriesBase ts in ephemeralTimeSeries)
                {
                    if (!this.timeSeries.ContainsKey(ts.Name)) //we've just removed all virtual ts, so the only ones left with the same name are recorded data
                    {
                        //We're calling GetData twice. Hummm. Let's change to make recalculate return true if we should add it. 
                        //if (ts.IsValid() && ts.GetData(forceCount, forceJustMe) != null)
                        if (ts.IsValid())
                        {
                            if(ts.Recalculate(forceCount, forceJustMe))
                                this.timeSeries.Add(ts.Name, ts);
                        }
                    }
                }
                Logging.Instance.PauseAccumulator("Activity.Recalculate(time series)");

            }
            else
            {
                foreach (KeyValuePair<string, TimeSeriesBase> kvp in timeSeries) //no longer try to add ephemeral time series - only add them on a full recalculate
                {
                    TimeSeriesBase ts = kvp.Value;
                    if (ts.IsValid())
                    {
                        ts.Recalculate(forceCount, forceJustMe);
                    }
                    else
                    {
                        Logging.Instance.Log($"Unforced TimSeries recalulation for {ts} isn't valid");
                    }
                }
            }

            //do the calculated fields last, as they rely on time series data
            Logging.Instance.ContinueAccumulator("Activity.Recalculate(calculated fields)");
            foreach (CalculateFieldBase calculate in CaclulateFieldFactory.Instance.PostTimeSeriesCalulators)
            {
                calculate.Recalculate(this, forceCount, forceJustMe);
            }
            Logging.Instance.PauseAccumulator("Activity.Recalculate(calculated fields)");

            Logging.Instance.PauseAccumulator("Activity.Recalculate");
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

                    if (!Climbed.Contains(hill))
                        Climbed.Add(hill);
                    if (!hill.Climbed.Contains(this))
                        hill.Climbed.Add(this);
                }
            }
            //if (Options.Instance.LogLevel == Options.Level.Debug && force)
            //    Logging.Instance.Debug($"Hill matching took {Logging.Instance.GetAndResetTime("hills")}, {nochecked} checked, {nomatched} matched");
        }

        private const string TagStravaActivityID = "Strava ID";
        public const string TagPrimarykey = TagStravaActivityID;
        private const string TagStartDateAndTimeLocal = "Start DateTime Local"; //be explicit about the time part, as sometimes we only want the date component
        private const string TagStartDateAndTimeUTC = "Start DateTime UTC"; //be explicit about the time part, as sometimes we only want the date component
        private const string TagActivityType = "Type";
        private const string FilenameTag = "Filename";
        private const string TagFileFullPath = "Filepath";
        public const string TagDescription = "Description";
        public const string TagName = "Name";
        public const string TagType = "Type";
        public const string TagAltitude = "Altitude";

        public const string TagDistance = "Distance"; //both a time series and a datum
        public const string TagElapsedTime = "Elapsed Time";
        public const string TagHeartRate = "Heart Rate";
        public const string TagPower = "Power";
        public const string TagHrPwr = "HrPwr";
        public const string TagMovingTime = "Moving Time";
        public const string TagTreadmillAngle = "Treadmill Angle";
        public static List<string> ActivityTypeRun = new List<string> { "Run", "Virtual Run" };
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

        public void ClearDataQualityIssues()
        {
            DataQualityIssues = null;
            foreach (KeyValuePair<string, TimeSeriesBase> kvp2 in TimeSeries)
                kvp2.Value.Highlights = null;

        }

        [MemoryPackOrder(6)]
        public List<Hill>? Climbed { get; set; } = null; //an empty list means we've checked and there's no matches


        public bool CheckSportType(List<string> sportsToInclude)
        {
            string? activitySportType = this.ActivityType?.Trim(); //had spaces after sport
            //if (activity.StartDateNoTime == DateTime.Now.AddDays(-1).Date)
            //{
            //    MessageBox.Show(activitySportType);
            //}
            if (activitySportType == null || sportsToInclude == null)
                return false;
            if (!sportsToInclude.Contains(activitySportType))
                return false;
            return true;
        }

    }
}
