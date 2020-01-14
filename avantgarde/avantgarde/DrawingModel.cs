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
        private List<DrawingLine> lines = new List<DrawingLine>();
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
            foreach(var dp in points)
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
            if(p1Line.Count == 0)
            {
                System.Console.WriteLine("p1 removed");
                points.Remove(p1);
            }
            if(p2Line.Count == 0)
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
                line.inkStroke.Selected = true;
                line.UpdateStroke();
                container.AddStroke(line.inkStroke);
            }
            container.DeleteSelected();
        }
        public void newLine(Point p1, Point p2)
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

            DrawingLine drawingLine = new DrawingLine(dp1, dp2);
            lines.Add(drawingLine);
            container.AddStroke(drawingLine.inkStroke);
        }
        private class DrawingLine
        {
            public DrawingLine(DrawingPoint p1, DrawingPoint p2)
            {
                this.p1 = p1;
                this.p2 = p2;
                this.UpdateStroke();
            }
            public DrawingPoint p1 { get; set; }
            public DrawingPoint p2 { get; set; }
            public InkStroke inkStroke { get; set; }
            public Boolean involve(DrawingPoint p)
            {
                return p1 == p || p2 == p;
            }
            public void UpdateStroke()
            {
                this.inkStroke = MakeStroke(p1.pos, p2.pos);
            }
            private InkStroke MakeStroke(Point start, Point end)
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
                return inkStrokeBuilder.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
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
    }
}
