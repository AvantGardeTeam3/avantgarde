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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace avantgarde.Menus
{
    public sealed partial class LibreToolBox : UserControl, INotifyPropertyChanged
    {   


        private int restrictedID = 0;
        private int RESTRICTED_NONE = 0;
        private int RESTRICTED_CLEAR_CANVAS = 1;
        private int RESTRICTED_GO_HOME = 2;

        private int BRIGHTNESS = 1;
        private int PROFILE = 0;

        public String backgroundHex { get; set; }
        private int[,] colourPaletteData { get; set; }

        public string[] colourPaletteDataHex { get; set; }
        
        public String[] colourPalette { get; set; }

        public Color colourSelection;
        public String colourHex { get; set; }

        public String mandalaLinesVisibility { get; set; }
        public LibreToolBox()
        {
            
            colourPalette = new String[5];
            drawingAttributes.Color = ColourManager.defaultColour;
            drawingAttributes.Size = new Size(10, 10);
            mandalaLinesVisibility = "Visible";
            mandalaLines = 8;
            propertyUpdate();
            this.InitializeComponent();
            updateBackgroundButton();
            initColourPalette();
            //styleFlyout();
            colourHex = ColourManager.defaultColour.ToString();
            brushSize = 10;
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Collapsed";
            getWindowAttributes();
            

            colourManager.colourManagerClosed += new EventHandler(colourManagerClosed);
            colourManager.updateColourSelection += new EventHandler(updateColourSelection);
            colourManager.backgroundSelectionChanged += new EventHandler(updateBackground);
            confirmTool.confirmDecisionMade += new EventHandler(confirmDecisionMade);
            colorCombination.ThemeUpdate += new EventHandler<ThemeColorArg>(ThemeUpdate);
            colorCombination.themePickerClose += themeClose;
            colorCombination.themePickerOpen += themeOpened;
        }
        
        private void themeClose(object sender, EventArgs e)
        {
            popupClosed?.Invoke(this, EventArgs.Empty);
            playButtonShow = true;
        }
        private bool playButtonShow = true;
        private void themeOpened(object sender, EventArgs e)
        {
            playButtonShow = false;
        }
        private string[] opacity = new string[5];
        private void ThemeUpdate(object sender,ThemeColorArg arg)
        {
            colourPaletteData = new int[5, 2];
            if (arg.LineChosen < 3)
            {
                colourManager.updatePresetThemes(arg.ColorFromProfile, arg.opacity, arg.order);
            }
            for(int i = 0; i < 5; i++)
            {
                colourPaletteData[i, 0] = arg.ColorFromProfile[i, 0];
                colourPaletteData[i, 1] = arg.ColorFromProfile[i, 1];
            }
            this.opacity = arg.opacity;
            for(int i=0; i < 5; i++)
            {
                colourPalette[i] = "#" + this.opacity[i] + colourManager.getColourHex(colourPaletteData[i, 0], colourPaletteData[i, 1]);
            }
            NotifyPropertyChanged();
        }
        private void updateBackgroundButton() {
            backgroundHex = colourManager.backgroundSelection.ToString();
            NotifyPropertyChanged();
        }
        private void initColourPalette() {
            colourPaletteData = new int[,] { { 2, 7 }, { 1, 8 }, { 1, 1 }, { 1, 6 }, { 11, 4 } };

            for (int x = 0; x < colourPaletteData.GetLength(0); x += 1)
            {
               
                colourPalette[x] = "#FF" + colourManager.getColourHex(colourPaletteData[x, PROFILE], colourPaletteData[x,BRIGHTNESS]);
                Debug.WriteLine(colourPalette[x]);
                Debug.WriteLine("b: "+ colourPaletteData[x, BRIGHTNESS]);
                Debug.WriteLine("p: " + colourPaletteData[x, PROFILE]);
            }
            NotifyPropertyChanged();
            
        }

        private InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private double brushSize { get; set; }

        public int mandalaLines { get; set; }
        private String paintbrushButtonState { get; set; }
        private String pencilButtonState { get; set; }
        private String highlighterButtonState { get; set; }

        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        public void hideMandalaLines() {
            mandalaLinesVisibility = "Collapsed";
            NotifyPropertyChanged();
        }
        private void getWindowAttributes()
        {
            WIDTH = (int)(Window.Current.Bounds.Width);
            HEIGHT = (int)(Window.Current.Bounds.Height);
        }

        public InkDrawingAttributes getDrawingAttributes()
        {
            updateSize();
            return drawingAttributes;
        }

        private void increaseBrushSize(object sender, RoutedEventArgs e)
        {
            brushSize++;
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
            else {
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
            else {

                return;
            }
            NotifyPropertyChanged();
            drawingAttributes.Size = new Size(brushSize, brushSize);
            propertyUpdate();
        }


        private void selectPaintbrush(object sender, RoutedEventArgs e)
        {
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Collapsed";
            NotifyPropertyChanged();
            drawingAttributes = new InkDrawingAttributes();
            propertyUpdate();
        }

        private void selectPencil(object sender, RoutedEventArgs e)
        {
            paintbrushButtonState = "Collapsed";
            pencilButtonState = "Visible";
            highlighterButtonState = "Collapsed";
            NotifyPropertyChanged();
            drawingAttributes = InkDrawingAttributes.CreateForPencil();
            propertyUpdate();

        }

        private void selectHighlighter(object sender, RoutedEventArgs e)
        {
            paintbrushButtonState = "Collapsed";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Visible";
            NotifyPropertyChanged();
            //drawingAttributes.PenTip = PenTipShape.Rectangle;
            propertyUpdate();
        }

        private void colourPalette0Clicked(object sender, RoutedEventArgs e)
        {
            colourManager.updateColour(colourPaletteData[0, PROFILE], colourPaletteData[0, BRIGHTNESS],opacity[0]);
        }

        private void colourPalette1Clicked(object sender, RoutedEventArgs e)
        {
            colourManager.updateColour(colourPaletteData[1, PROFILE], colourPaletteData[1, BRIGHTNESS],opacity[1]);
        }

        private void colourPalette2Clicked(object sender, RoutedEventArgs e)
        {
            colourManager.updateColour(colourPaletteData[2, PROFILE], colourPaletteData[2, BRIGHTNESS],opacity[2]);
        }

        private void colourPalette3Clicked(object sender, RoutedEventArgs e)
        {
            colourManager.updateColour(colourPaletteData[3, PROFILE], colourPaletteData[3, BRIGHTNESS],opacity[3]);
        }

        private void colourPalette4Clicked(object sender, RoutedEventArgs e)
        {
            colourManager.updateColour(colourPaletteData[4, PROFILE], colourPaletteData[4, BRIGHTNESS],opacity[4]);
        }

        private void toggleAutoSwitch(object sender, RoutedEventArgs e)
        {
            if (autoSwitchText.Text.CompareTo("AutoSwitch") == 0) {
                autoSwitchText.Text = "Switch on";
                autoSwitchText.Foreground = new SolidColorBrush(Colors.LightGreen);
                colourManager.autoSwitchTurnOn = true;
                //colourManager.colorRecycleListHex = 
            }
            else {
                autoSwitchText.Text = "AutoSwitch";
                autoSwitchText.Foreground = new SolidColorBrush(Colors.White);
                colourManager.autoSwitchTurnOn = false;
            }
            
        }

        private void initThemePicker(object sender, RoutedEventArgs e)
        {
            colorCombination.colorList = colourManager.getPresetColorTheme();
            colorCombination.openColorCombination();
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void saveButtonClicked(object sender, RoutedEventArgs e)
        {
            //TO DO
        }

        private void loadButtonClicked(object sender, RoutedEventArgs e)
        {
            //To do
        }
        private void exportButtonClicked(object sender, RoutedEventArgs e)
        {
            saveImageClicked?.Invoke(this, EventArgs.Empty);
        }

        private void updateSize()
        {
            drawingAttributes.Size = new Size(brushSize, brushSize);
        }

        public bool isOpen() {
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

        public ColourManager getColourManager() {

            return colourManager;
        }

        private void executeRestricted() {
            if (restrictedID == RESTRICTED_NONE) {
                return;
            }else if (restrictedID == RESTRICTED_CLEAR_CANVAS)
            {
                clearCanvasButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            else if (restrictedID == RESTRICTED_GO_HOME)
            {
                goHomeButtonClicked?.Invoke(this, EventArgs.Empty);
            }
            
        }
       

        public event EventHandler goHomeButtonClicked;
        public event EventHandler setBackgroundButtonClicked;
        public event EventHandler propertiesUpdated;
        public event EventHandler undoButtonClicked;
        public event EventHandler redoButtonClicked;
        public event EventHandler toolboxClosed;
        public event EventHandler colourSelectionUpdated;
        public event EventHandler clearCanvasButtonClicked;
        public event EventHandler popupOpened;
        public event EventHandler popupClosed;
        public event EventHandler saveImageClicked;
      


        private void confirmDecisionMade(object sender, EventArgs e)
        {
            popupClosed?.Invoke(this, EventArgs.Empty);
            if (confirmTool.decision) {
                executeRestricted();
            }
            restrictedID = RESTRICTED_NONE;
        }
        private void updateColourSelection(object sender, EventArgs e)
        {
            colourHex = colourManager.getColour().ToString();
            colourSelection = colourManager.getColour();
            colourSelectionUpdated?.Invoke(this, EventArgs.Empty);
            NotifyPropertyChanged();
        }


        private void propertyUpdate() {
            propertiesUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void goHome(object sender, RoutedEventArgs e)
        {
            restrictedID = 2;

            colourManager.close();

            confirmTool.setMessage("Are you sure you wish to exit Libre?");
            confirmTool.openConfirmTool();
            popupOpened?.Invoke(this, EventArgs.Empty);

        }

        private void updateBackground(object sender, EventArgs e) {
            setBackgroundButtonClicked?.Invoke(this, EventArgs.Empty);
            updateBackgroundButton();
            colourManager.selectingBackground = false;
        }
        private void setBackground(object sender, RoutedEventArgs e)
        {
            if (confirmTool.isOpen())
            {
                confirmTool.closeConfirmTool();
            }
            colourManager.selectingBackground = true;
            colourManager.openMenu();
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void undo(object sender, RoutedEventArgs e)
        {
            undoButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void redo(object sender, RoutedEventArgs e)
        {
            redoButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void clearCanvas(object sender, RoutedEventArgs e) {
            restrictedID = 1;

            colourManager.close();
            confirmTool.setMessage("Are you sure you want to clear the canvas?");
            confirmTool.openConfirmTool();
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void initColourManager(object sender, RoutedEventArgs e)
        {
            if (confirmTool.isOpen()) {
                confirmTool.closeConfirmTool();
            }
            colourManager.openMenu();
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void colourManagerClosed(object sender, EventArgs e)
        {
            if (playButtonShow)
            {
                popupClosed?.Invoke(this, EventArgs.Empty);
            }
            
        }


    }
}
