// ==========================================================================
// Copyright (c) 2011-2016, dlg.krakow.pl
// All Rights Reserved
//
// NOTICE: dlg.krakow.pl permits you to use, modify, and distribute this file
// in accordance with the terms of the license agreement accompanying it.
// ==========================================================================
using System.Xml;
using System.Globalization;

namespace FellrnrTrainingAnalysis.Utils.Gpx
{
    public static class GpxNamespaces
    {
        public const string GPX_NAMESPACE = "http://www.topografix.com/GPX/1/1";
        public const string GARMIN_EXTENSIONS_NAMESPACE = "http://www.garmin.com/xmlschemas/GpxExtensions/v3";
        public const string GARMIN_TRACKPOINT_EXTENSIONS_V1_NAMESPACE = "http://www.garmin.com/xmlschemas/TrackPointExtension/v1";
        public const string GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE = "http://www.garmin.com/xmlschemas/TrackPointExtension/v2";
        public const string GARMIN_WAYPOINT_EXTENSIONS_NAMESPACE = "http://www.garmin.com/xmlschemas/WaypointExtension/v1";
        public const string DLG_EXTENSIONS_NAMESPACE = "http://dlg.krakow.pl/gpx/extensions/v1";
    }

    public class GpxAttributes
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Version { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Creator { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class GpxMetadata
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Description { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GpxPerson Author { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GpxCopyright Copyright { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GpxLink Link { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DateTime? Time { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Keywords { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GpxBounds Bounds { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class GpxPoint
    {
        private const double EARTH_RADIUS = 6371; // [km]
        private const double RADIAN = Math.PI / 180;

        protected GpxProperties Properties_ = new GpxProperties();

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Elevation { get; set; }
        public DateTime? Time { get; set; }

        public double? MagneticVar
        {
            get { return Properties_.GetValueProperty<double>("MagneticVar"); }
            set { Properties_.SetValueProperty<double>("MagneticVar", value); }
        }

        public double? GeoidHeight
        {
            get { return Properties_.GetValueProperty<double>("GeoidHeight"); }
            set { Properties_.SetValueProperty<double>("GeoidHeight", value); }
        }

        public string Name
        {
            get { return Properties_.GetObjectProperty<string>("Name"); }
            set { Properties_.SetObjectProperty<string>("Name", value); }
        }

        public string Comment
        {
            get { return Properties_.GetObjectProperty<string>("Comment"); }
            set { Properties_.SetObjectProperty<string>("Comment", value); }
        }

        public string Description
        {
            get { return Properties_.GetObjectProperty<string>("Description"); }
            set { Properties_.SetObjectProperty<string>("Description", value); }
        }

        public string Source
        {
            get { return Properties_.GetObjectProperty<string>("Source"); }
            set { Properties_.SetObjectProperty<string>("Source", value); }
        }

        public IList<GpxLink> Links
        {
            get { return Properties_.GetListProperty<GpxLink>("Links"); }
        }

        public string Symbol
        {
            get { return Properties_.GetObjectProperty<string>("Symbol"); }
            set { Properties_.SetObjectProperty<string>("Symbol", value); }
        }

        public string Type
        {
            get { return Properties_.GetObjectProperty<string>("Type"); }
            set { Properties_.SetObjectProperty<string>("Type", value); }
        }

        public string FixType
        {
            get { return Properties_.GetObjectProperty<string>("FixType"); }
            set { Properties_.SetObjectProperty<string>("FixType", value); }
        }

        public int? Satelites
        {
            get { return Properties_.GetValueProperty<int>("Satelites"); }
            set { Properties_.SetValueProperty<int>("Satelites", value); }
        }

        public double? Hdop
        {
            get { return Properties_.GetValueProperty<double>("Hdop"); }
            set { Properties_.SetValueProperty<double>("Hdop", value); }
        }

        public double? Vdop
        {
            get { return Properties_.GetValueProperty<double>("Vdop"); }
            set { Properties_.SetValueProperty<double>("Vdop", value); }
        }

        public double? Pdop
        {
            get { return Properties_.GetValueProperty<double>("Pdop"); }
            set { Properties_.SetValueProperty<double>("Pdop", value); }
        }

        public double? AgeOfData
        {
            get { return Properties_.GetValueProperty<double>("AgeOfData"); }
            set { Properties_.SetValueProperty<double>("AgeOfData", value); }
        }

        public int? DgpsId
        {
            get { return Properties_.GetValueProperty<int>("DgpsId"); }
            set { Properties_.SetValueProperty<int>("DgpsId", value); }
        }

        public GpxLink HttpLink
        {
            get
            {
#pragma warning disable CS8603 // Possible null reference return.
                return Links.Where(l => l != null && l.Uri != null && l.Uri.Scheme == Uri.UriSchemeHttp).FirstOrDefault();
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        public GpxLink EmailLink
        {
            get
            {
#pragma warning disable CS8603 // Possible null reference return.
                return Links.Where(l => l != null && l.Uri != null && l.Uri.Scheme == Uri.UriSchemeMailto).FirstOrDefault();
#pragma warning restore CS8603 // Possible null reference return.
            }
        }

        public double GetDistanceFrom(GpxPoint other)
        {
            double thisLatitude = Latitude * RADIAN;
            double otherLatitude = other.Latitude * RADIAN;
            double deltaLongitude = Math.Abs(Longitude - other.Longitude) * RADIAN;

            double cos = Math.Cos(deltaLongitude) * Math.Cos(thisLatitude) * Math.Cos(otherLatitude) +
                Math.Sin(thisLatitude) * Math.Sin(otherLatitude);

            return EARTH_RADIUS * Math.Acos(Math.Max(Math.Min(cos, 1), -1));
        }
    }

    public class GpxWayPoint : GpxPoint
    {
        // GARMIN_EXTENSIONS, GARMIN_WAYPOINT_EXTENSIONS

        public double? Proximity
        {
            get { return Properties_.GetValueProperty<double>("Proximity"); }
            set { Properties_.SetValueProperty<double>("Proximity", value); }
        }

        public double? Temperature
        {
            get { return Properties_.GetValueProperty<double>("Temperature"); }
            set { Properties_.SetValueProperty<double>("Temperature", value); }
        }

        public double? Depth
        {
            get { return Properties_.GetValueProperty<double>("Depth"); }
            set { Properties_.SetValueProperty<double>("Depth", value); }
        }

        public string DisplayMode
        {
            get { return Properties_.GetObjectProperty<string>("DisplayMode"); }
            set { Properties_.SetObjectProperty<string>("DisplayMode", value); }
        }

        public IList<string> Categories
        {
            get { return Properties_.GetListProperty<string>("Categories"); }
        }

        public GpxAddress Address
        {
            get { return Properties_.GetObjectProperty<GpxAddress>("Address"); }
            set { Properties_.SetObjectProperty<GpxAddress>("Address", value); }
        }

        public IList<GpxPhone> Phones
        {
            get { return Properties_.GetListProperty<GpxPhone>("Phones"); }
        }

        // GARMIN_WAYPOINT_EXTENSIONS

        public int? Samples
        {
            get { return Properties_.GetValueProperty<int>("Samples"); }
            set { Properties_.SetValueProperty<int>("Samples", value); }
        }

        public DateTime? Expiration
        {
            get { return Properties_.GetValueProperty<DateTime>("Expiration"); }
            set { Properties_.SetValueProperty<DateTime>("Expiration", value); }
        }

        // DLG_EXTENSIONS

        public int? Level
        {
            get { return Properties_.GetValueProperty<int>("Level"); }
            set { Properties_.SetValueProperty<int>("Level", value); }
        }

        public IList<string> Aliases
        {
            get { return Properties_.GetListProperty<string>("Aliases"); }
        }

        public bool HasGarminExtensions
        {
            get
            {
                return Proximity != null || Temperature != null || Depth != null ||
                    DisplayMode != null || Address != null ||
                    Categories.Count != 0 || Phones.Count != 0;
            }
        }

        public bool HasGarminWaypointExtensions
        {
            get { return Samples != null || Expiration != null; }
        }

        public bool HasDlgExtensions
        {
            get { return Level != null || Aliases.Count != 0; }
        }

        public bool HasExtensions
        {
            get { return HasGarminExtensions || HasGarminWaypointExtensions || HasDlgExtensions; }
        }
    }

    public class GpxTrackPoint : GpxPoint
    {
        // GARMIN_EXTENSIONS, GARMIN_TRACKPOINT_EXTENSIONS_V1, GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? Temperature
        {
            get { return Properties_.GetValueProperty<double>("Temperature"); }
            set { Properties_.SetValueProperty<double>("Temperature", value); }
        }

        public double? Depth
        {
            get { return Properties_.GetValueProperty<double>("Depth"); }
            set { Properties_.SetValueProperty<double>("Depth", value); }
        }

        // GARMIN_TRACKPOINT_EXTENSIONS_V1, GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? WaterTemperature
        {
            get { return Properties_.GetValueProperty<double>("WaterTemperature"); }
            set { Properties_.SetValueProperty<double>("WaterTemperature", value); }
        }

        public int? HeartRate
        {
            get { return Properties_.GetValueProperty<int>("HeartRate"); }
            set { Properties_.SetValueProperty<int>("HeartRate", value); }
        }

        public int? Cadence
        {
            get { return Properties_.GetValueProperty<int>("Cadence"); }
            set { Properties_.SetValueProperty<int>("Cadence", value); }
        }

        // GARMIN_TRACKPOINT_EXTENSIONS_V2

        public double? Speed
        {
            get { return Properties_.GetValueProperty<double>("Speed"); }
            set { Properties_.SetValueProperty<double>("Speed", value); }
        }

        public double? Course
        {
            get { return Properties_.GetValueProperty<double>("Course"); }
            set { Properties_.SetValueProperty<double>("Course", value); }
        }

        public double? Bearing
        {
            get { return Properties_.GetValueProperty<double>("Bearing"); }
            set { Properties_.SetValueProperty<double>("Bearing", value); }
        }

        public bool HasGarminExtensions
        {
            get { return Temperature != null || Depth != null; }
        }

        public bool HasGarminTrackpointExtensionsV1
        {
            get { return WaterTemperature != null || HeartRate != null || Cadence != null; }
        }

        public bool HasGarminTrackpointExtensionsV2
        {
            get { return Speed != null || Course != null || Bearing != null; }
        }

        public bool HasExtensions
        {
            get { return HasGarminExtensions || HasGarminTrackpointExtensionsV1 || HasGarminTrackpointExtensionsV2; }
        }
    }

    public class GpxRoutePoint : GpxPoint
    {
        // GARMIN_EXTENSIONS

        public IList<GpxPoint> RoutePoints
        {
            get { return Properties_.GetListProperty<GpxPoint>("RoutePoints"); }
        }

        public bool HasExtensions
        {
            get { return RoutePoints.Count != 0; }
        }
    }

    public class GpxPointCollection<T> : IList<T> where T : GpxPoint
    {
        private readonly List<T> Points_ = new List<T>();

        public GpxPoint AddPoint(T point)
        {
            Points_.Add(point);
            return point;
        }

        public T StartPoint
        {
#pragma warning disable CS8603 // Possible null reference return.
            get { return (Points_.Count == 0) ? null : Points_[0]; }
#pragma warning restore CS8603 // Possible null reference return.
        }

        public T EndPoint
        {
#pragma warning disable CS8603 // Possible null reference return.
            get { return (Points_.Count == 0) ? null : Points_[Points_.Count - 1]; }
#pragma warning restore CS8603 // Possible null reference return.
        }

        public double GetLength()
        {
            double result = 0;

            for (int i = 1; i < Points_.Count; i++)
            {
                double dist = Points_[i].GetDistanceFrom(Points_[i - 1]);
                result += dist;
            }

            return result;
        }

        public double? GetMinElevation()
        {
            return Points_.Select(p => p.Elevation).Min();
        }

        public double? GetMaxElevation()
        {
            return Points_.Select(p => p.Elevation).Max();
        }

        public GpxPointCollection<GpxPoint> ToGpxPoints()
        {
            GpxPointCollection<GpxPoint> points = new GpxPointCollection<GpxPoint>();

            foreach (T gpxPoint in Points_)
            {
                GpxPoint point = new GpxPoint
                {
                    Longitude = gpxPoint.Longitude,
                    Latitude = gpxPoint.Latitude,
                    Elevation = gpxPoint.Elevation,
                    Time = gpxPoint.Time
                };

                points.Add(point);
            }

            return points;
        }

        public int Count
        {
            get { return Points_.Count; }
        }

        public int IndexOf(T item)
        {
            return Points_.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Points_.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            Points_.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return Points_[index]; }
            set { Points_[index] = value; }
        }

        public void Add(T item)
        {
            Points_.Add(item);
        }

        public void Clear()
        {
            Points_.Clear();
        }

        public bool Contains(T item)
        {
            return Points_.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Points_.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return Points_.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Points_.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public abstract class GpxTrackOrRoute
    {
        private readonly List<GpxLink> Links_ = new List<GpxLink>(0);

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Comment { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Description { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Source { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public int? Number { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Type { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public IList<GpxLink> Links
        {
            get { return Links_; }
        }

        // GARMIN_EXTENSIONS

        public GpxColor? DisplayColor { get; set; }

        public bool HasExtensions
        {
            get { return DisplayColor != null; }
        }

        public abstract double GetLength();
    }

    public class GpxRoute : GpxTrackOrRoute
    {
        private readonly GpxPointCollection<GpxRoutePoint> RoutePoints_ = new GpxPointCollection<GpxRoutePoint>();

        public GpxPointCollection<GpxRoutePoint> RoutePoints
        {
            get { return RoutePoints_; }
        }

        public override double GetLength()
        {
            double result = 0;
            GpxPoint? current = null;

            foreach (GpxRoutePoint routePoint in RoutePoints_)
            {
                if (current != null) result += routePoint.GetDistanceFrom(current);
                current = routePoint;

                foreach (GpxPoint gpxPoint in routePoint.RoutePoints)
                {
                    result += gpxPoint.GetDistanceFrom(current);
                    current = gpxPoint;
                }
            }

            return result;
        }

        public GpxPointCollection<GpxPoint> ToGpxPoints()
        {
            GpxPointCollection<GpxPoint> points = new GpxPointCollection<GpxPoint>();

            foreach (GpxRoutePoint routePoint in RoutePoints_)
            {
                points.Add(routePoint);

                foreach (GpxPoint gpxPoint in routePoint.RoutePoints)
                {
                    points.Add(gpxPoint);
                }
            }

            return points;
        }
    }

    public class GpxTrack : GpxTrackOrRoute
    {
        private readonly List<GpxTrackSegment> Segments_ = new List<GpxTrackSegment>(1);

        public IList<GpxTrackSegment> Segments
        {
            get { return Segments_; }
        }

        public override double GetLength()
        {
            return Segments_.Sum(s => s.TrackPoints.GetLength());
        }

        [Obsolete]
        public GpxPointCollection<GpxPoint> ToGpxPoints()
        {
            GpxPointCollection<GpxPoint> points = new GpxPointCollection<GpxPoint>();

            foreach (GpxTrackSegment segment in Segments_)
            {
                GpxPointCollection<GpxPoint> segmentPoints = segment.TrackPoints.ToGpxPoints();

                foreach (GpxPoint point in segmentPoints)
                {
                    points.Add(point);
                }
            }

            return points;
        }
    }

    public class GpxTrackSegment
    {
        readonly GpxPointCollection<GpxTrackPoint> TrackPoints_ = new GpxPointCollection<GpxTrackPoint>();

        public GpxPointCollection<GpxTrackPoint> TrackPoints
        {
            get { return TrackPoints_; }
        }
    }

    public class GpxLink
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Href { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Text { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string MimeType { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Uri Uri
        {
#pragma warning disable CS8603 // Possible null reference return.
            get { return Uri.TryCreate(Href, UriKind.Absolute, out Uri? result) ? result : null; }
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    public class GpxEmail
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Id { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Domain { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class GpxAddress
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string StreetAddress { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string City { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string State { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Country { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string PostalCode { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class GpxPhone
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Number { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string? Category { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class GpxPerson
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Name { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GpxEmail Email { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GpxLink Link { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class GpxCopyright
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Author { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public int? Year { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Licence { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }

    public class GpxBounds
    {
        public double MinLatitude { get; set; }
        public double MinLongitude { get; set; }
        public double MaxLatitude { get; set; }
        public double MaxLongitude { get; set; }
    }

    public enum GpxColor : uint
    {
        Black = 0xff000000,
        DarkRed = 0xff8b0000,
        DarkGreen = 0xff008b00,
        DarkYellow = 0x8b8b0000,
        DarkBlue = 0Xff00008b,
        DarkMagenta = 0xff8b008b,
        DarkCyan = 0xff008b8b,
        LightGray = 0xffd3d3d3,
        DarkGray = 0xffa9a9a9,
        Red = 0xffff0000,
        Green = 0xff00b000,
        Yellow = 0xffffff00,
        Blue = 0xff0000ff,
        Magenta = 0xffff00ff,
        Cyan = 0xff00ffff,
        White = 0xffffffff,
        Transparent = 0x00ffffff
    }
    public class GpxProperties
    {
        private class GpxListWrapper<T> : IList<T>
        {
            GpxProperties Properties_;
            string Name_;
            IList<T>? Items_;

            public GpxListWrapper(GpxProperties properties, string name)
            {
                this.Properties_ = properties;
                this.Name_ = name;
                this.Items_ = properties.GetObjectProperty<IList<T>>(name);
            }

            public int IndexOf(T item)
            {
                return (Items_ != null) ? Items_.IndexOf(item) : -1;
            }

            public void Insert(int index, T item)
            {
                if (Items_ == null && index != 0) throw new ArgumentOutOfRangeException();

                if (Items_ == null)
                {
                    Items_ = new List<T>();
                    Properties_.SetObjectProperty(Name_, Items_);
                }

                Items_.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                if (Items_ == null) throw new ArgumentOutOfRangeException();
                Items_.RemoveAt(index);
            }

            public T this[int index]
            {
                get
                {
                    if (Items_ == null) throw new ArgumentOutOfRangeException();
                    return Items_[index];
                }
                set
                {
                    if (Items_ == null) throw new ArgumentOutOfRangeException();
                    Items_[index] = value;
                }
            }

            public void Add(T item)
            {
                if (Items_ == null)
                {
                    Items_ = new List<T>();
                    Properties_.SetObjectProperty(Name_, Items_);
                }

                Items_.Add(item);
            }

            public void Clear()
            {
                if (Items_ != null)
                {
                    Items_.Clear();
                    Items_ = null;
                }
            }

            public bool Contains(T item)
            {
                return Items_ != null ? Items_.Contains(item) : false;
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                if (Items_ == null) return;
                Items_.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return Items_ != null ? Items_.Count : 0; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(T item)
            {
                return Items_ != null ? Items_.Remove(item) : false;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return (Items_ != null ? Items_ : Enumerable.Empty<T>()).GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        Dictionary<string, object>? Properties_ = null;

        public Nullable<T> GetValueProperty<T>(string name) where T : struct
        {
            if (Properties_ == null) return null;

            object? value;
            if (!Properties_.TryGetValue(name, out value)) return null;

            return (T)value;
        }

        public T GetObjectProperty<T>(string name) where T : class
        {
#pragma warning disable CS8603 // Possible null reference return.
            if (Properties_ == null) return null;
#pragma warning restore CS8603 // Possible null reference return.

            object? value;
#pragma warning disable CS8603 // Possible null reference return.
            if (!Properties_.TryGetValue(name, out value)) return null;
#pragma warning restore CS8603 // Possible null reference return.

            return (T)value;
        }

        public IList<T> GetListProperty<T>(string name)
        {
            return new GpxListWrapper<T>(this, name);
        }

        public void SetValueProperty<T>(string name, Nullable<T> value) where T : struct
        {
            if (value != null)
            {
                if (Properties_ == null) Properties_ = new Dictionary<string, object>();
                Properties_[name] = value.Value;
            }
            else if (Properties_ != null)
            {
                Properties_.Remove(name);
            }
        }

        public void SetObjectProperty<T>(string name, T value) where T : class
        {
            if (value != null)
            {
                if (Properties_ == null) Properties_ = new Dictionary<string, object>();
                Properties_[name] = value;
            }
            else if (Properties_ != null)
            {
                Properties_.Remove(name);
            }
        }
    }
    public enum GpxObjectType { None, Attributes, Metadata, WayPoint, Route, Track };

    public sealed class GpxReader : IDisposable
    {
        private readonly XmlReader Reader_;

        public GpxObjectType ObjectType { get; private set; }
        public GpxAttributes Attributes { get; private set; }
        public GpxMetadata Metadata { get; private set; }
        public GpxWayPoint WayPoint { get; private set; }
        public GpxRoute Route { get; private set; }
        public GpxTrack Track { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public GpxReader(Stream stream)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Reader_ = XmlReader.Create(stream);

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        if (Reader_.Name != "gpx") throw new FormatException(Reader_.Name);
                        Attributes = ReadGpxAttribures();
                        ObjectType = GpxObjectType.Attributes;
                        return;
                }
            }

            throw new FormatException();
        }

        public bool Read()
        {
            if (ObjectType == GpxObjectType.None) return false;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "metadata":
                                Metadata = ReadGpxMetadata();
                                ObjectType = GpxObjectType.Metadata;
                                return true;
                            case "wpt":
                                WayPoint = ReadGpxWayPoint();
                                ObjectType = GpxObjectType.WayPoint;
                                return true;
                            case "rte":
                                Route = ReadGpxRoute();
                                ObjectType = GpxObjectType.Route;
                                return true;
                            case "trk":
                                Track = ReadGpxTrack();
                                ObjectType = GpxObjectType.Track;
                                return true;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != "gpx") throw new FormatException(Reader_.Name);
                        ObjectType = GpxObjectType.None;
                        return false;
                }
            }

            ObjectType = GpxObjectType.None;
            return false;
        }

        public void Dispose()
        {
            Reader_.Close();
        }

        private GpxAttributes ReadGpxAttribures()
        {
            GpxAttributes attributes = new GpxAttributes();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "version":
                        attributes.Version = Reader_.Value;
                        break;
                    case "creator":
                        attributes.Creator = Reader_.Value;
                        break;
                }
            }

            return attributes;
        }

        private GpxMetadata ReadGpxMetadata()
        {
            GpxMetadata metadata = new GpxMetadata();
            if (Reader_.IsEmptyElement) return metadata;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "name":
                                metadata.Name = ReadContentAsString();
                                break;
                            case "desc":
                                metadata.Description = ReadContentAsString();
                                break;
                            case "author":
                                metadata.Author = ReadGpxPerson();
                                break;
                            case "copyright":
                                metadata.Copyright = ReadGpxCopyright();
                                break;
                            case "link":
                                metadata.Link = ReadGpxLink();
                                break;
                            case "time":
                                metadata.Time = ReadContentAsDateTime();
                                break;
                            case "keywords":
                                metadata.Keywords = ReadContentAsString();
                                break;
                            case "bounds":
                                ReadGpxBounds();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return metadata;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxWayPoint ReadGpxWayPoint()
        {
            string elementName = Reader_.Name;
            bool isEmptyElement = Reader_.IsEmptyElement;

            GpxWayPoint wayPoint = new GpxWayPoint();
            GetPointLocation(wayPoint);
            if (isEmptyElement) return wayPoint;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "extensions":
                                ReadWayPointExtensions(wayPoint);
                                break;
                            default:
                                if (!ProcessPointField(wayPoint)) SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return wayPoint;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxRoute ReadGpxRoute()
        {
            GpxRoute route = new GpxRoute();
            if (Reader_.IsEmptyElement) return route;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "name":
                                route.Name = ReadContentAsString();
                                break;
                            case "cmt":
                                route.Comment = ReadContentAsString();
                                break;
                            case "desc":
                                route.Description = ReadContentAsString();
                                break;
                            case "src":
                                route.Source = ReadContentAsString();
                                break;
                            case "link":
                                route.Links.Add(ReadGpxLink());
                                break;
                            case "number":
                                route.Number = int.Parse(ReadContentAsString());
                                break;
                            case "type":
                                route.Type = ReadContentAsString();
                                break;
                            case "rtept":
                                route.RoutePoints.Add(ReadGpxRoutePoint());
                                break;
                            case "extensions":
                                ReadRouteExtensions(route);
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return route;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxRoutePoint ReadGpxRoutePoint()
        {
            string elementName = Reader_.Name;
            bool isEmptyElement = Reader_.IsEmptyElement;

            GpxRoutePoint routePoint = new GpxRoutePoint();
            GetPointLocation(routePoint);
            if (isEmptyElement) return routePoint;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "extensions":
                                ReadRoutePointExtensions(routePoint);
                                break;
                            default:
                                if (!ProcessPointField(routePoint)) SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return routePoint;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxTrack ReadGpxTrack()
        {
            GpxTrack track = new GpxTrack();
            if (Reader_.IsEmptyElement) return track;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "name":
                                track.Name = ReadContentAsString();
                                break;
                            case "cmt":
                                track.Comment = ReadContentAsString();
                                break;
                            case "desc":
                                track.Description = ReadContentAsString();
                                break;
                            case "src":
                                track.Source = ReadContentAsString();
                                break;
                            case "link":
                                track.Links.Add(ReadGpxLink());
                                break;
                            case "number":
                                track.Number = int.Parse(ReadContentAsString());
                                break;
                            case "type":
                                track.Type = ReadContentAsString();
                                break;
                            case "trkseg":
                                track.Segments.Add(ReadGpxTrackSegment());
                                break;
                            case "extensions":
                                ReadTrackExtensions(track);
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return track;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxTrackSegment ReadGpxTrackSegment()
        {
            GpxTrackSegment segment = new GpxTrackSegment();
            if (Reader_.IsEmptyElement) return segment;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "trkpt":
                                segment.TrackPoints.Add(ReadGpxTrackPoint());
                                break;
                            case "extensions":
                                ReadTrackSegmentExtensions();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return segment;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxTrackPoint ReadGpxTrackPoint()
        {
            string elementName = Reader_.Name;
            bool isEmptyElement = Reader_.IsEmptyElement;

            GpxTrackPoint trackPoint = new GpxTrackPoint();
            GetPointLocation(trackPoint);
            if (isEmptyElement) return trackPoint;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "extensions":
                                ReadTrackPointExtensions(trackPoint);
                                break;
                            default:
                                if (!ProcessPointField(trackPoint)) SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return trackPoint;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxPerson ReadGpxPerson()
        {
            GpxPerson person = new GpxPerson();
            if (Reader_.IsEmptyElement) return person;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "name":
                                person.Name = ReadContentAsString();
                                break;
                            case "email":
                                person.Email = ReadGpxEmail();
                                break;
                            case "link":
                                person.Link = ReadGpxLink();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return person;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxEmail ReadGpxEmail()
        {
            GpxEmail email = new GpxEmail();
            if (Reader_.IsEmptyElement) return email;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "id":
                                email.Id = ReadContentAsString();
                                break;
                            case "domain":
                                email.Domain = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return email;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxLink ReadGpxLink()
        {
            GpxLink link = new GpxLink();

            string elementName = Reader_.Name;
            bool isEmptyElement = Reader_.IsEmptyElement;

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "href":
                        link.Href = Reader_.Value;
                        break;
                }
            }

            if (isEmptyElement) return link;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "text":
                                link.Text = ReadContentAsString();
                                break;
                            case "type":
                                link.MimeType = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return link;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxCopyright ReadGpxCopyright()
        {
            GpxCopyright copyright = new GpxCopyright();

            string elementName = Reader_.Name;
            bool isEmptyElement = Reader_.IsEmptyElement;

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "author":
                        copyright.Author = Reader_.Value;
                        break;
                }
            }

            if (isEmptyElement) return copyright;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        switch (Reader_.Name)
                        {
                            case "year":
                                copyright.Year = ReadContentAsInt();
                                break;
                            case "license":
                                copyright.Licence = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return copyright;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxBounds ReadGpxBounds()
        {
            if (!Reader_.IsEmptyElement) throw new FormatException(Reader_.Name);

            GpxBounds bounds = new GpxBounds();

            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "minlat":
                        bounds.MinLatitude = double.Parse(Reader_.Value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "maxlat":
                        bounds.MaxLatitude = double.Parse(Reader_.Value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "minlon":
                        bounds.MinLongitude = double.Parse(Reader_.Value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "maxlon":
                        bounds.MaxLongitude = double.Parse(Reader_.Value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                }
            }

            return bounds;
        }

        private void ReadWayPointExtensions(GpxWayPoint wayPoint)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        if (Reader_.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE || Reader_.NamespaceURI == GpxNamespaces.GARMIN_WAYPOINT_EXTENSIONS_NAMESPACE)
                        {
                            switch (Reader_.LocalName)
                            {
                                case "WaypointExtension":
                                    ReadGarminWayPointExtensions(wayPoint);
                                    break;
                                default:
                                    SkipElement();
                                    break;
                            }

                            break;
                        }

                        if (Reader_.NamespaceURI == GpxNamespaces.DLG_EXTENSIONS_NAMESPACE)
                        {
                            switch (Reader_.LocalName)
                            {
                                case "level":
                                    wayPoint.Level = ReadContentAsInt();
                                    break;
                                case "aliases":
                                    ReadWayPointAliases(wayPoint);
                                    break;
                                default:
                                    SkipElement();
                                    break;
                            }

                            break;
                        }

                        SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadRouteExtensions(GpxRoute route)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        if (Reader_.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (Reader_.LocalName)
                            {
                                case "RouteExtension":
                                    ReadGarminTrackOrRouteExtensions(route);
                                    break;
                                default:
                                    SkipElement();
                                    break;
                            }

                            break;
                        }

                        SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadRoutePointExtensions(GpxRoutePoint routePoint)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        if (Reader_.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (Reader_.LocalName)
                            {
                                case "RoutePointExtension":
                                    ReadGarminRoutePointExtensions(routePoint);
                                    break;
                                default:
                                    SkipElement();
                                    break;
                            }

                            break;
                        }

                        SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadTrackExtensions(GpxTrack track)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        if (Reader_.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE)
                        {
                            switch (Reader_.LocalName)
                            {
                                case "TrackExtension":
                                    ReadGarminTrackOrRouteExtensions(track);
                                    break;
                                default:
                                    SkipElement();
                                    break;
                            }

                            break;
                        }

                        SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadTrackSegmentExtensions()
        {
            SkipElement();
        }

        private void ReadTrackPointExtensions(GpxTrackPoint trackPoint)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:

                        if (Reader_.NamespaceURI == GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE ||
                            Reader_.NamespaceURI == GpxNamespaces.GARMIN_TRACKPOINT_EXTENSIONS_V1_NAMESPACE ||
                            Reader_.NamespaceURI == GpxNamespaces.GARMIN_TRACKPOINT_EXTENSIONS_V2_NAMESPACE)
                        {
                            switch (Reader_.LocalName)
                            {
                                case "TrackPointExtension":
                                    ReadGarminTrackPointExtensions(trackPoint);
                                    break;
                                default:
                                    SkipElement();
                                    break;
                            }

                            break;
                        }

                        SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminWayPointExtensions(GpxWayPoint wayPoint)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (Reader_.LocalName)
                        {
                            case "Proximity":
                                wayPoint.Proximity = ReadContentAsDouble();
                                break;
                            case "Temperature":
                                wayPoint.Temperature = ReadContentAsDouble();
                                break;
                            case "Depth":
                                wayPoint.Depth = ReadContentAsDouble();
                                break;
                            case "DisplayMode":
                                wayPoint.DisplayMode = ReadContentAsString();
                                break;
                            case "Categories":
                                ReadGarminCategories(wayPoint);
                                break;
                            case "Address":
                                wayPoint.Address = ReadGarminGpxAddress();
                                break;
                            case "PhoneNumber":
                                wayPoint.Phones.Add(ReadGarminGpxPhone());
                                break;
                            case "Samples":
                                wayPoint.Samples = ReadContentAsInt();
                                break;
                            case "Expiration":
                                wayPoint.Expiration = ReadContentAsDateTime();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminTrackOrRouteExtensions(GpxTrackOrRoute trackOrRoute)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (Reader_.LocalName)
                        {
                            case "DisplayColor":
                                trackOrRoute.DisplayColor = (GpxColor)Enum.Parse(typeof(GpxColor), ReadContentAsString(), false);
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminRoutePointExtensions(GpxRoutePoint routePoint)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (Reader_.LocalName)
                        {
                            case "rpt":
                                routePoint.RoutePoints.Add(ReadGarminAutoRoutePoint());
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminTrackPointExtensions(GpxTrackPoint trackPoint)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (Reader_.LocalName)
                        {
                            case "Temperature":
                            case "atemp":
                                trackPoint.Temperature = ReadContentAsDouble();
                                break;
                            case "wtemp":
                                trackPoint.WaterTemperature = ReadContentAsDouble();
                                break;
                            case "Depth":
                            case "depth":
                                trackPoint.Depth = ReadContentAsDouble();
                                break;
                            case "hr":
                                trackPoint.HeartRate = ReadContentAsInt();
                                break;
                            case "cad":
                                trackPoint.Cadence = ReadContentAsInt();
                                break;
                            case "speed":
                                trackPoint.Speed = ReadContentAsDouble();
                                break;
                            case "course":
                                trackPoint.Course = ReadContentAsDouble();
                                break;
                            case "bearing":
                                trackPoint.Bearing = ReadContentAsDouble();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadGarminCategories(GpxWayPoint wayPoint)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (Reader_.LocalName)
                        {
                            case "Category":
                                wayPoint.Categories.Add(ReadContentAsString());
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private void ReadWayPointAliases(GpxWayPoint wayPoint)
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (Reader_.LocalName)
                        {
                            case "alias":
                                wayPoint.Aliases.Add(ReadContentAsString());
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxPoint ReadGarminAutoRoutePoint()
        {
            GpxPoint point = new GpxPoint();

            string elementName = Reader_.Name;
            bool isEmptyElement = Reader_.IsEmptyElement;

            GetPointLocation(point);
            if (isEmptyElement) return point;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        SkipElement();
                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return point;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxAddress ReadGarminGpxAddress()
        {
            GpxAddress address = new GpxAddress();
            if (Reader_.IsEmptyElement) return address;

            string elementName = Reader_.Name;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Element:
                        switch (Reader_.LocalName)
                        {
                            case "StreetAddress":

                                if (string.IsNullOrEmpty(address.StreetAddress))
                                {
                                    address.StreetAddress = ReadContentAsString();
                                    break;
                                }

                                address.StreetAddress += " " + ReadContentAsString();
                                break;

                            case "City":
                                address.City = ReadContentAsString();
                                break;
                            case "State":
                                address.State = ReadContentAsString();
                                break;
                            case "Country":
                                address.Country = ReadContentAsString();
                                break;
                            case "PostalCode":
                                address.PostalCode = ReadContentAsString();
                                break;
                            default:
                                SkipElement();
                                break;
                        }

                        break;

                    case XmlNodeType.EndElement:
                        if (Reader_.Name != elementName) throw new FormatException(Reader_.Name);
                        return address;
                }
            }

            throw new FormatException(elementName);
        }

        private GpxPhone ReadGarminGpxPhone()
        {
            return new GpxPhone
            {
                Category = Reader_.GetAttribute("Category", GpxNamespaces.GARMIN_EXTENSIONS_NAMESPACE),
                Number = ReadContentAsString()
            };
        }

        private void SkipElement()
        {
            if (Reader_.IsEmptyElement) return;

            string elementName = Reader_.Name;
            int depth = Reader_.Depth;

            while (Reader_.Read())
            {
                if (Reader_.NodeType == XmlNodeType.EndElement)
                {
                    if (Reader_.Depth == depth && Reader_.Name == elementName) return;
                }
            }

            throw new FormatException(elementName);
        }

        private void GetPointLocation(GpxPoint point)
        {
            while (Reader_.MoveToNextAttribute())
            {
                switch (Reader_.Name)
                {
                    case "lat":
                        point.Latitude = double.Parse(Reader_.Value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                    case "lon":
                        point.Longitude = double.Parse(Reader_.Value, CultureInfo.InvariantCulture.NumberFormat);
                        break;
                }
            }
        }

        private bool ProcessPointField(GpxPoint point)
        {
            switch (Reader_.Name)
            {
                case "ele":
                    point.Elevation = ReadContentAsDouble();
                    return true;
                case "time":
                    point.Time = ReadContentAsDateTime();
                    return true;
                case "magvar":
                    point.MagneticVar = ReadContentAsDouble();
                    return true;
                case "geoidheight":
                    point.GeoidHeight = ReadContentAsDouble();
                    return true;
                case "name":
                    point.Name = ReadContentAsString();
                    return true;
                case "cmt":
                    point.Comment = ReadContentAsString();
                    return true;
                case "desc":
                    point.Description = ReadContentAsString();
                    return true;
                case "src":
                    point.Source = ReadContentAsString();
                    return true;
                case "link":
                    point.Links.Add(ReadGpxLink());
                    return true;
                case "sym":
                    point.Symbol = ReadContentAsString();
                    return true;
                case "type":
                    point.Type = ReadContentAsString();
                    return true;
                case "fix":
                    point.FixType = ReadContentAsString();
                    return true;
                case "sat":
                    point.Satelites = ReadContentAsInt();
                    return true;
                case "hdop":
                    point.Hdop = ReadContentAsDouble();
                    return true;
                case "vdop":
                    point.Vdop = ReadContentAsDouble();
                    return true;
                case "pdop":
                    point.Pdop = ReadContentAsDouble();
                    return true;
                case "ageofdgpsdata":
                    point.AgeOfData = ReadContentAsDouble();
                    return true;
                case "dgpsid":
                    point.DgpsId = ReadContentAsInt();
                    return true;
            }

            return false;
        }

        private string ReadContentAsString()
        {
            if (Reader_.IsEmptyElement) throw new FormatException(Reader_.Name);

            string elementName = Reader_.Name;
            string result = string.Empty;

            while (Reader_.Read())
            {
                switch (Reader_.NodeType)
                {
                    case XmlNodeType.Text:
                        result = Reader_.Value;
                        break;

                    case XmlNodeType.EndElement:
                        return result;

                    case XmlNodeType.Element:
                        throw new FormatException(elementName);
                }
            }

            throw new FormatException(elementName);
        }

        private int ReadContentAsInt()
        {
            string value = ReadContentAsString();
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        private double ReadContentAsDouble()
        {
            string value = ReadContentAsString();
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        private DateTime ReadContentAsDateTime()
        {
            string value = ReadContentAsString();
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        }
    }
}
