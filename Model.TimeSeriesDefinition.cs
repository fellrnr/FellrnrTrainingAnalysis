using CsvHelper;
using FellrnrTrainingAnalysis.Utils;
using System.Globalization;

namespace FellrnrTrainingAnalysis.Model
{
    public class TimeSeriesDefinition
    {
        public TimeSeriesDefinition() { }

        public string Name { get; set; } = "";

        public bool ShowReportGraph { get; set; } = true;

        public string Description { get; set; } = "";

        public string DisplayTitle { get; set; } = "";
        public enum SmoothingType { AverageWindow, SimpleExponential, InterpolateOnly, None };

        public SmoothingType Smoothing { get; set; } = SmoothingType.None;

        public int SmoothingWindow { get; set; } = 50;

        public enum DisplayUnitsType { Meters, Kilometers, Pace, Integer, None };

        public string Format(float value)
        {
            switch (DisplayUnits)
            {
                case DisplayUnitsType.Meters:
                    return Utils.Misc.FormatFloat(value, "{0:#,0} m", 1.0f);
                case DisplayUnitsType.Kilometers:
                    return Utils.Misc.FormatFloat(value, "{0:#,0.0} Km", 1.0f / 1000.0f);
                case DisplayUnitsType.Pace:
                    return Utils.Misc.FormatPace(value);
                case DisplayUnitsType.Integer:
                    return Utils.Misc.FormatFloat(value, "{0:#,0}", 1.0f);
                case DisplayUnitsType.None:
                    return value.ToString()!;
                default:
                    return "";
            }
        }

        public DisplayUnitsType DisplayUnits { get; set; } = DisplayUnitsType.None;

        public string DisplayUnitsName { get; set; } = "";

        public string ColorName { get; set; } = "";

        public Color GetColor()
        {
            Color retval;
            if (string.IsNullOrEmpty(ColorName))
                retval = Color.Black;
            else
                retval = Color.FromName(ColorName);
            if(retval.R == 255 && retval.G == 255 && retval.B == 255)
                retval = Color.DarkGray;

            retval = Color.FromArgb(Options.Instance.ActivityGraphAlpha, retval);

            return retval;
        }

        //Just to comment the CSV file
        public string Comment { get; set; } = "";

        private const string PathToCsv = "Config.TimeSeriesDefinition.csv";
        private static Dictionary<string, TimeSeriesDefinition>? map = null;
        public static TimeSeriesDefinition? FindTimeSeriesDefinition(string name)
        {
            if (map == null)
            {
                map = ReadFromCsv();
            }
            if (!map.ContainsKey(name))
            {
                //return null; 
                //Let's generate any missing definitions to make editing them easier
                TimeSeriesDefinition dataStreamDefinition = new TimeSeriesDefinition();
                dataStreamDefinition.Name = name;
                dataStreamDefinition.DisplayTitle = name;
                dataStreamDefinition.ShowReportGraph = false;
                map.Add(name, dataStreamDefinition);
                return dataStreamDefinition;
            }
            return map[name];
        }

        private static Dictionary<string, TimeSeriesDefinition> ReadFromCsv()
        {
            Dictionary<string, TimeSeriesDefinition> returnMap = new Dictionary<string, TimeSeriesDefinition>();
            using (var reader = new StreamReader(PathToCsv))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<TimeSeriesDefinition>();

                foreach (var record in records)
                {

                    if (!returnMap.ContainsKey(record.Name))
                    {
                        returnMap.Add(record.Name, record);
                    }
                }
            }
            return returnMap;
        }

        public static void WriteToCsv()
        {
            if (map == null)
                return;
            List<TimeSeriesDefinition> definitions = map.Values.ToList();
            using (var writer = new StreamWriter(PathToCsv))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(definitions);
            }
        }


        //don't use a property as it will confuse the ObjectListView editor
        public static List<TimeSeriesDefinition>? GetDefinitions() { return map?.Values.ToList(); }
        public static void SetDefinitions(List<TimeSeriesDefinition> value)
        {
            if (value == null)
            {
                map = null;
                return;
            }
            map = new Dictionary<string, TimeSeriesDefinition>();
            foreach (TimeSeriesDefinition dataStreamDefinition in value)
                map.Add(dataStreamDefinition.Name, dataStreamDefinition);
            WriteToCsv();
        }
    }
}
