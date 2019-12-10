using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{


    public sealed partial class Libre : Page
    {

        public Libre()
        {
        this.InitializeComponent();
        this.DataContext = this;
        inkCanvas.InkPresenter.InputDeviceTypes =
            Windows.UI.Core.CoreInputDeviceTypes.Mouse |
            Windows.UI.Core.CoreInputDeviceTypes.Pen |
            Windows.UI.Core.CoreInputDeviceTypes.Touch;
        }


        private void CloseColourPickerMenu(object sender, RoutedEventArgs e)
        {
            // if the Popup is open, then close it 
            this.DataContext = this;
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
        }
        private void initColourPickerMenu(object sender, RoutedEventArgs e)
        {
            //this.DataContext = ColourPicker;
            // open the Popup if it isn't open already 
            DataContext = ColourPickerData.getColourPickerData();
            if (!ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = true; }
        }

    }

    public class ColourPickerData
    {
        public int width { get; set; }
        public int height { get; set; }
        public int horizontalOffset { get; set; }
        public int verticalOffset { get; set; }
        public String[] prevColors { get; set; }

        public static ColourPickerData getColourPickerData()
        {
            int w = 800;
            int h = 600;
            String[] p_c = {"#cdff59", "#4ffff6", "#ff728e", "#1d283f", "#2b061e", "#ffeed6", "#fbbfca", "#bfd0f0" };
            

            var cpd = new ColourPickerData()
            {
                width = w,
                height = h,
                horizontalOffset = (int)(Window.Current.Bounds.Width - w) / 2,
                verticalOffset = (int)(Window.Current.Bounds.Height - h) / 2,
                prevColors = p_c

            };

            return cpd;
        }
        
    
    }

}


