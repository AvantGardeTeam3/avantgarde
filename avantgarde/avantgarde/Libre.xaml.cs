
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

        public ObservableCollection<Point> GazeHistory { get; set; } = new ObservableCollection<Point>();

        public int MaxGazeHistorySize { get; set; }

        private Point gazePoint = new Point(0, 0);
        private Point startPoint;
        private Point endPoint;
        private Point joystickPoint;
        public List<Point> pointList = new List<Point>();

        private bool PenDown = false;
        private bool JoystickInvoked = false;
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


        private Stack<InkStroke> redoStack = new Stack<InkStroke>();
        private Point iniP;

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

            if (PenDown)
            {
                InkStroke stroke = MakeStroke(startPoint, ToCanvasPoint(gazePoint));
                stroke.DrawingAttributes = inkToolBar.InkDrawingAttributes;
                stroke.Selected = true;
                inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
                inkCanvas.InkPresenter.StrokeContainer.AddStroke(stroke);
            }
        }

        private void GazeDwell()
        {
            Point point = ToCanvasPoint(gazePoint);
            Point? sp = Snapping(point);
            if (PenDown)
            {
                // drawing
                if (sp.HasValue)
                {
                    point = sp.Value;
                }
                EndDrawing(point);
            }
            else
            {
                // not drawing
                if (sp.HasValue)
                {
                    // snap to existing point
                    point = sp.Value;
                    InvokeJoystick(point);

                }
                else
                {
                    // no snapping
                    this.StartDrawing(point);
                }
            }
        }

        private void StartDrawing(Point start)
        {
            PenDown = true;
            startPoint = start;
            if (pointList.Count == 0) pointList.Add(startPoint);
            GazeInput.SetIsCursorVisible(canvas, true);
        }

        private void EndDrawing(Point end)
        {
            //if (inkPoints.Count == 0) return;
            //InkStroke inkStroke = 
            //inkStrokeBuilder.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
            //inkStroke.DrawingAttributes = inkToolBar.InkDrawingAttributes;
            //inkCanvas.InkPresenter.StrokeContainer.AddStroke(inkStroke);
            //inkStrokes.Add(inkStroke);
            PenDown = false;
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            endPoint = end;
            pointList.Add(endPoint);
            InkStroke stroke = MakeStroke(startPoint, endPoint);
            stroke.DrawingAttributes = inkToolBar.InkDrawingAttributes;
            inkCanvas.InkPresenter.StrokeContainer.AddStroke(stroke);
            GazeInput.SetIsCursorVisible(canvas, false);
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
            if(args.PointerState != PointerState.Dwell)
            {
                return;
            }
            Button button = (Button)sender;
            switch(button.Name)
            {
                case "UpKey":
                    this.StartDrawing(joystickPoint);
                    this.DismissJoystick();
                    break;
                case "DownKey":
                    break;
            }
        }

        private void StartNewLine()
        {

        }

        private void MovePoint()
        {

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
            for (int i = 0; i < pointNum; i++)
            {
                Point ip = new Point(start.X + i * deltaX / pointNum, start.Y + i * deltaY / pointNum);
                inkPoints.Add(new InkPoint(ip, 0.5f));
            }
            inkPoints.Add(new InkPoint(end, 0.5f));
            return inkStrokeBuilder.CreateStrokeFromInkPoints(inkPoints, System.Numerics.Matrix3x2.Identity);
        }
        private Point? Snapping(Point p)
        {
            foreach (Point ep in pointList)
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



        private void selectColour(object sender, RoutedEventArgs e)
        {
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
        private async void Redo_Button_Click(object sender, RoutedEventArgs e)
        {
            if (redoStack.Count() == 0)
            {
                return;
            }
            inkCanvas.InkPresenter.StrokeContainer.AddStroke(redoStack.Pop());
        }
        private async void UndoOne_Button_Click(object sender, RoutedEventArgs e)
        {
            int containerSize = 0;
            var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            foreach (var stroke in strokes)
            {
                containerSize++;
            }
            int index = 0;
            foreach (var s in strokes)
            {
                index++;
                if (index == containerSize)
                {
                    s.Selected = true;
                    redoStack.Push(s.Clone());
                }
            }
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }

        private Point startP = new Point(50, 100);
        private Point endP = new Point(100, 200);

        private void Square_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Triangle_Button_Click(object sender, RoutedEventArgs e)
        {
            Ellipse aaaa = new Ellipse();

            Point p = new Point(0, 1);

        }

        private void btnCircle_Click(object sender, RoutedEventArgs e)
        {
            Ellipse ellipse = new Ellipse()
            {
                Height = 10,
                Width = 10
            };
            if (endP.X >= startP.X)
            {
                ellipse.SetValue(Canvas.LeftProperty, startP.X);
                ellipse.Width = endP.X - startP.X;
            }
            else
            {
                ellipse.SetValue(Canvas.LeftProperty, startP.X);
                ellipse.Width = startP.X - endP.X;
            }
            if (endP.Y >= startP.Y)
            {
                ellipse.SetValue(Canvas.TopProperty, startP.Y);
                ellipse.Height = endP.Y - startP.Y;
            }
            else
            {
                ellipse.SetValue(Canvas.TopProperty, startP.Y);
                ellipse.Height = startP.Y - endP.Y;
            }
            Canvas canvas = new Canvas();
            canvas.Children.Add(ellipse);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (shape_popup.IsOpen == false)
            {
                shape_popup.IsOpen = true;
            }
            else
            {
                shape_popup.IsOpen = false;
            }
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

        public String getDefaultPrevColours(int i)
        {
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
