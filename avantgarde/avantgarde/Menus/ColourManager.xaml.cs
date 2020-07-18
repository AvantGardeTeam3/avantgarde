using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace avantgarde.Menus
{
    //Colour management system. Stores colour palette data and background data.
    //There are 13 different colour profiles, each with 10 brightnesses and 20 opacities.
    //Same system is used for all colour selections in program.
    //Eduardo Battistini
    public sealed partial class ColourManager : Windows.UI.Xaml.Controls.UserControl, INotifyPropertyChanged
    {
        public event EventHandler<Events.ColorEventArgs> Confirmed;
        public event EventHandler Canceled;
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static int DEFAULT_BRIGHTNESS = 5;
        private static int DEFAULT_PROFILE = 5;
        private static int DEFAULT_OPACITY = 10;

        private Utils.AGColor _color;
        private int _profile = 5;
        private int _brightness = 5;
        private int _opacity = 10;

        public Windows.UI.Color Color { get { return _color.Color; } private set { } }
        public String Hex { get { return _color.Hex; } private set { } }
        public int R { get { return Color.R; } private set { } }
        public int G { get { return Color.G; } private set { } }
        public int B { get { return Color.B; } private set { } }
        public int A { get { return Color.A; } private set { } }
        public String ColorName { get; private set; }

        private double width;
        private double height;

        private double horizontalOffset;
        private double verticalOffset;

        public ColourManager()
        {
            _color = new Utils.AGColor(DEFAULT_PROFILE, DEFAULT_BRIGHTNESS, DEFAULT_OPACITY);
            _profile = DEFAULT_PROFILE;
            _brightness = DEFAULT_BRIGHTNESS;
            _opacity = DEFAULT_OPACITY;
            DataContext = this;

            width = 850;
            height = 650;

            horizontalOffset = (int)(Window.Current.Bounds.Width - width) / 2;
            verticalOffset = (int)(Window.Current.Bounds.Height - height) / 2;

            DataContext = this;

            InitializeComponent();
            NotifyPropertyChanged();
        }

        public void Open(Utils.AGColor color = null)
        {
            if (ColourPickerMenu.IsOpen) return;
            ColourPickerMenu.IsOpen = true;
            if (color == null)
            {
                _profile = DEFAULT_PROFILE;
                _brightness = DEFAULT_BRIGHTNESS;
                _opacity = DEFAULT_OPACITY;
                _color = new Utils.AGColor(_profile, _brightness, _opacity);
                ColorName = Windows.UI.ColorHelper.ToDisplayName(_color.Color);
            }
            else
            {
                _profile = color.Profile;
                _brightness = color.Brightness;
                _opacity = color.Opacity;
                _color = color;
                ColorName = Windows.UI.ColorHelper.ToDisplayName(_color.Color);
            }
            Update();
        }

        public void Close()
        {
            if (ColourPickerMenu.IsOpen) { ColourPickerMenu.IsOpen = false; }
        }

        private void Update()
        {
            // update color
            _color = new Utils.AGColor(_profile, _brightness, _opacity);
            // update name
            ColorName = Windows.UI.ColorHelper.ToDisplayName(_color.Color);

            NotifyPropertyChanged();
        }

        private void OnPresetColorClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Windows.UI.Xaml.Controls.Button button = (Windows.UI.Xaml.Controls.Button)sender;
            if (!button.Name.Substring(0, 6).Equals("preset")) return;
            int num = Convert.ToInt32(button.Name.Substring(6));
            switch (num)
            {
                case 12:
                    _brightness = 0;
                    _profile = 12;
                    break;
                case 13:
                    _brightness = DEFAULT_BRIGHTNESS;
                    _profile = 12;
                    break;
                case 14:
                    _brightness = 10;
                    _profile = 12;
                    break;
                default:
                    _brightness = DEFAULT_BRIGHTNESS;
                    _profile = num;
                    break;
            }
            Update();
        }

        private void OnConfirmButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Confirmed(this, new Events.ColorEventArgs(_color));
        }

        private void OnCancelButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Canceled(this, EventArgs.Empty);
        }

        private void OnIncreaseBrightnessButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_brightness == 10) return;
            _brightness++;
            Update();
        }

        private void OnDecreaseBrightnessButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_brightness == 0) return;
            _brightness--;
            Update();
        }

        private void OnIncreaseOpacityButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_opacity == 100) return;
            _opacity++;
            Update();
        }

        private void OnDecreaseOpacityButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (_opacity == 0) return;
            _opacity--;
            Update();
        }
    }
}