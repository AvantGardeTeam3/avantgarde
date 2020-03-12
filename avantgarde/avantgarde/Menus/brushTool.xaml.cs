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
    public sealed partial class brushTool : UserControl, INotifyPropertyChanged

    {
        public String brushSelection;
        public int width { get; set; }
        public int height { get; set; }
        public int horizontalOffset { get; set; }
        public int verticalOffset { get; set; }
        public double brushSize { get; set; }
        public int mandalaLines { get; set; }
        private String paintbrushButtonState { get; set; }
        private String pencilButtonState { get; set; }

        public InkDrawingAttributes drawingAttributes;
        public brushTool()
        {
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            getWindowAttributes();
            this.InitializeComponent();
        }

        public void close()
        {
            if (brushToolMenu.IsOpen) { brushToolMenu.IsOpen = false; }
            NotifyPropertyChanged();

        }
        public void open() {

            if (!brushToolMenu.IsOpen) { brushToolMenu.IsOpen = true; }
            NotifyPropertyChanged();
        }

        public InkDrawingAttributes getDrawingAttributes() {
            updateSize();
            return drawingAttributes;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            width = 500;
            height = 400;
            horizontalOffset = (int)(Window.Current.Bounds.Width - width) / 2;
            verticalOffset = (int)(Window.Current.Bounds.Height - height) / 2;
        }

        private void increaseBrushSize(object sender, RoutedEventArgs e)
        {

            if (brushSize < 21)
            {
                brushSize++;
            }
            else
            {
                return;
            }
            NotifyPropertyChanged();
            drawingAttributes.Size = new Size(brushSize, brushSize);
            propertyUpdate();
        }

        private void decreaseMandalaLines(object sender, RoutedEventArgs e)
        {
            if (mandalaLines > 1)
            {
                mandalaLines--;
            }
            else
            {
                return;
            }
            NotifyPropertyChanged();
            propertyUpdate();

        }

        private void increaseMandalaLines(object sender, RoutedEventArgs e)
        {
            if (mandalaLines < 21)
            {
                mandalaLines++;
            }
            else
            {
                return;
            }
            NotifyPropertyChanged();
            propertyUpdate();
        }

        private void decreaseBrushSize(object sender, RoutedEventArgs e)
        {
            if (brushSize > 1)
            {
                brushSize--;
            }
            else
            {

                return;
            }
            NotifyPropertyChanged();
            drawingAttributes.Size = new Size(brushSize, brushSize);
            propertyUpdate();
        }


        private void selectPaintbrush(object sender, RoutedEventArgs e)
        {
            brushSelection = "paint";
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            NotifyPropertyChanged();
            drawingAttributes = new InkDrawingAttributes();
            propertyUpdate();
        }

        private void selectPencil(object sender, RoutedEventArgs e)
        {
            brushSelection = "pencil";
            paintbrushButtonState = "Collapsed";
            pencilButtonState = "Visible";
            NotifyPropertyChanged();
            drawingAttributes = InkDrawingAttributes.CreateForPencil();
            propertyUpdate();

        }

        private void closeMenu(object sender, RoutedEventArgs e)
        {
            close();
            menuClosed?.Invoke(this, EventArgs.Empty);
        }

        private void updateSize()
        {
            drawingAttributes.Size = new Size(brushSize, brushSize);
        }

        public event EventHandler propertiesUpdated;
        public event EventHandler menuClosed;
        private void propertyUpdate()
        {
            updateSize();
            propertiesUpdated?.Invoke(this, EventArgs.Empty);
        }


    }
}
