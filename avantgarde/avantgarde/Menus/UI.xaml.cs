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
        public bool IsDrawing;

        private String playButtonVisibility { get; set; }
        private String playButtonPosition { get; set; }
        private String toolBoxButtonVisibility { get; set; }
        public Utils.AGColor AGColor
        {
            get => libreToolBox.AGColor;
            private set { }
        }
        public int MandalaLineNumber
        {
            get => libreToolBox.MandalaLines;
            private set { }
        }
        public String Brush
        {
            get => libreToolBox.BrushSelection;
            private set { }
        }
        public Utils.AGColor BackgroundColor
        {
            get => libreToolBox.BackgroundColor;
            private set { }
        }
        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }
        public bool DrawState { get; set; }
        private String DrawStateIcon { get; set; }
       
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler undoButtonClicked;
        public event EventHandler redoButtonClicked;
        public event EventHandler animationButtonClicked;
        
        public event EventHandler goHomeButtonClicked;
        public event EventHandler drawStateChanged;
        
        public event EventHandler clearCanvas;
        public event EventHandler saveImageClick;

        // Color
        public event EventHandler<Events.ColorEventArgs> ColorSelectionUpdated;
        public event EventHandler<Events.ColorEventArgs> BackgroundColorUpdated;
        // Drawing Properties
        public event EventHandler DrawingPropertiesUpdated;

        public UI()
        {
            IsDrawing = false;
            playButtonPosition = "Center";
            toolBoxButtonVisibility = "Collapsed";
            playButtonVisibility = "Collapsed";
            
            //colourHex = ColourManager.defaultColour.ToString();
            getWindowAttributes();
            this.InitializeComponent();
            actionPanel.Visibility = Visibility.Collapsed;
            animationButton.Visibility = Visibility.Collapsed;
            DrawState = false;
            DrawStateIcon = "/Assets/icons/icon_play.png";

            
            libreToolBox.openToolbox();

            libreToolBox.BackgroundColorSelectionUpdated += OnBackgroundColorSelectionUpdated;
            libreToolBox.ColorSelectionUpdated += OnColorSelectionUpdate;

            libreToolBox.goHomeButtonClicked += new EventHandler(toolboxGoHomeButtonClicked);
            libreToolBox.propertiesUpdated += new EventHandler(toolboxPropertiesUpdated);
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
        
        private void OnColorSelectionUpdate(object sender, Events.ColorEventArgs args)
        {
            ColorSelectionUpdated(this, args);
        }

        private void OnBackgroundColorSelectionUpdated(object sender, Events.ColorEventArgs args)
        {
            BackgroundColorUpdated(this, args);
        }

        private void updateDrawStateUI() {
            if (DrawState)
            {
                if (libreToolBox.IsOpen()) { libreToolBox.closeToolbox(null, null); }
                playButtonPosition = "Right";
                toolBoxButtonVisibility = "Collapsed";
                animationButton.Visibility = Visibility.Collapsed;
                actionPanel.Visibility = Visibility.Collapsed;
                DrawStateIcon = "/Assets/icons/icon_pause.png";
            }
            else 
            {
                toolBoxButtonVisibility = "Visible";
                actionPanel.Visibility = Visibility.Visible;
                animationButton.Visibility = Visibility.Visible;
                DrawStateIcon = "/Assets/icons/icon_play.png";
            }
        }

        private void changeDrawState(object sender, RoutedEventArgs e)
        {

            if (IsDrawing)
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


            DrawState = !DrawState;
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
            DrawingPropertiesUpdated?.Invoke(this, EventArgs.Empty);
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

        private void saveImage(object sender, EventArgs e)
        {
            saveImageClick?.Invoke(this, EventArgs.Empty);
        }

        private void toolboxClosed(object sender, EventArgs e) {
            toolBoxButtonVisibility = "Visible";
            playButtonPosition = "Right";
            actionPanel.Visibility = Visibility.Visible;
            animationButton.Visibility = Visibility.Visible;
            playButtonVisibility = "Visible";
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
