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
        private int RESTRICTED_SAVE = 3;
        private int RESTRICTED_LOAD = 4;
        

        private int BRIGHTNESS = 1;
        private int PROFILE = 0;
        private int OPACITY = 2;

        public String brushSelection;

        public bool editingPalette;
        private bool autoswitch;

        public String backgroundHex { get; set; }
        private int[,] colourPaletteData = 
            new int[,] { { 5, 6, 100 }, { 0, 7, 100 }, { 6, 7, 100 }, { 2, 7, 100 }, { 9, 7, 100 } };
        public String[] colourPalette { get; set; }

        public Color colourSelection;
        public String colourHex { get; set; }

        public String mandalaLinesVisibility { get; set; }
        public LibreToolBox()
        {
            autoswitch = true;
            editingPalette = false;
            brushSelection = "paint";
            colourPalette = new String[5];
            
            mandalaLinesVisibility = "Visible";
            mandalaLines = 8;
            propertyUpdate();
            this.InitializeComponent();
            drawingAttributes.Color = ColourManager.defaultColour;
            drawingAttributes.Size = new Size(10, 10);
            autoswitchButton.Background = new SolidColorBrush(hexToColor("#a0cdff59"));
            updateBackgroundButton();
            initColourPalette(this.colourPaletteData);
            
            colourHex = ColourManager.defaultColour.ToString();
            brushSize = 10;
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Collapsed";
            getWindowAttributes();
            

            colourManager.colourManagerClosed += new EventHandler(colourManagerClosed);
            colourManager.updateColourSelection += new EventHandler(updateColourSelection);
            colourManager.backgroundSelectionChanged += new EventHandler(updateBackground);
            colourManager.paletteEdited += new EventHandler(updatePalette);
            colourManager.toggleAutoswitch += new EventHandler(cmToggleAutoSwitch);
            confirmTool.confirmDecisionMade += new EventHandler(confirmDecisionMade);
            fileManager.loadRequested += new EventHandler(load);
            fileManager.saveRequested += new EventHandler(save);
            fileManager.fileLoaded += new EventHandler(fileLoaded);

        }
        
     
            

       
        
        private void updateBackgroundButton() {
            backgroundHex = colourManager.backgroundSelection.ToString();
            NotifyPropertyChanged();
        }

        private void initColourPalette(int[,] data) {


            for (int x = 0; x < data.GetLength(0); x += 1)
            {
                colourPalette[x] = "#" + colourManager.getOpacityHex(data[x,OPACITY])
                    + colourManager.getColourHex(data[x, PROFILE], data[x,BRIGHTNESS]);
                colourManager.colourPalette[x] = hexToColor(colourPalette[x]);
            }

            colourManager.colourPaletteHex = colourPalette;
            colourManager.colourPaletteData = data;
            colourPaletteData = data;
            fileManager.colourPalette = data;

            

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
            brushSelection = "paint";
            paintbrushButtonState = "Visible";
            pencilButtonState = "Collapsed";
            highlighterButtonState = "Collapsed";
            NotifyPropertyChanged();
            drawingAttributes = new InkDrawingAttributes();
            propertyUpdate();
        }

        private void selectPencil(object sender, RoutedEventArgs e)
        {
            brushSelection = "pencil";
            paintbrushButtonState = "Collapsed";
            pencilButtonState = "Visible";
            highlighterButtonState = "Collapsed";
            NotifyPropertyChanged();
            drawingAttributes = InkDrawingAttributes.CreateForPencil();
            propertyUpdate();

        }


        private void colourPalette0Clicked(object sender, RoutedEventArgs e)
        {
            if (editingPalette) 
            {
                colourManager.editID = 0;
                colourManager.saveSelectionData();
                colourManager.loadColourData();
                popupOpened?.Invoke(this, EventArgs.Empty);
                colourManager.openMenu();
                colourManager.switchID = 0;

            }

            else
            {
                colourManager.updateColour(colourPaletteData[0, PROFILE], colourPaletteData[0, BRIGHTNESS], colourPaletteData[0, OPACITY]);
            }
        }

        private void colourPalette1Clicked(object sender, RoutedEventArgs e)
        {
            if (editingPalette)
            {
                colourManager.editID = 1;
                colourManager.saveSelectionData();
                colourManager.loadColourData();
                popupOpened?.Invoke(this, EventArgs.Empty);
                colourManager.openMenu();
                
            }

            else
            {
                colourManager.switchID = 1;
                colourManager.updateColour(colourPaletteData[1, PROFILE], colourPaletteData[1, BRIGHTNESS], colourPaletteData[1, OPACITY]);
            }
        }

        private void colourPalette2Clicked(object sender, RoutedEventArgs e)
        {
            if (editingPalette)
            {
                colourManager.editID = 2;
                colourManager.saveSelectionData();
                colourManager.loadColourData();
                popupOpened?.Invoke(this, EventArgs.Empty);
                colourManager.openMenu();
                
            }

            else
            {
                colourManager.switchID = 2;
                colourManager.updateColour(colourPaletteData[2, PROFILE], colourPaletteData[2, BRIGHTNESS], colourPaletteData[2, OPACITY]);
            }
        }

        private void colourPalette3Clicked(object sender, RoutedEventArgs e)
        {
            if (editingPalette)
            {
                colourManager.editID = 3;
                colourManager.saveSelectionData();
                colourManager.loadColourData();
                popupOpened?.Invoke(this, EventArgs.Empty);
                colourManager.openMenu();
                
            }

            else
            {
                colourManager.switchID = 3;
                colourManager.updateColour(colourPaletteData[3, PROFILE], colourPaletteData[3, BRIGHTNESS], colourPaletteData[3, OPACITY]);
            }
        }

        private void colourPalette4Clicked(object sender, RoutedEventArgs e)
        {
            if (editingPalette)
            {
                colourManager.editID = 4;
                colourManager.saveSelectionData();
                colourManager.loadColourData();
                popupOpened?.Invoke(this, EventArgs.Empty);
                colourManager.openMenu();
                
            }

            else
            {
                colourManager.switchID = 4;
                colourManager.updateColour(colourPaletteData[4, PROFILE], colourPaletteData[4, BRIGHTNESS], colourPaletteData[4, OPACITY]);
            }
        }

        private void updatePalette(object sender, EventArgs e) {
            colourPalette = colourManager.colourPaletteHex;
            editingPalette = false;
            editPaletteButton.Background = new SolidColorBrush(Colors.Transparent);
            fileManager.colourPalette = colourManager.colourPaletteData;
            NotifyPropertyChanged();
        }

        private void toggleAS() {
            autoswitch = !autoswitch;

            Fleur.autoswitch = autoswitch;

            if (autoswitch)
            {
                autoswitchButton.Background = new SolidColorBrush(hexToColor("#a0cdff59"));
            }
            else
            {
                autoswitchButton.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void cmToggleAutoSwitch(object sender, EventArgs e) {

            toggleAS();
        }
        private void toggleAutoSwitch(object sender, RoutedEventArgs e)
        {

            toggleAS();
        }

        private void saveButtonClicked(object sender, RoutedEventArgs e)
        {
            restrictedID = RESTRICTED_SAVE;
            int[] bgProps = colourManager.getBGColour();
            fileManager.bgProfile = bgProps[PROFILE];
            fileManager.bgBrightness = bgProps[BRIGHTNESS];
            fileManager.bgOpacity = bgProps[OPACITY];
            clearPopups();
            fileManager.open(FileManager.SAVING);
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void save(object sender, EventArgs e) {
            clearPopups();
            confirmTool.setMessage("Are you sure you wish to save to Slot " + fileManager.selectedSlot.ToString() + "? \n The slot will be overwritten.");
            confirmTool.openConfirmTool();
        }

        private void loadButtonClicked(object sender, RoutedEventArgs e)
        {
            restrictedID = RESTRICTED_LOAD;
            clearPopups();
            fileManager.open(FileManager.LOADING);
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void load(object sender, EventArgs e)
        {
            clearPopups();
            confirmTool.setMessage("Are you sure you wish to load from Slot " + fileManager.selectedSlot.ToString() + "? \n The current canvas will be lost.");
            confirmTool.openConfirmTool();
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
            }
            else if (restrictedID == RESTRICTED_LOAD) 
            {
                fileManager.load();
            }
            
        }

        private void clearPopups() {
            if (colourManager != null)
            {
                colourManager.close();
            }
            if (fileManager != null)
            {
                fileManager.close();
            }
            if (confirmTool != null)
            {
                confirmTool.closeConfirmTool();
            }

            popupClosed?.Invoke(this, EventArgs.Empty);

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
            restrictedID = RESTRICTED_GO_HOME;
            clearPopups();
            confirmTool.setMessage("Are you sure you wish to exit?");
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
            clearPopups();
            colourManager.selectingBackground = true;
            colourManager.saveSelectionData();
            colourManager.loadColourData();
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
            restrictedID = RESTRICTED_CLEAR_CANVAS;

            clearPopups();
            confirmTool.setMessage("Are you sure you want to clear the canvas?");
            confirmTool.openConfirmTool();
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void initColourManager(object sender, RoutedEventArgs e)
        {
            clearPopups();
            colourManager.openMenu();
            popupOpened?.Invoke(this, EventArgs.Empty);
        }

        private void editPalette(object sender, RoutedEventArgs e)
        {
            editingPalette = !editingPalette;
            colourManager.editingPalette = editingPalette;

            if (editingPalette)
            {
                editPaletteButton.Background = new SolidColorBrush(hexToColor("#a0cdff59"));
            }
            else {
                editPaletteButton.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void colourManagerClosed(object sender, EventArgs e)
        {
          
                popupClosed?.Invoke(this, EventArgs.Empty);
            
            
        }

        private void fileLoaded(object sender, EventArgs e)
        {   
            initColourPalette(fileManager.colourPalette);
            colourManager.setBGColour(fileManager.bgProfile, fileManager.bgBrightness, fileManager.bgOpacity);

            NotifyPropertyChanged();

            // generate canvas from Stroke data
        }


    }
}
