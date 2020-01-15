
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


    public partial class Libre : INotifyPropertyChanged
    {

        private GazeInputSourcePreview gazeInputSourcePreview;

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
        private int dwellTime = 400;

        private DrawingModel drawingModel;

        private DispatcherTimer dispatchTimer = new DispatcherTimer();


        private String backgroundHex { get; set; }

        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public InkToolbar inkToolBar = new InkToolbar();

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            WIDTH = (int)Window.Current.Bounds.Width;
        }
        public Libre()
        {
            getWindowAttributes();

            this.InitializeComponent();

            this.DataContext = this;

            backgroundHex = Colors.Black.ToString();

            toolbar.goHomeButtonClicked += new EventHandler(toolbar_goHomeButtonClicked);
            toolbar.setBackgroundButtonClicked += new EventHandler(toolbar_setBackgroundButtonClicked);
            toolbar.undoButtonClicked += new EventHandler(toolbar_undoButtonClicker);
            toolbar.redoButtonClicked += new EventHandler(toolbar_redoButtonClicked);

            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;
            DataContext = this;

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
            if(p.Y < 0)
            {
                gazeTimer.Stop();
                this.gazeTimerStarted = false;
                this.timer = 0;
            }
            TranslateTransform translateTarget = new TranslateTransform();
            translateTarget.X = p.X - radialProgressBar.ActualWidth / 2;
            translateTarget.Y = p.Y - radialProgressBar.ActualHeight / 2;
            radialProgressBar.RenderTransform = translateTarget;

            if (distance < 5 && !gazeTimerStarted && !JoystickInvoked)
            {
                radialProgressBar.Visibility = Visibility.Visible;
                gazeTimerStarted = true;
                gazeTimer.Start();
            }
            else if (distance > 5 && gazeTimerStarted)
            {
                radialProgressBar.Visibility = Visibility.Collapsed;
                gazeTimerStarted = false;
                gazeTimer.Stop();
                timer = 0;
            }
            if (PenDown && !JoystickInvoked)
            {
                if(indicatingStroke != null)
                {
                    indicatingStroke.Selected = true;
                    inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                    indicatingStroke = MakeStroke(startPoint, ToCanvasPoint(gazePoint));
                    inkCanvas.InkPresenter.StrokeContainer.AddStroke(indicatingStroke);
                } else
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
                drawingModel.newLine(startPoint, sp.Value);
            }
            else
            {
                drawingModel.newLine(startPoint, ToCanvasPoint(gazePoint));
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
        private List<Ellipse> indicators;
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
            System.Console.WriteLine(points.Count.ToString());
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
        private void toolbar_goHomeButtonClicked(object sender, EventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void toolbar_setBackgroundButtonClicked(object sender, EventArgs e)
        {
            backgroundHex = toolbar.getColourHex();
            NotifyPropertyChanged();
        }

        private void toolbar_redoButtonClicked(object sender, EventArgs e)
        {
            drawingModel.redo();
            this.ClearPointIndicators();
            this.AddPointIndicators(drawingModel.getPoints());
        }

        private void toolbar_undoButtonClicker(object sender, EventArgs e)
        {
            drawingModel.undo();
            this.ClearPointIndicators();
            this.AddPointIndicators(drawingModel.getPoints());
        }
    }
}
