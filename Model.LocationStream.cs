﻿using MemoryPack;

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
            //if(MinLat == null)
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



    }
}
