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

        public int selectedPalette = 0;

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

        public brushTool getBrushTool()
        {
            return this.brushTool;
        }

        public LibreToolBox()
        {
            autoswitch = true;
            editingPalette = false;
            brushSelection = "paint";
            colourPalette = new String[5];
            
           
            mandalaLines = 8;
            propertyUpdate();
            this.InitializeComponent();
            colourPalette0.BorderBrush = new SolidColorBrush(hexToColor("#ffcdff59"));
            colourPalette0.BorderThickness = new Thickness(5);
            drawingAttributes.Color = ColourManager.defaultColour;
            drawingAttributes.Size = new Size(10, 10);
            brushTool.drawingAttributes = this.drawingAttributes;

            autoswitchButton.Background = new SolidColorBrush(hexToColor("#a0cdff59"));
            updateBackgroundButton();
            initColourPalette(this.colourPaletteData);
            
            colourHex = ColourManager.defaultColour.ToString();
            brushSize = 10;
            brushTool.brushSize = this.brushSize;
            brushTool.mandalaLines = this.mandalaLines;
          
   
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
            brushTool.propertiesUpdated += new EventHandler(drawingAttributesUpdated);
            brushTool.menuClosed += new EventHandler(brushToolClosed);
        }



        private void updatePaletteSelection() {
            colourPalette0.BorderBrush = new SolidColorBrush(Colors.White);
            colourPalette1.BorderBrush = new SolidColorBrush(Colors.White);
            colourPalette2.BorderBrush = new SolidColorBrush(Colors.White);
            colourPalette3.BorderBrush = new SolidColorBrush(Colors.White);
            colourPalette4.BorderBrush = new SolidColorBrush(Colors.White);
            colourPalette0.BorderThickness = new Thickness(2);
            colourPalette1.BorderThickness = new Thickness(2);
            colourPalette2.BorderThickness = new Thickness(2);
            colourPalette3.BorderThickness = new Thickness(2);
            colourPalette4.BorderThickness = new Thickness(2);

            if (selectedPalette == 0)
            {
                colourPalette0.BorderBrush = new SolidColorBrush(hexToColor("#ffcdff59"));
                colourPalette0.BorderThickness = new Thickness(5);
            }
            else if (selectedPalette == 1)
            {
                colourPalette1.BorderBrush = new SolidColorBrush(hexToColor("#ffcdff59"));
                colourPalette1.BorderThickness = new Thickness(5);
            }
            else if (selectedPalette == 2)
            {
                colourPalette2.BorderBrush = new SolidColorBrush(hexToColor("#ffcdff59"));
                colourPalette2.BorderThickness = new Thickness(5);
            }
            else if (selectedPalette == 3)
            {
                colourPalette3.BorderBrush = new SolidColorBrush(hexToColor("#ffcdff59"));
                colourPalette3.BorderThickness = new Thickness(5);
            }
            else if (selectedPalette == 4)
            {
                colourPalette4.BorderBrush = new SolidColorBrush(hexToColor("#ffcdff59"));
                colourPalette4.BorderThickness = new Thickness(5);
            }
            NotifyPropertyChanged();
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

        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

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

                selectedPalette = 0;
                updatePaletteSelection();
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
                selectedPalette = 1;
                updatePaletteSelection();
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
                selectedPalette = 2;
                updatePaletteSelection();
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
                selectedPalette = 3;
                updatePaletteSelection();
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
                selectedPalette = 4;
                updatePaletteSelection();
                colourManager.switchID = 4;
                colourManager.updateColour(colourPaletteData[4, PROFILE], colourPaletteData[4, BRIGHTNESS], colourPaletteData[4, OPACITY]);
            }
        }

        private void updatePalette(object sender, EventArgs e) {
            colourPalette = colourManager.colourPaletteHex;
            
            fileManager.colourPalette = colourManager.colourPaletteData;
            NotifyPropertyChanged();
        }

        public void next() {
            selectedPalette++;
            if (selectedPalette == 5) {
                selectedPalette = 0;
            }
            updatePaletteSelection();
        }

        private void toggleAS() {
            autoswitch = !autoswitch;

            Configuration.fleur.Autoswitch = autoswitch;

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
            popupOpened?.Invoke(this, EventArgs.Empty);
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
            popupOpened?.Invoke(this, EventArgs.Empty);
            confirmTool.setMessage("Are you sure you wish to load from Slot " + fileManager.selectedSlot.ToString() + "? \n The current canvas will be lost.");
            confirmTool.openConfirmTool();
        }
        private void exportButtonClicked(object sender, RoutedEventArgs e)
        {
            saveImageClicked?.Invoke(this, EventArgs.Empty);
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

        public void openBrushTool(object sender, RoutedEventArgs e)
        {
            clearPopups();
            brushTool.open();
            popupOpened?.Invoke(this, EventArgs.Empty);
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
            if (brushTool != null)
            {
                brushTool.close();
            }

            popupClosed?.Invoke(this, EventArgs.Empty);

        }

        public event EventHandler goHomeButtonClicked;
        public event EventHandler setBackgroundButtonClicked;
        public event EventHandler propertiesUpdated;
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


        private void clearCanvas(object sender, RoutedEventArgs e) {
            restrictedID = RESTRICTED_CLEAR_CANVAS;

            clearPopups();
            confirmTool.setMessage("Are you sure you want to clear the canvas?");
            confirmTool.openConfirmTool();
            // clear strokeData and user/mandala strokes
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


        private void drawingAttributesUpdated(object sender, EventArgs e) {
            drawingAttributes = brushTool.getDrawingAttributes();
            drawingAttributes.Color = colourManager.getColour();
            mandalaLines = brushTool.mandalaLines;
            propertyUpdate();
        }

        private void brushToolClosed(object sender, EventArgs e)
        {
            popupClosed?.Invoke(this, EventArgs.Empty);
        }

        private void libreToolBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grid.Width = libreToolBox.ActualWidth;
            grid.Height = libreToolBox.ActualHeight;
        }
    }
}
