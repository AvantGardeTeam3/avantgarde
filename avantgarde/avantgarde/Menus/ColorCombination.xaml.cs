using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static avantgarde.Menus.ColourManager;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace avantgarde.Menus
{
    public sealed partial class ColorCombination : UserControl
    {
        private int order;
        public ColorCombination()
        {
            this.InitializeComponent();
            
        }
        public void openColorCombination()
        {
            if (!colorCombinationPopup.IsOpen)
            {
                colorCombinationPopup.IsOpen = true;
                order = 0;
                initialColorCombination(order);
                LoadDIYColorTheme();
            }
            else
            {
                colorCombinationPopup.IsOpen = false;
            }
            
        }



        public string[,] colorList = new string[6, 5]/*
        {
           /{"DAD870","FFCD58","FF9636","FF5C4D","CD5C5C" },
            { "99B898","FECEA8","FF847C","E84A5F","2A363B"},
            {"F8B195","F67280","C06C84","6C5B7B","355C7D" },
            {"A8E6CE","DCEDC2","FFD3B5","FFAAA6","FF8C94" },
            {"A8A7A7","CC527A","E8175D","474747","363636" },
            {"594F4F","547980","45ADA8","9DE0AD","E5FCC2" } }*/;


        
        private void initialColorCombination(int order)
        {
            colorCombination0.Background = new SolidColorBrush(hexToColor(colorList[order, 0]));
            colorCombination1.Background = new SolidColorBrush(hexToColor(colorList[order, 1]));
            colorCombination2.Background = new SolidColorBrush(hexToColor(colorList[order, 2]));
            colorCombination3.Background = new SolidColorBrush(hexToColor(colorList[order, 3]));
            colorCombination4.Background = new SolidColorBrush(hexToColor(colorList[order, 4]));
            colorCombination10.Background = new SolidColorBrush(hexToColor(colorList[order + 1, 0]));
            colorCombination11.Background = new SolidColorBrush(hexToColor(colorList[order + 1, 1]));
            colorCombination12.Background = new SolidColorBrush(hexToColor(colorList[order + 1, 2]));
            colorCombination13.Background = new SolidColorBrush(hexToColor(colorList[order + 1, 3]));
            colorCombination14.Background = new SolidColorBrush(hexToColor(colorList[order + 1, 4]));
            DIYColorButtons[0] = colorCombination21;
            DIYColorButtons[1] = colorCombination22;
            DIYColorButtons[2] = colorCombination23;
            DIYColorButtons[3] = colorCombination24;
            DIYColorButtons[4] = colorCombination25;
            DIYColorButtons[5] = colorCombination0;
            DIYColorButtons[6] = colorCombination1;
            DIYColorButtons[7] = colorCombination2;
            DIYColorButtons[8] = colorCombination3;
            DIYColorButtons[9] = colorCombination4;
            DIYColorButtons[10] = colorCombination10;
            DIYColorButtons[11] = colorCombination11;
            DIYColorButtons[12] = colorCombination12;
            DIYColorButtons[13] = colorCombination13;
            DIYColorButtons[14] = colorCombination14;
            
        }
        private Color hexToColor(string hex)
        {
            
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            hex = "#" + hex;
            return Color.FromArgb(a, r, g, b);
        }
        private ColourManager newColorManager; 
        private int ColorSelectIndex=0;
        private Button[] DIYColorButtons = new Button[15];
        private string[] colorHexStore = new string[5] {"#FFFFFFFF", "#FFFFFFFF", "#FFFFFFFF", "#FFFFFFFF", "#FFFFFFFF" };
        private void changeDIYColour()
        {
            newColorManager = Controller.ControllerFactory.gazeController.colourManager;
            newColorManager.openMenu();
            newColorManager.themeChoosing = true;
            newColorManager.themeColorSelected += ColorChosen;
        }
   
        private void colorCombination21_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 0;
            changeDIYColour();
        }
        private string[] DIYcolorHex = new string[5];
        
        private void ColorChosen(object sender, ThemeColorArg e)
        {
            DIYColorButtons[ColorSelectIndex].Background = new SolidColorBrush( e.colorSelected);
            if (ColorSelectIndex < 5)
            {
                colorHexStore[ColorSelectIndex] = e.colorHex;
            }
        }
        private void colorCombination22_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 1;
            changeDIYColour();
        }

        private void colorCombination23_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 2;
            changeDIYColour();
        }

        private void colorCombination24_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 3;
            changeDIYColour();
        }

        private void colorCombination25_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 4;
            changeDIYColour();
        }

        private void recycle_Click(object sender, RoutedEventArgs e)
        {
            Color[] recycleColors = new Color[5];
            ColourManager colorManager = new ColourManager();
            colorManager.AutoChangeColor(recycleColors);
            colorCombinationPopup.IsOpen = false;
        }
        private String[] colorRecycleListHex = new String[5] {"DAD870",
        "FFCD58","FF9636","FF5C4D","CD5C5C"};

        private async void SaveColorTheme()
        {

            await Windows.Storage.FileIO.WriteLinesAsync(sampleFile,colorHexStore);
            
        }
        private Windows.Storage.StorageFile colorThemeFile;
        private IList<string> colorThemeFileContent;
        private void prePage_Click(object sender, RoutedEventArgs e)
        {
            order -= 2;
            if (order == -2)   //the final page
            {
                order = 4;
            }
            initialColorCombination(order);
        }

        private void nextPage_Click(object sender, RoutedEventArgs e)
        {
            order += 2;
            if (order == 6)   //the frist page
            {
                order = 0;
            }
            initialColorCombination(order);
        }

        

        private Color[] getUserDefineColorTheme()
        {
            Color[] recycleColors = new Color[5];
            SolidColorBrush color1 = (SolidColorBrush)colorCombination21.Background;
            recycleColors[0] = color1.Color;
            SolidColorBrush color2 = (SolidColorBrush)colorCombination22.Background;
            recycleColors[1] = color2.Color;
            SolidColorBrush color3 = (SolidColorBrush)colorCombination23.Background;
            recycleColors[2] = color3.Color;
            SolidColorBrush color4 = (SolidColorBrush)colorCombination24.Background;
            recycleColors[3] = color4.Color;
            SolidColorBrush color5 = (SolidColorBrush)colorCombination25.Background;
            recycleColors[4] = color5.Color;
            return recycleColors;
        }

        private string NON = "non";
        private Windows.Storage.StorageFile sampleFile;
        private async void LoadDIYColorTheme()
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            FileInfo fileInfo = new FileInfo("colorTheme.txt");
            /*if (!fileInfo.Exists)
            {
                sampleFile =
              await storageFolder.GetFileAsync("colorTheme.txt");
                await Windows.Storage.FileIO.WriteTextAsync(sampleFile,"non\nnon\nnon\nnon\nnon");
            }*/
            Debug.WriteLine(storageFolder.Path);
            sampleFile =
              await storageFolder.GetFileAsync("colorTheme.txt");
            colorThemeFile = sampleFile;
            colorThemeFileContent = await Windows.Storage.FileIO.ReadLinesAsync(sampleFile);
            
            for(int i = 0; i < 5; i++)
            {
                if (string.Compare(colorThemeFileContent[i],NON)==0)
                {
                    
                }
                else
                {
                    colorHexStore[i] = colorThemeFileContent[i];
                    DIYColorButtons[i].Background = new SolidColorBrush(completeHexToColor(colorThemeFileContent[i]));
                }
            }
            DIYcolorHex = colorHexStore;
        }

        private Color completeHexToColor(string hex)
        {
            byte a = (byte)(Convert.ToUInt32(hex.Substring(1, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(3, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(5, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(7, 2), 16));

            return Color.FromArgb(a, r, g, b);
        }

        private void LineSelectRadioButton_Checked(object sender, RoutedEventArgs e)
        {
        }

        private string[] recycleColor = new string[5];
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            colorCombinationPopup.IsOpen = false;
            SaveColorTheme();
            ThemeColorArg args = new ThemeColorArg();
            args.recycleColor =getUserDefineColorTheme();
            ThemeUpdate?.Invoke(this, args);
        }

        public event EventHandler<ThemeColorArg> ThemeUpdate; 
        private void colorCombination0_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 5;
            changeDIYColour();
        }

        private void colorCombination1_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 6;
            changeDIYColour();
        }

        private void colorCombination2_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 7;
            changeDIYColour();
        }

        private void colorCombination3_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 8;
            changeDIYColour();
        }

        private void colorCombination4_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 9;
            changeDIYColour();
        }

        private void colorCombination10_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 10;
            changeDIYColour();
        }

        private void colorCombination11_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 11;
            changeDIYColour();
        }

        private void colorCombination12_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 12;
            changeDIYColour();
        }

        private void colorCombination13_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 13;
            changeDIYColour();
        }

        private void colorCombination14_Click(object sender, RoutedEventArgs e)
        {
            ColorSelectIndex = 14;
            changeDIYColour();
        }


        private void checkLine1_Click(object sender, RoutedEventArgs e)
        {
            checkLine2.IsChecked = false;
            checkLine3.IsChecked = false;
            
        }

        private void checkLine2_Click(object sender, RoutedEventArgs e)
        {
            checkLine1.IsChecked = false;
            checkLine3.IsChecked = false;
        }

        private void checkLine3_Click(object sender, RoutedEventArgs e)
        {
            checkLine1.IsChecked = false;
            checkLine2.IsChecked = false;
            //recycleColor = colorHexStore;
        }
    }
}
