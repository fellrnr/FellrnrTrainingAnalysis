using GMap.NET;
using GMap.NET.WindowsForms.Markers;

namespace FellrnrTrainingAnalysis.Utils
{
    public class GmapMarkerWithLabel : GMarkerGoogle
    {
        private readonly Font _font;
        private GMarkerGoogle _innerMarker;
        private readonly string? _caption;
        private Pen? BoxPen;
        public GmapMarkerWithLabel(PointLatLng p, GMarkerGoogleType type, Pen? boxPen = null, string? caption = null)
            : base(p, type)
        {
            _font = new Font("Arial", 9);
            _innerMarker = new GMarkerGoogle(p, type);

            _caption = caption;
            BoxPen = boxPen;
        }

        public override void OnRender(Graphics g)
        {
            base.OnRender(g);

            if (_caption != null)
            {
                var stringSize = g.MeasureString(_caption, _font);
                var localPoint = new PointF(LocalPosition.X - stringSize.Width / 2, LocalPosition.Y + stringSize.Height);
                g.DrawString(_caption, _font, Brushes.Black, localPoint);
            }

            if (BoxPen != null)
            {
                g.DrawRectangle(BoxPen, base.LocalPosition.X, base.LocalPosition.Y, base.Size.Width, base.Size.Height);
            }
        }
    }
}
