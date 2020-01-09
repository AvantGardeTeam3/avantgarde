
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{


    public partial class Libre : INotifyPropertyChanged
    {
        private GazeInputSourcePreview gazeInputSourcePreview;

        public ObservableCollection<Point> GazeHistory { get; set; } = new ObservableCollection<Point>();

        public int MaxGazeHistorySize { get; set; }

        private Point gazePoint = new Point(0,0);
        private Point startPoint;
        private Point endPoint;
        public List<Point> pointList = new List<Point>();

        private bool PenDown = false;
        private InkStrokeBuilder inkStrokeBuilder = new InkStrokeBuilder();
        private List<InkStroke> inkStrokes = new List<InkStroke>();
        //private List<InkPoint> inkPoints = new List<InkPoint>();
        private DispatcherTimer gazeTimer = new DispatcherTimer();
        private bool gazeTimerStarted = false;
        private int timer = 0;
        private int dwellTime = 400;

        private GazePointer gazePointer;
        private DispatcherTimer dispatchTimer = new DispatcherTimer();

        public String colourSelectionHex { get; set; }
        public static String colourSelectionTemp { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Libre()
        {
            Menus.ColourManager colourManager = new Menus.ColourManager();
            colourSelectionHex = colourManager.selectionHex;

            this.InitializeComponent();
            this.DataContext = this;
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;
            DataContext = this;

            MaxGazeHistorySize = 100;

            gazeInputSourcePreview = GazeInputSourcePreview.GetForCurrentView();
            gazeInputSourcePreview.GazeMoved += GazeInputSourcePreview_GazeMoved;

            gazeTimer.Tick += GazeTimer_Tick;
            gazeTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);

            gazePointer = GazeInput.GetGazePointer(null);
            dispatchTimer.Tick += DispatchTimer_Tick;
            dispatchTimer.Interval = new TimeSpan(0, 0, 1);
            dispatchTimer.Start();
        }
        private void GazeTimer_Tick(object sender, object e)
        {
            this.timer += 20;
            radialProgressBar.Value = Convert.ToDouble(timer) / Convert.ToDouble(dwellTime) * 120.0 - 20;

            if(this.timer >= this.dwellTime)
            {
                if (!PenDown)
                {
                    StartDrawing();
                    PenDown = true;
                }
                else
                {
                    EndDrawing();
                    PenDown = false;
                }
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
            GazeHistory.Add(point.Value);

            Point p = ToCanvasPoint(gazePoint);
            TranslateTransform translateTarget = new TranslateTransform();
            translateTarget.X = p.X - radialProgressBar.ActualWidth / 2;
            translateTarget.Y = p.Y - radialProgressBar.ActualHeight / 2;
            radialProgressBar.RenderTransform = translateTarget;

            if (GazeHistory.Count > MaxGazeHistorySize)
            {
                GazeHistory.RemoveAt(0);
            }
            if (distance < 300 && !gazeTimerStarted)
            {
                gazeTimerStarted = true;
                gazeTimer.Start();
            }
            else if (distance >= 5 && gazeTimerStarted)
            {
                    gazeTimerStarted = false;
                    gazeTimer.Stop();
                    timer = 0;
            }

            if (PenDown)
            {
                InkStroke stroke = MakeStroke(startPoint, ToCanvasPoint(gazePoint));
                stroke.DrawingAttributes = inkToolBar.InkDrawingAttributes;
                stroke.Selected = true;
                inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                inkCanvas.InkPresenter.StrokeContainer.AddStroke(stroke);
            }
        }

        private void StartDrawing()
        {
            Point? sp = Snapping((ToCanvasPoint(gazePoint)));
            if (sp.HasValue)
            {
                Point p = sp.Value;
                TranslateTransform translateTarget = new TranslateTransform();
                Debug.WriteLine(String.Format("{0}:{1}", canvasJoyStick.Width, canvasJoyStick.Height));
                translateTarget.X = p.X - canvasJoyStick.Width / 2;
                translateTarget.Y = p.Y - canvasJoyStick.Height / 2;
                canvasJoyStick.RenderTransform = translateTarget;
                canvasJoyStick.Visibility = Visibility.Visible;
                startPoint = sp.Value;
            }
            else
            {
                startPoint = ToCanvasPoint(gazePoint);
            }
            if (pointList.Count == 0) pointList.Add(startPoint);
            PenDown = true;
            GazeInput.SetIsCursorVisible(canvas, true);
        }

        private void EndDrawing()
        {
            //if (inkPoints.Count == 0) return;
            //InkStroke inkStroke = 
            //inkStrokeBuilder.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
            //inkStroke.DrawingAttributes = inkToolBar.InkDrawingAttributes;
            //inkCanvas.InkPresenter.StrokeContainer.AddStroke(inkStroke);
            //inkStrokes.Add(inkStroke);
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            Point? sp = Snapping(ToCanvasPoint(gazePoint));
            if (sp.HasValue)
            {
                endPoint = sp.Value;
            }
            else
            {
                endPoint = ToCanvasPoint(gazePoint);
            }
            pointList.Add(endPoint);
            InkStroke stroke = MakeStroke(startPoint, endPoint);
            stroke.DrawingAttributes = inkToolBar.InkDrawingAttributes;
            inkCanvas.InkPresenter.StrokeContainer.AddStroke(stroke);
            PenDown = false;
            GazeInput.SetIsCursorVisible(canvas, false);
        }
        private Point ToCanvasPoint(Point point)
        {
            Double x = point.X;
            Double y = point.Y;
            y -= TopGrid.RowDefinitions[0].ActualHeight;
            return new Point(x, y);
        }
        private InkStroke MakeStroke(Point start, Point end)
        {
            List<InkPoint> inkPoints = new List<InkPoint>();
            Double deltaX = end.X - start.X;
            Double deltaY = end.Y - start.Y;
            Double distance = Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2);
            distance = Math.Sqrt(distance);

            int pointNum = Convert.ToInt32(Math.Ceiling(distance / 10.0));
            for(int i = 0; i < pointNum; i++)
            {
                Point ip = new Point(start.X + i * deltaX / pointNum, start.Y + i * deltaY / pointNum);
                inkPoints.Add(new InkPoint(ip, 0.5f));
            }
            inkPoints.Add(new InkPoint(end, 0.5f));
            return inkStrokeBuilder.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
        }
        private Point? Snapping(Point p)
        {
            foreach(Point ep in pointList)
            {
                double distance = Math.Sqrt(Math.Pow(p.X - ep.X, 2) + Math.Pow(p.Y - ep.Y, 2));
                if (distance < 30) return ep;
            }
            return null;
        }

        private void initColourPickerMenu(object sender, RoutedEventArgs e)
        {
            colourManager.openMenu();
        }

    }


}
