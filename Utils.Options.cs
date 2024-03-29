﻿using System.Text.Json;
using System.ComponentModel;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;

namespace FellrnrTrainingAnalysis.Utils
{
    public class Options
    {
        public Options() //has to be public for the deserializer to create one
        {
        }
        static Options() { }
        public static Options Instance { get; set; } = new Options();

        private const string fileName = @"FellrnrTrainingAnalysisConfig.json";
        static string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string AppDataSubFolder = "FellrnrTrainingData";
        static string AppDataPath = Path.Combine(AppDataFolder, AppDataSubFolder);

        public static void LoadConfig()
        {
            string path = Path.Combine(AppDataPath, fileName);
            if (File.Exists(path))
            {
                string jsonFromFile = File.ReadAllText(path);
                Options? Config = JsonSerializer.Deserialize<Options>(jsonFromFile);
                if (Config != null)
                {
                    Instance = Config;
                }
                else
                {
                    Logging.Instance.Error(string.Format("Failed to deserialize options from {0}", path));
                }
            }
            else
            {
                Logging.Instance.Error(string.Format("File does not exist for options - {0}", path));
            }

        }

        public static void SaveConfig()
        {
            string json = JsonSerializer.Serialize(Instance);
            string path = Path.Combine(AppDataPath, fileName);
            File.WriteAllText(path, json);
        }

        //for more big text:
        //http://patorjk.com/software/taag/#p=display&f=Big&t=Debug
        //http://patorjk.com/software/taag/#p=display&c=c%2B%2B&f=Big&t=Debug

        //Import
        //    _____                            _   
        //   |_   _|                          | |  
        //     | |  _ __ ___  _ __   ___  _ __| |_ 
        //     | | | '_ ` _ \| '_ \ / _ \| '__| __|
        //    _| |_| | | | | | |_) | (_) | |  | |_ 
        //   |_____|_| |_| |_| .__/ \___/|_|   \__|
        //                   | |                   
        //                   |_|                   

        public DateTime? OnlyLoadAfter { get; set; }
        public bool ImportUnknownFitFields { get; set; } = false; //May only save a tiny amount of time and space, but reduces the noise of extra fields
        public bool ImportDeveloperFitFields { get; set; } = false;

        //Strava
        //     _____ _                        
        //    / ____| |                       
        //   | (___ | |_ _ __ __ ___   ____ _ 
        //    \___ \| __| '__/ _` \ \ / / _` |
        //    ____) | |_| | | (_| |\ V / (_| |
        //   |_____/ \__|_|  \__,_| \_/ \__,_|
        //                                    
        //                                    
        public int ClientId { get; set; } = 0;
        public string ClientSecret { get; set; } = "";


        public bool StravaApiOveridesData { get; set; } = false;

        //Stream numbers...
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

        [Description("2048-1 is get all streams currently defined. See StravaApiV3Sharp - StreamTypes.cs for details")]
        public int StravaStreamTypesToRetrieve { get; set; } = 2048 - 1;

        [Description("Strava doesn't tell you where the timer pauses are, so you have to guess. If there's a gap longer than this in the time stream, we'll assume it's a timer pause")]
        public int StravaMaximumGap = 5;




        //Grade adjusted pace
        //     _____               _                    _ _           _           _   _____               
        //    / ____|             | |          /\      | (_)         | |         | | |  __ \              
        //   | |  __ _ __ __ _  __| | ___     /  \   __| |_ _   _ ___| |_ ___  __| | | |__) |_ _  ___ ___ 
        //   | | |_ | '__/ _` |/ _` |/ _ \   / /\ \ / _` | | | | / __| __/ _ \/ _` | |  ___/ _` |/ __/ _ \
        //   | |__| | | | (_| | (_| |  __/  / ____ \ (_| | | |_| \__ \ ||  __/ (_| | | |  | (_| | (_|  __/
        //    \_____|_|  \__,_|\__,_|\___| /_/    \_\__,_| |\__,_|___/\__\___|\__,_| |_|   \__,_|\___\___|
        //                                              _/ |                                              
        //                                             |__/                                               

        [Description("Multiplier for X squared where X is the slope")]
        public double GradeAdjustmentX2 { get; set; } = 15.14;

        [Description("Multiplier for X where X is the slope")]
        public double GradeAdjustmentX { get; set; } = 2.896;

        [Description("Offset for grade adjustment")]
        public double GradeAdjustmentOffset { get; set; } = 1.00; //was 1.0098 but that's silly as flat has to be 1.0

        [Description("Min slope (GPX can have noise that creates silly slopes)")]
        public double MinSlope { get; set; } = -0.5;

        [Description("Max slope (GPX can have noise that creates silly slopes)")]
        public double MaxSlope { get; set; } = 0.5;

        [Description("How to do the elevation smoothing")]
        public SmoothingOptions GADSmoothingType { get; set; } = SmoothingOptions.SimpleExponential;

        [Description("How big should the smoothing window be (in meters)")]
        public int GADSmoothingWindow { get; set; } = 50;

