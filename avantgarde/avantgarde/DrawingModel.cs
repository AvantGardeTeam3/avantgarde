using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

namespace avantgarde
{
    public class DrawingModel
    {
        private List<DrawingPoint> points = new List<DrawingPoint>();
        private List<DrawingPoint> controlPoints = new List<DrawingPoint>();
        private List<DrawingLine> lines = new List<DrawingLine>();
        private List<BezierCurve> curves = new List<BezierCurve>();
        private Stack<DrawingLine> undoStack = new Stack<DrawingLine>();

        public InkDrawingAttributes attributes { get; set; }

        private InkStrokeContainer container;
        public DrawingModel(InkStrokeContainer inkStrokeContainer)
        {
            this.container = inkStrokeContainer;
            attributes = new InkDrawingAttributes();
        }
        public List<Point> getPoints()
        {
            List<Point> retval = new List<Point>();
            foreach (var dp in points)
            {
                retval.Add(dp.pos);
            }
            retval.AddRange(getControlPoints());
            return retval;
        }
        public List<Point> getControlPoints()
        {
            List<Point> retval = new List<Point>();
            foreach(var dp in controlPoints)
            {
                retval.Add(dp.pos);
            }
            return retval;
        }
        public void redo()
        {
            if (undoStack.Count == 0)
            {
                // no previous redo, do nothing
                return;
            }
            DrawingLine line = undoStack.Pop();
            if (!points.Contains(line.p1))
            {
                points.Add(line.p1);
            }
            if (!points.Contains(line.p2))
            {
                points.Add(line.p2);
            }
            this.lines.Add(line);
            this.container.AddStroke(line.inkStroke);
        }
        public void undo()
        {
            if (lines.Count == 0)
            {
                // no drawn stroke(s), do nothing
                return;
            }
            DrawingLine last = this.lines.ElementAt(lines.Count - 1);
            lines.Remove(last);
            undoStack.Push(last);
            last.inkStroke.Selected = true;
            InkStroke temp = last.inkStroke.Clone();
            container.DeleteSelected();
            last.inkStroke = temp;
            // if point is only connected to the last line, remove them
            DrawingPoint p1 = last.p1;
            DrawingPoint p2 = last.p2;
            List<DrawingLine> p1Line = lines.FindAll(x => x.p1 == p1 || x.p2 == p1);
            List<DrawingLine> p2Line = lines.FindAll(x => x.p1 == p2 || x.p2 == p2);
            if (p1Line.Count == 0)
            {
                System.Console.WriteLine("p1 removed");
                points.Remove(p1);
            }
            if (p2Line.Count == 0)
            {
                System.Console.WriteLine("p2 removed");
                points.Remove(p2);
            }
        }
        public void move(Point point, Point position)
        {
            DrawingPoint dp = points.Find(x => x.pos == point);
            dp.pos = position;
            List<DrawingLine> involvedLines = lines.FindAll(x => x.involve(dp));
            foreach (var line in involvedLines)
            {
                // line.inkStroke.Selected = true;
                line.UpdateStroke();
                // container.AddStroke(line.inkStroke);
            }
            // container.DeleteSelected();
        }
        public void moveControlPoint(Point point, Point position)
        {
            DrawingPoint dp = controlPoints.Find(x => x.pos == point);
            dp.pos = position;
            BezierCurve curve = curves.Find(x => x.GetControlPoint() == point);
            curve.GetInkStroke().Selected = true;
            container.DeleteSelected();
            curve = new BezierCurve(curve.GetP0(), curve.GetP3(), dp.pos, curve.GetDrawingAttributes());
            container.AddStroke(curve.GetInkStroke());
        }
        public void newLine(Point p1, Point p2, InkDrawingAttributes attributes)
        {
            undoStack.Clear();
            DrawingPoint dp1;
            if (points.Exists(x => x.pos == p1))
            {
                dp1 = points.Find(x => x.pos == p1);
            }
            else
            {
                dp1 = new DrawingPoint(p1);
                points.Add(dp1);
            }

            DrawingPoint dp2;
            if (points.Exists(x => x.pos == p2))
            {
                dp2 = points.Find(x => x.pos == p2);
            }
            else
            {
                dp2 = new DrawingPoint(p2);
                points.Add(dp2);
            }

            /*Point p0 = dp1.pos;
            Point p3 = dp2.pos;
            Point p11 = new Point(p0.X, p0.Y + 100);
            Point p22 = new Point(p3.X, p3.Y + 100);

            BezierCurve curve = new BezierCurve(p0, p11, p22, p3, attributes);
            container.AddStroke(curve.GetInkStroke());*/

            DrawingLine drawingLine = new DrawingLine(dp1, dp2, attributes);
            lines.Add(drawingLine);
            container.AddStroke(drawingLine.inkStroke);
        }
        public void newCurve(Point p0, Point p3, Point control, InkDrawingAttributes attributes)
        {
            undoStack.Clear();
            DrawingPoint dp1;
            if (points.Exists(x => x.pos == p0))
            {
                dp1 = points.Find(x => x.pos == p0);
            }
            else
            {
                dp1 = new DrawingPoint(p0);
                points.Add(dp1);
            }

            DrawingPoint dp2;
            if (points.Exists(x => x.pos == p3))
            {
                dp2 = points.Find(x => x.pos == p3);
            }
            else
            {
                dp2 = new DrawingPoint(p3);
                points.Add(dp2);
            }
            controlPoints.Add(new DrawingPoint(control));
            BezierCurve curve = new BezierCurve(p0, p3, control, attributes);
            this.curves.Add(curve);
            container.AddStroke(curve.GetInkStroke());
        }
        private class DrawingLine
        {
            public DrawingLine(DrawingPoint p1, DrawingPoint p2, InkDrawingAttributes attributes)
            {
                this.p1 = p1;
                this.p2 = p2;
                this.attributes = attributes;
                this.UpdateStroke();
            }
            private InkDrawingAttributes attributes;
            public DrawingPoint p1 { get; set; }
            public DrawingPoint p2 { get; set; }
            public InkStroke inkStroke { get; set; }
            public Boolean involve(DrawingPoint p)
            {
                return p1 == p || p2 == p;
            }
            public void UpdateStroke()
            {
                this.inkStroke = MakeStroke(p1.pos, p2.pos, this.attributes);
            }
            private static InkStroke MakeStroke(Point start, Point end, InkDrawingAttributes attributes)
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
                InkStrokeBuilder inkStrokeBuilder = new InkStrokeBuilder();
                InkStroke stroke = inkStrokeBuilder.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
                stroke.DrawingAttributes = attributes;
                return stroke;
            }
        }
        private class DrawingPoint
        {
            public DrawingPoint(Point point)
            {
                this.pos = point;
            }
            public Point pos { get; set; }
        }
        private class BezierCurve
        {
            private Point p0;
            private Point p1;
            private Point p2;
            private Point p3;
            private Point controlPoint;
            private InkDrawingAttributes drawingAttributes;
            private InkStroke inkStroke;
            public Point GetP0() { return p0; }
            public Point GetP3() { return p3; }
            public InkDrawingAttributes GetDrawingAttributes() { return drawingAttributes; }
            public Point GetControlPoint() { return controlPoint; }
            public BezierCurve(Point p0, Point p3, Point control, InkDrawingAttributes drawingAttributes)
            {
                List<Point> points = GetAllPoints(p0, p3, control);
                this.p0 = p0;
                this.p1 = points[1];
                this.p2 = points[2];
                this.p3 = p3;
                this.controlPoint = control;
                this.drawingAttributes = drawingAttributes;
                this.inkStroke = MakeStroke(this, drawingAttributes);
            }
            public List<Point> getPoints(int num)
            {
                List<Point> retval = new List<Point>();
                Double interval = 1.0 / num;
                Double t = 0.0;
                for (int i = 0; i < num; i++)
                {
                    retval.Add(this.getPoint(t));
                    t += interval;
                }
                retval.Add(this.getPoint(t));
                return retval;
            }

