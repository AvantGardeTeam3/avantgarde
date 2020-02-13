using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class UI : UserControl, INotifyPropertyChanged

    {
        public String colourHex { get; set; }
        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        public bool drawState { get; set; }
        public String drawStateIcon { get; set; }

        public Color colourSelection;
       

        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            WIDTH = (int)Window.Current.Bounds.Width;
            HEIGHT = (int)Window.Current.Bounds.Height;
        }

        public UI()
        {
            colourHex = ColourManager.defaultColour.ToString();
            getWindowAttributes();
            this.InitializeComponent();
            drawState = false;
            drawStateIcon = "/Assets/icons/icon_play.png";

            colourManager.updateColourSelection += new EventHandler(updateColourSelection);

            libreToolBox.goHomeButtonClicked += new EventHandler(toolboxGoHomeButtonClicked);
            libreToolBox.propertiesUpdated += new EventHandler(toolboxPropertiesUpdated);
            libreToolBox.setBackgroundButtonClicked += new EventHandler(backgroundColourUpdated);

        }

        public InkDrawingAttributes getDrawingAttributes() {
            return libreToolBox.getDrawingAttributes();
        }

        public String getBackgroundHex() {
            return colourManager.getColour().ToString();
        }

        public Color getColour() {
            return colourSelection;
        }

        private void updateDrawStateIcon() {
            if (drawState)
            {
                drawStateIcon = "/Assets/icons/icon_pause.png";
            }
            else 
            {
                drawStateIcon = "/Assets/icons/icon_play.png";
            }
        }

        public event EventHandler undoButtonClicked;
        public event EventHandler redoButtonClicked;
        public event EventHandler backgroundButtonClicked;
        public event EventHandler goHomeButtonClicked;
        public event EventHandler drawStateChanged;
        public event EventHandler drawingPropertiesUpdated;
        public event EventHandler colourSelectionUpdated;


        private void changeDrawState(object sender, RoutedEventArgs e)
        {
            drawState = !drawState;
            updateDrawStateIcon();
            drawStateChanged?.Invoke(this, EventArgs.Empty);
            NotifyPropertyChanged();
        }

        private void undo(object sender, RoutedEventArgs e)
        {
            undoButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void redo(object sender, RoutedEventArgs e)
        {
            redoButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void toolboxGoHomeButtonClicked(object sender, EventArgs e)
        {
            goHomeButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void toolboxPropertiesUpdated(object sender, EventArgs e)
        {
            drawingPropertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void initToolbox(object sender, RoutedEventArgs e)
        {
            libreToolBox.openToolbox();
        }


        private void initColourManager(object sender, RoutedEventArgs e)
        {
            colourManager.openMenu();
        }

        private void updateColourSelection(object sender, EventArgs e) {
            colourHex = colourManager.getColour().ToString();
            colourSelection = colourManager.getColour();
            colourSelectionUpdated?.Invoke(this, EventArgs.Empty);
            NotifyPropertyChanged();
        }

        private void backgroundColourUpdated(object sender, EventArgs e) {

            backgroundButtonClicked?.Invoke(this, EventArgs.Empty);
        }

    }
}
