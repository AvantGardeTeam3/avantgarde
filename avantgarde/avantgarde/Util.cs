using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace avantgarde
{
    class Util
    {
        private static InkStrokeBuilder inkStrokeBuilder = new InkStrokeBuilder();
        public static double distance(Point p1, Point p2)
        {
            double dx = p1.X - p2.X;
            double dy = p1.Y - p2.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
        public static Point? snapping(List<Point> points, Point p, double snapDistance)
        {
            foreach (Point ep in points)
            {
                double distance = Math.Sqrt(Math.Pow(p.X - ep.X, 2) + Math.Pow(p.Y - ep.Y, 2));
                if (distance < snapDistance) return ep;
            }
            return null;
        }
        public static InkStroke MakeStroke(Point start, Point end)
        {
            List<InkPoint> inkPoints = new List<InkPoint>();
            Double deltaX = end.X - start.X;
            Double deltaY = end.Y - start.Y;
            Double distance = Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2);
            distance = Math.Sqrt(distance);

            int pointNum = Convert.ToInt32(Math.Ceiling(distance / 10.0));
            for (int i = 0; i < pointNum; i++)
            {
                Point ip = new Point(start.X + i * deltaX / pointNum, start.Y + i * deltaY / pointNum);
                inkPoints.Add(new InkPoint(ip, 0.5f));
            }
            inkPoints.Add(new InkPoint(end, 0.5f));
            return inkStrokeBuilder.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
        }
        public static Point MidPoint(Point p1, Point p2)
        {
            return new Point((p1.X + p2.X) / 2, (p2.Y + p2.Y) / 2);
        }
    }
}
