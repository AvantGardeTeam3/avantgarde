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

namespace avantgarde.Controller
{
    class GazeController
    {
        private Libre page;
        private UI ui;
        private GazeInputSourcePreview gazeInputSourcePreview;
        private DrawingModel drawingModel;
        private DispatcherTimer Timer = new DispatcherTimer();
        private RadialProgressBar progressBar;
        private InkStrokeContainer container;
        private int TimerValue = 0;
        private bool TimerStarted = false;

        public bool Paused { get; set; }
        private ControllerState state;
        private Point GazePoint = new Point(0, 0);

        private Point lineStartPoint;

        private InkStroke StrokeIndication = null;
        private VerticalJoystick ActiveVerticalJoystick = null;
        private Point joystickPosition;
        public GazeController(Libre page)
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
        }
        private void GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
            if (Paused) return;
            var point = args.CurrentPoint.EyeGazePosition;
            if (!point.HasValue)
            {
                return;
            }
            double distance = Util.distance(GazePoint, point.Value);
            GazePoint = point.Value;
            if (distance < 5 && !TimerStarted)
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
            else if(distance >= 5 && TimerStarted)
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
                    if(StrokeIndication != null)
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
            Point? controlPoint = Util.snapping(drawingModel.GetControlPoints(), point, Configuration.GazeSnapDistance);
            Point? midPoint = Util.snapping(drawingModel.GetMidPoints(), point, Configuration.GazeSnapDistance);
            Point? endPoint = Util.snapping(drawingModel.GetEndPoints(), point, Configuration.GazeSnapDistance);
            if (controlPoint.HasValue)
            {
                // control point selected
                ActiveVerticalJoystick = InvokeVerticalJoystick(controlPoint.Value, ControlPointJoystickEventHandler);
                state = ControllerState.selectControl;
            }
            else if (midPoint.HasValue)
            {
                // mid point selected
                ActiveVerticalJoystick = InvokeVerticalJoystick(midPoint.Value, MidPointJoystickEventHandler);
                state = ControllerState.selectMid;
            }
            else if (endPoint.HasValue)
            {
                // end points selected
                ActiveVerticalJoystick = InvokeVerticalJoystick(endPoint.Value, EndPointJoystickEventHandler);
                state = ControllerState.selectP0P3;
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
            if(StrokeIndication != null)
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
                // drawingModel.newLine(startPoint, sp.Value, toolbar.getDrawingAttributes());
            }
            else
            {
                Double midX = (lineStartPoint.X + GazePoint.X) / 2;
                Double midY = (lineStartPoint.Y + GazePoint.Y) / 2;
                drawingModel.newCurve(lineStartPoint, GazePoint, ui.getDrawingAttributes());
                // drawingModel.newLine(startPoint, ToCanvasPoint(gazePoint), toolbar.getDrawingAttributes());
            }
            this.state = ControllerState.idle;
        }

        private void StartMovingP0P3(Point point)
        {

        }

        private void EndMovingP0P3(Point point)
        {

        }

        private void StartMovingMid(Point point)
        {

        }

        private void EndMovingMid(Point point)
        {

        }

        private void StartMovingControl(Point point)
        {

        }

        private void EndMovingControl(Point point)
        {

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
                    // this.StartDrawing(joystickPoint);
                    // this.DismissJoystick();
                    break;
                case "DownKey":
                    this.page.GetCanvas().Children.Remove(ActiveVerticalJoystick);
                    this.state = ControllerState.idle;
                    // this.StartMoving(joystickPoint);
                    // this.DismissJoystick();
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
                    break;
                case "DownKey":
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
                    break;
                case "DownKey":
                    break;
            }
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
