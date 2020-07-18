using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using avantgarde.Controller;
using static avantgarde.Menus.ColourManager;
using static avantgarde.Configuration;
using Windows.UI.Xaml.Media.Imaging;

namespace avantgarde.Menus
{
    //Toolbox class. Contains all user controls accessed from toolbox: file manager, colour manager, confirm tool, tutorial, brush tool
    //Controls data flow from user controls to UI or main class (Fleur)
    //Eduardo Battistini
    public sealed partial class LibreToolBox : UserControl, INotifyPropertyChanged
    {   

        private int restrictedID = 0;
        private int RESTRICTED_NONE = 0;
        private int RESTRICTED_CLEAR_CANVAS = 1;
        private int RESTRICTED_GO_HOME = 2;
        private int RESTRICTED_SAVE = 3;
        private int RESTRICTED_LOAD = 4;
        private int RESTRICTED_EXPORT = 5;

        private Color highlightColor = Utils.AGColor.MakeColor("#ffcdff59");

        private int selectedPalette = 0;
        private Utils.AGColor[] colors;
        public Utils.AGColor AGColor
        {
            get => colors[selectedPalette];
            private set { colors[selectedPalette] = value; }
        }
        private Utils.AGColor backgroundColor;
        public Utils.AGColor BackgroundColor
        {
            get => backgroundColor;
            private set { backgroundColor = value; }
        }
        private Button[] palettes;

        public ColourManager ColorManager
        {
            get => colorManager;
            private set { }
        }

        public String BrushSelection = "paint";
        public int SelectedSlot = 1;

        private bool editingPalette = false;
        private bool editingBackground = false;
        private bool autoswitch = true;

        private double brushSize { get; set; }
        public int MandalaLines { get; set; }
        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        private Button selectedButton;

        private InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler goHomeButtonClicked;
        
        public event EventHandler propertiesUpdated;
        public event EventHandler toolboxClosed;
        // Color
        public event EventHandler<Events.ColorEventArgs> ColorSelectionUpdated;
        public event EventHandler<Events.ColorEventArgs> BackgroundColorSelectionUpdated;

        public event EventHandler clearCanvasButtonClicked;
        public event EventHandler popupOpened;
        public event EventHandler popupClosed;
        public event EventHandler saveImageClicked;


        public LibreToolBox()
        {
            getWindowAttributes();

            MandalaLines = 8;
            propertyUpdate();
            this.InitializeComponent();

            // set the top-left palette as the default selected palette
            palettes = new Button[] { palette0, palette1, palette2, palette3, palette4 };
            palette0.BorderBrush = new SolidColorBrush(highlightColor);
            palette0.BorderThickness = new Thickness(5);
            
            // set the default colors for the color palettes and background
            colors = new Utils.AGColor[] {
                new Utils.AGColor(5,6,100),
                new Utils.AGColor(0,7,100),
                new Utils.AGColor(6,7,100),
                new Utils.AGColor(2,7,100),
                new Utils.AGColor(9,7,100)
                };
            backgroundColor = new Utils.AGColor(12, 1, 100);
            UpdatePalettesAndBackground();

            // set the default color and size for the brush
            drawingAttributes.Color = Utils.AGColor.MakeColor(5,5,100);
            drawingAttributes.Size = new Size(10, 10);
            brushTool.drawingAttributes = this.drawingAttributes;
            brushSize = 10;
            brushTool.brushSize = this.brushSize;
            brushTool.mandalaLines = this.MandalaLines;

            autoswitchButton.Background = new SolidColorBrush(highlightColor);
            
            // highlight the tutorial button
            selectButton(tutorialButton);
            // open the tutorial
            tutorial.open(1);

            // color manager events
            colorManager.Confirmed += OnColorManagerConfirm;
            colorManager.Canceled += OnColorManagerCancel;

            confirmTool.confirmDecisionMade += new EventHandler(confirmDecisionMade);

            // file manager events
            fileManager.loadRequested += new EventHandler(load);
            fileManager.saveRequested += new EventHandler(save);
            fileManager.fileLoaded += new EventHandler(fileLoaded);
            fileManager.fileManagerClosed += new EventHandler(popupClosedEvent);

            // brush tool events
            brushTool.propertiesUpdated += new EventHandler(drawingAttributesUpdated);
            brushTool.menuClosed += new EventHandler(popupClosedEvent);

            // tutorial events
            tutorial.tutorialClosed += new EventHandler(popupClosedEvent);
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            WIDTH = (int)(Window.Current.Bounds.Width);
            HEIGHT = (int)(Window.Current.Bounds.Height);
        }

