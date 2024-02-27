using FellrnrTrainingAnalysis.Model;
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
            List<float> LocationLats = new List<float>();
            List<float> LocationLons = new List<float>();
            List<float>? LocationElev = new List<float>();
            DateTime start = DateTime.MinValue;

            GpxPoint? previous = null;
            foreach (GpxPoint gpxPoint in gpxPoints)
            {
                if(gpxPoint.Time == null)
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
                    if(previous != null)
                    {
                        double distance = gpxPoint.GetDistanceFrom(previous);
                        LocationDistance.Add((float)distance);
                    }
                    previous = gpxPoint;

                    if(gpxPoint.Elevation == null)
                    {
                        LocationElev = null; //kill the stream
                    }
                    else
                    {
                        if(LocationElev != null)
                            LocationElev.Add((float)gpxPoint.Elevation);
                    }
                }
                LocationLats.Add((float)gpxPoint.Latitude);
                LocationLons.Add((float)gpxPoint.Longitude);
            }
            if (LocationTimes != null)
            {
                Activity.LocationStream = new LocationStream(LocationTimes.ToArray(), LocationLats.ToArray(), LocationLons.ToArray());
                Activity.AddDataStream(Activity.TagDistance, LocationTimes.ToArray(), LocationDistance.ToArray());
                if (LocationElev != null && LocationElev.Count > 0)
                {
                    Activity.AddDataStream(Activity.TagAltitude, LocationTimes.ToArray(), LocationElev.ToArray());
                }
            }
            else
            {
                Activity.LocationStream = new LocationStream(null, LocationLats.ToArray(), LocationLons.ToArray());
            }

        }
    }
}
