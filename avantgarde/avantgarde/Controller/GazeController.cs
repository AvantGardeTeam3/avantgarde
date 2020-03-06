using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input.Preview;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.UI.Controls;

using avantgarde.Menus;
using Windows.UI.Xaml.Media;
using avantgarde.Joysticks;
using Microsoft.Toolkit.Uwp.Input.GazeInteraction;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using avantgarde.Drawing;

namespace avantgarde.Controller
{
    class GazeController
    {
        private IDrawMode page;
        private UI ui;
        private GazeInputSourcePreview gazeInputSourcePreview;
        private DrawingModel drawingModel;
        private DispatcherTimer Timer = new DispatcherTimer();
        private RadialProgressBar progressBar;
        private InkStrokeContainer container;
        private int TimerValue = 0;
        private bool TimerStarted = false;
        private bool _paused;
        public bool Paused
        {
            get
            {
                return _paused;
            }
            set
            {
                UpdateView();
                _paused = value;
            }
        }
        private ControllerState state;
        private Point GazePoint = new Point(0, 0);
        private Point? selectedPoint;


        private Point lineStartPoint;

        private InkStroke StrokeIndication = null;
        private VerticalJoystick ActiveVerticalJoystick = null;
        private Joystick ActiveJoystick = null;
        private Point joystickPosition;

        private List<Line> gridLines = null;
        private List<Shape> indicators = new List<Shape>();

        private BezierCurve _selectedCurve = null;
        private BezierCurve SelectedCurve
        {
            get
            {
                return _selectedCurve;
            }
            set
            {
                _selectedCurve = value;
            }
        }

