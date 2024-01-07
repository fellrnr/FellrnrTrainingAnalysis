using de.schumacher_bw.Strava.Model;
using FellrnrTrainingAnalysis.Model;
using GMap.NET;
using GMap.NET.WindowsForms;
using Microsoft.VisualBasic.Logging;
using System.IO.Compression;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;

namespace FellrnrTrainingAnalysis.Utils
{
    public class Misc //the class of misfit toys
    {
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

        public static GMapRoute? GmapActivity(Activity? activity, string selectedDataStream, int width, int alpha)
        {
            if (activity == null) { return null; }

            if (activity.LocationStream == null) { return null; }

            List<PointLatLng> points = new List<PointLatLng>();

            LocationStream locationStream = activity.LocationStream;


            DataStreamBase? dataStream = null;
            if (activity.TimeSeries.ContainsKey(selectedDataStream))
                dataStream = activity.TimeSeries[selectedDataStream];

            const uint INTERVAL = 30;
            if (dataStream != null)
            {
                AlignedTimeSeries? aligned = Utils.TimeSeries.Align(locationStream, dataStream);

                if (aligned != null)
                {
                    points = Utils.Misc.SampleLocations(aligned.Time, aligned.Lats, aligned.Lons, INTERVAL);

                    GMapRouteColored routeColored = new GMapRouteColored(points, "Route", aligned.Secondary, alpha, width, 
                        dataStream.Percentile(DataStreamBase.StaticsValue.SD2High),
                        dataStream.Percentile(DataStreamBase.StaticsValue.SD2Low));

                    return routeColored;
                }
            }

            points = Utils.Misc.SampleLocations(locationStream.Times, locationStream.Latitudes, locationStream.Longitudes, INTERVAL);

            GMapRoute route = new GMapRoute(points, "Route");
            route.Stroke = new Pen(Color.FromArgb(alpha, 255, 0, 0), width);
            return route;
        }

    }
}
