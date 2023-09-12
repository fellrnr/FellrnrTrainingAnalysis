using FellrnrTrainingAnalysis.Model;
using GMap.NET;

namespace FellrnrTrainingAnalysis.Utils
{
    public class GMapRouteColored : GMap.NET.WindowsForms.GMapRoute
    {
        private IDataStream DataStream { get; set; }
        private Activity Activity { get; set; }

        static GMapRouteColored()
        {
            initColorsBlocks();
        }

        public int Alpha;

        public GMapRouteColored(IEnumerable<PointLatLng> points, string name, IDataStream dataStream, Activity activity, int alpha)
                : base(points, name)
        {
            DataStream= dataStream;
            Activity = activity;
            Alpha = alpha;
        }

        public override void OnRender(Graphics g)
        {
            Tuple<uint[], float[]>? data = DataStream.GetData(Activity);
            if(data == null)
            {
                base.OnRender(g); return;
            }
            float[] zvalues = data.Item2;
            float min = zvalues.Min();
            float max = zvalues.Max();

            List<Color> colors = new List<Color>();
            for (int i = 0; i < LocalPoints.Count - 1; i++)
            {
                int red = 255;
                int green = 255;
                int blue = 255;
                Color c;
                if (i < zvalues.Length)
                {
                    float z = zvalues[i];
                    c = GetColorForValue(z-min, max-min, Alpha);
                }
                else
                {
                    c = Color.FromArgb(255, red, green, blue);
                }
                Pen pen = new Pen(c, 10);
                colors.Add(c);
                g.DrawLine(pen, LocalPoints[i].X, LocalPoints[i].Y, LocalPoints[i + 1].X, LocalPoints[i + 1].Y);
            }
        }

        private static void initColorsBlocks()
        {
            /*
            ColorsOfMap.AddRange(new Color[]{
            Color.FromArgb(Alpha, 0, 0, 0) ,//Black
            Color.FromArgb(Alpha, 0, 0, 0xFF) ,//Blue
            Color.FromArgb(Alpha, 0, 0xFF, 0xFF) ,//Cyan
            Color.FromArgb(Alpha, 0, 0xFF, 0) ,//Green
            Color.FromArgb(Alpha, 0xFF, 0xFF, 0) ,//Yellow
            Color.FromArgb(Alpha, 0xFF, 0, 0) ,//Red
            Color.FromArgb(Alpha, 0xFF, 0xFF, 0xFF) // White
            });
            */
            ColorsOfMap.AddRange(new Color[]{
            Color.FromArgb(0xFF, 0, 0xFF, 0) ,//Green
            Color.FromArgb(0xFF, 0xFF, 0xFF, 0) ,//Yellow
            Color.FromArgb(0xFF, 0xFF, 0, 0) //Red
        });
        }


        private static Color fromColor(int alpha, Color color)
        {
            return Color.FromArgb(alpha, color.R, color.G, color.B);
        }

        private static Color GetColorForValue(double val, double maxVal, int alpha)
        {
            if (val <= 0)
                return fromColor(alpha, ColorsOfMap[0]);
            if (val >= maxVal)
                return fromColor(alpha, ColorsOfMap[ColorsOfMap.Count-1]);

            double valPerc = val / maxVal;// value%
            double colorPerc = 1d / (ColorsOfMap.Count - 1);// % of each block of color. the last is the "100% Color"
            double blockOfColor = valPerc / colorPerc;// the integer part repersents how many block to skip
            int blockIdx = (int)Math.Truncate(blockOfColor);// Idx of 
            double valPercResidual = valPerc - (blockIdx * colorPerc);//remove the part represented of block 
            double percOfColor = valPercResidual / colorPerc;// % of color of this block that will be filled

            Color cTarget = ColorsOfMap[blockIdx];
            Color cNext = cNext = ColorsOfMap[blockIdx + 1];

            var deltaR = cNext.R - cTarget.R;
            var deltaG = cNext.G - cTarget.G;
            var deltaB = cNext.B - cTarget.B;

            var R = cTarget.R + (deltaR * percOfColor);
            var G = cTarget.G + (deltaG * percOfColor);
            var B = cTarget.B + (deltaB * percOfColor);

            Color c = ColorsOfMap[0];
            c = Color.FromArgb(alpha, (byte)R, (byte)G, (byte)B);
            return c;
        }
        public static List<Color> ColorsOfMap = new List<Color>();

    }
}
