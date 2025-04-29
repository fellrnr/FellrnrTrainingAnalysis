using FellrnrTrainingAnalysis.Model;
using GMap.NET;
using GMap.NET.WindowsForms;
using GoogleApi.Entities.Search.Video.Common.Enums;
using Microsoft.VisualBasic.ApplicationServices;
using ScottPlot;
using System;
using System.ComponentModel;
using System.Data;
using System.IO.Compression;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading;

namespace FellrnrTrainingAnalysis.Utils
{
    public class Misc //the class of misfit toys
    {

        static List<Color> DefaultColorMap = new List<Color>() {
            Color.FromArgb(0xFF, 0, 0xFF, 0) ,//Green
            Color.FromArgb(0xFF, 0xFF, 0xFF, 0) ,//Yellow
            Color.FromArgb(0xFF, 0xFF, 0, 0), //Red
            };

        public static Color GetColorForValue(double val, double maxVal, int alpha, List<Color>? ColorMap)
        {
            if (ColorMap == null)
                ColorMap = DefaultColorMap;

            if (val <= 0)
                return fromColor(alpha, ColorMap[0]);
            if (val >= maxVal)
                return fromColor(alpha, ColorMap[ColorMap.Count - 1]);

            double valPerc = val / maxVal;// value%
            double colorPerc = 1d / (ColorMap.Count - 1);// % of each block of color. the last is the "100% Color"
            double blockOfColor = valPerc / colorPerc;// the integer part repersents how many block to skip
            int blockIdx = (int)Math.Truncate(blockOfColor);// Idx of 
            double valPercResidual = valPerc - (blockIdx * colorPerc);//remove the part represented of block 
            double percOfColor = valPercResidual / colorPerc;// % of color of this block that will be filled

            Color cTarget = ColorMap[blockIdx];
            Color cNext = ColorMap[blockIdx + 1];

            var deltaR = cNext.R - cTarget.R;
            var deltaG = cNext.G - cTarget.G;
            var deltaB = cNext.B - cTarget.B;

            var R = cTarget.R + (deltaR * percOfColor);
            var G = cTarget.G + (deltaG * percOfColor);
            var B = cTarget.B + (deltaB * percOfColor);

            Color c = ColorMap[0];
            c = Color.FromArgb(alpha, (byte)R, (byte)G, (byte)B);
            return c;
        }

        private static Color fromColor(int alpha, Color color)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

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

            if (new System.IO.FileInfo(filename).Length == 0)
            {
                throw new Exception("File " + filename + " is empty");
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
            if (metersPerSecond == 0)
                return "0:00";

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
            CheckLists(database, sb);
            sb.AppendLine("Done");
        }

        private static void CheckLists(Database database, StringBuilder sb, bool cleanup = true)
        {
            Athlete athlete = database.CurrentAthlete;
            List<Activity> deleteme = new List<Activity>();
            foreach (KeyValuePair<string, Model.Activity> kvp in athlete.Activities)
            {
                Activity activity = kvp.Value;
                if(activity.StartDateTimeLocal == null)
                {
                    sb.AppendLine($"Activity with no local time {activity}");
                }
                else if (!athlete.ActivitiesByLocalDateTime.ContainsKey(activity.StartDateTimeLocal.Value))
                {
                    sb.AppendLine($"Activity in activity list but not ActivitiesByLocalDateTime {activity}");
                }
                else if (athlete.ActivitiesByLocalDateTime[activity.StartDateTimeLocal.Value].PrimaryKey() != activity.PrimaryKey())
                {
                    sb.AppendLine($"Activity {activity} with same local time as {athlete.ActivitiesByLocalDateTime[activity.StartDateTimeLocal.Value]} but wrong key");
                }

                if (activity.StartDateTimeUTC == null)
                {
                    sb.AppendLine($"Activity with no UTC time {activity}");
                }
                else if (!athlete.ActivitiesByUTCDateTime.ContainsKey(activity.StartDateTimeUTC.Value))
                {
                    sb.AppendLine($"Activity in activity list but not ActivitiesByUTCDateTime {activity}");
                }
                else if (athlete.ActivitiesByUTCDateTime[activity.StartDateTimeUTC.Value].PrimaryKey() != activity.PrimaryKey())
                {
                    sb.AppendLine($"Activity {activity} with same UTC time as {athlete.ActivitiesByUTCDateTime[activity.StartDateTimeUTC.Value]} but wrong key");
                    if(cleanup) deleteme.Add(activity);
                }
            }
            foreach (var activity in deleteme)
                athlete.DeleteActivityBeforeRecalcualte(activity);

            foreach (var kvp in athlete.ActivitiesByLocalDateTime)
            {
                Activity activity = kvp.Value;
                if (activity.StartDateTimeLocal == null)
                {
                    sb.AppendLine($"Activity with no local time {activity}");
                }
                else if (!athlete.Activities.ContainsKey(activity.PrimaryKey()))
                {
                    sb.AppendLine($"Activity in ActivitiesByLocalDateTime but not Activities {activity}");
                }
            }
            foreach (var kvp in athlete.ActivitiesByUTCDateTime)
            {
                Activity activity = kvp.Value;
                if (activity.StartDateTimeUTC == null)
                {
                    sb.AppendLine($"Activity with no UTC time {activity}");
                }
                else if (!athlete.Activities.ContainsKey(activity.PrimaryKey()))
                {
                    sb.AppendLine($"Activity in ActivitiesByUTCDateTime but not Activities {activity}");
                }
            }

        }


