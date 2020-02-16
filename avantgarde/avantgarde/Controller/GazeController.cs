using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input.Preview;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;

using avantgarde.Menus;

namespace avantgarde.Controller
{
    class GazeController
    {
        private UI ui;
        private GazeInputSourcePreview gazeInputSourcePreview;
        private DrawingModel drawingModel;
        private DispatcherTimer Timer = new DispatcherTimer();
        private int TimerValue = 0;
        private bool TimerStarted = false;

        private ControllerState state;
        private Point GazePoint = new Point(0, 0);

        private Point lineStartPoint;
        public GazeController(Libre page, GazeInputSourcePreview gazeInputSourcePreview, DrawingModel drawingModel, UI ui)
        {
            this.gazeInputSourcePreview = gazeInputSourcePreview;
            this.drawingModel = drawingModel;
            this.state = ControllerState.idle;
            gazeInputSourcePreview.GazeMoved += GazeMoved;
            Timer.Tick += GazeTimer_Tick;
            this.ui = ui;
        }
        private void GazeMoved(GazeInputSourcePreview sender, GazeMovedPreviewEventArgs args)
        {
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
                }
            }
            else if(distance >= 5 && TimerStarted)
            {
                // reset timer
                TimerStarted = false;
                this.Timer.Stop();
                this.TimerValue = 0;
            }
        }
        private void GazeDwell(Point gazePoint)
        {
            switch (this.state)
            {
                case ControllerState.pause:
                    break;
                case ControllerState.idle:
                    StartLine(GazePoint);
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
            if (this.TimerValue >= Configuration.DrawingDwellTimeMs)
            {
                GazeDwell(GazePoint);
                this.TimerValue = 0;
            }
        }
        private void IdleGazeDwell(Point point)
        {

        }
        private void StartLine(Point point)
        {
            lineStartPoint = point;
            this.state = ControllerState.drawing;
        }

        private void EndLine(Point point)
        {
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
