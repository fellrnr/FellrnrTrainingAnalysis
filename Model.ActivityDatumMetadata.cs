using CsvHelper;
using System.Globalization;

namespace FellrnrTrainingAnalysis.Model
{
    //It also gives us critical metadata, such as should this field be used for filtering, what are the units, etc. 
    public class ActivityDatumMetadata
    {
        public ActivityDatumMetadata() { }
        //public string FellrnrName { get; set; } //our internal name

        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public enum DisplayUnitsType { Meters, Kilometers, Pace, TimeSpan, BPM, Integer, None }; //Integer is misc value

        public DisplayUnitsType DisplayUnits { get; set; } = DisplayUnitsType.None;


        public int? PositionInTree { get; set; } //null for don't show
        public int? PositionInReport { get; set; }  //null for don't show

        public int? ColumnSize { get; set; } //null for resize dynamically


        public int? DecimalPlaces { get; set; } //Only for floating point numbers, obviously

        public bool? Invisible { get; set; } //for hidden columns like Strava ID

        public string Comment { get; set; } = ""; //for commenting the CSV file

        private const string PathToCsv = "Config.ActivityDatumMetadata.csv";
        private static Dictionary<string, ActivityDatumMetadata>? map = null;
        public static ActivityDatumMetadata? FindMetadata(string name)
        {
            if (map == null)
                map = ReadFromCsv();
            
            if (map == null)
                map = new Dictionary<string, ActivityDatumMetadata>();

            if (!map.ContainsKey(name))
            {
                //return null; 
                //Let's generate any missing definitions to make editing them easier
                ActivityDatumMetadata activityDatumMetadata = new ActivityDatumMetadata();
                activityDatumMetadata.Name = name;
                activityDatumMetadata.Title = name;
                activityDatumMetadata.PositionInReport = null;
                activityDatumMetadata.PositionInTree = null;
                map.Add(name, activityDatumMetadata);

                return activityDatumMetadata;
            }
            return map[name];
        }

        private static Dictionary<string, ActivityDatumMetadata>? ReadFromCsv()
        {
            Dictionary<string, ActivityDatumMetadata> returnMap = new Dictionary<string, ActivityDatumMetadata>();
            if(!File.Exists(PathToCsv)) { return null; }
            using (var reader = new StreamReader(PathToCsv))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<ActivityDatumMetadata>();

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
            List<ActivityDatumMetadata> definitions = map.Values.ToList();
            using (var writer = new StreamWriter(PathToCsv))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(definitions);
            }
        }


        //don't use a property as it will confuse the ObjectListView editor
        public static List<ActivityDatumMetadata>? GetDefinitions() { return map?.Values.ToList(); }
        public static void SetDefinitions(List<ActivityDatumMetadata> value)
        {
            if (value == null)
            {
                map = null;
                return;
            }
            map = new Dictionary<string, ActivityDatumMetadata>();
            foreach (ActivityDatumMetadata activityDatumMetadata in value)
                map.Add(activityDatumMetadata.Name, activityDatumMetadata);
            WriteToCsv();
        }


        public static int LastPositionInReport()
        {
            if (map == null)
                map = ReadFromCsv();
            if (map == null)
                return 0;
            int maxReportColumn = 0;
            foreach(KeyValuePair<string, ActivityDatumMetadata> kvp in map)
            {
                if (kvp.Value.PositionInReport != null)
                {
                    int positionInReport = (int)kvp.Value.PositionInReport;
                    maxReportColumn = Math.Max(maxReportColumn, positionInReport);
                }
            }
            return maxReportColumn+1; //positions are zero indexed
        }
    }
}
