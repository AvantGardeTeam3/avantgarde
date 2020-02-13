
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;

using System.ComponentModel;
using System.Data;
using System.Windows.Input;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

using Windows.Devices.Input.Preview;
using System.Collections.ObjectModel;
using Windows.UI.Input.Inking;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Microsoft.Toolkit.Uwp;

using Windows.UI.Xaml.Navigation;
using RoutedEventArgs = Windows.UI.Xaml.RoutedEventArgs;
using Windows.UI.Xaml.Shapes;
using Windows.Devices.Input;
using Windows.UI.Input.Inking.Analysis;

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
        private int HEIGHT { get; set; }
        //the coordinate of the centre of the canvas
        private Point inkCanvasCentre;
        //the stroke drew by the user
        List<InkStroke> userStrokes = new List<InkStroke>();
        //used to build a stroke
        private InkStrokeBuilder inkStrokeBuilder;
        private int numberOfLines = 24;
        Stack<InkStroke> redoStack = new Stack<InkStroke>();

        private Point gazePoint = new Point(0, 0);
        private Point startPoint;
        private Point joystickPoint;

        private InkStroke indicatingStroke = null;

        private bool IsMoving = false;
        private bool PenDown = false;
        private bool JoystickInvoked = false;
        private DispatcherTimer gazeTimer = new DispatcherTimer();
        private bool gazeTimerStarted = false;
        private int timer = 0;
        private int dwellTime = 600;

        private DrawingModel drawingModel;

        private DispatcherTimer dispatchTimer = new DispatcherTimer();

        private InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
        private GazeInputSourcePreview gazeInputSourcePreview;

        private List<Ellipse> indicators;
        private bool drawState { get; set; }

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


            HEIGHT = (int)Window.Current.Bounds.Height;
            WIDTH = (int)Window.Current.Bounds.Width;
            backgroundHex = Colors.Black.ToString();
            toolbar.redoButtonClicked += this.Redo_Button_Click;
            toolbar.undoButtonClicked += this.Undo_Button_Click;
            toolbar.setBackgroundButtonClicked += this.toolbar_setBackgroundButtonClicked;

            gazeInputSourcePreview = GazeInputSourcePreview.GetForCurrentView();
            gazeInputSourcePreview.GazeMoved += GazeInputSourcePreview_GazeMoved;

            gazeTimer.Tick += GazeTimer_Tick;
            gazeTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);

            dispatchTimer.Tick += DispatchTimer_Tick;
            dispatchTimer.Interval = new TimeSpan(0, 0, 1);
            dispatchTimer.Start();

            drawingModel = new DrawingModel(inkCanvas.InkPresenter.StrokeContainer);
            indicators = new List<Ellipse>();
        }

        private void GazeTimer_Tick(object sender, object e)
        {
            this.timer += 20;
            radialProgressBar.Value = Convert.ToDouble(timer) / Convert.ToDouble(dwellTime) * 120.0 - 20;

            if (this.timer >= this.dwellTime)
            {
                GazeDwell();
                timer = 0;
            }
        }
        private void DispatchTimer_Tick(object sender, object e)
        {
        }

        private void GazeInputSourcePreview_GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
            var point = args.CurrentPoint.EyeGazePosition;
            if (!point.HasValue)
            {
                return;
            }
            double distance = Math.Pow(gazePoint.X - point.Value.X, 2) + Math.Pow(gazePoint.Y - point.Value.Y, 2);
            distance = Math.Sqrt(distance);
            gazePoint.X = point.Value.X;
            gazePoint.Y = point.Value.Y;

            Point p = ToCanvasPoint(gazePoint);
            if (!drawState)
            {
                gazeTimer.Stop();
                this.gazeTimerStarted = false;
                this.timer = 0;
            }
            TranslateTransform translateTarget = new TranslateTransform();
            translateTarget.X = p.X - radialProgressBar.ActualWidth / 2;
            translateTarget.Y = p.Y - radialProgressBar.ActualHeight / 2;
            radialProgressBar.RenderTransform = translateTarget;

            int lockOnDistance = 5;

            if (distance < lockOnDistance && !gazeTimerStarted && !JoystickInvoked)
            {
                radialProgressBar.Visibility = Visibility.Visible;
                gazeTimerStarted = true;
                gazeTimer.Start();
            }
            else if (distance > lockOnDistance && gazeTimerStarted)
            {
                radialProgressBar.Visibility = Visibility.Collapsed;
                gazeTimerStarted = false;
                gazeTimer.Stop();
                timer = 0;
            }
            if (PenDown && !JoystickInvoked)
            {
                if (indicatingStroke != null)
                {
                    indicatingStroke.DrawingAttributes = drawingAttributes;
                    indicatingStroke.Selected = true;
                    inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                    indicatingStroke = MakeStroke(startPoint, ToCanvasPoint(gazePoint));
                    inkCanvas.InkPresenter.StrokeContainer.AddStroke(indicatingStroke);
                }
                else
                {
                    indicatingStroke = MakeStroke(startPoint, ToCanvasPoint(gazePoint));
                    inkCanvas.InkPresenter.StrokeContainer.AddStroke(indicatingStroke);
                }
            }
        }

        private void GazeDwell()
        {
            if (JoystickInvoked)
            {
                return;
            }
            if (IsMoving)
            {
                EndMoving(ToCanvasPoint(gazePoint));
                return;
            }
            if (!PenDown)
            {
                // not drawing

                Point? sp = Snapping(ToCanvasPoint(gazePoint));
                if (sp.HasValue)
                {
                    InvokeJoystick(sp.Value);
                }
                else
                {
                    StartDrawing(ToCanvasPoint(gazePoint));
                }
            }
            else
            {
                // drawing
                EndDrawing(gazePoint);
            }
        }

        private void StartMoving(Point start)
        {
            IsMoving = true;
            startPoint = start;
        }

        private void EndMoving(Point end)
        {
            IsMoving = false;
            drawingModel.move(startPoint, end);
            this.ClearPointIndicators();
            List<Point> points = drawingModel.getPoints();
            this.AddPointIndicators(points);
        }

        private void StartDrawing(Point start)
        {
            PenDown = true;
            startPoint = start;
        }

        private void EndDrawing(Point end)
        {
            PenDown = false;
            Point? sp = Snapping(ToCanvasPoint(gazePoint));
            if (sp.HasValue)
            {
                drawingModel.newLine(startPoint, sp.Value, drawingAttributes);
            }
            else
            {
                drawingModel.newLine(startPoint, ToCanvasPoint(gazePoint), drawingAttributes);
            }
            List<Point> points = drawingModel.getPoints();
            AddPointIndicators(points);
            if (indicatingStroke != null)
            {
                indicatingStroke.Selected = true;
                inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                indicatingStroke = null;
            }
        }

        private List<Line> GridLines = new List<Line>();

        private void InitGrid()
        {
            int interval = 50;
            // horizontal grid lines
            for (int y = 0; y < HEIGHT; y += interval)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = WIDTH;
                line.Y1 = y;
                line.Y2 = y;
                line.Stroke = new SolidColorBrush(Colors.LightSteelBlue);
                line.StrokeThickness = 1;
                Canvas.SetTop(line, 0);
                Canvas.SetLeft(line, 0);
                canvas.Children.Add(line);
                this.GridLines.Add(line);
            }
            // vertical grid lines
            for (int x = 0; x < WIDTH; x += interval)
            {
                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 0;
                line.Y2 = HEIGHT;
                line.Stroke = new SolidColorBrush(Colors.LightSteelBlue);
                line.StrokeThickness = 1;
                Canvas.SetTop(line, 0);
                Canvas.SetLeft(line, 0);
                canvas.Children.Add(line);
                this.GridLines.Add(line);
            }
        }

        private void ShowGrid()
        {
            if (GridLines.Count == 0)
            {
                InitGrid();
            }
            foreach (Line line in GridLines)
            {
                line.Visibility = Visibility.Visible;
            }
        }

        private void HideGrid()
        {
            foreach (Line line in GridLines)
            {
                line.Visibility = Visibility.Collapsed;
            }
        }
        private void ClearPointIndicators()
        {
            foreach (var ellipse in indicators)
            {
                canvas.Children.Remove(ellipse);
            }
            indicators.Clear();
        }
        private void AddPointIndicators(List<Point> points)
        {
            foreach (var point in points)
            {
                var ellipse = new Ellipse();
                ellipse.Width = 20;
                ellipse.Height = 20;
                ellipse.Fill = new SolidColorBrush(Windows.UI.Colors.SteelBlue);
                ellipse.Visibility = Visibility.Visible;
                TranslateTransform translateTarget = new TranslateTransform();
                translateTarget.X = point.X - ellipse.Width / 2;
                translateTarget.Y = point.Y - ellipse.Height / 2;
                ellipse.RenderTransform = translateTarget;
                indicators.Add(ellipse);
                canvas.Children.Add(ellipse);
            }

            if (!drawState)
            {
                HidePointIndicators();
            }
        }

        private void HidePointIndicators()
        {
            foreach (Ellipse ellipse in indicators)
            {
                ellipse.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowPointIndicators()
        {
            foreach (Ellipse ellipse in indicators)
            {
                ellipse.Visibility = Visibility.Visible;
            }
        }
        private void InvokeJoystick(Point center)
        {
            JoystickInvoked = true;
            joystickPoint = center;
            TranslateTransform translateTarget = new TranslateTransform();
            translateTarget.X = center.X - canvasJoystick.Width / 2;
            translateTarget.Y = center.Y - canvasJoystick.Height / 2;
            canvasJoystick.RenderTransform = translateTarget;
            canvasJoystick.Visibility = Visibility.Visible;
            canvasJoystick.GazeStateChangeHandler.Add(JoystickGazeEventHandler);
        }

        private void DismissJoystick()
        {
            JoystickInvoked = false;
            canvasJoystick.Visibility = Visibility.Collapsed;
            canvasJoystick.GazeStateChangeHandler.Remove(JoystickGazeEventHandler);
        }

        private void JoystickGazeEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell)
            {
                return;
            }
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "UpKey":
                    this.StartDrawing(joystickPoint);
                    this.DismissJoystick();
                    break;
                case "DownKey":
                    this.StartMoving(joystickPoint);
                    this.DismissJoystick();
                    break;
            }
        }
        private Point ToCanvasPoint(Point point)
        {
            Double x = point.X;
            Double y = point.Y;
            y -= TopGrid.RowDefinitions[0].ActualHeight;
            return new Point(x, y);
        }
        private Point? Snapping(Point p)
        {
            List<Point> points = drawingModel.getPoints();
            foreach (Point ep in points)
            {
                double distance = Math.Sqrt(Math.Pow(p.X - ep.X, 2) + Math.Pow(p.Y - ep.Y, 2));
                if (distance < 30) return ep;
            }
            return null;
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
