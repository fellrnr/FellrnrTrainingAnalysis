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
            List<float> LocationLats = new List<float>();
            List<float> LocationLons = new List<float>();
            DateTime start = DateTime.MinValue;

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
                }
                LocationLats.Add((float)gpxPoint.Latitude);
                LocationLons.Add((float)gpxPoint.Longitude);
            }
            if (LocationTimes != null)
                Activity.LocationStream = new LocationStream(LocationTimes.ToArray(), LocationLats.ToArray(), LocationLons.ToArray());
            else
                Activity.LocationStream = new LocationStream(null, LocationLats.ToArray(), LocationLons.ToArray());

        }
    }
}