        public InkDrawingAttributes getDrawingAttributes()
        {
            brushTool.getDrawingAttributes();
            return drawingAttributes;
        }

        private void UpdatePalettesAndBackground()
        {
            for(int i = 0; i < 5; i++)
            {
                palettes[i].Background = new SolidColorBrush(colors[i].Color);
            }

            backgroundButton.Background = new SolidColorBrush(backgroundColor.Color);
        }

        private void PaletteClick(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (!button.Name.Substring(0, 7).Equals("palette")) return;
            int num = Convert.ToInt32(button.Name.Substring(7));


            //unhighlight the previous selected palette
            palettes[selectedPalette].BorderBrush = new SolidColorBrush(Colors.White);
            palettes[selectedPalette].BorderThickness = new Thickness(2);

            selectedPalette = num;

            //highlight the currently selected palette
            palettes[selectedPalette].BorderBrush = new SolidColorBrush(highlightColor);
            palettes[selectedPalette].BorderThickness = new Thickness(5);

            if (editingPalette)
            {
                // open color manager
                Utils.AGColor color = colors[num];
                colorManager.Open(color);
            }
            else
            {
                
            }
        }

        private void BackgroundClick(object sender, RoutedEventArgs e)
        {
            editingBackground = true;
            colorManager.Open(backgroundColor);
        }

        private void OnColorManagerConfirm(object sender, Events.ColorEventArgs e)
        {
            Utils.AGColor color = e.Color;
            if (editingBackground)
            {
                backgroundColor = color;
                editingBackground = false;
                UpdatePalettesAndBackground();
                BackgroundColorSelectionUpdated(this, new Events.ColorEventArgs(color));
            }
            else
            {
                colors[selectedPalette] = e.Color;
                UpdatePalettesAndBackground();
            }
            colorManager.Close();
        }

        private void OnColorManagerCancel(object sender, EventArgs e)
        {
            if (editingBackground)
            {
                editingBackground = false;
            }
            else
            {

            }
            colorManager.Close();
        }

