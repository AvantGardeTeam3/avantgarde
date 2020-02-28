using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

using avantgarde.Drawing;
using avantgarde.Menus;

namespace avantgarde
{
    public class DrawingModel
    {
        private List<Point> midPoints = new List<Point>(); 
        private List<Point> endPoints = new List<Point>();
        private List<Point> controlPoints = new List<Point>();
        private List<BezierCurve> curves = new List<BezierCurve>();
        private Stack<BezierCurve> undoStack = new Stack<BezierCurve>();

        private bool fleur;

        public InkDrawingAttributes attributes { get; set; }

        private InkStrokeContainer container;
        public DrawingModel(InkStrokeContainer inkStrokeContainer, bool f)
        {
            this.container = inkStrokeContainer;
            this.fleur = f;
        }
        public List<Point> GetMidPoints()
        {
            List<Point> retPoints = new List<Point>();
            foreach(BezierCurve curve in curves)
            {
                if (!curve.Modified) retPoints.Add(curve.MidPoint);
                //retPoints.Add(curve.MidPoint);
            }
            return retPoints;
        }
        public List<Point> GetEndPoints()
        {
            List<Point> retPoints = new List<Point>();
            foreach(BezierCurve curve in curves)
            {
                Point p0 = curve.P0;
                Point p3 = curve.P3;
                if (!retPoints.Contains(p0)) retPoints.Add(p0);
                if (!retPoints.Contains(p3)) retPoints.Add(p3);
            }
            return retPoints;
        }
        public List<Point> GetControlPoints()
        {
            List<Point> retPoints = new List<Point>();
            foreach(BezierCurve curve in curves)
            {
                Point p1 = curve.P1;
                Point p2 = curve.P2;
                if (curve.Modified)
                {
                    retPoints.Add(p1);
                    retPoints.Add(p2);
                }
            }
            return retPoints;
        }
        public void redo()
        {
            //if (undoStack.Count == 0)
            //{
            //    // no previous undo, do nothing
            //    return;
            //}
            //DrawingLine line = undoStack.Pop();
            //if (!points.Contains(line.p1))
            //{
            //    points.Add(line.p1);
            //}
            //if (!points.Contains(line.p2))
            //{
            //    points.Add(line.p2);
            //}
            //this.lines.Add(line);
            //this.container.AddStroke(line.inkStroke);
        }
        public void undo()
        {
            //if (lines.Count == 0)
            //{
            //    // no drawn stroke(s), do nothing
            //    return;
            //}
            //DrawingLine last = this.lines.ElementAt(lines.Count - 1);
            //lines.Remove(last);
            //undoStack.Push(last);
            //last.inkStroke.Selected = true;
            //InkStroke temp = last.inkStroke.Clone();
            //container.DeleteSelected();
            //last.inkStroke = temp;
            //// if point is only connected to the last line, remove them
            //DrawingPoint p1 = last.p1;
            //DrawingPoint p2 = last.p2;
            //List<DrawingLine> p1Line = lines.FindAll(x => x.p1 == p1 || x.p2 == p1);
            //List<DrawingLine> p2Line = lines.FindAll(x => x.p1 == p2 || x.p2 == p2);
            //if (p1Line.Count == 0)
            //{
            //    System.Console.WriteLine("p1 removed");
            //    points.Remove(p1);
            //}
            //if (p2Line.Count == 0)
            //{
            //    System.Console.WriteLine("p2 removed");
            //    points.Remove(p2);
            //}
        }
        public void move(Point point, Point position)
        {
            if (midPoints.Contains(point))
            {
                moveMidPoint(point, position);
            }
            else if (controlPoints.Contains(point))
            {
                moveControlPoint(point, position);
            }
            else if (endPoints.Contains(point))
            {
                moveEndPoints(point, position);
            }
        }
        public void moveMidPoint(Point point, Point position)
        {
            BezierCurve curve = curves.Find(x => x.MidPoint == point);
            InkStroke stroke = curve.InkStroke;
            stroke.Selected = true;
            container.DeleteSelected();
            curve.MidPoint = position;
            container.AddStroke(curve.InkStroke);
        }

        public void moveControlPoint(Point point, Point position)
        {
            BezierCurve curve = curves.Find(x => x.P1 == point || x.P2 == point);
            InkStroke stroke = curve.InkStroke;
            stroke.Selected = true;
            container.DeleteSelected();
            if(curve.P1 == point)
            {
                curve.P1 = position;
            } else
            {
                curve.P2 = position;
            }
            container.AddStroke(curve.InkStroke);
        }

        public void moveEndPoints(Point point, Point position)
        {
            List<BezierCurve> involvedCurves = curves.FindAll(x => x.P0 == point || x.P3 == point);
            involvedCurves.ForEach(x => x.InkStroke.Selected = true);
            container.DeleteSelected();

            foreach(BezierCurve curve in involvedCurves)
            {
                if (curve.P0 == point)
                {
                    curve.P0 = position;
                }
                else
                {
                    curve.P3 = position;
                }
            }

            involvedCurves.ForEach(x => container.AddStroke(x.InkStroke));
        }

        public event EventHandler curveDrawn;
        public void newCurve(Point p0, Point p3, InkDrawingAttributes attributes)
        {
            BezierCurve curve = new BezierCurve(p0, p3, attributes);
            Point p1 = curve.P1;
            Point p2 = curve.P2;
            Point midPoint = curve.MidPoint;
            midPoints.Add(midPoint);
            if (!endPoints.Contains(p0)){
                endPoints.Add(p0);
            }
            if (!endPoints.Contains(p3)) { endPoints.Add(p3); }
            // controlPoints.Add(p1);
            // controlPoints.Add(p2);

            curves.Add(curve);
            if (fleur) {
                LineDrawnEventArgs args = new LineDrawnEventArgs();
                args.stroke = curve.InkStroke;
                curveDrawn?.Invoke(this, args);
            }
            else
            {
                container.AddStroke(curve.InkStroke);
            }
        }

        public class LineDrawnEventArgs : EventArgs
        {
            public InkStroke stroke { get; set; }
        }
    }
}
