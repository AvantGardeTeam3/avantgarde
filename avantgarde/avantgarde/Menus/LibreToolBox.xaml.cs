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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace avantgarde.Menus
{
    public sealed partial class LibreToolBox : UserControl, INotifyPropertyChanged
    {
        public LibreToolBox()
        {
            brushSize = 10;
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Collapsed";
            getWindowAttributes();
            this.InitializeComponent();
            this.InitializeComponent();
        }

        private InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();

        public event PropertyChangedEventHandler PropertyChanged;

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

        public InkDrawingAttributes getDrawingAttributes()
        {
            updateSizeAndColour();
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
            drawingAttributes.PenTip = PenTipShape.Rectangle;
            updateSizeAndColour();
        }

        private void updateSizeAndColour()
        {
            drawingAttributes.Size = new Size(brushSize, brushSize);
          //  drawingAttributes.Color = colourManager.getColour();
        }

        private void initColourManager(object sender, RoutedEventArgs e)
        {
         //   colourManager.openMenu();
        }

        public String getColourHex()
        {
            //   return colourManager.getColour().ToString();

            return "";
        }


        public event EventHandler goHomeButtonClicked;
        public event EventHandler setBackgroundButtonClicked;

        private void goHome(object sender, RoutedEventArgs e)
        {
            goHomeButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void setBackground(object sender, RoutedEventArgs e)
        {
            setBackgroundButtonClicked?.Invoke(this, EventArgs.Empty);
        }

    }
}