            private Point getPoint(Double t)
            {
                Double x = Math.Pow(1 - t, 3) * p0.X + 3 * Math.Pow(1 - t, 2) * t * p1.X + 3 * (1 - t) * t * t * p2.X + Math.Pow(t, 3) * p3.X;
                Double y = Math.Pow(1 - t, 3) * p0.Y + 3 * Math.Pow(1 - t, 2) * t * p1.Y + 3 * (1 - t) * t * t * p2.Y + Math.Pow(t, 3) * p3.Y;
                return new Point(x, y);
            }
            private static InkStroke MakeStroke(BezierCurve curve, InkDrawingAttributes attributes)
            {
                List<InkPoint> inkPoints = new List<InkPoint>();
                foreach(Point point in curve.getPoints(50))
                {
                    inkPoints.Add(new InkPoint(point, 0.5f));
                }
                InkStrokeBuilder isb = new InkStrokeBuilder();
                InkStroke stroke = isb.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
                stroke.DrawingAttributes = attributes;
                return stroke;
            }
            public InkStroke GetInkStroke() { return this.inkStroke; }
            public static List<Point> GetAllPoints(Point p0, Point p3, Point control)
            {
                Double midX = (p0.X + p3.X) / 2;
                Double midY = (p0.Y + p3.Y) / 2;

                Double dhx = control.X - midX;
                Double dhy = control.Y - midY;

                Point p1 = new Point(p0.X + dhx, p0.Y + dhy);
                Point p2 = new Point(p3.X + dhx, p3.Y + dhy);

                List<Point> points = new List<Point>();
                points.Add(p0);
                points.Add(p1);
                points.Add(p2);
                points.Add(p3);
                return points;
            }
        }
    }
}
