using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FellrnrTrainingAnalysis.Model
{
    //It also gives us critical metadata, such as should this field be used for filtering, what are the units, etc. 
    internal class ActivityDatumMetadata
    {
        public ActivityDatumMetadata() { }
        //public string FellrnrName { get; set; } //our internal name

        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public enum DataTypeEnum { Float, String, DateTime, TimeSpan }
        public DataTypeEnum DataType { get; set; }

        public enum DisplayUnitsType { Meters, Kilometers, Pace, None };

        public DisplayUnitsType DisplayUnits { get; set; } = DisplayUnitsType.None;


        public int PositionInTree { get; set; }
        public int PositionInReport { get; set;}

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
                activityDatumMetadata.PositionInReport = -1;
                activityDatumMetadata.PositionInTree = -1;
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


        public static int LastPositionInTree()
        {
            if (map == null)
                map = ReadFromCsv();
            if (map == null)
                return 0;
            int maxTreeColumn = 0;
            foreach(KeyValuePair<string, ActivityDatumMetadata> kvp in map)
            {
                maxTreeColumn = Math.Max(maxTreeColumn, kvp.Value.PositionInTree);
            }
            return maxTreeColumn+1; //positions are zero indexed
        }
    }
}
