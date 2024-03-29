using FellrnrTrainingAnalysis.Model;
using FellrnrTrainingAnalysis.Utils;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class OverviewMap : UserControl
    {
        public OverviewMap()
        {
            InitializeComponent();
            ConfigureGmap();

            hillsComboBox.Items.Add("ALL");
            foreach (string s in Hill.Classes) { hillsComboBox.Items.Add(s); }
            hillsComboBox.Items.Add("None");

            showComboBox.Items.Add("None");

        }


        private Database? _database;

        private FilterActivities? _filterActivities;

        public void Display(Database database, FilterActivities filterActivities)
        {
            Logging.Instance.TraceEntry("OverviewMap.Display");
            _database = database;
            _filterActivities = filterActivities;

            IReadOnlyCollection<string> TimeSeriesNames = database.CurrentAthlete.AllTimeSeriesNames;
            foreach (string s in TimeSeriesNames)
            {
                if (!showComboBox.Items.Contains(s))
                    showComboBox.Items.Add(s);
            }


            IReadOnlyCollection<string> AllActivityTypes = database.CurrentAthlete.AllActivityTypes;
            if (!sportComboBox.Items.Contains("All"))
                sportComboBox.Items.Add("All");
            foreach (string s in AllActivityTypes)
            {
                if (!sportComboBox.Items.Contains(s))
                    sportComboBox.Items.Add(s);
            }

            if (sportComboBox.Items.Contains("Run")) { sportComboBox.Text = "Run"; }

            RefreshMap(true);
            Logging.Instance.TraceLeave();
        }

        private void RefreshMap(bool zoom)
        {
            if (_database == null) { return; }

            gmap.Overlays.Clear();
            DisplayHills();
            DisplayActivities();

            //gmap.ZoomAndCenterMarkers("hills");
            if (zoom)
            {
                gmap.ZoomAndCenterRoutes("lastRoutes");
            }
            gmap.Position = gmap.Position; //this will cause gmap to invoke ForceUpdateOverlays(), without doing this the map shows all markers in the center of the map, collapsed. 
            gmap.Refresh();
        }

        private void DisplayHills()
        {
            if (_database == null) { return; }
            int added = 0;
            int total = 0;
            string typeOfHill = hillsComboBox.Text;
            if (_database.Hills != null)
            {
                GMapOverlay markers = new GMapOverlay("hills");
                foreach (Hill hill in _database.Hills)
                {
                    if (typeOfHill == "ALL" || hill.IsA(typeOfHill))
                    {
                        bool add = false;
                        total++;
                        int count = (int)hillNumericUpDown.Value;
                        switch (hillOpComboBox.Text)
                        {
                            case "All":
                                add = true;
                                break;
                            case "=":
                                add = (hill.ClimbedCount == count);
                                break;
                            case ">":
                                add = (hill.ClimbedCount > count);
                                break;
                            case "<":
                                add = (hill.ClimbedCount < count);
                                break;
                        }
                        if (add)
                        {
                            if (labelHillsCheckBox.Checked)
                            {
                                GmapMarkerWithLabel marker = new GmapMarkerWithLabel(new PointLatLng(hill.Latitude, hill.Longitude),
                                    GMarkerGoogleType.blue_pushpin, caption: hill.Name)
                                {
                                    ToolTipText = $"{hill.Name} climbed {hill.ClimbedCount} times"
                                };
                                markers.Markers.Add(marker);
                            }
                            else
                            {
                                GMarkerGoogleType? pin = null;
                                pin = GMarkerGoogleType.blue_pushpin;

                                GMapMarker marker = new GMarkerGoogle(new PointLatLng(hill.Latitude, hill.Longitude), (GMarkerGoogleType)pin)
                                {
                                    ToolTipText = $"{hill.Name} climbed {hill.ClimbedCount} times"
                                };
                                markers.Markers.Add(marker);
                            }
                            added++;
                        }

                    }
                }
                gmap.Overlays.Add(markers);
                countLabel.Text = $"({added}/{total})";
            }
        }

        private void DisplayActivities()
        {
            if (_database == null || _filterActivities == null) { return; }

            List<Activity> activities = _filterActivities.GetActivities(_database);
            if (activities.Count == 0)
                return;

            GMapOverlay routesOverlay = new GMapOverlay("routes");
            GMapRoute? previousRoute = null;

            string activityType = sportComboBox.Text;
            foreach (Activity activity in activities)
            {
                if (activityType == "All" || activityType == activity.ActivityType)
                {
                    GMapRoute? route = DisplayActivity(activity);
                    if (route != null)
                    {
                        if (previousRoute != null)
                        {
                            routesOverlay.Routes.Add(previousRoute);
                        }
                        previousRoute = route;
                    }
                }
            }
            gmap.Overlays.Add(routesOverlay);


            //zoom to the last activity with a route, otherwise we just see the whole world
            GMapOverlay lastRouteOverlay = new GMapOverlay("lastRoute");
            lastRouteOverlay.Routes.Add(previousRoute);
            gmap.Overlays.Add(lastRouteOverlay);

        }

        private GMapRoute? DisplayActivity(Activity activity)
        {
            string selectedTimeSeries = showComboBox.Text;
            int width = (int)widthNumericUpDown.Value;
            int alpha = (int)alphaNumericUpDown.Value;


            GMapRoute? route = Utils.Misc.GmapActivity(activity, selectedTimeSeries, width, alpha);
            return route;
        }

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

        #region Event Handlers
        private void showComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshMap(false);
        }

        private void widthNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            RefreshMap(false);
        }

        private void alphaNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            RefreshMap(false);
        }

        private void hillsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshMap(false);
        }

        private void hillOpComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshMap(false);
        }

        private void hillNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            RefreshMap(false);
        }

        private void labelHillsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            RefreshMap(false);
        }
        #endregion
    }
}
