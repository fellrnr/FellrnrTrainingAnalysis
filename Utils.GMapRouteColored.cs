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
        private float Bottom { get; set; }
        private int Alpha { get; }
        private int Width { get; }


        public override void OnRender(Graphics g)
        {

            Color? previousColor = null;
            for (int i = 0; i < LocalPoints.Count - 1; i++)
            {
                Color c = ColorForRange(i);
                if (previousColor == null) { previousColor = c; }

                long x1 = LocalPoints[i].X;
                long x2 = LocalPoints[i + 1].X;
                long y1 = LocalPoints[i].Y;
                long y2 = LocalPoints[i + 1].Y;


                int margin = 4;
                if ((x1 > x2 + margin || x1 < x2 - margin) &&
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

        private Color ColorForRange(int i)
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
                if (z < Bottom)
                    z = Bottom;
                c = Utils.Misc.GetColorForValue(z - Bottom, diff, Alpha, ColorsOfMap);
            }
            else
            {
                c = Color.FromArgb(255, red, green, blue);
            }

            return c;
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



        public static List<Color> ColorsOfMap = new List<Color>();

    }
}
