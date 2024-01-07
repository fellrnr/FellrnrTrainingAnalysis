using FellrnrTrainingAnalysis.Model;
using GMap.NET;
using System.Drawing.Drawing2D;

namespace FellrnrTrainingAnalysis.Utils
{
    public class GMapRouteColored : GMap.NET.WindowsForms.GMapRoute
    {

        static GMapRouteColored()
        {
            initColorsBlocks();
        }


        public GMapRouteColored(IEnumerable<PointLatLng> points, string name, float[] zValues, int alpha, int width, float top, float bottom)
                : base(points, name)
        {
            ZValues = zValues;
            Alpha = alpha;
            Width = width;
            Top = top;
            Bottom = bottom;
        }

        private float[] ZValues;
        private float Top { get; set; }
        private float Bottom {  get; set; }
        private int Alpha { get; }
        private int Width { get; }


        public override void OnRender(Graphics g)
        {

            Color? previousColor = null;
            for (int i = 0; i < LocalPoints.Count - 1; i++)
            {

                int red = 255;
                int green = 255;
                int blue = 255;
                Color c;
                float diff = Top - Bottom;
                if (i < ZValues.Length)
                {
                    float z = ZValues[i];
                    if (z > Top)
                        z = Top;
                    if(z < Bottom)
                        z = Bottom;
                    c = GetColorForValue(z-Bottom, diff, Alpha);
                }
                else
                {
                    c = Color.FromArgb(255, red, green, blue);
                }
                if(previousColor ==  null) { previousColor = c; }

                long x1 = LocalPoints[i].X;
                long x2 = LocalPoints[i + 1].X;
                long y1 = LocalPoints[i].Y;
                long y2 = LocalPoints[i + 1].Y;


                int margin = 4;
                if ((x1 > x2+margin || x1 < x2 - margin) &&
                    (y1 > y2 + margin || y1 < y2 - margin))
                {
                    using (LinearGradientBrush linGrBrush = new LinearGradientBrush(new Point((int)x1, (int)y1), new Point((int)x2, (int)y2), previousColor.Value, c))
                    using (Pen pen = new Pen(linGrBrush, Width))
                    {
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }
                else
                {
                    using (Pen pen = new Pen(c, 10))
                    {
                        g.DrawLine(pen, x1, y1, x2, y2);
                    }
                }
                previousColor = c;
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
            Color cNext = ColorsOfMap[blockIdx + 1];

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