        public GazeController(IDrawMode page)
        {
            this.page = page;
            this.gazeInputSourcePreview = page.GetGazeInputSourcePreview();
            this.drawingModel = page.GetDrawingModel();
            this.state = ControllerState.idle;
            this.progressBar = page.GetRadialProgressBar();
            this.container = page.GetInkCanvas().InkPresenter.StrokeContainer;
            this.ui = page.GetUI();
            this.gazeInputSourcePreview.GazeMoved += GazeMoved;
            Timer.Tick += GazeTimer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            ui.drawStateChanged += this.DrawStateChanged;
            InitGrid();
            HideGrid();
            DrawStateChanged(null, null);
        }
        private void DrawStateChanged(object sender, EventArgs e)
        {
            Paused = !ui.drawState;
        }
        private void GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
            // if (Paused) return;
            var point = args.CurrentPoint.EyeGazePosition;
            if (!point.HasValue)
            {
                return;
            }
            double distance = Util.distance(GazePoint, point.Value);
            GazePoint = point.Value;
            if (distance < 5 && !TimerStarted && !Paused)
            {
                if (state == ControllerState.idle ||
                    state == ControllerState.drawing ||
                    state == ControllerState.movingP0P3 ||
                    state == ControllerState.movingMid ||
                    state == ControllerState.movingControl)
                {
                    // start timer
                    TimerStarted = true;
                    this.Timer.Start();
                    this.progressBar.Visibility = Visibility.Visible;
                }
            }
            if ((Paused || distance >= 5) && TimerStarted)
            {
                // reset timer
                TimerStarted = false;
                this.Timer.Stop();
                this.TimerValue = 0;
                this.progressBar.Visibility = Visibility.Collapsed;
            }
            Moved(point.Value);
        }
        private void Moved(Point point)
        {
            TranslateTransform translateTarget = new TranslateTransform
            {
                X = point.X - progressBar.ActualWidth / 2,
                Y = point.Y - progressBar.ActualHeight / 2
            };
            progressBar.RenderTransform = translateTarget;
            switch (state)
            {
                case ControllerState.drawing:
                    if (StrokeIndication != null)
                    {
                        StrokeIndication.Selected = true;
                        container.DeleteSelected();
                    }
                    StrokeIndication = Util.MakeStroke(lineStartPoint, point);
                    container.AddStroke(StrokeIndication);
                    break;
            }
        }
        private void GazeDwell(Point gazePoint)
        {
            switch (this.state)
            {
                case ControllerState.pause:
                    break;
                case ControllerState.idle:
                    IdleGazeDwell(GazePoint);
                    break;
                case ControllerState.drawing:
                    EndLine(GazePoint);
                    break;
                case ControllerState.movingP0P3:
                    EndMovingP0P3(GazePoint);
                    break;
                case ControllerState.movingMid:
                    EndMovingMid(GazePoint);
                    break;
                case ControllerState.movingControl:
                    EndMovingControl(GazePoint);
                    break;
                case ControllerState.selectP0P3:
                    break;
                case ControllerState.selectMid:
                    break;
                case ControllerState.selectControl:
                    break;
            }
        }
        private void GazeTimer_Tick(object sender, object e)
        {
            this.TimerValue += 20;
            this.progressBar.Value = Convert.ToDouble(TimerValue) / Convert.ToDouble(Configuration.DrawingDwellTimeMs) * 120.0 - 20;
            if (this.TimerValue >= Configuration.DrawingDwellTimeMs)
            {
                GazeDwell(GazePoint);
                this.TimerValue = 0;
            }
        }
        private void IdleGazeDwell(Point point)
        {
            List<Point> controlPoints = new List<Point>();
            if(_selectedCurve != null)
            {
                controlPoints.Add(_selectedCurve.P1);
                controlPoints.Add(_selectedCurve.P2);
            }
            Point? controlPoint = Util.snapping(drawingModel.GetControlPoints(), point, Configuration.GazeSnapDistance);
            Point? midPoint = Util.snapping(drawingModel.GetMidPoints(), point, Configuration.GazeSnapDistance);
            Point? endPoint = Util.snapping(drawingModel.GetEndPoints(), point, Configuration.GazeSnapDistance);
            Point? halfPoint = Util.snapping(drawingModel.GetHalfPoints(), point, Configuration.GazeSnapDistance);
            if (controlPoint.HasValue)
            {
                // control point selected
                selectedPoint = controlPoint.Value;
                ActiveVerticalJoystick = InvokeVerticalJoystick(controlPoint.Value, ControlPointJoystickEventHandler);
                state = ControllerState.selectControl;
            }
            else if (midPoint.HasValue)
            {
                // mid point selected
                selectedPoint = midPoint.Value;
                _selectedCurve = drawingModel.FindCurveByHalfPoint(selectedPoint.Value);
                UpdateView();
                ActiveVerticalJoystick = InvokeVerticalJoystick(midPoint.Value, MidPointJoystickEventHandler);
                state = ControllerState.selectMid;
            }
            else if (endPoint.HasValue)
            {
                // end points selected
                selectedPoint = endPoint.Value;
                ActiveVerticalJoystick = InvokeVerticalJoystick(endPoint.Value, EndPointJoystickEventHandler);
                state = ControllerState.selectP0P3;
            }
            else if (halfPoint.HasValue)
            {
                selectedPoint = halfPoint.Value;
                _selectedCurve = drawingModel.FindCurveByHalfPoint(selectedPoint.Value);
                UpdateView();
            }
            else
            {
                // no point selected
                StartLine(point);
            }
        }
        private void StartLine(Point point)
        {
            lineStartPoint = point;
            this.state = ControllerState.drawing;
        }

        private void EndLine(Point point)
        {
            this.progressBar.Visibility = Visibility.Collapsed;
            if (StrokeIndication != null)
            {
                StrokeIndication.Selected = true;
                this.container.DeleteSelected();
                StrokeIndication = null;
            }
            Point? sp = Util.snapping(drawingModel.GetEndPoints(), GazePoint, Configuration.GazeSnapDistance);
            if (sp.HasValue)
            {
                Double midX = (lineStartPoint.X + sp.Value.X) / 2;
                Double midY = (lineStartPoint.Y + sp.Value.Y) / 2;
                drawingModel.newCurve(lineStartPoint, sp.Value, ui.getDrawingAttributes());
            }
            else
            {
                Double midX = (lineStartPoint.X + GazePoint.X) / 2;
                Double midY = (lineStartPoint.Y + GazePoint.Y) / 2;
                drawingModel.newCurve(lineStartPoint, GazePoint, ui.getDrawingAttributes());
            }
            this.state = ControllerState.idle;
            UpdateView();
        }