        private static void CheckForNaN(Database database, StringBuilder sb)
        {
            Athlete athlete = database.CurrentAthlete;
            foreach (KeyValuePair<DateTime, Model.Day> kvp in athlete.Days)
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
            foreach (KeyValuePair<string, TimeSeriesBase> kvp in activity.TimeSeries)
            {
                TimeSeriesBase timeSeriesBase = kvp.Value;
                if (timeSeriesBase.IsValid())
                {
                    TimeValueList? tvl = timeSeriesBase.GetData(forceCount: 0, forceJustMe: false);
                    if (tvl == null)
                    {
                        sb.AppendLine($"TimeSeries returned null for GetData when valid, {activity}, {timeSeriesBase}");
                    }
                    else
                    {
                        foreach (float f in tvl.Values)
                        {
                            if (float.IsNaN(f))
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

        public static string s2hms(double? secs)
        {
            return s2hms((uint?)secs);
        }
        public static string s2hms(uint? secs)
        {
            if (secs == null) { return "null"; }
            TimeSpan t = TimeSpan.FromSeconds(secs!.Value);

            string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s", t.Hours, t.Minutes, t.Seconds);

            return answer;
        }

        public static DataTable ConvertToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;

        }


        public static float StandardDeviation(float[] array)
        {
            float sumSqDiff = 0;
            float avg = array.Average();

            foreach (float x in array)
            {
                float diff;
                diff = x - avg;
                sumSqDiff += diff * diff;
            }
            float sd = (float)Math.Sqrt(sumSqDiff / array.Length);
            return sd;
        }
        public static double StandardDeviation(double[] array)
        {
            double sumSqDiff = 0;
            double avg = array.Average();

            foreach (double x in array)
            {
                double diff;
                diff = x - avg;
                sumSqDiff += diff * diff;
            }
            double sd = (double)Math.Sqrt(sumSqDiff / array.Length);
            return sd;
        }
    }


    //http://blog.functionalfun.net/2008/07/reporting-progress-during-linq-queries.html
    //https://stackoverflow.com/questions/55584078/how-can-i-report-progress-from-a-plinq-query

    /*
    static class Extensions
    {
        public static ParallelQuery<T> WithProgressReporting<T>(this ParallelQuery<T> sequence, System.Action increment)
        {
            return sequence.Select(x =>
            {
                increment?.Invoke();
                return x;
            });
        }
    }
    */
    /*
    public static class Extensions
    {
        public static IEnumerable<T> WithProgressReporting<T>(this IEnumerable<T> sequence, Action<int> reportProgress)
        {
            if (sequence == null) { throw new ArgumentNullException("sequence"); }

            // make sure we can find out how many elements are in the sequence
            ICollection<T>? collection = sequence as ICollection<T>;
            if (collection == null)
            {
                // buffer the entire sequence
                collection = new List<T>(sequence);
            }

            int total = collection.Count;
            return collection.WithProgressReporting(total, reportProgress);
        }

        public static IEnumerable<T> WithProgressReporting<T>(this IEnumerable<T> sequence, long itemCount, Action<int> reportProgress)
        {
            if (sequence == null) { throw new ArgumentNullException("sequence"); }

            int completed = 0;
            foreach (var item in sequence)
            {
                yield return item;

                completed++;
                reportProgress((int)(((double)completed / itemCount) * 100));
            }
        }
    }
*/
}

