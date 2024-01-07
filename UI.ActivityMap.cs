using FellrnrTrainingAnalysis.Model;
using GMap.NET.WindowsForms;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;
using FellrnrTrainingAnalysis.Utils;
using System.Collections.ObjectModel;
using static FellrnrTrainingAnalysis.Utils.TimeSeries;
using System.Diagnostics.Eventing.Reader;
using Microsoft.VisualBasic;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ActivityMap : UserControl
    {
        //TODO: can't add a GMap.net to the designer due to an error regarding SHA1 provider
        //public SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();

        public ActivityMap()
        {
            InitializeComponent();
            showComboBox.Items.Add("None");
        }

        Activity? Activity = null;
        List<Hill>? Hills = null;
        private bool FirstTime = true;

        public void DisplayActivity(Activity? activity, List<Hill>? hills)
        {
            Logging.Instance.Enter("UI.ActivityMap.DisplayActivity");
            if (FirstTime)
            {
                ConfigureGmap();
                FirstTime = false;
            }
            Activity = activity;
            Hills = hills;
            gmap.Overlays.Clear();
            if (Activity == null) { return; }

            if (Activity.LocationStream == null) { return; }

            List<string> TimeSeriesNames = Activity.TimeSeriesNames;
            foreach (string s in TimeSeriesNames)
            {
                if (!showComboBox.Items.Contains(s))
                    showComboBox.Items.Add(s);
            }

            List<string> deleteMe = new List<string>();
            foreach (string s in showComboBox.Items)
            {
                if (!TimeSeriesNames.Contains(s))
                    deleteMe.Add(s);
            }
            foreach (string s in deleteMe) { showComboBox.Items.Remove(s); }

            DisplayMap(true);
            Logging.Instance.Leave();
        }

        private void DisplayMap(bool zoomAndCenter)
        {
            gmap.Overlays.Clear();

            //do this first to see if it helps with all markers shown in the middle
            DisplayHills();

            string selectedDataStream = showComboBox.Text;

            int width = (int)widthNumericUpDown.Value;
            int alpha = (int)alphaNumericUpDown.Value;

            GMapOverlay routes = new GMapOverlay("routes");

            GMapRoute? route = Utils.Misc.GmapActivity(Activity, selectedDataStream, width, alpha);
            if (route != null)
            {
                routes.Routes.Add(route);
                gmap.Overlays.Add(routes);
                gmap.ZoomAndCenterRoute(route);
            }

            gmap.Position = gmap.Position; //this will cause gmap to invoke ForceUpdateOverlays(), without doing this the map shows all markers in the center of the map, collapsed. 

            gmap.Refresh();
        }



        Font tooltipFont = new Font("Arial", 10);
        private Pen BoxPen = new Pen(Color.Purple, 3.0f);
        private void DisplayHills()
        {
            if (Hills == null) return;

            GMapOverlay markers = new GMapOverlay("hills");
            foreach (Hill hill in Hills)
            {
                GMarkerGoogleType? pin = null;
                string? classOfHill = null;
                foreach (KeyValuePair<string, GMarkerGoogleType> kvp in ClassesToMarker)
                {
                    if (hill.IsA(kvp.Key))
                    {
                        classOfHill = kvp.Key;
                        pin = kvp.Value;
                        break;
                    }
                }

                if (pin != null)
                {
                    if (Activity != null && Activity.Climbed != null && Activity.Climbed.Contains(hill))
                    {
                        GmapMarkerWithLabel marker = new GmapMarkerWithLabel(new PointLatLng(hill.Latitude, hill.Longitude), pin.Value, boxPen: BoxPen)
                        {
                            ToolTipText = $"{hill.Name} climbed {hill.ClimbedCount} times ({classOfHill})",
                        };
                        markers.Markers.Add(marker);

                    }
                    else
                    {
                        GMapMarker marker = new GMarkerGoogle(
                                    new PointLatLng(hill.Latitude, hill.Longitude),
                                    (GMarkerGoogleType)pin)
                        {
                            ToolTipText = $"{hill.Name} climbed {hill.ClimbedCount} times ({classOfHill})",
                        };
                        marker.ToolTip.Font = tooltipFont;
                        markers.Markers.Add(marker);
                    }
                }
            }
            gmap.Overlays.Add(markers);
        }

        private static Dictionary<string, GMarkerGoogleType> ClassesToMarker = new Dictionary<string, GMarkerGoogleType>()
        {
{ "Wainwright", GMarkerGoogleType.green_pushpin },
{ "Wainwright Outlying Fell", GMarkerGoogleType.pink_pushpin } ,
{ "Birkett", GMarkerGoogleType.yellow_pushpin },
{ "Marilyn",GMarkerGoogleType.lightblue_pushpin },
{ "Munro", GMarkerGoogleType.blue_pushpin },
{ "Corbett",GMarkerGoogleType.purple_pushpin },
{ "Graham",GMarkerGoogleType.red_pushpin },
{ "Donald",GMarkerGoogleType.blue_dot },
{ "Hewitt",GMarkerGoogleType.green_dot },
{ "Nuttall",GMarkerGoogleType.yellow_dot },
{ "Dewey",GMarkerGoogleType.orange_dot },
{ "High Hill of Britain",GMarkerGoogleType.purple_dot },
{ "Fellranger",GMarkerGoogleType.red_dot },
{ "Historic County Top",GMarkerGoogleType.red_small },
{ "Current County/UA Top",GMarkerGoogleType.green_small },
{ "Administrative County Top",GMarkerGoogleType.purple_small },
        };

        private void ConfigureGmap()
        {
            gmap.MapProvider = GMap.NET.MapProviders.OpenStreet4UMapProvider.Instance;

            //6RJZ0JcQ1Nvr0bb8LmMm~a3Tdp97LgtLdW7aUlfC02A~AglDreKSRA-i1kj0snWD05VIev1U5yqC_LiaM8pOAP8Tr7SPyl2ns1m4mTGlxXTQ
            //GMap.NET.MapProviders.BingOSMapProvider.Instance.ClientKey = "6RJZ0JcQ1Nvr0bb8LmMm~a3Tdp97LgtLdW7aUlfC02A~AglDreKSRA-i1kj0snWD05VIev1U5yqC_LiaM8pOAP8Tr7SPyl2ns1m4mTGlxXTQ";
            //https://www.bingmapsportal.com/Application#

            //GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerOnly;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            gmap.DragButton = MouseButtons.Left;
            //gmap.ZoomAndCenterMarkers = true;
            gmap.Position = new GMap.NET.PointLatLng(54.6123, -3.1179); //latrigg
            gmap.MinZoom = 2;
            gmap.MaxZoom = 18;
            gmap.Zoom = 13;
            gmap.ShowCenter = false;
        }

        private void showComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DisplayMap(false);
        }

        private void widthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            DisplayMap(false);
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            DisplayMap(false);
        }
    }
}
