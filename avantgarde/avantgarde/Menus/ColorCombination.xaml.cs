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
            opacity =  new string[6, 5] { { "FF", "FF", "FF", "FF", "FF"},
            { "FF", "FF", "FF", "FF", "FF"},
            { "FF", "FF", "FF", "FF", "FF"},
            { "FF", "FF", "FF", "FF", "FF"},
            { "FF", "FF", "FF", "FF", "FF"},
            { "FF", "FF", "FF", "FF", "FF"}};
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



        public string[,] colorList = new string[6, 5];/*
        {
           /{"DAD870","FFCD58","FF9636","FF5C4D","CD5C5C" },
            { "99B898","FECEA8","FF847C","E84A5F","2A363B"},
            {"F8B195","F67280","C06C84","6C5B7B","355C7D" },
            {"A8E6CE","DCEDC2","FFD3B5","FFAAA6","FF8C94" },
            {"A8A7A7","CC527A","E8175D","474747","363636" },
            {"594F4F","547980","45ADA8","9DE0AD","E5FCC2" } }*/

        public string[,] opacity;
        
        private void initialColorCombination(int order)
        {
   
            
            for (int i = 0; i < 5; i++)
            {
                DIYColorButtons[5 + i].Background = new SolidColorBrush(hexToColor(colorList[order, i],i));
                DIYColorButtons[10 + i].Background = new SolidColorBrush(hexToColor(colorList[order + 1, i], i));
            }
        }
        private Color hexToColor(string hex,int orderOfPresetColorButton)
        {
            hex = opacity[order, orderOfPresetColorButton] + hex;
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
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
            //Debug.WriteLine(storageFolder.Path);
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
        }

        private Color completeHexToColor(string hex)
        {
            byte a = (byte)(Convert.ToUInt32(hex.Substring(1, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(3, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(5, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(7, 2), 16));

            return Color.FromArgb(a, r, g, b);
        }


        private string[] recycleColor = new string[5];
        private string[,] colourData = new string[13, 11] {
                    { "000000", "330000", "660000","990000", "CC0000", "FF0000", "FF3333", "FF6666", "FF9999", "FFCCCC", "FFFFFF"},
                    { "000000", "331900", "663300","994C00", "CC6600", "FF8000", "FF9933", "FFB266", "FFCC99", "FFE5CC", "FFFFFF"},
                    { "000000", "333300", "666600","999900", "CCCC00", "FFFF00", "FFFF33", "FFFF66", "FFFF99", "FFFFCC", "FFFFFF"},
                    { "000000", "193300", "336600","4C9900", "66CC00", "80FF00", "99FF33", "B2FF66", "CCFF99", "E5FFCC", "FFFFFF"},
                    { "000000", "003300", "006600","009900", "00CC00", "00FF00", "33FF33", "66FF66", "99FF99", "CCFFCC", "FFFFFF"},
                    { "000000", "003319", "006633","00994C", "00CC66", "00FF80", "33FF99", "66FFB2", "99FFCC", "CCFFE5", "FFFFFF"},
                    { "000000", "003333", "006666","009999", "00CCCC", "00FFFF", "33FFFF", "66FFFF", "99FFFF", "CCFFFF", "FFFFFF"},
                    { "000000", "001933", "003366","004C99", "0066CC", "0080FF", "3399FF", "66B2FF", "99CCFF", "CCE5FF", "FFFFFF"},
                    { "000000", "000033", "000066","000099", "0000CC", "0000FF", "3333FF", "6666FF", "9999FF", "CCCCFF", "FFFFFF"},
                    { "000000", "190033", "330066","4C0099", "6600CC", "7F00FF", "9933FF", "B266FF", "CC99FF", "E5CCFF", "FFFFFF"},
                    { "000000", "330033", "660066","990099", "CC00CC", "FF00FF", "FF33FF", "FF66FF", "FF99FF", "FFCCFF", "FFFFFF"},
                    { "000000", "330019", "660033","99004C", "CC0066", "FF007F", "FF3399", "FF66B2", "FF99CC", "FFCCE5", "FFFFFF"},
                    { "000000", "000000", "202020","404040", "606060", "808080", "A0A0A0", "C0C0C0", "E0E0E0", "FFFFFF", "FFFFFF"}
                                    };

        private int[,] GetColorProfileArray(int cate)
        {
            int[,] result = new int[5, 2];
            if(cate == 3)
            {
                for (int i = 0; i < 5; i++)
                {
                    int a = 0, b = 0;
                    for (int j = 0; j < 13; j++)
                    {
                        for (int k = 0; k < 11; k++)
                        {
                            if (colorHexStore[i].Substring(3, 6).CompareTo(colourData[j, k]) == 0)
                            {
                                result[i, 0] = j;
                                result[i, 1] = k;
                            }
                        }
                    }
                }
                return result;
            }
            else if(cate == 1){
                for (int i = 0; i < 5; i++)
                {
                    int a = 0, b = 0;
                    for (int j = 0; j < 13; j++)
                    {
                        for (int k = 0; k < 11; k++)
                        {
                            if (colorList[order,i].CompareTo(colourData[j, k]) == 0)
                            {
                                result[i, 0] = j;
                                result[i, 1] = k;
                            }
                        }
                    }
                }
                return result;
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {
                        for (int k = 0; k < 11; k++)
                        {
                            if (colorList[order+1, i].CompareTo(colourData[j, k]) == 0)
                            {
                                result[i, 0] = j;
                                result[i, 1] = k;
                            }
                        }
                    }
                }
                return result;
            }
            
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            colorCombinationPopup.IsOpen = false;
            SaveColorTheme();
            ThemeColorArg args = new ThemeColorArg();
            if (checkLine3.IsChecked==true)
            {
                args.ColorFromProfile = GetColorProfileArray(3);
                string[] brightness = new string[5];
                for(int i = 0; i < 5; i++)
                {
                    brightness[i] = colorHexStore[i].Substring(1, 2);
                }
                args.opacity = brightness;
                args.LineChosen = 3;
            }
            else if (checkLine1.IsChecked == true)
            { 
                args.LineChosen = 1;
                args.order = order;
                args.ColorFromProfile = GetColorProfileArray(1);
                for(int i = 0; i < 5; i++)
                {
                    opacitySent[i] = opacity[order, i];
                }
                args.opacity = opacitySent;
            }
            else
            {
                args.ColorFromProfile = GetColorProfileArray(2);
                args.LineChosen = 2;
                args.order = order+1;
                for (int i = 0; i < 5; i++)
                {
                    opacitySent[i] = opacity[order+1, i];
                }
                args.opacity = opacitySent;
            }
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

        private int[,] colorSent = new int[5,2];
        private string[] opacitySent = new string[5];
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
