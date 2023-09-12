using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FellrnrTrainingAnalysis.Model
{
    [Serializable]
    public class Hill
    {

        public Hill(string name, int number, float latitude, float longitude, float elevation, List<Activity> climbed, List<string> classifications)
        {
            Name = name;
            Number = number;
            Latitude = latitude;
            Longitude = longitude;
            Elevation = elevation;
            Climbed = climbed;
            Classifications = classifications;
        }

        //hillnumber hillname    region parent  classification metres  feet gridref     gridref10 colgridref  colheight drop    feature observations    survey revision    comments map50   map25 xcoord  ycoord latitude    longitude


        [Name(" hillname")] //note leading space
        public string Name { get; set; }

        [Name("hillnumber")]
        public int Number { get; set; }

        [Name(" latitude")] //note leading space
        public float Latitude { get; }

        [Name(" longitude")] //note leading space
        public float Longitude { get; }

        [Name(" metres")] //note leading space
        public float Elevation { get; set; } //meters

        [Ignore]
        public int ClimbedCount { get { return Climbed.Count(); } }

        [Ignore]
        public List<Activity> Climbed { get; set; } = new List<Activity>();

        [Ignore]
        public List<string> Classifications { get; set; } = new List<string>();

        public string PrimarClassificationCode
        {
            get
            {
                if (Classifications.Contains(WAINWRIGHT))
                    return "(W)";
                else if (Classifications.Contains(WAINWRIGHT_OUTLYING_FELL))
                    return "(WOF)";
                else if (Classifications.Contains(BIRKETT))
                    return "(B)";
                else if (Classifications.Contains(MUNRO))
                    return "(M)";
                return "";
            }
        }

        public string ExtendedName { get { return Name + " " + PrimarClassificationCode; } }

        public bool IsA(string classification) { return Classifications.Contains(classification); }

        public static List<Hill> Reload()
        {
            SortedList<string, Hill> sortedList = new SortedList<string, Hill>();
            using (var reader = new StreamReader("Config.Hills.csv"))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                IEnumerable<HillCsv> records = csv.GetRecords<HillCsv>();

                foreach (HillCsv record in records)
                {
                    string classCsv = record.Classification;
                    string[] classes = classCsv.Split(',');
                    List<string> classifications = new List<string>();
                    foreach (string c in classes)
                    {
                        if (ClassificationLookup.ContainsKey(c))
                        {
                            classifications.Add(ClassificationLookup[c]);
                        }
                    }
                    if (classifications.Count > 0)
                    {
                        Hill hill = new Hill(record.Name, record.Number, record.Latitude, record.Longitude, record.Elevation, new List<Activity>(), classifications);
                        sortedList.Add(hill.Name + classCsv, hill); //duplicate hill names
                        if (Utils.Options.Instance.LogLevel == Utils.Options.Level.Debug)
                            Utils.Logging.Instance.Debug($"{record.Name}, #{record.Number}, {record.Latitude}/{record.Longitude}/{record.Elevation}m, {classCsv}");
                    }
                }
            }
            List<Hill> retval = sortedList.Values.ToList();

            //if (Utils.Options.Instance.LogLevel == Utils.Options.Level.Debug)
                //Dump(retval);

            return retval;
        }

        public static void Dump(List<Hill> retval, string classification = "")
        {
            Utils.Logging.Instance.Debug($"Total {retval.Count} hills");
            foreach(Hill hill in retval) 
            { 
                if(classification == "" || hill.IsA(classification))
                    Utils.Logging.Instance.Debug(hill.ToString());
            }
        }



        public const float CLOSE_ENOUGH = 70; //70 meters is used in other situations

        public float DistanceTo(float lat, float lon)
        {
            double d1 = this.Latitude * (Math.PI / 180.0);
            double num1 = this.Longitude * (Math.PI / 180.0);
            double d2 = lat * (Math.PI / 180.0);
            double num2 = lon * (Math.PI / 180.0) - num1;
            double d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return (float)(6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3))));
        }

        public override string ToString()
        {
            return $"{Name}, #{Number}, {Latitude}/{Longitude}/{Elevation}m, Climbed {ClimbedCount}, {string.Join(",", Classifications)}";
        }

        private class HillCsv
        {
            //remember, CSV helper requires a default constructor

            [Name(" hillname")] //note leading space
            public string Name { get; set; } = "";

            [Name("hillnumber")]
            public int Number { get; set; }

            [Name(" latitude")] //note leading space
            public float Latitude { get; set; }

            [Name(" longitude")] //note leading space
            public float Longitude { get; set; }

            [Name(" metres")] //note leading space
            public float Elevation { get; set; } //meters
            [Name(" classification")] //note leading space
            public string Classification { get; set; } = ""; //csv
        }


        public const string MARILYN = "Marilyn";
        public const string MUNRO = "Munro";
        public const string MUNRO_TOP = "Munro Top";
        public const string CORBETT = "Corbett";
        public const string NUTTALL = "Nuttall";
        public const string WAINWRIGHT = "Wainwright";
        public const string WAINWRIGHT_OUTLYING_FELL = "Wainwright Outlying Fell";
        public const string BIRKETT = "Birkett";

        public ReadOnlyCollection<string> Classes = new List<string>()
        {
            MARILYN,
            MUNRO,
            MUNRO_TOP,
            CORBETT,
            NUTTALL,
            WAINWRIGHT,
            WAINWRIGHT_OUTLYING_FELL,
            BIRKETT,
        }.AsReadOnly();

        private static Dictionary<string, string> ClassificationLookup = new Dictionary<string, string>()
        {
            { "Ma", "Marilyn" },
            { "M", "Munro" },
            { "MT", "Munro Top" },
            { "C", "Corbett" },
            { "N", "Nuttall" },
            { "W", "Wainwright" },
            { "WO", "Wainwright Outlying Fell" },
            { "B", "Birkett" },

        };

    }
}
