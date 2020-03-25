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

        public IDrawMode page;
        private UI ui;
        private GazeInputSourcePreview gazeInputSourcePreview;
        public DrawingModel drawingModel;
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
                if (_paused != value)
                {
                    if (value) Pause();
                    else Resume();
                }
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


        private List<InkStroke> mandalaStrokes = null;
        private List<Line> gridLines = null;
        public ColourManager colourManager;
        public InkCanvas inkCanvas { get; set; }


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
            this.inkCanvas = page.GetInkCanvas();
            this.ui = page.GetUI();
            this.gazeInputSourcePreview.GazeMoved += GazeMoved;
            this.colourManager = page.GetColourManager();
            Timer.Tick += GazeTimer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            ui.drawStateChanged += this.DrawStateChanged;
            InitGrid();
            HideGrid();
            DrawStateChanged(null, null);
            thisController = this;
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
                    state == ControllerState.drawing)
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
                        page.GetInkCanvas().InkPresenter.StrokeContainer.DeleteSelected();
                    }
                    StrokeIndication = Util.MakeStroke(lineStartPoint, point);
                    StrokeIndication.DrawingAttributes = ui.getDrawingAttributes();
                    page.GetInkCanvas().InkPresenter.StrokeContainer.AddStroke(StrokeIndication);
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
            if (_selectedCurve != null)
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
                // ActiveVerticalJoystick = InvokeVerticalJoystick(controlPoint.Value, ControlPointJoystickEventHandler);
                // state = ControllerState.selectControl;
                state = ControllerState.movingControl;
                joystickPosition = selectedPoint.Value;
                ActiveJoystick = InvokeJoystick(controlPoint.Value, MoveControlPointJoystickEventHandler);
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
                ActiveVerticalJoystick = InvokeVerticalJoystick(halfPoint.Value, HalfPointJoystickEventHandler);
                state = ControllerState.selectHalf;
            }
            else
            {
                // no point selected
                StartLine(point);
            }
        }

        private void Pause()
        {
            List<BezierCurve> curves = drawingModel.getCurves();
            mandalaStrokes = new List<InkStroke>();
            foreach (BezierCurve curve in curves)
            {
                List<InkStroke> strokes = ((Fleur)page).TransformStroke(curve.InkStroke, curve.NumOfReflection);
                strokes.ForEach(x => x.DrawingAttributes = curve.InkStroke.DrawingAttributes);
                page.GetInkCanvas().InkPresenter.StrokeContainer.AddStrokes(strokes);
                mandalaStrokes.AddRange(strokes);
            }
            if (ActiveJoystick != null) page.GetCanvas().Children.Remove(ActiveJoystick);
            if (ActiveVerticalJoystick != null) page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
            state = ControllerState.idle;
            UpdateView();
        }

        private void Resume()
        {
            if (mandalaStrokes != null)
            {
                // remove all mandala strokes
                mandalaStrokes.ForEach(x => x.Selected = true);
                page.GetInkCanvas().InkPresenter.StrokeContainer.DeleteSelected();
                mandalaStrokes = null;
            }
            UpdateView();
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
                page.GetInkCanvas().InkPresenter.StrokeContainer.DeleteSelected();
                StrokeIndication = null;
            }
            Point? sp = Util.snapping(drawingModel.GetEndPoints(), GazePoint, Configuration.GazeSnapDistance);
            BezierCurve bezierCurve;
            if (sp.HasValue)
            {
                bezierCurve = drawingModel.newCurve(lineStartPoint, sp.Value, ((Fleur)page).getStrokeData());
            }
            else
            {
                bezierCurve = drawingModel.newCurve(lineStartPoint, GazePoint, ((Fleur)page).getStrokeData());
            }
            bezierCurve.strokeData = ((Fleur)page).getStrokeData();
            bezierCurve.NumOfReflection = page.GetUI().getMandalaLines();
            this.state = ControllerState.idle;
            if (Configuration.fleur.Autoswitch)
            {
                page.GetUI().UIGetColourManager().nextColour();
                page.GetUI().getToolbox().next();
            }
            // ((Fleur)page).curveDrawn(null, null);
            // UpdateCanvas();
            page.GetInkCanvas().InkPresenter.StrokeContainer.AddStroke(bezierCurve.InkStroke);
            UpdateView();
        }

        private void StartMovingP0P3(Point point)
        {

        }

        private void EndMovingP0P3(Point point)
        {
            List<BezierCurve> curves = drawingModel.getCurves().FindAll(x => x.P0 == selectedPoint.Value || x.P3 == selectedPoint.Value);
            curves.ForEach(x => x.InkStroke.Selected = true);
            page.GetInkCanvas().InkPresenter.StrokeContainer.DeleteSelected();
            curves = drawingModel.moveEndPoints(selectedPoint.Value, point);
            curves.ForEach(x => page.GetInkCanvas().InkPresenter.StrokeContainer.AddStroke(x.InkStroke));

            // UpdateCanvas();
            UpdateIndicator();
            this.state = ControllerState.idle;
        }

        private void StartMovingMid(Point point)
        {

        }

        private void EndMovingMid(Point point)
        {
            BezierCurve curve = drawingModel.getCurves().Find(x => x.MidPoint == selectedPoint.Value);
            curve.InkStroke.Selected = true;
            page.GetInkCanvas().InkPresenter.StrokeContainer.DeleteSelected();
            curve = drawingModel.moveMidPoint(selectedPoint.Value, point);
            page.GetInkCanvas().InkPresenter.StrokeContainer.AddStroke(curve.InkStroke);
            // UpdateCanvas();
            UpdateIndicator();
            this.state = ControllerState.idle;
        }

        private void StartMovingControl(Point point)
        {

        }

        private void EndMovingControl(Point point)
        {
            BezierCurve curve = drawingModel.getCurves().Find(x => x.P1 == selectedPoint.Value || x.P2 == selectedPoint.Value);
            curve.InkStroke.Selected = true;
            page.GetInkCanvas().InkPresenter.StrokeContainer.DeleteSelected();
            curve = drawingModel.moveControlPoint(selectedPoint.Value, point);
            page.GetInkCanvas().InkPresenter.StrokeContainer.AddStroke(curve.InkStroke);

            //_selectedCurve = null;
            //UpdateCanvas();
            UpdateIndicator();
            //this.state = ControllerState.idle;   
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
            joystick.Width = 300;
            joystick.Height = 300;

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
                    ActiveVerticalJoystick = null;
                    this.StartLine(joystickPosition);
                    this.state = ControllerState.drawing;
                    break;
                case "DownKey":
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveVerticalJoystick = null;
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
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveVerticalJoystick = null;
                    BezierCurve curve = drawingModel.FindCurveByHalfPoint(selectedPoint.Value);
                    drawingModel.deleteCurve(curve);
                    if (curve == _selectedCurve) _selectedCurve = null;
                    curve.InkStroke.Selected = true;
                    container.DeleteSelected();
                    _selectedCurve = null;
                    UpdateView();
                    this.state = ControllerState.idle;
                    break;
                case "DownKey":
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveVerticalJoystick = null;
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
                    ActiveVerticalJoystick = null;
                    state = ControllerState.idle;
                    break;
                case "DownKey":
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveVerticalJoystick = null;
                    ActiveJoystick = InvokeJoystick(joystickPosition, MoveControlPointJoystickEventHandler);
                    break;
            }
        }

        private void HalfPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell) return;
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "UpKey":
                    page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveVerticalJoystick = null;
                    BezierCurve curve = drawingModel.FindCurveByHalfPoint(selectedPoint.Value);
                    drawingModel.deleteCurve(curve);
                    if (curve == _selectedCurve) _selectedCurve = null;
                    curve.InkStroke.Selected = true;
                    container.DeleteSelected();
                    _selectedCurve = null;
                    UpdateView();
                    this.state = ControllerState.idle;
                    break;
                case "DownKey":
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    ActiveVerticalJoystick = null;
                    this.state = ControllerState.idle;
                    break;
            }
        }
        private void MoveEndPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell) return;
            Button button = (Button)sender;
            bool exitFlag = false;
            switch (button.Name)
            {
                case "MidKey":
                    this.page.GetCanvas().Children.Remove(ActiveJoystick);
                    this.state = ControllerState.idle;
                    exitFlag = true;
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
            EndMovingP0P3(joystickPosition);
            if (!exitFlag)
            {
                this.state = ControllerState.movingP0P3;
            }
            else
            {
                ActiveJoystick = null;
            }
            // drawingModel.moveEndPoints(selectedPoint.Value, joystickPosition);
            selectedPoint = joystickPosition;
            // UpdateCanvas();
            UpdateIndicator();
        }

        private void MoveMidPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell) return;
            Button button = (Button)sender;
            bool exitFlag = false;
            switch (button.Name)
            {
                case "MidKey":
                    this.page.GetCanvas().Children.Remove(ActiveJoystick);
                    this.state = ControllerState.idle;
                    exitFlag = true;
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
            EndMovingMid(joystickPosition);
            if (!exitFlag) { this.state = ControllerState.movingMid; }
            else { ActiveJoystick = null; }
            // drawingModel.moveMidPoint(selectedPoint.Value, joystickPosition);
            selectedPoint = joystickPosition;
            // UpdateCanvas();
            UpdateIndicator();
        }

        private void MoveControlPointJoystickEventHandler(object sender, StateChangedEventArgs args)
        {
            if (args.PointerState != PointerState.Dwell) return;
            Button button = (Button)sender;
            bool exitFlag = false;
            switch (button.Name)
            {
                case "MidKey":
                    this.page.GetCanvas().Children.Remove(ActiveJoystick);
                    this.state = ControllerState.idle;
                    exitFlag = true;
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
            EndMovingControl(joystickPosition);
            if (!exitFlag)
            {
                this.state = ControllerState.movingControl;
            }
            else
            {
                ActiveJoystick = null;
            }
            // drawingModel.moveControlPoint(selectedPoint.Value, joystickPosition);
            selectedPoint = joystickPosition;
            // UpdateCanvas();
            UpdateIndicator();
        }

        public void Undo()
        {
            BezierCurve curve = drawingModel.undo();
            if (curve != null)
            {
                curve.InkStroke.Selected = true;
                page.GetInkCanvas().InkPresenter.StrokeContainer.DeleteSelected();
            }
            if (curve == _selectedCurve)
            {
                _selectedCurve = null;
            }
            Resume();
            Pause();
        }

        public void Redo()
        {
            BezierCurve curve = drawingModel.redo();
            if (curve != null)
            {
                curve.UpdateStroke();
                page.GetInkCanvas().InkPresenter.StrokeContainer.AddStroke(curve.InkStroke);
            }
            Resume();
            Pause();
        }

        public void ClearCanvas()
        {
            drawingModel.Clear();
            mandalaStrokes = null;
            _selectedCurve = null;
            page.GetInkCanvas().InkPresenter.StrokeContainer.Clear();
        }

        public void UpdateCanvas()
        {
            InkStrokeContainer container = page.GetInkCanvas().InkPresenter.StrokeContainer;
            List<InkStroke> dm_strokes = drawingModel.GetStrokes();
            foreach (InkStroke stroke in container.GetStrokes())
            {
                if (!dm_strokes.Contains(stroke))
                {
                    stroke.Selected = true;
                }
            }
            container.DeleteSelected();
            foreach (InkStroke stroke in dm_strokes)
            {
                if (!container.GetStrokes().Contains(stroke))
                {
                    container.AddStroke(stroke);
                }
            }
            //InkStrokeContainer container = new InkStrokeContainer();
            //container.AddStrokes(drawingModel.GetStrokes());
            //page.GetInkCanvas().InkPresenter.StrokeContainer = container;
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
            ellipse.Width = 30;
            ellipse.Height = 30;
            ellipse.Fill = new SolidColorBrush(ColourManager.hexToColor("#ffcdff59"));
            ellipse.Opacity = 0.5d;
            ellipse.Visibility = Visibility.Visible;
            TranslateTransform translateTarget = new TranslateTransform();
            translateTarget.X = point.X - ellipse.Width / 2;
            translateTarget.Y = point.Y - ellipse.Height / 2;
            ellipse.RenderTransform = translateTarget;
            indicators.Add(ellipse);
            this.page.GetCanvas().Children.Add(ellipse);
        }

        public void Load(List<StrokeData> strokeDatas)
        {
            drawingModel.Clear();
            page.GetInkCanvas().InkPresenter.StrokeContainer.Clear();
            _selectedCurve = null;
            foreach (StrokeData data in strokeDatas)
            {
                InkDrawingAttributes attributes;
                if (data.brush.Equals("pencil"))
                {
                    attributes = InkDrawingAttributes.CreateForPencil();
                }
                else
                {
                    attributes = new InkDrawingAttributes();
                }
                attributes.Color = page.GetUI().getColour(data.colourProfile, data.brightness, data.opacity);
                attributes.Size = data.size;
                BezierCurve curve = drawingModel.newCurve(data.p0, data.p3, data);
                curve.P1 = data.p1;
                curve.P2 = data.p2;
                curve.Modified = data.modified;
                curve.NumOfReflection = data.reflections;
                curve.strokeData = data;
                page.GetInkCanvas().InkPresenter.StrokeContainer.AddStroke(curve.InkStroke);
            }
            List<BezierCurve> curves = drawingModel.getCurves();
            if (mandalaStrokes != null) mandalaStrokes.Clear();
            Resume();
            Pause();
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
            rectangle.Width = 20;
            rectangle.Height = 20;
            rectangle.Fill = new SolidColorBrush(ColourManager.hexToColor("#ff4ffff6"));
            rectangle.Opacity = 0.5;
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

        private static GazeController thisController;
        public static GazeController GetGazeController()
        {
            return thisController;
        }


        private List<InkStroke> strokesToReplay = null;
        private InkStrokeBuilder strokeBuilder = null;
        private DispatcherTimer inkReplayTimer;
        private DateTime beginTimeOfReplay;
        private DateTimeOffset beginTimeOfRecordedSession;
        private DateTimeOffset endTimeOfRecordedSession;
        private TimeSpan durationOfRecordedSession;

        private List<InkStroke> GetTimedStrokes(List<BezierCurve> curves)
        {
            List<InkStroke> retStrokes = new List<InkStroke>();
            DateTime dateTime = DateTime.Now;
            foreach (var curve in curves)
            {
                InkStroke newStroke = curve.InkStroke.Clone();
                newStroke.StrokeStartedTime = dateTime;
                newStroke.StrokeDuration = TimeSpan.FromSeconds(Configuration.StokeDuration);
                retStrokes.Add(newStroke);

                List<InkStroke> relStrokes = ((Fleur)page).TransformStroke(newStroke, curve.NumOfReflection);
                relStrokes.ForEach(x => x.StrokeStartedTime = dateTime);
                relStrokes.ForEach(x => x.StrokeDuration = TimeSpan.FromSeconds(Configuration.StokeDuration));

                retStrokes.AddRange(relStrokes);
                dateTime = dateTime.AddSeconds(Configuration.StokeDuration);
            }
            return retStrokes;
        }

        public void StartReplay()
        {
            page.GetUI().StartReplay();
            if (strokeBuilder == null)
            {
                strokeBuilder = new InkStrokeBuilder();
                inkReplayTimer = new DispatcherTimer();
                inkReplayTimer.Interval = new TimeSpan(TimeSpan.TicksPerSecond / Configuration.ReplayFPS);
                inkReplayTimer.Tick += InkReplayTimer_Tick;
            }

            strokesToReplay = GetTimedStrokes(drawingModel.getCurves());

            //ReplayButton.IsEnabled = false;
            //inkCanvas.InkPresenter.IsInputEnabled = false;
            //ClearCanvasStrokeCache();

            // Calculate the beginning of the earliest stroke and the end of the latest stroke.
            // This establishes the time period during which the strokes were collected.
            beginTimeOfRecordedSession = DateTimeOffset.MaxValue;
            endTimeOfRecordedSession = DateTimeOffset.MinValue;
            foreach (InkStroke stroke in strokesToReplay)
            {
                DateTimeOffset? startTime = stroke.StrokeStartedTime;
                TimeSpan? duration = stroke.StrokeDuration;
                if (startTime.HasValue && duration.HasValue)
                {
                    if (beginTimeOfRecordedSession > startTime.Value)
                    {
                        beginTimeOfRecordedSession = startTime.Value;
                    }
                    if (endTimeOfRecordedSession < startTime.Value + duration.Value)
                    {
                        endTimeOfRecordedSession = startTime.Value + duration.Value;
                    }
                }
            }

            // If we found at least one stroke with a timestamp, then we can replay.
            if (beginTimeOfRecordedSession != DateTimeOffset.MaxValue)
            {
                page.GetInkCanvas().InkPresenter.StrokeContainer.Clear();
                durationOfRecordedSession = endTimeOfRecordedSession - beginTimeOfRecordedSession;

                beginTimeOfReplay = DateTime.Now;
                inkReplayTimer.Start();
                // backupContainer = page.GetInkCanvas().InkPresenter.StrokeContainer;
            }
            else
            {
                // There was nothing to replay. Either there were no strokes at all,
                // or none of the strokes had timestamps.
                StopReplay();
            }
        }

        private void StopReplay()
        {
            page.GetUI().EndReplay();
            inkReplayTimer?.Stop();
            page.GetInkCanvas().InkPresenter.IsInputEnabled = true;
            drawingModel.getCurves().ForEach(x => x.UpdateStroke());
            page.GetInkCanvas().InkPresenter.StrokeContainer.Clear();
            mandalaStrokes.Clear();
            foreach (BezierCurve curve in drawingModel.getCurves())
            {
                InkStroke stroke = curve.InkStroke;
                mandalaStrokes.AddRange(((Fleur)page).TransformStroke(stroke, curve.NumOfReflection));
            }
            page.GetInkCanvas().InkPresenter.StrokeContainer.AddStrokes(drawingModel.GetStrokes());
            page.GetInkCanvas().InkPresenter.StrokeContainer.AddStrokes(mandalaStrokes);
        }
        private void InkReplayTimer_Tick(object sender, object e)
        {
            var currentTimeOfReplay = DateTimeOffset.Now;
            TimeSpan timeElapsedInReplay = currentTimeOfReplay - beginTimeOfReplay;

            DateTimeOffset timeEquivalentInRecordedSession = beginTimeOfRecordedSession + timeElapsedInReplay;
            page.GetInkCanvas().InkPresenter.StrokeContainer.Clear();
            page.GetInkCanvas().InkPresenter.StrokeContainer.AddStrokes(GetCurrentStrokesView(timeEquivalentInRecordedSession));

            if (timeElapsedInReplay > durationOfRecordedSession)
            {
                StopReplay();
            }
        }
        private List<InkStroke> GetCurrentStrokesView(DateTimeOffset time)
        {
            List<InkStroke> retStrokes = new List<InkStroke>();

            // The purpose of this sample is to demonstrate the timestamp usage,
            // not the algorithm. (The time complexity of the code is O(N^2).)
            foreach (InkStroke stroke in strokesToReplay)
            {
                InkStroke s = GetPartialStroke(stroke, time);
                if (s != null)
                {
                    retStrokes.Add(s);
                }
            }

            return retStrokes;
        }

        private InkStroke GetPartialStroke(InkStroke stroke, DateTimeOffset time)
        {
            DateTimeOffset? startTime = stroke.StrokeStartedTime;
            TimeSpan? duration = stroke.StrokeDuration;
            if (!startTime.HasValue || !duration.HasValue)
            {
                // If a stroke does not have valid timestamp, then treat it as
                // having been drawn before the playback started.
                // We must return a clone of the stroke, because a single stroke cannot
                // exist in more than one container.
                return stroke.Clone();
            }

            if (time < startTime.Value)
            {
                // Stroke has not started
                return null;
            }

            if (time >= startTime.Value + duration.Value)
            {
                // Stroke has already ended.
                // We must return a clone of the stroke, because a single stroke cannot exist in more than one container.
                return stroke.Clone();
            }

            // Stroke has started but not yet ended.
            // Create a partial stroke on the assumption that the ink points are evenly distributed in time.
            IReadOnlyList<InkPoint> points = stroke.GetInkPoints();
            var portion = (time - startTime.Value).TotalMilliseconds / duration.Value.TotalMilliseconds;
            var count = (int)((points.Count - 1) * portion) + 1;
            InkStroke ret = strokeBuilder.CreateStrokeFromInkPoints(points.Take(count), System.Numerics.Matrix3x2.Identity, startTime, time - startTime);
            ret.DrawingAttributes = stroke.DrawingAttributes;
            return ret;
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
        selectControl,
        selectHalf
    }
}
