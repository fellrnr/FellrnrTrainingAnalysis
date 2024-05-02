using FellrnrTrainingAnalysis.Utils.Gpx;
using GMap.NET;
using GMap.NET.MapProviders;
using MemoryPack;
using ScottPlot.Drawing.Colormaps;

namespace FellrnrTrainingAnalysis.Model
{
    [MemoryPackable]
    [Serializable]
    public partial class LocationStream
    {
        public LocationStream(uint[]? times, float[] latitudes, float[] longitudes)
        {
            Times = times;
            Latitudes = latitudes;
            Longitudes = longitudes;
        }

        [MemoryPackInclude]
        float? MinLat = null;
        [MemoryPackInclude]
        float? MaxLat = null;
        [MemoryPackInclude]
        float? MinLon = null;
        [MemoryPackInclude]
        float? MaxLon = null;

        public bool WithinBounds(float lat, float lon)
        {
            //cache these as we do it a lot when looking for hills
            if (MinLat == null)
            {
                //need to add some margin, as we might be slightly short of a peak, but within the margin
                float minLat = Latitudes.Min();
                float maxLat = Latitudes.Max();
                float minLon = Longitudes.Min();
                float maxLon = Longitudes.Max();
                MinLat = OffsetLat(minLat, Hill.CLOSE_ENOUGH * -4);
                MaxLat = OffsetLat(maxLat, Hill.CLOSE_ENOUGH * 4);
                MinLon = OffsetLon(minLat, minLon, Hill.CLOSE_ENOUGH * -4);
                MaxLon = OffsetLon(maxLat, maxLon, Hill.CLOSE_ENOUGH * 4);
            }
            return (MinLat <= lat && lat <= MaxLat && MinLon <= lon && lon <= MaxLon);
        }

        const float DiameterEarthKm = 6378.137f;
        const float OneMeterInDegreesLattitude = (float)(1 / ((2 * Math.PI / 360) * DiameterEarthKm)) / 1000;
        const float PiOver180 = (float)(Math.PI / 180);
        private float OffsetLat(float lat, float meters)
        {
            return lat + (meters * OneMeterInDegreesLattitude);
        }

        private float OffsetLon(float lat, float lon, float meters)
        {
            return (float)(lon + (meters * OneMeterInDegreesLattitude) / Math.Cos(lat * PiOver180));
        }


        [MemoryPackInclude]
        public uint[]? Times { get; set; }
        [MemoryPackInclude]
        public float[] Latitudes { get; set; }
        [MemoryPackInclude]
        public float[] Longitudes { get; set; }


        private const double EARTH_RADIUS = 6371; // [km]
        private const double RADIAN = Math.PI / 180;

        public float[] LocationToDistance()
        {
            float[] retval = new float[Latitudes.Length];
            Tuple<float, float>? previous = null;
            float distance = 0;
            for (int i = 0; i < Latitudes.Length; i++)
            {
                if (previous != null)
                {
                    double Latitude = Latitudes[i];
                    double Longitude = Longitudes[i];
                    double otherLatitude = previous.Item1;
                    double otherLongitude = previous.Item2;

                    double thisLatitudeRad = Latitude * RADIAN;
                    double otherLatitudeRad = otherLatitude * RADIAN;
                    double deltaLongitudeRad = Math.Abs(Longitude - otherLongitude) * RADIAN;

                    double cos = Math.Cos(deltaLongitudeRad) * Math.Cos(thisLatitudeRad) * Math.Cos(otherLatitudeRad) +
                        Math.Sin(thisLatitudeRad) * Math.Sin(otherLatitudeRad);

                    double distanceKm = EARTH_RADIUS * Math.Acos(Math.Max(Math.Min(cos, 1), -1));

                    double distanceM = distanceKm * 1000.0;
                    distance += (float)distanceM;
                    retval[i] = distance;
                }
                previous = new Tuple<float, float>(Latitudes[i], Longitudes[i]);

            }
            return retval;
        }
    }
}
