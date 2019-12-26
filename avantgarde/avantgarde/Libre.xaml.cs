
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

        ColourPickerData colourPickerData = new ColourPickerData();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Color selection { get; set; }
        public String selection_hex { get; set; }

        public Libre()
        {
            selection = Colors.Blue;
            selection_hex = selection.ToString();
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

        public Color hexToColor(string hex)
        {
            //replace # occurences
            if (hex.IndexOf('#') != -1)
                hex = hex.Replace("#", "");


            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));

            return Color.FromArgb(a, r, g, b);
        }

        private void setColour0(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(0));
        }

        private void setColour1(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(1));
        }

        private void setColour2(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(2));
        }

        private void setColour3(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(3));
        }

        private void setColour4(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(4));
        }

        private void setColour5(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(5));
        }

        private void setColour6(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(6));
        }

        private void setColour7(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(7));
        }



        private void selectColour(object sender, RoutedEventArgs e) {
            selection = ColourPicker.Color;
            selection_hex = selection.ToString();
            colourPickerData.addColourToPrevColours(selection.ToString());
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
            NotifyPropertyChanged();
        }

        private void cancelColourPick(object sender, RoutedEventArgs e)
        {
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
        }

        
        private void initColourPickerMenu(object sender, RoutedEventArgs e)
        {
            DataContext = colourPickerData.getColourPickerData();
            if (!ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = true; }
            ColourPicker.Color = selection;
        }

    }

    public class ColourPickerData : INotifyPropertyChanged
    {
        public ICommand setColour { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int horizontalOffset { get; set; }
        public int verticalOffset { get; set; }
        public String[] prevColors { get; set; }

        public static String[] defaultPrevColours = { "#ffcdff59", "#ff4ffff6", "#ffff728e", "#ff1d283f", "#ff2b061e", "#ffffeed6", "#fffbbfca", "#ffbfd0f0" };

        public event PropertyChangedEventHandler PropertyChanged;

        public String getDefaultPrevColours(int i) {
            return defaultPrevColours[i];
        }

        public ColourPickerData getColourPickerData()
        {
            int w = 800;
            int h = 600;

            var cpd = new ColourPickerData()
            {
                width = w,
                height = h,
                horizontalOffset = (int)(Window.Current.Bounds.Width - w) / 2,
                verticalOffset = (int)(Window.Current.Bounds.Height - h) / 2,
                prevColors = defaultPrevColours,
                setColour = new SetColourCommand()

            };

            return cpd;
        }

        public void addColourToPrevColours(String c)
        {
            if (c.Equals(defaultPrevColours[0]))
            {
                return;
            }

            for (int i = (defaultPrevColours.Length - 1); i > 0; i--)
            {
                defaultPrevColours[i] = defaultPrevColours[i - 1];
            }
            defaultPrevColours[0] = c;
        }


    }

    class SetColourCommand : Libre, ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            //   ColourPicker.Color = Colors.Beige;
            //   setColour20();
            String c = ColourPickerData.defaultPrevColours[Int32.Parse(parameter.ToString())];
            selection = hexToColor(c);
            selection_hex = selection.ToString();


            //if (ColourPickerMenu.IsOpen)
            //{
            //    Debug.WriteLine("hello");
            //    ColourPickerMenu.IsOpen = false;
            //}

            Debug.WriteLine(selection_hex);
        }
    }



}
