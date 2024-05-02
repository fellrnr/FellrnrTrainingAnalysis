using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using FellrnrTrainingAnalysis.Utils.Gpx;

namespace FellrnrTrainingAnalysis.Action
{
    public class GpxProcessor
    {
        public GpxProcessor(Activity activity)
        {
            Activity = activity;
        }

        private Model.Activity Activity { get; set; }

        private GpxPointCollection<GpxPoint> gpxPoints = new GpxPointCollection<GpxPoint>();

        public void ProcessGpx()
        {
            if (Activity.FileFullPath == null)
                return;


            string filename = Activity.FileFullPath;
            using (Stream file = Utils.Misc.DecompressAndOpenFile(filename))
            using (GpxReader reader = new GpxReader(file))
            {
                while (reader.Read())
                {
                    switch (reader.ObjectType)
                    {
                        case GpxObjectType.Metadata:

                            break;
                        case GpxObjectType.WayPoint:

                            break;
                        case GpxObjectType.Route:
                            GpxRoute gpxRoute = reader.Route;
                            ProcessGpxRoute(gpxRoute);
                            break;
                        case GpxObjectType.Track:
                            GpxTrack gpxTrack = reader.Track;
                            LoadTrack(gpxTrack);

                            break;
                    }
                }
            }

        }

        private void LoadTrack(GpxTrack gpxTrack)
        {
            gpxPoints.Clear();

            foreach (GpxTrackSegment segment in gpxTrack.Segments)
            {
                GpxPointCollection<GpxPoint> segmentPoints = segment.TrackPoints.ToGpxPoints();

                foreach (GpxPoint point in segmentPoints)
                {
                    gpxPoints.Add(point);
                }
            }

            ProcessGpxPoints();
        }

        private void ProcessGpxRoute(GpxRoute gpxRoute)
        {
            gpxPoints.Clear();
            gpxPoints = gpxRoute.ToGpxPoints();

            ProcessGpxPoints();
        }


        private void ProcessGpxPoints()
        {
            List<uint>? LocationTimes = new List<uint>();
            List<float> LocationDistance = new List<float>();
            List<float> LocationSpeed = new List<float>();
            List<float> LocationLats = new List<float>();
            List<float> LocationLons = new List<float>();
            List<float>? LocationElev = new List<float>();
            DateTime start = DateTime.MinValue;
            float distance = 0;
            GpxPoint? previous = null;
            foreach (GpxPoint gpxPoint in gpxPoints)
            {
                if (gpxPoint.Time == null)
                {
                    LocationTimes = null;// any point without time, kill the time stream
                }
                else
                {
                    DateTime dateTime = (DateTime)gpxPoint.Time;
                    if (LocationTimes != null)
                    {
                        if (start == DateTime.MinValue)
                            start = dateTime;
                        TimeSpan timeSpan = dateTime - start;
                        uint offset = (uint)timeSpan.TotalSeconds;
                        LocationTimes.Add(offset);
                    }
                    if (previous != null)
                    {
                        double distanceKm = gpxPoint.GetDistanceFromInKm(previous);
                        double distanceM = distanceKm * 1000.0;
                        distance += (float)distanceM;
                        LocationDistance.Add(distance);

                        if (previous.Time != null)
                        {
                            DateTime dateTimePrevious = (DateTime)previous.Time;
                            TimeSpan timeSpan = dateTime - dateTimePrevious;
                            float offset = (float)timeSpan.TotalSeconds;
                            float speed = (float)offset != 0 ? (float)distanceM / offset : 0;
                            //GPX data quality is pretty grim
                            //if (speed > 5.0)
                            //    Logging.Instance.Debug($"Too fast {speed}");
                            LocationSpeed.Add(speed); //speed in m/s
                        }
                        else
                        {
                            LocationSpeed.Add(0f); //add the first point at zero
                        }
                    }
                    else
                    {
                        LocationDistance.Add(0f); //add the first point at zero
                        LocationSpeed.Add(0f); //add the first point at zero
                    }
                    previous = gpxPoint;

                    if (gpxPoint.Elevation == null)
                    {
                        LocationElev = null; //kill the stream
                    }
                    else
                    {
                        if (LocationElev != null)
                            LocationElev.Add((float)gpxPoint.Elevation);
                    }
                }
                LocationLats.Add((float)gpxPoint.Latitude);
                LocationLons.Add((float)gpxPoint.Longitude);
            }
            if (LocationTimes != null)
            {
                Activity.LocationStream = new LocationStream(LocationTimes.ToArray(), LocationLats.ToArray(), LocationLons.ToArray());

                Activity.AddTimeSeries(Activity.TagDistance, LocationTimes.ToArray(), LocationDistance.ToArray());

                //need to create the time series 
                if (Options.Instance.GPXSmoothingWindow != 0)
                {
                    List<float> to1sec = Utils.TimeSeriesUtils.InterpolateToOneSecond(LocationTimes.ToArray(), LocationSpeed.ToArray());
                    float[] smoothedSpeed = TimeSeriesUtils.WindowSmoothed(to1sec.ToArray(), Options.Instance.GPXSmoothingWindow);
                    Activity.AddTimeSeries(new TimeSeriesRecorded(Activity.TagSpeed, new TimeValueList(smoothedSpeed), Activity)); //already interpolated to 1 sec
                }
                else
                {
                    //just add as is
                    Activity.AddTimeSeries(Activity.TagSpeed, LocationTimes.ToArray(), LocationSpeed.ToArray());
                }
                if (LocationElev != null && LocationElev.Count > 0)
                {
                    Activity.AddTimeSeries(Activity.TagAltitude, LocationTimes.ToArray(), LocationElev.ToArray());
                }
            }
            else
            {
                Logging.Instance.Error($"Trying to load GPX, but no time data, {Activity}");
                //Activity.LocationStream = new LocationStream(null, LocationLats.ToArray(), LocationLons.ToArray());
            }

        }
    }
}
