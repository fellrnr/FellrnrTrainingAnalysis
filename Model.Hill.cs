using CsvHelper;
using CsvHelper.Configuration.Attributes;
using MemoryPack;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json.Serialization;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class Hill
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


        [MemoryPackInclude]
        [Name(" hillname")] //note leading space
        public string Name { get; set; }

        [MemoryPackInclude]
        [Name("hillnumber")]
        public int Number { get; set; }

        [MemoryPackInclude]
        [Name(" latitude")] //note leading space
        public float Latitude { get; set; }

        [MemoryPackInclude]
        [Name(" longitude")] //note leading space
        public float Longitude { get; set; }

        [MemoryPackInclude]
        [Name(" metres")] //note leading space
        public float Elevation { get; set; } //meters

        [JsonIgnore]
        [Ignore]
        [MemoryPackIgnore]
        public int ClimbedCount { get { return Climbed.Count(); } }

        [MemoryPackInclude]
        [JsonIgnore]
        [Ignore]
        public List<Activity> Climbed { get; set; } = new List<Activity>();

        [MemoryPackInclude]
        [JsonIgnore]
        [Ignore]
        public List<string> Classifications { get; set; } = new List<string>();

        [MemoryPackIgnore]
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

        [MemoryPackIgnore]
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
                        string trimmedClass = c.Trim();
                        if (ClassificationLookup.ContainsKey(trimmedClass))
                        {
                            classifications.Add(ClassificationLookup[trimmedClass]);
                        }
                    }
                    if (classifications.Count > 0)
                    {
                        Hill hill = new Hill(record.Name, record.Number, record.Latitude, record.Longitude, record.Elevation, new List<Activity>(), classifications);
                        string hillkey = hill.Name + classCsv;
                        if (!sortedList.ContainsKey(hillkey))
                            sortedList.Add(hillkey, hill); //duplicate hill names, even with the classification added
                        if (Utils.Options.Instance.DebugHills)
                            Utils.Logging.Instance.Debug($"{record.Name}, #{record.Number}, {record.Latitude}/{record.Longitude}/{record.Elevation}m, {classCsv}");
                    }
                }
            }
            List<Hill> retval = sortedList.Values.ToList();

            if (Utils.Options.Instance.DebugHills)
                Dump(retval);

            return retval;
        }

        public static void Dump(List<Hill> retval, string classification = "")
        {
            Utils.Logging.Instance.Debug($"Total {retval.Count} hills");
            foreach (Hill hill in retval)
            {
                if (classification == "" || hill.IsA(classification))
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
            //return $"{Name}, #{Number}, {Latitude}/{Longitude}/{Elevation}m, Climbed {ClimbedCount}, {string.Join(",", Classifications)}";
            return $"{Name}, {Elevation:#,0}m, Climbed x{ClimbedCount}";
        }


        //hillnumber, hillname, region, parent, classification, metres, feet, gridref, gridref10, colgridref, colheight, drop, feature, observations, survey, revision, comments, map50, map25, xcoord, ycoord, latitude, longitude, country, climbed, tumponly,Ma,Ma=,Hu,Hu=,Tu,Sim,5,M,MT,F,C,G,D,DT,Hew,N,Dew,DDew,HF,4,3,2,1,0,W,WO,B,E,HHB,Sy,Fel,CoH,CoU,CoA,CoL,SIB,sMa,sHu,sSim,s5,s4,Mur,CT,GT,DN,BL,Bg,Y,Cm,T100,xMT,xC,xG,xN,xDT,O,Un,P600,P500


        //Number,Name,Parent (SMC),Parent name (SMC),Section,Region,Area,Island,Topo Section,County,Classification,Map 1:50k,Map 1:25k,Metres,Feet,Grid ref,Grid ref 10,Drop,Col grid ref,Col height,Feature,Observations,Survey,Climbed,Country,County Top,Revision,Comments,Streetmap/MountainViews,Geograph,Hill-bagging,Xcoord,Ycoord,Latitude,Longitude,GridrefXY,_Section,Parent (Ma),Parent name (Ma),MVNumber,Ma,Ma=,Hu,Hu=,Tu,Sim,5,M,MT,F,C,G,D,DT,Hew,N,Dew,DDew,HF,4,3,2,1,0,W,WO,B,E,HHB,Sy,Fel,CoH,CoH=,CoU,CoU=,CoA,CoA=,CoL,CoL=,SIB,sMa,sHu,sSim,s5,s4,Mur,CT,GT,BL,Bg,Y,Cm,T100,xMT,xC,xG,xN,xDT,Dil,VL,A,Ca,Bin,O,Un

        private class HillCsv
        {
            //remember, CSV helper requires a default constructor

            [Name("Number")]
            public int Number { get; set; }

            [Name("Name")]
            public string Name { get; set; } = "";

            [Name("Latitude")]
            public float Latitude { get; set; }

            [Name("Longitude")]
            public float Longitude { get; set; }

            [Name("Metres")]
            public float Elevation { get; set; } //meters

            [Name("Classification")]
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

        public static ReadOnlyCollection<string> Classes = new List<string>()
        {
"Wainwright",
"Wainwright Outlying Fell",
"Birkett",
"Marilyn",
"Marilyn twin-top",
"Hump",
"Hump twin-top",
"Tump",
"Simm",
"Dodd",
"Munro",
"Munro Top",
"Furth",
"Corbett",
"Graham",
"Donald",
"Donald Top",
"Hewitt",
"Nuttall",
"Dewey",
"Donald Dewey",
"Highland Five",
"400-499m Tump",
"300-399m Tump",
"200-299m Tump",
"100-199m Tump",
"0-99m Tump",
"Ethel",
"High Hill of Britain",
"Synge",
"Fellranger",
"Historic County Top",
"Historic County twin-top",
"Current County/UA Top",
"Current County/UA twin-top",
"Administrative County Top",
"Administrative County twin-top",
"London Borough Top",
"London Borough twin-top",
"Significant Island of Britain",
"Submarilyn",
"Subhump",
"Subsimm",
"Subdodd",
"Sub 490-499m hill",
"Murdo",
        }.AsReadOnly();


        //TODO: make which hills to load configurable. (Loading them all will require a lot of processing power when scanning activities.) 
        private static Dictionary<string, string> ClassificationLookup = new Dictionary<string, string>()
        {
{ "W",  "Wainwright" },
{ "WO", "Wainwright Outlying Fell" },
{ "B",  "Birkett" },
{ "M",  "Munro" },
//{ "MT", "Munro Top" },
{ "Ma", "Marilyn" },
//{ "Ma=",    "Marilyn twin-top" },
//{ "Hu", "Hump" },
//{ "Hu=",    "Hump twin-top" },
//{ "Tu", "Tump" },
//{ "Sim",    "Simm" },
//{ "5",  "Dodd" },
//{ "F",  "Furth" },
//{ "C",  "Corbett" },
//{ "G",  "Graham" },
//{ "D",  "Donald" },
//{ "DT", "Donald Top" },
//{ "Hew",    "Hewitt" },
//{ "N",  "Nuttall" },
//{ "Dew",    "Dewey" },
//{ "DDew",   "Donald Dewey" },
//{ "HF", "Highland Five" },
//{ "4",  "400-499m Tump" },
//{ "3",  "300-399m Tump" },
//{ "2",  "200-299m Tump" },
//{ "1",  "100-199m Tump" },
//{ "0",  "0-99m Tump" },
//{ "E",  "Ethel" },
//{ "HHB",    "High Hill of Britain" },
//{ "Sy", "Synge" },
{ "Fel",    "Fellranger" },
//{ "CoH",    "Historic County Top" },
//{ "CoH=",   "Historic County twin-top" },
//{ "CoU",    "Current County/UA Top" },
//{ "CoU=",   "Current County/UA twin-top" },
//{ "CoA",    "Administrative County Top" },
//{ "CoA=",   "Administrative County twin-top" },
//{ "CoL",    "London Borough Top" },
//{ "CoL=",   "London Borough twin-top" },
//{ "SIB",    "Significant Island of Britain" },
//{ "sMa",    "Submarilyn" },
//{ "sHu",    "Subhump" },
//{ "sSim",   "Subsimm" },
//{ "s5", "Subdodd" },
//{ "s4", "Sub 490-499m hill" },
//{ "Mur",    "Murdo" },

        };

    }
}
