using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace avantgarde.Menus
{
    public sealed partial class ColourManager : UserControl, INotifyPropertyChanged
    {

        public Color selection { get; set; }
        public String selectionHex { get; set; }

        public static Color defaultColour = Colors.Coral;

        ColourPickerData colourPickerData = new ColourPickerData();

        public String[] prevColours { get; set;}

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Color getColour() {
            return selection;
        }

        public ColourManager()
        {
            this.InitializeComponent();
            DataContext = colourPickerData.getColourPickerData();
            prevColours = colourPickerData.getDefaultPrevColours();
            selection = defaultColour;
            selectionHex = selection.ToString();
        }

        public void openMenu() {
            prevColours = colourPickerData.getDefaultPrevColours();
            NotifyPropertyChanged();
            if (!ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = true; }
            ColourPicker.Color = selection;
        }

        private Color hexToColor(string hex)
        {
            //replace # occurences
            if (hex.IndexOf('#') != -1)
                hex = hex.Replace("#", "");


            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));

            return Color.FromArgb(a, r, g, b);
        }

        private void setColour0(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColour(0));
        }

        private void setColour1(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColour(1));
        }

        private void setColour2(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColour(2));
        }

        private void setColour3(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColour(3));
        }

        private void setColour4(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColour(4));
        }

        private void setColour5(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColour(5));
        }

        private void setColour6(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColour(6));
        }

        private void setColour7(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColour(7));
        }

        private void selectColour(object sender, RoutedEventArgs e)
        {
            selection = ColourPicker.Color;
            selectionHex = selection.ToString();
            colourPickerData.addColourToPrevColours(selection.ToString());
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
            NotifyPropertyChanged();
            
        }

        private void cancelColourPick(object sender, RoutedEventArgs e)
        {
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
        }

        public class ColourPickerData 
        {
            //public ICommand setColour { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int horizontalOffset { get; set; }
            public int verticalOffset { get; set; }
            public String[] prevColours { get; set; }

            public static String[] defaultPrevColours = { "#ffcdff59", "#ff4ffff6", "#ffff728e", "#ff1d283f", "#ff2b061e", "#ffffeed6", "#fffbbfca", "#ffbfd0f0" };



            public String[] getDefaultPrevColours()
            {
                return defaultPrevColours;
            }

            public String getDefaultPrevColour(int i)
            {
                return defaultPrevColours[i];
            }

            public ColourPickerData getColourPickerData()
            {
                int w = 800;
                int h = 600;

                var cpd = new ColourPickerData()
                {
                    width = w,
                    height = h,
                    horizontalOffset = (int)(Window.Current.Bounds.Width - w) / 2,
                    verticalOffset = (int)(Window.Current.Bounds.Height - h) / 2,
                    prevColours = defaultPrevColours,

                };

                return cpd;
            }

            public void addColourToPrevColours(String c)
            {
                if (c.Equals(defaultPrevColours[0]))
                {
                    return;
                }

                for (int i = (defaultPrevColours.Length - 1); i > 0; i--)
                {
                    defaultPrevColours[i] = defaultPrevColours[i - 1];
                }
                defaultPrevColours[0] = c;
            }


        }

        //class SetColourCommand : Libre, ICommand
        //{
        //    public event EventHandler CanExecuteChanged;

        //    public bool CanExecute(object parameter)
        //    {
        //        return true;
        //    }
        //    public void Execute(object parameter)
        //    {
        //        //   ColourPicker.Color = Colors.Beige;
        //        //   setColour20();
        //        String c = ColourPickerData.defaultPrevColours[Int32.Parse(parameter.ToString())];
        //        selection = hexToColor(c);
        //        selection_hex = selection.ToString();


        //        //if (ColourPickerMenu.IsOpen)
        //        //{
        //        //    Debug.WriteLine("hello");
        //        //    ColourPickerMenu.IsOpen = false;
        //        //}

        //        Debug.WriteLine(selection_hex);
        //    }
        //}



    }


}

