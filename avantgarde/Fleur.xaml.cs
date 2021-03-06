﻿
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
using Syncfusion.UI.Xaml.ImageEditor;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{
    public sealed partial class Fleur : INotifyPropertyChanged, IDrawMode
    {
        public static InkCanvas CurrentCanvas = null;
        public List<StrokeData> getAllStrokeData() {
            List<Drawing.BezierCurve> curves = drawingModel.getCurves();
            List<StrokeData> data = new List<StrokeData>();
            foreach(var curve in curves)
            {
                data.Add(curve.strokeData);
            }
            return data;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Autoswitch = true;
        private bool IsSquare = false;

        SfImageEditor editor;

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
            ui.animationButtonClicked += new EventHandler(animationButtonClicked);
            ui.backgroundButtonClicked += new EventHandler(backgroundColourUpdated);
            ui.colourSelectionUpdated += new EventHandler(updateColourSelection);
            ui.clearCanvas += new EventHandler(clearCanvas);

            drawingModel = new DrawingModel();
            // controller = new Controller.GazeController(this);       
            Controller.ControllerFactory.MakeAController(this);
            controller = Controller.ControllerFactory.gazeController;
            controller.x1 = 0;
            controller.y1 = 0;
            controller.x2 = WIDTH;
            controller.y2 = HEIGHT;

            blockGrid.ColumnDefinitions[1].Width = new GridLength(HEIGHT);
            blockGrid.Visibility = Visibility.Collapsed;

            // drawingModel.curveDrawn += new EventHandler(curveDrawn);

            editor = new SfImageEditor();

            Configuration.fleur = this;
            Configuration.ui = this.GetUI();
            controller.HideGrid();
            Fleur.CurrentCanvas = inkCanvas;
        }



        public StrokeData getStrokeData()
        {
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
            else
            {
                newStrokeData.brush = "paint";
            }
            return newStrokeData;
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
        public List<InkStroke> Transfrom(List<InkStroke> u)
        {
            List<InkStroke> transformedStrokes = new List<InkStroke>();
            foreach (InkStroke stroke in u)
            {
                transformedStrokes.AddRange(TransformStroke(stroke, numberOfLines));
            }
            return transformedStrokes;
        }
        public List<InkStroke> TransformStroke(InkStroke stroke, int num)
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

        public GazeInputSourcePreview GetGazeInputSourcePreview() { return GazeInputSourcePreview.GetForCurrentView(); }
        public DrawingModel GetDrawingModel() { return this.drawingModel; }
        public UI GetUI() { return this.ui; }
        public RadialProgressBar GetRadialProgressBar() { return this.radialProgressBar; }
        public InkCanvas GetInkCanvas() { return inkCanvas; }
        public Canvas GetCanvas() { return this.canvas; }

        public ColourManager GetColourManager() { return this.ui.UIGetColourManager(); }

        private async void redo(object sender, EventArgs e)
        {
            //if (redoStack.Count() == 0)
            //{
            //    return;
            //}
            //userStrokes.Add(redoStack.Pop());
            //InkCanvas_refresh();
            controller.Redo();
        }

        private async void undo(object sender, EventArgs e)
        {
            //int containerSize = 0;
            //var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            //foreach (InkStroke stroke in strokes)
            //{
            //    containerSize++;
            //}
            //if (containerSize == 0)
            //{
            //    return;
            //}
            //int index = 0;
            //foreach (InkStroke s in strokes)
            //{
            //    index++;
            //    if (index >= containerSize - (numberOfLines - 1))
            //    {
            //        s.Selected = true;
            //    }
            //}
            //int size = userStrokes.Count();
            //redoStack.Push(userStrokes.ElementAt(size - 1));
            //userStrokes.RemoveAt(userStrokes.Count() - 1);
            //inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
            controller.Undo();
        }

        private void goHomeButtonClicked(object sender, EventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        private void drawStateButtonClicked(object sender, EventArgs e)
        {
            controller.Paused = !controller.Paused;
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
        private void animationButtonClicked(object sender, EventArgs e)
        {
            controller.StartReplay();
        }
        private void clearCanvas(object sender, EventArgs e)
        {
            controller.ClearCanvas();
        }

        public bool isSquare()
        {
            return IsSquare;
        }

        public void SwitchSquare()
        {
            if (!IsSquare)
            {
                //controller.x1 = (inkCanvas.ActualWidth - inkCanvas.ActualHeight) / 2;
                //controller.x2 = controller.x1 + inkCanvas.ActualHeight;
                blockGrid.Visibility = Visibility.Visible;
                IsSquare = !IsSquare;
            }
            else
            {
                //controller.x1 = 0;
                //controller.x2 = WIDTH;
                blockGrid.Visibility = Visibility.Collapsed;
                IsSquare = !IsSquare;
            }
            
        }

        public async void ExportScreenShot(String name, bool isSquare)
        {
            Color background = Controller.ControllerFactory.gazeController.colourManager.backgroundSelection;
            
            StorageFile file;
            String fileName = "avant-garde-" + DateTime.Now.ToString().Replace("/", "-").Replace(" ", "-").Replace(":", "-") + ".png";

            if (isSquare)
            {
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync("temp.png",
                    CreationCollisionOption.ReplaceExisting);
                Debug.WriteLine(file.Path);
            }
            else {

                StorageFolder folder = KnownFolders.PicturesLibrary;
                file = await folder.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.ReplaceExisting);

            }

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96);
           
            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(background);
                ds.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());

            }
            if (file != null)
            {
                using (IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Png, 1f);

                    if (isSquare)
                    {
                        if (name != null)
                        {
                            fileName = name;
                        }

                        var bpm = await GetCroppedBitmapAsync(fileStream, new Point(WIDTH / 2 - HEIGHT / 2, 0), new Size(HEIGHT, HEIGHT), 1);
                        await SaveWriteableBitmapAsPNG(bpm, fileName);

                    }
                }
            }



        }

        private static async Task SaveWriteableBitmapAsPNG(WriteableBitmap bmp, string fileName)
        {
            StorageFile outputFile;
            if (!fileName.StartsWith("a"))
            {
                outputFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileName + ".png", CreationCollisionOption.ReplaceExisting);
            }
            else
            {
                outputFile = await KnownFolders.PicturesLibrary.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            }
            using (var writeStream = await outputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await EncodeWriteableBitmap(bmp, writeStream, BitmapEncoder.PngEncoderId);
            }
            //return outputFile;
        }

        private static async Task EncodeWriteableBitmap(WriteableBitmap bmp, IRandomAccessStream writeStream, Guid encoderId)
        {
            // Copy buffer to pixels
            byte[] pixels;
            using (var stream = bmp.PixelBuffer.AsStream())
            {
                pixels = new byte[(uint)stream.Length];
                await stream.ReadAsync(pixels, 0, pixels.Length);
            }

            // Encode pixels into stream
            var encoder = await BitmapEncoder.CreateAsync(encoderId, writeStream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied,
               (uint)bmp.PixelWidth, (uint)bmp.PixelHeight,
               96, 96, pixels);
            await encoder.FlushAsync();
        }


        public static async Task<WriteableBitmap> GetCroppedBitmapAsync(IRandomAccessStream originalImage,
           Point startPoint, Size cropSize, double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1;
            }


            var startPointX = (uint)Math.Floor(startPoint.X * scale);
            var startPointY = (uint)Math.Floor(startPoint.Y * scale);
            var height = (uint)Math.Floor(cropSize.Height * scale);
            var width = (uint)Math.Floor(cropSize.Width * scale);


            var decoder = await BitmapDecoder.CreateAsync(originalImage);

            var scaledWidth = (uint)Math.Floor(decoder.PixelWidth * scale);
            var scaledHeight = (uint)Math.Floor(decoder.PixelHeight * scale);


            if (startPointX + width > scaledWidth)
            {
                startPointX = scaledWidth - width;
            }

            if (startPointY + height > scaledHeight)
            {
                startPointY = scaledHeight - height;
            }

            var pixels = await GetPixelData(decoder, startPointX, startPointY, width, height,
                scaledWidth, scaledHeight);

            var cropBmp = new WriteableBitmap((int)width, (int)height);
            var pixStream = cropBmp.PixelBuffer.AsStream();
            pixStream.Write(pixels, 0, (int)(width * height * 4));

            return cropBmp;
        }

        private static async Task<byte[]> GetPixelData(BitmapDecoder decoder, uint startPointX, uint startPointY,
           uint width, uint height, uint scaledWidth, uint scaledHeight)
        {
            var transform = new BitmapTransform();
            var bounds = new BitmapBounds();
            bounds.X = startPointX;
            bounds.Y = startPointY;
            bounds.Height = height;
            bounds.Width = width;
            transform.Bounds = bounds;

            transform.ScaledWidth = scaledWidth;
            transform.ScaledHeight = scaledHeight;

            var pix = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.ColorManageToSRgb);
            var pixels = pix.DetachPixelData();
            return pixels;
        }

    }
}
