using CsvHelper;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    //This class allows us to map from an input to internal values. This is because different sources might call one datum (e.g. start time) various different things. 
    public class ActivityDatumMapping
    {
        public ActivityDatumMapping() { ExternalName = ""; InternalName = ""; Comment = ""; } 


        public enum DataSourceEnum { StravaCSV, StravaAPI, FitFile }
        public DataSourceEnum DataSource { get; set; }
        public enum LevelType { Athlete, Day, Activity, DataStream }
        public LevelType Level { get; set; }
        public string ExternalName { get; set; }
        public string InternalName { get; set; }


        //TODO: support boolean flags for commute?
        //TODO: support time zones; the Strava CSV is UTC time, Strava API gives both, FIT give UTC plus offset
        //Note, we could use Int for identifiers, such as strava activity id, but string is easier and fast enough
        //TimeSpan time series are converted to float number of seconds 
        public enum DataTypeEnum { Float, String, DateTime, TimeSpan }
        public DataTypeEnum DataType { get; set; }

        public bool Import { get; set; } //if there's no entry, then the default is to not import. 

        public float ScalingFactor { get; set; } //for type Float only

        public string Comment { get; set; } 

        private static Dictionary<DataSourceEnum, Dictionary<LevelType, Dictionary<string, ActivityDatumMapping>>>? map = null;
        public static ActivityDatumMapping? MapRecord(DataSourceEnum dataSource, LevelType level, string externalName) 
        { 
            if(map == null)
            {
                map = new Dictionary<DataSourceEnum, Dictionary<LevelType, Dictionary<string, ActivityDatumMapping>>>();
                using (var reader = new StreamReader("Config.ActivityDatumMapping.csv"))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<ActivityDatumMapping>();
                    
                    foreach (var record in records)
                    {
                        if(!map.ContainsKey(record.DataSource))
                        {
                            map.Add(record.DataSource, new Dictionary<LevelType, Dictionary<string, ActivityDatumMapping>>());
                        }
                        if (!map[record.DataSource].ContainsKey(record.Level))
                        {
                            map[record.DataSource].Add(record.Level, new Dictionary<string, ActivityDatumMapping>());
                        }
                        if (!map[record.DataSource][record.Level].ContainsKey(record.ExternalName))
                        {
                            map[record.DataSource][record.Level].Add(record.ExternalName, record);
                        }

                    }
                }
            }
            if (!map.ContainsKey(dataSource))
            {
                return null;
            }
            if (!map[dataSource].ContainsKey(level))
            {
                return null;
            }
            if (!map[dataSource][level].ContainsKey(externalName))
            {
                return null;
            }
            return map[dataSource][level][externalName];
        }

    }
}

/*
 * 
 * The following is a list of all fields found in my fit files


Found key 1st SmO2 Sensor 1371 on L. Quad, count 22731
Found key 1st THb Sensor 1371 on L. Quad, count 22731
Found key AccumulatedPower, count 954511
Found key ActivityType, count 484422
Found key Ahead Time, count 79502
Found key Air Power, count 101715
Found key Altitude, count 13048609
Found key Cadence, count 18300843
Found key currBrakingGs, count 45320
Found key currContactTime, count 45320
Found key currFlightRatio, count 45320
Found key currFSType, count 45320
Found key currHeartRate, count 167565
Found key currHemoConc, count 8459
Found key currHemoPerc, count 8459
Found key currImpactGs, count 45320
Found key currPower, count 45320
Found key currPronation, count 45320
Found key Distance, count 17758516
Found key eE, count 2058425
Found key Elevation, count 1922077
Found key EnhancedAltitude, count 15561577
Found key EnhancedSpeed, count 10147876
Found key Fellrnr_FS_cardic_cost, count 5669742
Found key Fellrnr_FS_hrpwr, count 5669742
Found key Fellrnr_FS_power, count 5669742
Found key Fellrnr_FS_smooth_hr, count 5669742
Found key fellrnr_FS_smooth_pace, count 5664916
Found key fellrnr_FS_smooth_power, count 4826
Found key Fellrnr_FS_smooth_power, count 5669742
Found key fellrnr_hrpwr, count 529118
Found key fellrnr_power_estimate, count 529118
Found key Fellrnr_SHP_cardic_cost, count 4899113
Found key Fellrnr_SHP_hrpwr, count 4899113
Found key Fellrnr_SHP_power, count 4899113
Found key Fellrnr_SHP_smooth_hr, count 4899113
Found key Fellrnr_SHP_smooth_power, count 4899113
Found key fellrnr_smooth_hr, count 529118
Found key fellrnr_smooth_power, count 156494
Found key fellrnr_smooth_power_estimate, count 372624
Found key Form Power, count 2025588
Found key FractionalCadence, count 10133331
Found key frnrRE, count 572649
Found key Ground Time, count 2240679
Found key HeartRate, count 16930647
Found key hrpwr, count 13768
Found key inclineRunn, count 107211
Found key LeftPedalSmoothness, count 11816
Found key LeftRightBalance, count 734924
Found key LeftTorqueEffectiveness, count 11816
Found key Leg Spring Stiffness, count 2025588
Found key PositionLat, count 13617568
Found key PositionLong, count 13617568
Found key power, count 13389
Found key Power, count 3324405
Found key pwr, count 1574053
Found key rE, count 2005949
Found key Resistance, count 156314
Found key RightPedalSmoothness, count 11816
Found key RightTorqueEffectiveness, count 11816
Found key RP_Power, count 55121
Found key RS_Braking_GS_L, count 17220
Found key RS_Braking_GS_R, count 17220
Found key RS_ContactTime_L, count 17220
Found key RS_ContactTime_R, count 17220
Found key RS_Flight_L, count 13389
Found key RS_Flight_R, count 13389
Found key RS_FootStrike_L, count 17220
Found key RS_FootStrike_R, count 17220
Found key RS_Impact_GS_L, count 17220
Found key RS_Impact_GS_R, count 17220
Found key RS_Power_AVG, count 3831
Found key RS_Pronation_L, count 3831
Found key RS_Pronation_R, count 3831
Found key SaturatedHemoglobinPercent, count 585267
Found key smooth_hr, count 13768
Found key smooth_pwr, count 13768
Found key Speed, count 7479033
Found key StanceTime, count 468338
Found key StanceTimeBalance, count 468351
Found key StanceTimePercent, count 468338
Found key StepLength, count 480601
Found key Temperature, count 10146434
Found key TotalHemoglobinConc, count 585267
Found key tpRE, count 572846
Found key unknown, count 16143882
Found key Vertical Oscillation, count 2240679
Found key VerticalOscillation, count 480626
Found key VerticalRatio, count 480600
 * 
 */
