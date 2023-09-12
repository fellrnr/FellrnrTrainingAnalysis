using CsvHelper;
using System.Globalization;

namespace FellrnrTrainingAnalysis.Model
{
    public class DataStreamDefinition
    {
        public DataStreamDefinition() { }

        public string Name { get; set; } = "";

        public bool ShowReportGraph { get; set; } = true;

        public string Description { get; set; } = "";

        public string DisplayTitle { get; set; } = "";
        public enum SmoothingType { AverageWindow, SimpleExponential, InterpolateOnly, None };

        public SmoothingType Smoothing { get; set; } = SmoothingType.None;

        public int SmoothingWindow { get; set; } = 50;

        public enum DisplayUnitsType { Meters, Kilometers, Pace, None };

        public DisplayUnitsType DisplayUnits { get; set; } = DisplayUnitsType.None;

        public string DisplayUnitsName { get; set; } = "";

        //Just to comment the CSV file
        public string Comment { get; set; } = "";

        private const string PathToCsv = "Config.DataStreamDefinition.csv";
        private static Dictionary<string, DataStreamDefinition>? map = null;
        public static DataStreamDefinition? FindDataStreamDefinition(string name)
        {
            if (map == null)
            {
                map = ReadFromCsv();
            }
            if (!map.ContainsKey(name))
            {
                //return null; 
                //Let's generate any missing definitions to make editing them easier
                DataStreamDefinition dataStreamDefinition = new DataStreamDefinition();
                dataStreamDefinition.Name = name;
                dataStreamDefinition.DisplayTitle = name;
                dataStreamDefinition.ShowReportGraph = false;
                map.Add(name, dataStreamDefinition);
                return dataStreamDefinition;
            }
            return map[name];
        }

        private static Dictionary<string, DataStreamDefinition> ReadFromCsv()
        {
            Dictionary<string, DataStreamDefinition> returnMap = new Dictionary<string, DataStreamDefinition>();
            using (var reader = new StreamReader(PathToCsv))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<DataStreamDefinition>();

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
            if(map == null)
                return;
            List<DataStreamDefinition> definitions = map.Values.ToList();
            using (var writer = new StreamWriter(PathToCsv))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(definitions);
            }
        }


        //don't use a property as it will confuse the ObjectListView editor
        public static List<DataStreamDefinition>? GetDefinitions() { return map?.Values.ToList(); }
        public static void SetDefinitions(List<DataStreamDefinition> value)
        {
            if (value == null)
            {
                map = null;
                return;
            }
            map = new Dictionary<string, DataStreamDefinition>();
            foreach (DataStreamDefinition dataStreamDefinition in value)
                map.Add(dataStreamDefinition.Name, dataStreamDefinition);
            WriteToCsv();
        }
    }
}
