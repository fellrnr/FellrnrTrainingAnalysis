using de.schumacher_bw.Strava.Endpoint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class Activity : Extensible
    {
        public Activity()
        {
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


        //NB:The date of an activity could change with the time zone. Not sure if Strava has these in UTC or local time
        //An activity must have a start date
        public DateTime? StartDateTime { 
            get { return GetNamedDateTimeDatum(StartDateAndTimeTag); }
            set { if(value != null) AddOrReplaceDatum(new TypedDatum<DateTime>(StartDateAndTimeTag, true, (DateTime)value)); }
        }
        public DateTime? StartDateNoTime { get { return StartDateTime == null ? null : ((DateTime)StartDateTime).Date; } }


        public override Utils.DateTimeTree Id { get { return new Utils.DateTimeTree(StartDateTime!.Value, Utils.DateTimeTree.DateTreeType.Time); } } //HACK: to see if tree works


        public string? Filename { get { return GetNamedStringDatum(FilenameTag); } }
        public string? ActivityType { get { return GetNamedStringDatum(ActivityTypeTag); } }

        //This needs to be static so callers can validate and find an activity given a dictionary of string/Datum by primary keys
        public static bool WillBeValid(Dictionary<string, Datum> activityData) { return activityData.ContainsKey(Activity.StravaActivityIDTag) && activityData.ContainsKey(Activity.StartDateAndTimeTag); }

        //This needs to be static so callers can find an activity given a dictionary of string/Datum by primary keys
        public static string ExpectedPrimaryKey(Dictionary<string, Datum> activityData) { return ((TypedDatum<string>)activityData[Activity.StravaActivityIDTag]).Data; }

        public string PrimaryKey() { return ((TypedDatum<string>)Data[Activity.StravaActivityIDTag]).Data; }


        //This needs to be static so callers can find the date to create the Day so they can create the Activity
        public static DateTime ExpectedStartDateTime(Dictionary<string, Datum> activityData) { return ((TypedDatum<DateTime>)activityData[Activity.StartDateAndTimeTag]).Data; }

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
        private Dictionary<string, IDataStream> timeSeries = new Dictionary<string, IDataStream>();

        public ReadOnlyDictionary<string, IDataStream> TimeSeries { get { return timeSeries.AsReadOnly(); } }

        public List<String> TimeSeriesNames { get { return TimeSeries.Keys.ToList(); } } 

        public void AddDataStreams(Dictionary<string, KeyValuePair<List<uint>, List<float>>> dataStreams)
        {
            foreach (KeyValuePair<string, KeyValuePair<List<uint>, List<float>>> kvp in dataStreams)
            {
                string name = kvp.Key;
                if (!TimeSeries.ContainsKey(name))
                {
                    KeyValuePair<List<uint>, List<float>> timesAndValues = kvp.Value;
                    List<uint> times = timesAndValues.Key;
                    List<float> values = timesAndValues.Value;
                    AddDataStream(name, times.ToArray(), values.ToArray());
                }

            }
        }


        public void AddDataStream(string name, uint[] times, float[] values)
        {
            if (!timeSeries.ContainsKey(name))
            {
                DataStream activityDataStream = new DataStream(name, new Tuple<uint[], float[]>(times, values));
                timeSeries.Add(name, activityDataStream);
            }
        }

        public void AddDataStream(IDataStream dataStream)
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

        //TODO: Add HRV data to activity data, not really a time series

        //TODO: Add Lap data, not a time series


        //a list of URIs to photos that were attached to the workout, typically from Strava
        public List<Uri>? PhotoUris { get; set; }

        public override void Recalculate(bool force)
        {
            base.Recalculate(force);
            List<IDataStream> dataStreams = DataStreamFactory.Instance.DataStreams;

            foreach (IDataStream dataStream in dataStreams)
            {
                if (dataStream.IsValid(this))
                {
                    try
                    {
                        dataStream.Recalculate(this, force);
                        this.AddDataStream(dataStream);
                    }
                    catch (Exception ex)
                    {
                        Logging.Instance.Error(string.Format("Failed to process activity {0} from {1} for data stream {2} due to {3}", 
                            this.PrimaryKey(), this.StartDateTime, dataStream.Name, ex));
                    }
                }
            }

        }


        private const string StravaActivityIDTag = "Strava ID";
        public const string PrimarykeyTag = StravaActivityIDTag;
        private const string StartDateAndTimeTag = "Start DateTime"; //be explicit about the time part, as sometimes we only want the date component
        private const string ActivityTypeTag = "Type";
        private const string FilenameTag = "Filename";
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
        public const string MovingTimeTag = "Moving Time";
        public const string DistanceTag = "Distance";
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
    }
}
