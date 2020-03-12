
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
using Microsoft.Toolkit.Uwp.UI.Controls;
using avantgarde.Menus;
using Windows.Storage;
using Microsoft.Graphics.Canvas;
using Windows.Storage.Pickers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{
    public sealed partial class Fleur : INotifyPropertyChanged, IDrawMode
    {
        public static List<StrokeData> getStrokeData() {
            return strokeData;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static bool autoswitch = true;

        private Controller.GazeController controller;

        public DrawingModel drawingModel;

        private InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();

        public static Color colourSelection { get; set; }

        public static String[] colourPalette;
        private String backgroundHex { get; set; }
        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        private int numberOfLines { get; set; }

        //the coordinate of the centre of the canvas
        private Point inkCanvasCentre;
        public static List<StrokeData> strokeData = new List<StrokeData>();
        public List<InkStroke> userStrokes = new List<InkStroke>();
        public List<InkStroke> mandalaStrokes = new List<InkStroke>();
        private InkStrokeBuilder inkStrokeBuilder;


        Stack<InkStroke> redoStack = new Stack<InkStroke>();

       
        private void getWindowAttributes()
        {
            WIDTH = (int)Window.Current.Bounds.Width;
            HEIGHT = (int)Window.Current.Bounds.Height;
        }
        public Fleur()
        {

            numberOfLines = 10;
            getWindowAttributes();
            this.InitializeComponent();
            drawingAttributes = ui.getDrawingAttributes();
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;
            //register the event with UWP
            //inkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkCanvasCentre.X = canvas.ActualWidth / 2;
            inkCanvasCentre.Y = canvas.ActualHeight / 2;
            inkStrokeBuilder = new InkStrokeBuilder();

            colourSelection = Menus.ColourManager.defaultColour;
            radialProgressBar.Foreground = new SolidColorBrush(colourSelection);

            this.DataContext = this;

            backgroundHex = ui.getBackgroundHex();

            ui.goHomeButtonClicked += new EventHandler(goHomeButtonClicked);
            ui.drawStateChanged += new EventHandler(drawStateButtonClicked);
            ui.drawingPropertiesUpdated += new EventHandler(drawingPropertiesUpdated);
            ui.undoButtonClicked += new EventHandler(undo);
            ui.redoButtonClicked += new EventHandler(redo);
            ui.backgroundButtonClicked += new EventHandler(backgroundColourUpdated);
            ui.colourSelectionUpdated += new EventHandler(updateColourSelection);
            ui.clearCanvas += new EventHandler(clearCanvas);
            ui.saveIamgeClicked += saveIamge;

            drawingModel = new DrawingModel(inkCanvas.InkPresenter.StrokeContainer, true);
            // controller = new Controller.GazeController(this);       
            Controller.ControllerFactory.MakeAController(this);
            controller = Controller.ControllerFactory.gazeController;

            // drawingModel.curveDrawn += new EventHandler(curveDrawn);


            controller.HideGrid();
        }

        private async void saveIamge(object sender, EventArgs e)
        {
            //InkCanvas inkCanvas = ControllerFactory.gazeController.inkCanvas;
            Color background = Controller.ControllerFactory.gazeController.colourManager.backgroundSelection;

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96);
            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            savePicker.FileTypeChoices.Add(".jpg", new[] { ".jpg" });
            savePicker.SuggestedFileName = "Avant Garde Project";
            StorageFile file = await savePicker.PickSaveFileAsync();
            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(background);
                ds.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            }
            if (file != null)
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
                }
            }
        }

        private void storeStrokeData() {
            int COLOUR_PROFILE = 0;
            int BRIGHTNESS = 1;
            int OPACITY = 2;
            
            StrokeData newStrokeData = new StrokeData();
            newStrokeData.reflections = numberOfLines;
            newStrokeData.colourProfile = ui.getColourAttributes(COLOUR_PROFILE);
            newStrokeData.brightness = ui.getColourAttributes(BRIGHTNESS);
            newStrokeData.opacity = ui.getColourAttributes(OPACITY);
            newStrokeData.size = drawingAttributes.Size;

            if (String.Compare(ui.getBrush(), "pencil") == 0)
            {
                newStrokeData.brush = "pencil";
            }
            else {
                newStrokeData.brush = "paint";
            }

            strokeData.Add(newStrokeData);
        }

        private void curveDrawn(object sender, EventArgs e) {
            DrawingModel.LineDrawnEventArgs arg = (DrawingModel.LineDrawnEventArgs)e;
            InkStroke s = arg.stroke;
            s.DrawingAttributes = ui.getDrawingAttributes();
            
            storeStrokeData();
            userStrokes.Add(s);
            inkCanvas.InkPresenter.StrokeContainer.AddStroke(s);
            mandalaStrokes.AddRange(this.Transfrom(userStrokes));

            if (autoswitch)
            {
                ui.UIGetColourManager().nextColour();
            }


        }
        
        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs e)
        {
            //called everytime user finishes a stroke
            IReadOnlyList<InkStroke> strokes = e.Strokes;
            foreach (InkStroke stroke in strokes)
            {
                //stroke.DrawingAttributes = toolbar.getDrawingAttributes();
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
                transformedStrokes.AddRange(TransformStroke(stroke, numberOfLines));
            }
            return transformedStrokes;
        }
        private List<InkStroke> TransformStroke(InkStroke stroke, int num)
        {   //turning a stroke into a set of transformed strokes
            IReadOnlyList<InkPoint> inkPoints = stroke.GetInkPoints();
            List<InkStroke> ret = new List<InkStroke>();
            double thetaDiff = Math.PI * 2 / num;
            for (int i = 1; i < num; i++)
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

        private void InkCanvas_refresh()
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            inkCanvas.InkPresenter.StrokeContainer.AddStrokes(this.Transfrom(userStrokes));
        }


        public GazeInputSourcePreview GetGazeInputSourcePreview() { return GazeInputSourcePreview.GetForCurrentView(); }
        public DrawingModel GetDrawingModel() { return this.drawingModel; }
        public UI GetUI() { return this.ui; }
        public RadialProgressBar GetRadialProgressBar() { return this.radialProgressBar; }
        public InkCanvas GetInkCanvas() { return inkCanvas; }
        public Canvas GetCanvas() { return this.canvas; }

        public ColourManager GetColourManager() { return this.ui.UIGetColourManager(); }


        private async void redo(object sender, EventArgs e)
        {
            if (redoStack.Count() == 0)
            {
                return;
            }
            userStrokes.Add(redoStack.Pop());
            InkCanvas_refresh();
        }

        private async void undo(object sender, EventArgs e)
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
            redoStack.Push(userStrokes.ElementAt(size - 1));
            userStrokes.RemoveAt(userStrokes.Count() - 1);
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }

        private void goHomeButtonClicked(object sender, EventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        private void redoButtonClicked(object sender, EventArgs e)
        {
            drawingModel.redo();
        }
        private void undoButtonClicked(object sender, EventArgs e)
        {
            drawingModel.undo();
        }
        private void drawStateButtonClicked(object sender, EventArgs e)
        {
            controller.Paused = !controller.Paused;

            //inkCanvas.InkPresenter.StrokeContainer.Clear();
            //List<InkStroke> newStrokes;

            //if (controller.Paused)
            //{
            //    newStrokes = mandalaStrokes;
            //}
            //else
            //{
            //    newStrokes = userStrokes;
            //}

            //foreach (InkStroke s in newStrokes)
            //{
            //    inkCanvas.InkPresenter.StrokeContainer.AddStroke(s.Clone());
            //}
            List<InkStroke> strokes = drawingModel.GetStrokes();
            if (controller.Paused)
            {
                strokes = Transfrom(strokes);
                strokes.ForEach(x => inkCanvas.InkPresenter.StrokeContainer.AddStroke(x));
            } else
            {
                IReadOnlyList<InkStroke> all = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
                foreach(InkStroke stroke in all)
                {
                    if (!strokes.Contains(stroke))
                    {
                        stroke.Selected = true;
                    }
                }
                inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            }
            
            //inkCanvas.InkPresenter.StrokeContainer.AddStrokes(strokes);
        }
        private void drawingPropertiesUpdated(object sender, EventArgs e)
        {
            drawingAttributes = ui.getDrawingAttributes();
            numberOfLines = ui.getMandalaLines();
        }
        private void backgroundColourUpdated(object sender, EventArgs e)
        {
            backgroundHex = ui.getBackgroundHex();
            NotifyPropertyChanged();
        }
        private void updateColourSelection(object sender, EventArgs e)
        {
            colourSelection = ui.getColour();
            drawingAttributes.Color = colourSelection;
            radialProgressBar.Foreground = new SolidColorBrush(colourSelection);
        }

        private void clearCanvas(object sender, EventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
            // clear all lists
        }
    }
}
