using FellrnrTrainingAnalysis.Model;
using GMap.NET.WindowsForms;
using GMap.NET;
using GMap.NET.WindowsForms.Markers;
using FellrnrTrainingAnalysis.Utils;

namespace FellrnrTrainingAnalysis.UI
{
    public partial class ActivityMap : UserControl
    {
        //TODO: can't add a GMap.net to the designer due to an error regarding SHA1 provider
        //public SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();

        private GMapControl? gmap;
        public ActivityMap()
        {
            InitializeComponent();



            //The map messes up visual studio designer as we have Strava API library that uses .net standard 2.0
            //SetupGMapInsteadOfDesigner();
            //ConfigureGmap();
        }

        Activity? Activity = null;
        List<Hill>? Hills = null;
        private bool FirstTime = true;

        public void DisplayActivity(Activity? activity, List<Hill>? hills)
        {
            if(FirstTime)
            {
                gmap = new GMapControl();
                SetupGMapInsteadOfDesigner();
                ConfigureGmap();
                FirstTime= false;
            }
            Activity = activity;
            Hills = hills;
            gmap.Overlays.Clear();
            if (Activity == null ) { return; }

            if(Activity.LocationStream == null ) { return; }

            List<string> TimeSeriesNames = Activity.TimeSeriesNames;
            foreach (string s in TimeSeriesNames)
            {
                if(!showComboBox.Items.Contains(s))
                    showComboBox.Items.Add(s);
            }

            List<string> deleteMe = new List<string>();
            foreach(string s in showComboBox.Items)
            {
                if(!TimeSeriesNames.Contains(s))
                    deleteMe.Add(s);
            }
            foreach(string s in deleteMe) { showComboBox.Items.Remove(s); }

            DisplayMap(true);
        }

        private void DisplayMap(bool zoomAndCenter)
        {
            gmap.Overlays.Clear();



            if (Activity == null) { return; }

            if (Activity.LocationStream == null) { return; }

            string selectedDataStream = showComboBox.Text;
            int width = (int)widthNumericUpDown.Value;
            int alpha = (int)alphaNumericUpDown.Value;
            GMapOverlay routes = new GMapOverlay("routes");
            List<PointLatLng> points = new List<PointLatLng>();
            for (int i = 0; i < Activity.LocationStream.Latitudes.Count(); i++)
            {
                float lat = Activity.LocationStream.Latitudes[i];
                float lon = Activity.LocationStream.Longitudes[i];
                points.Add(new PointLatLng(lat, lon));
            }
            if (Activity.TimeSeries.ContainsKey(selectedDataStream))
            {
                IDataStream dataStream = Activity.TimeSeries[selectedDataStream];
                GMapRouteColored route = new GMapRouteColored(points, "Route", dataStream, Activity, alpha);
                routes.Routes.Add(route);
                gmap.Overlays.Add(routes);
                //if(zoomAndCenter)
                    gmap.ZoomAndCenterRoute(route);
            }
            else
            {
                GMapRoute route = new GMapRoute(points, "Route");
                route.Stroke = new Pen(Color.Red, width);
                routes.Routes.Add(route);
                gmap.Overlays.Add(routes);

                //TODO: without zoom and center, the route doesn't get displayed
                //if (zoomAndCenter)
                    gmap.ZoomAndCenterRoute(route);
            }
            DisplayHills();
            gmap.Refresh();
        }


        private void DisplayHills()
        {
            if(Hills == null) return;

            GMapOverlay markers = new GMapOverlay("hills");
            foreach (Hill hill in Hills)
            {
                if (hill.IsA(Hill.WAINWRIGHT))
                {
                    GMapMarker marker = new GMarkerGoogle(
                                new PointLatLng(hill.Latitude, hill.Longitude),
                                GMarkerGoogleType.blue_pushpin)
                    {
                        ToolTipText = $"{hill.Name} climbed {hill.ClimbedCount} times"
                    };
                    markers.Markers.Add(marker);
                }
            }
            gmap.Overlays.Add(markers);
        }

        private void SetupGMapInsteadOfDesigner()
        {
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();

            // 
            // gMap
            // 
            this.gmap.Bearing = 0F;
            this.gmap.CanDragMap = true;
            this.gmap.EmptyTileColor = Color.Navy;
            this.gmap.GrayScaleMode = false;
            this.gmap.HelperLineOption = HelperLineOptions.DontShow;
            this.gmap.LevelsKeepInMemory = 5;
            this.gmap.Location = new System.Drawing.Point(12, 3);
            this.gmap.MarkersEnabled = true;
            this.gmap.MouseWheelZoomEnabled = true;
            this.gmap.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            this.gmap.Name = "gMapControl1";
            this.gmap.NegativeMode = false;
            this.gmap.PolygonsEnabled = true;
            this.gmap.RetryLoadTile = 0;
            this.gmap.RoutesEnabled = true;
            this.gmap.ScaleMode = ScaleModes.Integer;
            this.gmap.SelectedAreaFillColor = Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.gmap.ShowTileGridLines = false;
            this.gmap.TabIndex = 0;
            this.gmap.Zoom = 0D;
            this.gmap.TabIndex = 1;


            this.gmap.Anchor = ((System.Windows.Forms.AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom)
            | AnchorStyles.Left)
            | AnchorStyles.Right)));
            this.gmap.BackColor = SystemColors.MenuHighlight;
            this.gmap.Location = new System.Drawing.Point(6, 61);
            this.gmap.Name = "gmap";
            this.gmap.Size = new System.Drawing.Size(645, 311);
            this.gmap.TabIndex = 1;


            this.Controls.Add(this.gmap);

            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
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
            //gmap.SetPositionByKeywords("Paris, France");
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
