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
        public List<BezierCurve> curves = new List<BezierCurve>();
        private List<Point> midPoints = new List<Point>(); 
        private List<Point> endPoints = new List<Point>();
        
        private Stack<BezierCurve> undoStack = new Stack<BezierCurve>();

        public BezierCurve FindCurveByHalfPoint(Point point)
        {
            return curves.Find(x => x.HalfPoint == point);
        }

        public InkDrawingAttributes attributes { get; set; }

        public List<BezierCurve> getCurves() {
            return curves;
        }
        public DrawingModel()
        {
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
        public List<Point> GetHalfPoints()
        {
            List<Point> retPoints = new List<Point>();
            foreach(BezierCurve curve in curves)
            {
                if (curve.Modified) retPoints.Add(curve.HalfPoint);
            }
            return retPoints;
        }
        public void Clear()
        {
            this.curves.Clear();
            this.endPoints.Clear();
            this.midPoints.Clear();
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
        public void Load(List<StrokeData> data)
        {
            this.curves.Clear();
            this.midPoints.Clear();
            this.endPoints.Clear();
            foreach(StrokeData stroke in data)
            {
                BezierCurve curve = new BezierCurve(stroke, InkDrawingAttributes.CreateForPencil());
                curves.Add(curve);
                this.midPoints.Add(curve.MidPoint);
                this.endPoints.Add(curve.P0);
                this.endPoints.Add(curve.P3);
            }
        }

        public BezierCurve moveMidPoint(Point point, Point position)
        {
            BezierCurve curve = curves.Find(x => x.MidPoint == point);
            curve.MidPoint = position;
            return curve;
        }

        public BezierCurve moveControlPoint(Point point, Point position)
        {
            BezierCurve curve = curves.Find(x => x.P1 == point || x.P2 == point);
            if(curve.P1 == point)
            {
                curve.P1 = position;
            } else
            {
                curve.P2 = position;
            }
            return curve;
        }

        public List<BezierCurve> moveEndPoints(Point point, Point position)
        {
            List<BezierCurve> involvedCurves = curves.FindAll(x => x.P0 == point || x.P3 == point);
            involvedCurves.ForEach(x => x.InkStroke.Selected = true);

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
            return involvedCurves;
        }

        // public event EventHandler curveDrawn;
        public BezierCurve newCurve(Point p0, Point p3, InkDrawingAttributes attributes)
        {
            BezierCurve curve = new BezierCurve(p0, p3, attributes);
            Point midPoint = curve.MidPoint;
            midPoints.Add(midPoint);
            if (!endPoints.Contains(p0)) { endPoints.Add(p0); }
            if (!endPoints.Contains(p3)) { endPoints.Add(p3); }

            curves.Add(curve);
            return curve;
        }

        public void deleteCurve(BezierCurve curve)
        {
            this.curves.Remove(curve);
        }

        public List<InkStroke> GetStrokes()
        {
            List<InkStroke> ret = new List<InkStroke>();
            curves.ForEach(x => ret.Add(x.InkStroke));
            return ret;
        }

        public class LineDrawnEventArgs : EventArgs
        {
            public InkStroke stroke { get; set; }

        }
    }
}
