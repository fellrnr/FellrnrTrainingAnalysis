using de.schumacher_bw.Strava.Model;
using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.UI;
using GMap.NET;
using GMap.NET.WindowsForms;
using Microsoft.VisualBasic.Logging;
using System.IO.Compression;
using System.Text;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;

namespace FellrnrTrainingAnalysis.Utils
{
    public class Misc //the class of misfit toys
    {

        public static void RunCommand(string target)
        {
            try
            {
                //System.Diagnostics.Process.Start(target);
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = target, UseShellExecute = true }); //use shell execute is false by default now
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        public static Stream DecompressAndOpenFile(string filename)
        {
            //we have to unzip the .gz files strava gives us to a temp file. Using the GZFileStream doesn't work as the FIT toolkit seeks around, which GZ doesn't support

            if (filename.ToLower().EndsWith(".gz"))
            {
                string newfile = filename.Remove(filename.Length - 3);
                if (!File.Exists(newfile))
                {
                    using FileStream compressedFileStream = File.Open(filename, FileMode.Open);
                    using FileStream outputFileStream = File.Create(newfile);
                    using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);
                    decompressor.CopyTo(outputFileStream);
                }
                filename = newfile;
            }

            FileStream fitSource = new FileStream(filename, FileMode.Open, FileAccess.Read);
            return fitSource;
        }


        public static List<PointLatLng> SampleLocations(uint[]? Times, float[] Lats, float[] Lons, uint interval)
        {
            List<PointLatLng> points = new List<PointLatLng>();

            if (Times != null)
            {
                uint lastTime = 0;
                for (int i = 0; i < Lats.Count(); i++)
                {
                    if (Times[i] >= lastTime)
                    {
                        float lat = Lats[i];
                        float lon = Lons[i];
                        points.Add(new PointLatLng(lat, lon));
                        lastTime = Times[i] + interval;
                    }
                }
            }
            else
            {
                //if we have one second recording, this will be close. If not, it's the best we can do
                for (int i = 0; i < Lats.Count(); i += (int)interval)
                {
                    float lat = Lats[i];
                    float lon = Lons[i];
                    points.Add(new PointLatLng(lat, lon));
                }
            }
            return points;
        }

        //as above, but sample a secondary array (too ugly to combine the two methods)
        public static Tuple<List<PointLatLng>, List<float>> SampleLocations(uint[] Times, float[] Lats, float[] Lons, float[] secondary, uint interval)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            List<float> values = new List<float>();
            Tuple<List<PointLatLng>, List<float>> retval = new Tuple<List<PointLatLng>, List<float>>(points, values);

