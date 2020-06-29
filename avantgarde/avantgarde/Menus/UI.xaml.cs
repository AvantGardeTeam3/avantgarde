using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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


namespace avantgarde.Menus
{
    //Main UI Class. Connects Toolbox events to main class (Fleur). Contains logic for switching between Drawing and Viewing Mode
    public sealed partial class UI : UserControl, INotifyPropertyChanged

    {
        public bool isDrawing;

        private String playButtonVisibility { get; set; }
        private String playButtonPosition { get; set; }
        private String toolBoxButtonVisibility { get; set; }
        public String colourHex { get; set; }
        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }
        public bool drawState { get; set; }
        public String drawStateIcon { get; set; }

        public Color colourSelection;
       
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler undoButtonClicked;
        public event EventHandler redoButtonClicked;
        public event EventHandler animationButtonClicked;
        public event EventHandler backgroundButtonClicked;
        public event EventHandler goHomeButtonClicked;
        public event EventHandler drawStateChanged;
        public event EventHandler drawingPropertiesUpdated;
        public event EventHandler colourSelectionUpdated;
        public event EventHandler clearCanvas;
        public event EventHandler saveImageClick;

        public UI()
        {
            isDrawing = false;
            playButtonPosition = "Center";
            toolBoxButtonVisibility = "Collapsed";
            playButtonVisibility = "Collapsed";
            
            colourHex = ColourManager.defaultColour.ToString();
            getWindowAttributes();
            this.InitializeComponent();
            actionPanel.Visibility = Visibility.Collapsed;
            animationButton.Visibility = Visibility.Collapsed;
            drawState = false;
            drawStateIcon = "/Assets/icons/icon_play.png";

            
            libreToolBox.openToolbox();

            libreToolBox.colourSelectionUpdated += new EventHandler(updateColourSelection);
            libreToolBox.goHomeButtonClicked += new EventHandler(toolboxGoHomeButtonClicked);
            libreToolBox.propertiesUpdated += new EventHandler(toolboxPropertiesUpdated);
            libreToolBox.setBackgroundButtonClicked += new EventHandler(backgroundColourUpdated);
            libreToolBox.toolboxClosed += new EventHandler(toolboxClosed);
            libreToolBox.clearCanvasButtonClicked += new EventHandler(clearCanvasButtonClicked);
            libreToolBox.popupOpened += new EventHandler(hidePlayButton);
            libreToolBox.popupClosed += new EventHandler(showPlayButton);
            libreToolBox.saveImageClicked += new EventHandler(saveImage);

        }


        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            WIDTH = (int)Window.Current.Bounds.Width;
            HEIGHT = (int)Window.Current.Bounds.Height;
        }


        public InkDrawingAttributes getDrawingAttributes() {
            return libreToolBox.getDrawingAttributes();
        }

        public String getBrush() {
            return libreToolBox.brushSelection;
        }
        public String getBackgroundHex() {
            return libreToolBox.getColourManager().getBackgroundColour().ToString();
        }

        public Color getColour(int profile, int brightness, int opacity)
        {
            return libreToolBox.getColourManager().getColour(profile, brightness, opacity);
        }

        public Color getColour() {
            return colourSelection;
        }

        public int getMandalaLines() {
            return libreToolBox.mandalaLines;
        }

        public int getColourAttributes(int att) {
            if (att == 0)
            {
                return libreToolBox.getColourManager().colourProfile;
            }
            else if (att == 1)
            {
                return libreToolBox.getColourManager().brightness;
            }
            else {
                return libreToolBox.getColourManager().opacity;
            }
        }
        public LibreToolBox getToolbox() {
            return libreToolBox;
        }

        public ColourManager UIGetColourManager() { return libreToolBox.getColourManager(); }
        private void updateDrawStateUI() {
            if (drawState)
            {
                if (libreToolBox.isOpen()) { libreToolBox.closeToolbox(null, null); }
                playButtonPosition = "Right";
                toolBoxButtonVisibility = "Collapsed";
                animationButton.Visibility = Visibility.Collapsed;
                actionPanel.Visibility = Visibility.Collapsed;
                drawStateIcon = "/Assets/icons/icon_pause.png";
            }
            else 
            {
                toolBoxButtonVisibility = "Visible";
                actionPanel.Visibility = Visibility.Visible;
                animationButton.Visibility = Visibility.Visible;
                drawStateIcon = "/Assets/icons/icon_play.png";
            }
        }

        private void changeDrawState(object sender, RoutedEventArgs e)
        {

            if (isDrawing)
            {
            
                if (String.Compare(playButtonPosition, "Right") == 0)
                {
                    playButtonPosition = "Left";
                }
                else
                {
                    playButtonPosition = "Right";
                }
                NotifyPropertyChanged();
                return;
            }


            drawState = !drawState;
            updateDrawStateUI();
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
            toolBoxButtonVisibility = "Collapsed";
            playButtonPosition = "Center";
            actionPanel.Visibility = Visibility.Collapsed;
            animationButton.Visibility = Visibility.Collapsed;
            NotifyPropertyChanged();
        }

        private void clearCanvasButtonClicked(object sender, EventArgs e)
        {
            clearCanvas?.Invoke(this, EventArgs.Empty);

        }

        private void updateColourSelection(object sender, EventArgs e) {
            colourHex = libreToolBox.getColourManager().getColour().ToString();
            colourSelection = libreToolBox.getColourManager().getColour();
            colourSelectionUpdated?.Invoke(this, EventArgs.Empty);
            NotifyPropertyChanged();
        }


        private void backgroundColourUpdated(object sender, EventArgs e) {

            backgroundButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void saveImage(object sender, EventArgs e)
        {
            saveImageClick?.Invoke(this, EventArgs.Empty);
        }

        private void toolboxClosed(object sender, EventArgs e) {
            toolBoxButtonVisibility = "Visible";
            playButtonPosition = "Right";
            actionPanel.Visibility = Visibility.Visible;
            animationButton.Visibility = Visibility.Visible;
            NotifyPropertyChanged();
        }

        private void hidePlayButton(object sender, EventArgs e)
        {
            playButtonVisibility = "Collapsed";
            NotifyPropertyChanged();
        }

        private void showPlayButton(object sender, EventArgs e)
        {
            playButtonVisibility = "Visible";
            NotifyPropertyChanged();
        }

        private void animationButton_Click(object sender, RoutedEventArgs e)
        {
            animationButtonClicked?.Invoke(this, EventArgs.Empty);
            NotifyPropertyChanged();
        }

        public void StartReplay()
        {
            animationButton.IsEnabled = false;
            undoButton.IsEnabled = false;
            redoButton.IsEnabled = false;
            toolBoxButton.IsEnabled = false;
            playButton.IsEnabled = false;
        }

        public void EndReplay()
        {
            animationButton.IsEnabled = true;
            undoButton.IsEnabled = true;
            redoButton.IsEnabled = true;
            toolBoxButton.IsEnabled = true;
            playButton.IsEnabled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // switch square
            Configuration.fleur.SwitchSquare();
        }
    }
}
