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
        public enum DisplayUnitsType { Meters, Kilometers, Pace, TimeSpan, BPM, Integer, Percent, None }; //Integer is misc value

        public DisplayUnitsType DisplayUnits { get; set; } = DisplayUnitsType.None;


        public int? PositionInTree { get; set; } //null for don't show
        public int? PositionInReport { get; set; }  //null for don't show

        public int? ColumnSize { get; set; } //null for resize dynamically


        public int? DecimalPlaces { get; set; } //Only for floating point numbers, obviously

        public bool? Invisible { get; set; } //for hidden columns like Strava ID

        public string Comment { get; set; } = ""; //for commenting the CSV file

        private const string PathToCsv = "Config.ActivityDatumMetadata.csv";

        private static Dictionary<string, ActivityDatumMetadata>? _map = null;
        private static int _maxReportColumn = int.MinValue;
        private static Dictionary<string, ActivityDatumMetadata> Map
        {
            get
            {
                if (_map == null)
                {
                    _maxReportColumn = int.MinValue;
                    _map = ReadFromCsv();
                }

                if (_map == null)
                    _map = new Dictionary<string, ActivityDatumMetadata>();

                return _map;
            }
        }

        public static ActivityDatumMetadata? FindMetadata(string name)
        {
            if (!Map.ContainsKey(name))
            {
                //return null; 
                //Let's generate any missing definitions to make editing them easier
                ActivityDatumMetadata activityDatumMetadata = new ActivityDatumMetadata();
                activityDatumMetadata.Name = name;
                activityDatumMetadata.Title = name;
                activityDatumMetadata.PositionInReport = null;
                activityDatumMetadata.PositionInTree = null;
                Map.Add(name, activityDatumMetadata);

                return activityDatumMetadata;
            }
            return Map[name];
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
            if (Map == null)
                return;
            List<ActivityDatumMetadata> definitions = Map.Values.ToList();
            using (var writer = new StreamWriter(PathToCsv))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(definitions);
            }
        }


        //don't use a property as it will confuse the ObjectListView editor
        public static List<ActivityDatumMetadata>? GetDefinitions() { return Map?.Values.ToList(); }
        public static void SetDefinitions(List<ActivityDatumMetadata> value)
        {
            _maxReportColumn = int.MinValue;
            if (value == null)
            {
                _map = null;
                return;
            }
            _map = new Dictionary<string, ActivityDatumMetadata>();
            foreach (ActivityDatumMetadata activityDatumMetadata in value)
                _map.Add(activityDatumMetadata.Name, activityDatumMetadata);
            WriteToCsv();
        }


        public static int LastPositionInReport()
        {
            if(_maxReportColumn != int.MinValue) return _maxReportColumn+1;

            _maxReportColumn = 0;
            foreach(KeyValuePair<string, ActivityDatumMetadata> kvp in Map)
            {
                if (kvp.Value.PositionInReport != null)
                {
                    int positionInReport = (int)kvp.Value.PositionInReport;
                    _maxReportColumn = Math.Max(_maxReportColumn, positionInReport);
                }
            }
            return _maxReportColumn +1; //positions are zero indexed
        }
    }
}