        private void StartMovingP0P3(Point point)
        {

        }

        private void EndMovingP0P3(Point point)
        {
            drawingModel.moveEndPoints(selectedPoint.Value, point);
            UpdateIndicator();
            this.state = ControllerState.idle;
        }

        private void StartMovingMid(Point point)
        {

        }

        private void EndMovingMid(Point point)
        {
            drawingModel.moveMidPoint(selectedPoint.Value, point);
            UpdateIndicator();
            this.state = ControllerState.idle;
        }

        private void StartMovingControl(Point point)
        {

        }

        private void EndMovingControl(Point point)
        {
            drawingModel.moveControlPoint(selectedPoint.Value, point);
            _selectedCurve = null;
            UpdateIndicator();
            this.state = ControllerState.idle;   
        }

        private VerticalJoystick InvokeVerticalJoystick(Point center, Action<object, StateChangedEventArgs> func)
        {
            joystickPosition = center;
            VerticalJoystick joystick = new VerticalJoystick
            {
                Height = 200,
                Width = 200
            };

            TranslateTransform translateTarget = new TranslateTransform
            {
                X = center.X - joystick.Width / 2,
                Y = center.Y - joystick.Height / 2
            };
            joystick.RenderTransform = translateTarget;
            joystick.Visibility = Visibility.Visible;
            joystick.GazeStateChangeHandler.Add(func);

            this.page.GetCanvas().Children.Add(joystick);
            return joystick;
        }
        private Joystick InvokeJoystick(Point center, Action<object, StateChangedEventArgs> func)
        {
            Joystick joystick = new Joystick()
            {
                Height = 200,
                Width = 200
            };
            TranslateTransform translateTarget = new TranslateTransform
            {
                X = center.X - joystick.Width / 2,
                Y = center.Y - joystick.Height / 2
            };
            joystick.RenderTransform = translateTarget;
            joystick.Visibility = Visibility.Visible;
            joystick.GazeStateChangeHandler.Add(func);

            this.page.GetCanvas().Children.Add(joystick);
            return joystick;
        }
        private void EndPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell)
            {
                return;
            }
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "UpKey":
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    this.StartLine(joystickPosition);
                    this.state = ControllerState.drawing;
                    break;
                case "DownKey":
                    //this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    //this.StartMovingP0P3(selectedPoint.Value);
                    //this.state = ControllerState.movingP0P3;
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveJoystick = InvokeJoystick(joystickPosition, MoveEndPointJoystickEventHandler);
                    break;
            }
        }
        private void MidPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell)
            {
                return;
            }
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "UpKey":
                    //this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    //this.StartMovingMid(selectedPoint.Value);
                    //this.state = ControllerState.movingMid;
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    BezierCurve curve = drawingModel.FindCurveByHalfPoint(selectedPoint.Value);
                    drawingModel.delteCruve(curve);
                    curve.InkStroke.Selected = true;
                    container.DeleteSelected();
                    _selectedCurve = null;
                    UpdateView();
                    this.state = ControllerState.idle;
                    break;
                case "DownKey":
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveJoystick = InvokeJoystick(joystickPosition, MoveMidPointJoystickEventHandler);
                    break;
            }
        }
        private void ControlPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell)
            {
                return;
            }
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "UpKey":
                    page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    state = ControllerState.idle;
                    break;
                case "DownKey":
                    //this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    //StartMovingControl(selectedPoint.Value);
                    //this.state = ControllerState.movingControl;
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveJoystick = InvokeJoystick(joystickPosition, MoveControlPointJoystickEventHandler);
                    break;
            }
        }

        private void MoveEndPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell) return;
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "MidKey":
                    this.page.GetCanvas().Children.Remove(ActiveJoystick);
                    this.state = ControllerState.idle;
                    break;
                case "UpKey":
                    joystickPosition.Y -= Configuration.JoystickMoveDistance;
                    break;
                case "DownKey":
                    joystickPosition.Y += Configuration.JoystickMoveDistance;
                    break;
                case "LeftKey":
                    joystickPosition.X -= Configuration.JoystickMoveDistance;
                    break;
                case "RightKey":
                    joystickPosition.X += Configuration.JoystickMoveDistance;
                    break;
            }
            TranslateTransform translateTarget = new TranslateTransform
            {
                X = joystickPosition.X - ActiveJoystick.Width / 2,
                Y = joystickPosition.Y - ActiveJoystick.Height / 2
            };
            ActiveJoystick.RenderTransform = translateTarget;
            drawingModel.moveEndPoints(selectedPoint.Value, joystickPosition);
            selectedPoint = joystickPosition;
            UpdateIndicator();
        }

        private void MoveMidPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell) return;
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "MidKey":
                    this.page.GetCanvas().Children.Remove(ActiveJoystick);
                    this.state = ControllerState.idle;
                    break;
                case "UpKey":
                    joystickPosition.Y -= Configuration.JoystickMoveDistance;
                    break;
                case "DownKey":
                    joystickPosition.Y += Configuration.JoystickMoveDistance;
                    break;
                case "LeftKey":
                    joystickPosition.X -= Configuration.JoystickMoveDistance;
                    break;
                case "RightKey":
                    joystickPosition.X += Configuration.JoystickMoveDistance;
                    break;
            }
            TranslateTransform translateTarget = new TranslateTransform
            {
                X = joystickPosition.X - ActiveJoystick.Width / 2,
                Y = joystickPosition.Y - ActiveJoystick.Height / 2
            };
            ActiveJoystick.RenderTransform = translateTarget;
            drawingModel.moveMidPoint(selectedPoint.Value, joystickPosition);
            selectedPoint = joystickPosition;
            UpdateIndicator();
        }


        private void MoveControlPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell) return;
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "MidKey":
                    this.page.GetCanvas().Children.Remove(ActiveJoystick);
                    this.state = ControllerState.idle;
                    break;
                case "UpKey":
                    joystickPosition.Y -= Configuration.JoystickMoveDistance;
                    break;
                case "DownKey":
                    joystickPosition.Y += Configuration.JoystickMoveDistance;
                    break;
                case "LeftKey":
                    joystickPosition.X -= Configuration.JoystickMoveDistance;
                    break;
                case "RightKey":
                    joystickPosition.X += Configuration.JoystickMoveDistance;
                    break;
            }
            TranslateTransform translateTarget = new TranslateTransform
            {
                X = joystickPosition.X - ActiveJoystick.Width / 2,
                Y = joystickPosition.Y - ActiveJoystick.Height / 2
            };
            ActiveJoystick.RenderTransform = translateTarget;
            drawingModel.moveControlPoint(selectedPoint.Value, joystickPosition);
            selectedPoint = joystickPosition;
            UpdateIndicator();
        }
        private void UpdateView()
        {
            if (Paused)
            {
                HideGrid();
                HideIndicator();
            }
            else
            {
                ShowGrid();
                UpdateIndicator();
                ShowIndicator();
            }
        }

        private void InitGrid()
        {
            this.gridLines = new List<Line>();
            int interval = 50;
            int height = (int)Window.Current.Bounds.Height;
            int width = (int)Window.Current.Bounds.Width;
            Canvas canvas = page.GetCanvas();
            // horizontal grid lines
            for (int y = 0; y < height; y += interval)
            {
                Line line = new Line();
                line.X1 = 0;
                line.X2 = width;
                line.Y1 = y;
                line.Y2 = y;
                line.Stroke = new SolidColorBrush(Colors.LightSteelBlue);
                line.StrokeThickness = 1;
                line.Visibility = Visibility.Collapsed;
                Canvas.SetTop(line, 0);
                Canvas.SetLeft(line, 0);
                canvas.Children.Add(line);
                this.gridLines.Add(line);
            }
            // vertical grid lines
            for (int x = 0; x < width; x += interval)
            {
                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 0;
                line.Y2 = height;
                line.Stroke = new SolidColorBrush(Colors.LightSteelBlue);
                line.StrokeThickness = 1;
                line.Visibility = Visibility.Collapsed;
                Canvas.SetTop(line, 0);
                Canvas.SetLeft(line, 0);
                canvas.Children.Add(line);
                this.gridLines.Add(line);
            }
        }

        private void ShowGrid()
        {
            if (gridLines == null) return;
            gridLines.ForEach(x => x.Visibility = Visibility.Visible);
        }

        public void HideGrid()
        {
            if (gridLines == null) return;
            gridLines.ForEach(x => x.Visibility = Visibility.Collapsed);
        }

        private void UpdateIndicator()
        {
            ClearIndicator();
            if (_selectedCurve != null)
            {
                AddCurve(_selectedCurve);
            }

            
            List<Point> points = drawingModel.GetEndPoints();
            points.ForEach(x => AddIndicator(x));
            List<Point> midPoints = drawingModel.GetMidPoints();
            midPoints.ForEach(x => AddIndicator(x));
            //List<Point> controlPoints = drawingModel.GetControlPoints();
            //controlPoints.ForEach(x => AddIndicator(x));
            List<Point> halfPoints = drawingModel.GetHalfPoints();
            halfPoints.ForEach(x => AddIndicator(x));
            ShowIndicator();
        }

        private void AddIndicator(Point point)
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
            this.page.GetCanvas().Children.Add(ellipse);
        }

        private void AddCurve(BezierCurve curve)
        {
            AddLine(curve.P0, curve.P1);
            AddLine(curve.P3, curve.P2);
            AddControlIndicator(curve.P1);
            AddControlIndicator(curve.P2);
        }

        private void AddControlIndicator(Point point)
        {
            var rectangle = new Rectangle();
            rectangle.Width = 15;
            rectangle.Height = 15;
            rectangle.Fill = new SolidColorBrush(Windows.UI.Colors.SteelBlue);
            rectangle.Visibility = Visibility.Visible;
            TranslateTransform translateTarget = new TranslateTransform();
            translateTarget.X = point.X - rectangle.Width / 2;
            translateTarget.Y = point.Y - rectangle.Height / 2;
            rectangle.RenderTransform = translateTarget;
            indicators.Add(rectangle);
            this.page.GetCanvas().Children.Add(rectangle);
        }

        private void AddLine(Point p1, Point p2)
        {
            var line = new Line()
            {
                X1 = p1.X,
                Y1 = p1.Y,
                X2 = p2.X,
                Y2 = p2.Y
            };
            line.Stroke = new SolidColorBrush(Windows.UI.Colors.Black);
            line.StrokeThickness = 2;
            line.Fill = new SolidColorBrush(Windows.UI.Colors.Black);
            line.Visibility = Visibility.Visible;
            indicators.Add(line);
            this.page.GetCanvas().Children.Add(line);
        }

        private void ClearIndicator()
        {
            this.indicators.ForEach(x => this.page.GetCanvas().Children.Remove(x));
            this.indicators.Clear();
        }
        
        private void HideIndicator()
        {
            this.indicators.ForEach(x => x.Visibility = Visibility.Collapsed);
        }

        private void ShowIndicator()
        {
            this.indicators.ForEach(x => x.Visibility = Visibility.Visible);
        }
    }
    enum ControllerState
    {
        idle,
        pause,
        drawing,
        movingP0P3,
        movingMid,
        movingControl,
        selectP0P3,
        selectMid,
        selectControl
    }
}
