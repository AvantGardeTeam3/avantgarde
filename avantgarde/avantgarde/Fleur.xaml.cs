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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{
    public sealed partial class Fleur : INotifyPropertyChanged, IDrawMode
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Controller.GazeController controller;

        private DrawingModel drawingModel;

        private InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();

        public static Color colourSelection { get; set; }

        private String backgroundHex { get; set; }
        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        private int numberOfLines { get; set; }

        //the coordinate of the centre of the canvas
        private Point inkCanvasCentre;
        List<InkStroke> userStrokes = new List<InkStroke>();
        List<InkStroke> mandalaStrokes = new List<InkStroke>();
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

            drawingModel = new DrawingModel(inkCanvas.InkPresenter.StrokeContainer, true);
            controller = new Controller.GazeController(this);

            drawingModel.curveDrawn += new EventHandler(curveDrawn);


            controller.HideGrid();
        }

        private void curveDrawn(object sender, EventArgs e)
        {
            DrawingModel.LineDrawnEventArgs arg = (DrawingModel.LineDrawnEventArgs)e;
            InkStroke s = arg.stroke;
            s.DrawingAttributes = ui.getDrawingAttributes();
            userStrokes.Add(s);
            inkCanvas.InkPresenter.StrokeContainer.AddStroke(s);
            mandalaStrokes.AddRange(this.Transfrom(userStrokes));
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
            for (int i = 0; i < num; i++)
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

            inkCanvas.InkPresenter.StrokeContainer.Clear();
            List<InkStroke> newStrokes;

            if (controller.Paused)
            {
                newStrokes = mandalaStrokes;
            }
            else
            {
                newStrokes = userStrokes;
            }

            foreach (InkStroke s in newStrokes)
            {
                inkCanvas.InkPresenter.StrokeContainer.AddStroke(s.Clone());
            }

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
        }

        private void clearCanvas(object sender, EventArgs e)
        {
            inkCanvas.InkPresenter.StrokeContainer.Clear();
        }
    }
}