            uint lastTime = 0;
            for (int i = 0; i < Lats.Count(); i++)
            {
                if (Times[i] >= lastTime)
                {
                    float lat = Lats[i];
                    float lon = Lons[i];
                    points.Add(new PointLatLng(lat, lon));
                    values.Add(secondary[i]);
                    lastTime = Times[i] + interval;
                }
            }
            return retval;
        }

        public static GMapRoute? GmapActivity(Activity? activity, string selectedTimeSeries, int width, int alpha)
        {
            if (activity == null) { return null; }

            if (activity.LocationStream == null) { return null; }

            List<PointLatLng> points = new List<PointLatLng>();

            LocationStream locationStream = activity.LocationStream;


            TimeSeriesBase? dataStream = null;
            if (activity.TimeSeries.ContainsKey(selectedTimeSeries))
                dataStream = activity.TimeSeries[selectedTimeSeries];

            const uint INTERVAL = 30;
            if (dataStream != null)
            {
                AlignedTimeLocationSeries? aligned = AlignedTimeLocationSeries.Align(locationStream, dataStream, forceCount: 0, forceJustMe: false);

                if (aligned != null)
                {
                    points = Utils.Misc.SampleLocations(aligned.Time, aligned.Lats, aligned.Lons, INTERVAL);

                    GMapRouteColored routeColored = new GMapRouteColored(points, "Route", aligned.Secondary, alpha, width, 
                        dataStream.Percentile(TimeSeriesBase.StaticsValue.SD2High),
                        dataStream.Percentile(TimeSeriesBase.StaticsValue.SD2Low));

                    return routeColored;
                }
            }

            points = Utils.Misc.SampleLocations(locationStream.Times, locationStream.Latitudes, locationStream.Longitudes, INTERVAL);

            GMapRoute route = new GMapRoute(points, "Route");
            route.Stroke = new Pen(Color.FromArgb(alpha, 255, 0, 0), width);
            return route;
        }
        public static string FormatFloat(float value, string format, float mulitplier)
        {
            value = value * mulitplier;
            return string.Format(format, value);
        }

        //pace is in 
        public static string FormatPace(float value)
        {
            float metersPerSecond = value;
            float minutesPerKm = 16.666666667f / metersPerSecond; //https://www.aqua-calc.com/convert/speed/meter-per-second-to-minute-per-kilometer
            float secondsPerKm = minutesPerKm * 60;
            return FormatTime(secondsPerKm);

        }

        public static string FormatTime(float totalSeconds)
        {
            float secInHour = 60 * 60;
            float hours = (float)Math.Floor(totalSeconds / secInHour);
            float remainder = totalSeconds % secInHour;
            float secInMin = 60;
            float mins = (float)Math.Floor(remainder / secInMin);
            float secs = remainder % secInMin;

            if (hours > 0)
            {
                return string.Format("{0}:{1:00}:{2:00}", hours, mins, secs);
            }
            else
            {
                return string.Format("{0}:{1:00}", mins, secs);
            }
        }

        public static string EscapeForCsv(string cell)
        {
            var escapeChars = new[] { ',', '\'', '\n' };
            StringBuilder sb = new StringBuilder();
            var escape = false;

            if (cell.Contains("\""))
            {
                escape = true;
                cell = cell.Replace("\"", "\"\"");
            }
            else if (cell.IndexOfAny(escapeChars) >= 0)
                escape = true;
            if (escape)
                sb.Append('"');
            sb.Append(cell);
            if (escape)
                sb.Append('"');

            return sb.ToString();
        }


        //From https://www.codeproject.com/Tips/5271389/Simple-Word-Wrapping-in-Csharp
        //MIT License
        static readonly char[] _WordBreakChars = new char[] { ' ', '_', '\t', '.', '+', '-', '(', ')', '[', ']', '\"', /*'\'',*/ '{', '}', '!', '<', '>', '~', '`', '*', '$', '#', '@', '!', '\\', '/', ':', ';', ',', '?', '^', '%', '&', '|', '\n', '\r', '\v', '\f', '\0' };
        public static string WordWrap(string text, int width, params char[] wordBreakChars)
        {
            if (string.IsNullOrEmpty(text) || 0 == width || width >= text.Length)
                return text;
            if (null == wordBreakChars || 0 == wordBreakChars.Length)
                wordBreakChars = _WordBreakChars;
            var sb = new StringBuilder();
            var sr = new StringReader(text);
            string line;
            var first = true;
            while (null != (line = sr.ReadLine()!))
            {
                var col = 0;
                if (!first)
                {
                    sb.AppendLine();
                    col = 0;
                }
                else
                    first = false;
                var words = line.Split(wordBreakChars);

                for (var i = 0; i < words.Length; i++)
                {
                    var word = words[i];
                    if (0 != i)
                    {
                        sb.Append(" ");
                        ++col;
                    }
                    if (col + word.Length > width)
                    {
                        sb.AppendLine();
                        col = 0;
                    }
                    sb.Append(word);
                    col += word.Length;
                }
            }
            return sb.ToString();
        }


        public class ProgressReport
        {
            private string taskName;
            private int maximum;

            public ProgressReport(string taskName, int maximum)
            {
                this.taskName = taskName;
                this.maximum = maximum;
            }

            public string TaskName { get => taskName; set => taskName = value; }
            public int Maximum { get => maximum; set => maximum = value; }
        }


        public static void IntegrityCheck(Database database, StringBuilder sb)
        {
            CheckCalendar(database, sb);
            CheckForNaN(database, sb);
        }

        private static void CheckForNaN(Database database, StringBuilder sb)
        {
            Athlete athlete = database.CurrentAthlete;
            foreach(KeyValuePair<DateTime, Model.Day> kvp in athlete.Days)
            {
                CheckData(sb, kvp.Value);
            }
            foreach (KeyValuePair<string, Model.Activity> kvp in athlete.Activities)
            {
                CheckData(sb, kvp.Value);
            }
            foreach (KeyValuePair<string, Model.Activity> kvp in athlete.Activities)
            {
                CheckTimeSeries(sb, kvp.Value);
            }
        }

        private static void CheckTimeSeries(StringBuilder sb, Activity activity)
        {
            foreach(KeyValuePair<string, TimeSeriesBase> kvp in activity.TimeSeries)
            {
                TimeSeriesBase timeSeriesBase = kvp.Value;
                if(timeSeriesBase.IsValid())
                {
                    TimeValueList? tvl = timeSeriesBase.GetData(forceCount: 0, forceJustMe: false);
                    if(tvl == null)
                    {
                        sb.AppendLine($"TimeSeries returned null for GetData when valid, {activity}, {timeSeriesBase}");
                    }
                    else
                    {
                        foreach(float f in tvl.Values)
                        {
                            if(float.IsNaN(f))
                            {
                                sb.AppendLine($"TimeSeries found NaN, {activity}, {timeSeriesBase}");
                                break;
                            }
                        }
                    }
                }
            }
        }

        private static void CheckData(StringBuilder sb, Extensible extensible)
        {
            foreach (Datum d in extensible.DataValues)
            {
                if (d != null && d is TypedDatum<float>)
                {
                    TypedDatum<float> typedDatum = (TypedDatum<float>)d;
                    if (float.IsNaN(typedDatum.Data))
                    {
                        sb.AppendLine($"Found Nan for {typedDatum} on {extensible}");
                    }
                }
            }
        }

        private static void CheckCalendar(Database database, StringBuilder sb)
        {
            Dictionary<Activity, CalendarNode> integrityCheckCalendar = new Dictionary<Activity, CalendarNode>();
            foreach (KeyValuePair<DateTime, CalendarNode> kvp in database.CurrentAthlete.CalendarTree)
            {
                CheckCalendar(kvp.Value, integrityCheckCalendar, sb);
            }

        }
        private static void CheckCalendar(CalendarNode node, Dictionary<Activity, CalendarNode> integrityCheckCalendar, StringBuilder sb)
        {
            foreach (KeyValuePair<DateTime, Extensible> kvp in node.Children)
            {
                if (kvp.Value is CalendarNode)
                {
                    CheckCalendar((CalendarNode)kvp.Value, integrityCheckCalendar, sb);
                }
                else if (kvp.Value is Activity)
                {
                    Activity activity = (Activity)kvp.Value;
                    if (integrityCheckCalendar.ContainsKey(activity))
                    {
                        CalendarNode other = integrityCheckCalendar[activity];
                        DateTime? utc = activity.StartDateTimeUTC;
                        DateTime? local = activity.StartDateTimeLocal;


                        sb.AppendLine($"Found duplicate {activity} in {node} and {other} {utc} {local}");
                    }
                    else
                    {
                        integrityCheckCalendar.Add(activity, node);
                    }
                }
            }
        }

    }

}

