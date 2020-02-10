using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace avantgarde
{
    class BezierCurve
    {
        private Point p0;
        private Point p1;
        private Point p2;
        private Point p3;
        public BezierCurve(Point p0, Point p1, Point p2, Point p3)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
        public List<Point> getPoints(int num)
        {
            List<Point> retval = new List<Point>();
            Double interval = 1.0 / num;
            Double t = 0.0;
            for(int i = 0; i < num; i++)
            {
                retval.Add(this.getPoint(t));
                t += interval;
            }
            return retval;
        }

        private Point getPoint(Double t)
        {
            Double x = Math.Pow(1 - t, 3) * p0.X + 3 * Math.Pow(1 - t, 2) * t * p1.X + 3 * (1 - t) * t * t * p2.X + Math.Pow(t, 3) * p3.X;
            Double y = Math.Pow(1 - t, 3) * p0.Y + 3 * Math.Pow(1 - t, 2) * t * p1.Y + 3 * (1 - t) * t * t * p2.Y + Math.Pow(t, 3) * p3.Y;
            return new Point(x, y);
        }
    }
}
