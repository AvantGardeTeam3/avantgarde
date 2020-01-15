using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace avantgarde.Menus
{
    public sealed partial class ToolBar : UserControl, INotifyPropertyChanged

    {
        private InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();

        public event PropertyChangedEventHandler PropertyChanged;

        private String visibility { get; set; }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double brushSize { get; set; }
        private String paintbrushButtonState { get; set; }
        private String pencilButtonState { get; set; }
        private String highlighterButtonState { get; set; }

        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        private void getWindowAttributes()
        {
            WIDTH = (int)Window.Current.Bounds.Width;
            HEIGHT = (int)Window.Current.Bounds.Height;
        }

        public ToolBar()
        {
            brushSize = 10;
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Collapsed";
            getWindowAttributes();
            visibility = "Collapsed";
            this.InitializeComponent();
            expander.IsExpanded = false;
        }

        public InkDrawingAttributes getDrawingAttributes() {
            return drawingAttributes;
        }

        private void increaseBrushSize(object sender, RoutedEventArgs e)
        {
            brushSize++;
            NotifyPropertyChanged();
            drawingAttributes.Size = new Size(brushSize, brushSize);
        }

        private void decreaseBrushSize(object sender, RoutedEventArgs e)
        {
            if (brushSize == 0) return;
            brushSize--;
            NotifyPropertyChanged();
            drawingAttributes.Size = new Size(brushSize, brushSize);
        }

        private void selectPaintbrush(object sender, RoutedEventArgs e)
        {
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Collapsed";
            NotifyPropertyChanged();
            drawingAttributes = new InkDrawingAttributes();
            updateSizeAndColour();
        }

        private void selectPencil(object sender, RoutedEventArgs e)
        {
            paintbrushButtonState = "Collapsed";
            pencilButtonState = "Visible";
            highlighterButtonState = "Collapsed";
            NotifyPropertyChanged();
            drawingAttributes = InkDrawingAttributes.CreateForPencil();
            updateSizeAndColour();
        }

        private void selectHighlighter(object sender, RoutedEventArgs e)
        {
            paintbrushButtonState = "Collapsed";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Visible";
            NotifyPropertyChanged();
          //  drawingAttributes.PenTip = PenTipShape.Rectangle;
            updateSizeAndColour();
        }

        private void updateSizeAndColour() { 
            drawingAttributes.Size = new Size(brushSize, brushSize);
            drawingAttributes.Color = colourManager.getColour();
        }

        private void initColourManager(object sender, RoutedEventArgs e)
        {
            colourManager.openMenu();
        }

        public event EventHandler goHomeButtonClicked;
        public event EventHandler setBackgroundButtonClicked;

        private void goHome(object sender, RoutedEventArgs e)
        {
            expander.IsExpanded = false;
            //goHomeButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void setBackground(object sender, RoutedEventArgs e)
        {
            setBackgroundButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        public String getColourHex() {
            return colourManager.getColour().ToString();
        }

        private void expandToolbar(object sender, RoutedEventArgs e)
        {
            expander.IsExpanded = true;
            //visibility = "Visible";
            NotifyPropertyChanged();
        }
    }
}