        //Debug
        //    _____       _                 
        //   |  __ \     | |                
        //   | |  | | ___| |__  _   _  __ _ 
        //   | |  | |/ _ \ '_ \| | | |/ _` |
        //   | |__| |  __/ |_) | |_| | (_| |
        //   |_____/ \___|_.__/ \__,_|\__, |
        //                             __/ |
        //                            |___/ 

        public enum Level { Debug, Log, Error };
        public Level LogLevel { get; set; } = Level.Debug;

        public bool InMemory { get; set; } = true;

        [Description("Fit performance debug is fairly cheap and just gives how long it took for each file")]
        public bool DebugFitPerformance { get; set;  } = false; //This is fairly cheap, but still adds 40 seconds to a 7 minute import

        [Description("Fit field debug is fairly cheap and gives a summary of all the fields found in the files loaded")]
        public bool DebugFitFields { get; set; } = true;

        [Description("Fit loading debug is expensive and gives a lot of details")]
        public bool DebugFitLoading { get; set;  } = false; //This is very expensive!

        [Description("Fit extra details debug is wildly expensive and gives a lot of details of the things we don't look at")]
        public bool DebugFitExtraDetails { get; set; } = false; //This is very expensive!

        [Description("Add the raw data to the data grids (report) to debug formatting or underlying data issues")]
        public bool DebugAddRawDataToGrids { get; set; } = false;

        [Description("Disable time axis in graphs and display raw data")]
        public bool DebugDisableTimeAxis { get; set; } = false;

        [Description("Debug the hills")]
        public bool DebugHills { get; set; } = false;

        [Description("Debug strava API")]
        public bool DebugStravaAPI { get; set; } = false;


        //Email
        //    ______                 _ _ 
        //   |  ____|               (_) |
        //   | |__   _ __ ___   __ _ _| |
        //   |  __| | '_ ` _ \ / _` | | |
        //   | |____| | | | | | (_| | | |
        //   |______|_| |_| |_|\__,_|_|_|
        //                               
        //                               
        [Description("Send email with goals data - SMTP host")]
        public string EmailSmtpHost { get; set; } = "smtp.gmail.com";

        [Description("Send email with goals data - SMTP port")]
        public int EmailSmtpPort { get; set; } = 587;

        [Description("Send email with goals data - sending email account")]
        public string EmailAccount { get; set; } = "";

        [Description("Send email with goals data - sending email app password *make sure you understand the security risks of this*")]
        public string EmailPassword { get; set; } = "";

        [Description("Send email with goals data - where to send the message")]
        public string EmailDestination { get; set; } = "";


        //Physiological Metrics
        //    _____  _               _       _             _           _   __  __      _        _          
        //   |  __ \| |             (_)     | |           (_)         | | |  \/  |    | |      (_)         
        //   | |__) | |__  _   _ ___ _  ___ | | ___   __ _ _  ___ __ _| | | \  / | ___| |_ _ __ _  ___ ___ 
        //   |  ___/| '_ \| | | / __| |/ _ \| |/ _ \ / _` | |/ __/ _` | | | |\/| |/ _ \ __| '__| |/ __/ __|
        //   | |    | | | | |_| \__ \ | (_) | | (_) | (_| | | (_| (_| | | | |  | |  __/ |_| |  | | (__\__ \
        //   |_|    |_| |_|\__, |___/_|\___/|_|\___/ \__, |_|\___\__,_|_| |_|  |_|\___|\__|_|  |_|\___|___/
        //                  __/ |                     __/ |                                                
        //                 |___/                     |___/                                                 
        [Description("Starting point (in the absense of daily values) for Critical Power in Watts")]
        public int StartingCriticalPower { get; set; } = 250;

        [Description("Starting point (in the absense of daily values) for Max Heart Rate")]
        public int StartingMaxHeartRate { get; set; } = 174;

        [Description("Starting point (in the absense of daily values) for Resting Heart Rate")]
        public int StartingRestingHeartRate { get; set; } = 45;

        [Description("Starting point (in the absense of daily values) for Weight (Kg)")]
        public float StartingWeight { get; set; } = 60.0f;

        [Description("Offset from Resting Heart Rate to Standing Heart Rate, normally 26")]
        public int RestingHeartRateToStanding { get; set; } = 26;

        //    _____  _           _             
        //   |  __ \(_)         | |            
        //   | |  | |_ ___ _ __ | | __ _ _   _ 
        //   | |  | | / __| '_ \| |/ _` | | | |
        //   | |__| | \__ \ |_) | | (_| | |_| |
        //   |_____/|_|___/ .__/|_|\__,_|\__, |
        //                | |             __/ |
        //                |_|            |___/ 

        [Description("Seperate Activity Graphs rather than overlaying them")]
        public bool SpaceOutActivityGraphs { get; set; } = false;

        [Description("The activity types to show")]
        public string OnlyShowActivityTypes { get; set; } = "";


    }
}
