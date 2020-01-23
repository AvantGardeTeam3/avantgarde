using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using System.ComponentModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Fleur : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private String backgroundHex { get; set; }
        private int WIDTH { get; set; }
        //the coordinate of the centre of the canvas
        private Point inkCanvasCentre;
        //the stroke drew by the user
        List<InkStroke> userStrokes = new List<InkStroke>();
        //used to build a stroke
        private InkStrokeBuilder inkStrokeBuilder;
        private int numberOfLines = 24;
        Stack<InkStroke> redoStack = new Stack<InkStroke>();
        public Fleur()
        {
            this.InitializeComponent();
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;
            //register the event with UWP
            inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkCanvasCentre.X = canvas.ActualWidth / 2;
            inkCanvasCentre.Y = canvas.ActualHeight / 2;
            inkStrokeBuilder = new InkStrokeBuilder();

            WIDTH = (int)Window.Current.Bounds.Width;
            backgroundHex = Colors.Black.ToString();
            this.toolbar.redoButtonClicked += this.Redo_Button_Click;
            this.toolbar.undoButtonClicked += this.Undo_Button_Click;
            this.toolbar.setBackgroundButtonClicked += this.toolbar_setBackgroundButtonClicked;
        }
        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs e)
        {
            //called everytime user finishes a stroke
            IReadOnlyList<InkStroke> strokes = e.Strokes;
            foreach (InkStroke stroke in strokes)
            {
                stroke.DrawingAttributes = toolbar.getDrawingAttributes();
                userStrokes.Add(stroke);
            }
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            inkCanvas.InkPresenter.StrokeContainer.AddStrokes(this.Transfrom(userStrokes));
        }
        private List<InkStroke> Transfrom(List<InkStroke> u)
        {
            List<InkStroke> transformedStrokes = new List<InkStroke>();
            foreach (InkStroke stroke in u)
            {
                transformedStrokes.AddRange(TransformStroke(stroke, 24));
            }
            return transformedStrokes;
        }
        private List<InkStroke> TransformStroke(InkStroke stroke, int num)
        {   //turning a stroke into a set of transformed strokes
            IReadOnlyList<InkPoint> inkPoints = stroke.GetInkPoints();
            List<InkStroke> ret = new List<InkStroke>();
            double thetaDiff = Math.PI * 2 / num;
            for (int i = 0; i < num; i++)
            {
                List<InkPoint> transformedInkPoints = new List<InkPoint>();
                foreach (InkPoint ip in inkPoints)
                {
                    Single pressure = ip.Pressure;
                    Point transformedPoint = TransformPoint(ip.Position, thetaDiff * i); 
                    transformedInkPoints.Add(new InkPoint(transformedPoint, pressure));
                }
                InkStroke transformedStroke = inkStrokeBuilder.CreateStrokeFromInkPoints(transformedInkPoints, System.Numerics.Matrix3x2.Identity, stroke.StrokeStartedTime, stroke.StrokeDuration);
                transformedStroke.DrawingAttributes = stroke.DrawingAttributes;
                ret.Add(transformedStroke);
            }
            return ret;
        }
        private Point TransformPoint(Point point, double thetaDiff)
        {   //spinning the point to the centre
            Point relPoint = ToRelative(point);
            Point polarPoint = ToPolar(relPoint);
            polarPoint.Y += thetaDiff;
            return ToAbsolute(ToCoordinate(polarPoint));
        }
        private Point ToPolar(Point point)
        {   //turn coordinate into polar form
            double distance = Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
            double x = point.X;
            double y = point.Y;
            double theta;
            if (x == 0 && y == 0)
                return new Point(distance, 0);
            if (y == 0 && x > 0)
                return new Point(distance, Math.PI / 2);
            if (y == 0 && x < 0)
                return new Point(distance, 3 / 2 * Math.PI);
            if (x == 0 && y > 0)
                return new Point(distance, 0);
            if (x == 0 && y < 0)
                return new Point(distance, Math.PI);

            if (x > 0 && y > 0)
                theta = Math.Atan(x / y);
            else if (x > 0 && y < 0)
                theta = Math.PI - Math.Atan(-x / y);
            else if (x < 0 && y > 0)
                theta = 2 * Math.PI - Math.Atan(-x / y);
            else
                theta = Math.Atan(x / y) + Math.PI;

            return new Point(distance, theta);
        }
        private Point ToCoordinate(Point point)
        {   //turn polar form into cooridnate
            Point ret = new Point();
            double theta = point.Y;
            double r = point.X;
            ret.X = r * Math.Sin(theta);
            ret.Y = r * Math.Cos(theta);
            return ret;
        }
        private Point ToRelative(Point point)
        {   //return coordinate relative to the centre point
            inkCanvasCentre.X = canvas.ActualWidth / 2;
            inkCanvasCentre.Y = canvas.ActualHeight / 2;
            return new Point(point.X - inkCanvasCentre.X, inkCanvasCentre.Y - point.Y);
        }
        private Point ToAbsolute(Point point)
        {   //return coordinate relative to origin point
            inkCanvasCentre.X = canvas.ActualWidth / 2;
            inkCanvasCentre.Y = canvas.ActualHeight / 2;
            return new Point(point.X + inkCanvasCentre.X, inkCanvasCentre.Y - point.Y);
        }
        private async void Redo_Button_Click(object sender, EventArgs e)
        {
            if (redoStack.Count() == 0)
            {
                return;
            }
            userStrokes.Add(redoStack.Pop());
            InkCanvas_refresh();
        }
        private void InkCanvas_refresh()
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            inkCanvas.InkPresenter.StrokeContainer.AddStrokes(this.Transfrom(userStrokes));
        }
        private async void Undo_Button_Click(object sender, EventArgs e)
        {
            int containerSize = 0;
            var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            foreach (InkStroke stroke in strokes)
            {
                containerSize++;
            }
            if (containerSize == 0)
            {
                return;
            }
            int index = 0;
            foreach (InkStroke s in strokes)
            {
                index++;
                if (index >= containerSize - (numberOfLines - 1))
                {
                    s.Selected = true;
                }
            }
            int size = userStrokes.Count();
            redoStack.Push(userStrokes.ElementAt(size-1));
            userStrokes.RemoveAt(userStrokes.Count() - 1);
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }
        private void toolbar_setBackgroundButtonClicked(object sender, EventArgs e)
        {
            backgroundHex = toolbar.getColourHex();
            NotifyPropertyChanged();
        }
    }
}
