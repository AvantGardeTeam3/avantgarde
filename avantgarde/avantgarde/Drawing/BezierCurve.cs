using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace avantgarde.Drawing
{
    public class BezierCurve
    {
        private bool _modified = false;
        public bool Modified
        {
            get { return _modified; }
            private set { _modified = value; }
        }
        private Point _p0;
        public Point P0
        {
            get { return _p0; }
            set
            {
                _p0.X = value.X;
                _p0.Y = value.Y;
                UpdateStroke();
            }
        }
        private Point _p1;
        public Point P1
        {
            get { return _p1; }
            set
            {
                _p1.X = value.X;
                _p1.Y = value.Y;
                UpdateStroke();
            }
        }
        private Point _p2;
        public Point P2
        {
            get { return _p2; }
            set
            {
                _p2.X = value.X;
                _p2.Y = value.Y;
                UpdateStroke();
            }
        }
        private Point _p3;
        public Point P3
        {
            get { return _p3; }
            set
            {
                _p3.X = value.X;
                _p3.Y = value.Y;
                UpdateStroke();
            }
        }
        private Point _midPoint;
        public Point MidPoint
        {
            get { return _midPoint; }
            set
            {
                _midPoint.X = value.X;
                _midPoint.Y = value.Y;
                MoveMidPoint();
                UpdateStroke();
            }
        }
        public InkDrawingAttributes DrawingAttributes { get; private set; }
        public InkStroke InkStroke { get; private set; }
        public BezierCurve(Point p0, Point p3, InkDrawingAttributes drawingAttributes)
        {
            _p0 = p0;
            _p1 = p0;
            _p2 = p3;
            _p3= p3;
            _midPoint = Util.MidPoint(_p0, _p3);
            this.DrawingAttributes = drawingAttributes;
            UpdateStroke();
        }
        private List<Point> GetPoints(int num)
        {
            List<Point> retval = new List<Point>();
            Double interval = 1.0 / num;
            Double t = 0.0;
            for (int i = 0; i < num; i++)
            {
                retval.Add(this.CurveFunction(t));
                t += interval;
            }
            retval.Add(this.CurveFunction(t));
            return retval;
        }

        private Point CurveFunction(Double t)
        {
            Double x = Math.Pow(1 - t, 3) * P0.X + 3 * Math.Pow(1 - t, 2) * t * P1.X + 3 * (1 - t) * t * t * P2.X + Math.Pow(t, 3) * P3.X;
            Double y = Math.Pow(1 - t, 3) * P0.Y + 3 * Math.Pow(1 - t, 2) * t * P1.Y + 3 * (1 - t) * t * t * P2.Y + Math.Pow(t, 3) * P3.Y;
            return new Point(x, y);
        }
        private static InkStroke MakeStroke(BezierCurve curve)
        {
            List<InkPoint> inkPoints = new List<InkPoint>();
            foreach (Point point in curve.GetPoints(50))
            {
                inkPoints.Add(new InkPoint(point, 0.5f));
            }
            InkStrokeBuilder isb = new InkStrokeBuilder();
            InkStroke stroke = isb.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
            stroke.DrawingAttributes = curve.DrawingAttributes;
            return stroke;
        }
        private void MoveMidPoint()
        {
            _modified = true;
            Double midX = (P0.X + P3.X) / 2;
            Double midY = (P0.Y + P3.Y) / 2;

            Double dhx = _midPoint.X - midX;
            Double dhy = _midPoint.Y - midY;
            
            P1 = new Point(P1.X + dhx, P0.Y + dhy);
            P2 = new Point(P3.X + dhx, P3.Y + dhy);
        }
        private void UpdateStroke()
        {
            this.InkStroke = MakeStroke(this);
        }
    }
}