        private void toggleAS() {
            autoswitch = !autoswitch;

            Configuration.fleur.Autoswitch = autoswitch;

            if (autoswitch)
            {
                autoswitchButton.Background = new SolidColorBrush(new Utils.AGColor("#ffcdff59").Color);
            }
            else
            {
                autoswitchButton.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void toggleAutoSwitch(object sender, RoutedEventArgs e)
        {
            toggleAS();
        }

        private void saveButtonClicked(object sender, RoutedEventArgs e)
        {
            restrictedID = RESTRICTED_SAVE;
            clearPopups();
            fileManager.open(FileManager.SAVING);
            popupOpened?.Invoke(this, EventArgs.Empty);
            selectButton(saveButton);
        }

        private void save(object sender, EventArgs e) {
            clearPopups();
            selectButton(saveButton);
            popupOpened?.Invoke(this, EventArgs.Empty);
            confirmTool.setMessage("Are you sure you wish to save to Slot " + fileManager.selectedSlot.ToString() + "? \n The slot will be overwritten.");
            confirmTool.openConfirmTool();
            SelectedSlot = fileManager.selectedSlot;
        }

        private void loadButtonClicked(object sender, RoutedEventArgs e)
        {
            restrictedID = RESTRICTED_LOAD;
            clearPopups();
            fileManager.open(FileManager.LOADING);
            popupOpened?.Invoke(this, EventArgs.Empty);
            selectButton(loadButton);
        }

        private void load(object sender, EventArgs e)
        {
            clearPopups();
            selectButton(loadButton);
            popupOpened?.Invoke(this, EventArgs.Empty);
            confirmTool.setMessage("Are you sure you wish to load from Slot " + fileManager.selectedSlot.ToString() + "? \n The current canvas will be lost.");
            confirmTool.openConfirmTool();
        }
        private void exportButtonClicked(object sender, RoutedEventArgs e)
        {
            restrictedID = RESTRICTED_EXPORT;
            clearPopups();
            confirmTool.setMessage("Are you sure you want to export the canvas?");
            confirmTool.openConfirmTool();
            // clear strokeData and user/mandala strokes
            popupOpened?.Invoke(this, EventArgs.Empty);
            selectButton(exportButton);
        }

        public bool IsOpen() {
            return libreToolBox.IsOpen;
        }
        
        public void openToolbox() {
            if (!libreToolBox.IsOpen) { libreToolBox.IsOpen = true; }
        }

        public void closeToolbox(object sender, RoutedEventArgs e)
        {
            if (libreToolBox.IsOpen) { libreToolBox.IsOpen = false; }
            toolboxClosed?.Invoke(this, EventArgs.Empty);
        }

        public void openBrushTool(object sender, RoutedEventArgs e)
        {
            clearPopups();
            brushTool.open();
            popupOpened?.Invoke(this, EventArgs.Empty);
            selectButton(drawButton);
        }

        private void selectButton(Button b) {
            b.Background = new SolidColorBrush(highlightColor);
            selectedButton = b;
        }
        
        private void clearButtonSelections()
        {
            selectedButton.Background = new SolidColorBrush(Colors.Transparent);
        }

        private void executeRestricted() {

            //executes actions that require confirmation from confirm tool

            if (restrictedID == RESTRICTED_NONE)
            {
                return;
            }
            else if (restrictedID == RESTRICTED_CLEAR_CANVAS)
            {
                clearCanvasButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (restrictedID == RESTRICTED_GO_HOME)
            {
                goHomeButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (restrictedID == RESTRICTED_SAVE)
            {
                fileManager.save();
                Configuration.fleur.ExportScreenShot(SelectedSlot.ToString());
                // Update image here
                Image[] slots = fileManager.GetImages();
                String source = ApplicationData.Current.LocalFolder.Path + "\\" + SelectedSlot.ToString() + ".png";
                BitmapImage image = new BitmapImage(new Uri(source));
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                slots[SelectedSlot - 1].Source = image;
            }
            else if (restrictedID == RESTRICTED_LOAD)
            {
                fileManager.load();
            }
            else if (restrictedID == RESTRICTED_EXPORT)
            {
                saveImageClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        private void clearPopups() {
            if (colorManager != null)
            {
                colorManager.Close();
            }
            if (fileManager != null)
            {
                fileManager.close();
            }
            if (confirmTool != null)
            {
                confirmTool.closeConfirmTool();
            }
            if (brushTool != null)
            {
                brushTool.close();
            }
            if (tutorial != null)
            {
                tutorial.close();
            }

            popupClosed?.Invoke(this, EventArgs.Empty);
        }
    
        private void confirmDecisionMade(object sender, EventArgs e)
        {
            popupClosed?.Invoke(this, EventArgs.Empty);
            if (confirmTool.decision) {
                executeRestricted();
            }
            restrictedID = RESTRICTED_NONE;
            clearButtonSelections();
        }

        private void propertyUpdate() {
            propertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void goHome(object sender, RoutedEventArgs e)
        {
            restrictedID = RESTRICTED_GO_HOME;
            clearPopups();
            confirmTool.setMessage("Are you sure you wish to exit?");
            confirmTool.openConfirmTool();
            popupOpened?.Invoke(this, EventArgs.Empty);
            selectButton(exitButton);
        }

        private void setBackground(object sender, RoutedEventArgs e)
        {
            clearPopups();
            // TODO
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void clearCanvas(object sender, RoutedEventArgs e) {
            restrictedID = RESTRICTED_CLEAR_CANVAS;

            clearPopups();
            confirmTool.setMessage("Are you sure you want to clear the canvas?");
            confirmTool.openConfirmTool();
            // clear strokeData and user/mandala strokes
            popupOpened?.Invoke(this, EventArgs.Empty);
            selectButton(clearButton);
        }

        private void editPalette(object sender, RoutedEventArgs e)
        {
            editingPalette = !editingPalette;

            if (editingPalette)
            {
                editPaletteButton.Background = new SolidColorBrush(highlightColor);
            }
            else {
                editPaletteButton.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void popupClosedEvent(object sender, EventArgs e)
        {
            clearButtonSelections();
            popupClosed?.Invoke(this, EventArgs.Empty);
        }

        private void initTutorial(object sender, RoutedEventArgs e)
        {
            clearPopups();
            selectButton(tutorialButton);
            tutorial.open(2);
            popupOpened?.Invoke(this, EventArgs.Empty);
        }


        /// <summary>
        /// load color palettes and background color from file, invoked when a save event occurs in file manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fileLoaded(object sender, EventArgs e)
        {   
            NotifyPropertyChanged();
        }

        /// <summary>
        /// update drawing attritbute
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void drawingAttributesUpdated(object sender, EventArgs e) {
            drawingAttributes = brushTool.getDrawingAttributes();
            drawingAttributes.Color = colors[selectedPalette].Color;
            MandalaLines = brushTool.mandalaLines;
            propertyUpdate();
        }

        private void libreToolBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.Width = libreToolBox.ActualWidth;
            grid.Height = libreToolBox.ActualHeight;
        }
    }
}