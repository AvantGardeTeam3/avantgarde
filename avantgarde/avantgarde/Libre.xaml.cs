
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{


    public partial class Libre : INotifyPropertyChanged
    {

        ColourPickerData colourPickerData = new ColourPickerData();

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Color selection { get; set; }
        public String selection_hex { get; set; }

        public Libre()
        {
            selection = Colors.Blue;
            selection_hex = selection.ToString();
            this.InitializeComponent();
            this.DataContext = this;
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;
        }

        public Color hexToColor(string hex)
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
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(0));
        }

        private void setColour1(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(1));
        }

        private void setColour2(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(2));
        }

        private void setColour3(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(3));
        }

        private void setColour4(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(4));
        }

        private void setColour5(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(5));
        }

        private void setColour6(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(6));
        }

        private void setColour7(object sender, RoutedEventArgs e)
        {
            ColourPicker.Color = hexToColor(colourPickerData.getDefaultPrevColours(7));
        }



        private void selectColour(object sender, RoutedEventArgs e) {
            selection = ColourPicker.Color;
            selection_hex = selection.ToString();
            colourPickerData.addColourToPrevColours(selection.ToString());
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
            NotifyPropertyChanged();
        }

        private void cancelColourPick(object sender, RoutedEventArgs e)
        {
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
        }

        
        private void initColourPickerMenu(object sender, RoutedEventArgs e)
        {
            DataContext = colourPickerData.getColourPickerData();
            if (!ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = true; }
            ColourPicker.Color = selection;
        }

    }

    public class ColourPickerData : INotifyPropertyChanged
    {
        public ICommand setColour { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int horizontalOffset { get; set; }
        public int verticalOffset { get; set; }
        public String[] prevColors { get; set; }

        public static String[] defaultPrevColours = { "#ffcdff59", "#ff4ffff6", "#ffff728e", "#ff1d283f", "#ff2b061e", "#ffffeed6", "#fffbbfca", "#ffbfd0f0" };

        public event PropertyChangedEventHandler PropertyChanged;

        public String getDefaultPrevColours(int i) {
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
                prevColors = defaultPrevColours,
                setColour = new SetColourCommand()

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

    class SetColourCommand : Libre, ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }
        public void Execute(object parameter)
        {
            //   ColourPicker.Color = Colors.Beige;
            //   setColour20();
            String c = ColourPickerData.defaultPrevColours[Int32.Parse(parameter.ToString())];
            selection = hexToColor(c);
            selection_hex = selection.ToString();


            //if (ColourPickerMenu.IsOpen)
            //{
            //    Debug.WriteLine("hello");
            //    ColourPickerMenu.IsOpen = false;
            //}

            Debug.WriteLine(selection_hex);
        }
    }



}
