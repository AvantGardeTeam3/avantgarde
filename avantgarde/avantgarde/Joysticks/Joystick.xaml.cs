using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.Devices.Input.Preview;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using System.ComponentModel;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace avantgarde.Joysticks
{
    public sealed partial class Joystick : UserControl, INotifyPropertyChanged
    {
        public int joystickStateX { get; set; }
        public int joystickStateY { get; set; }


        public Joystick()
        {
            this.InitializeComponent();
            joystickStateX = 1;
            joystickStateY = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void JoystickUI_Loaded(object sender, RoutedEventArgs e) {

            //replace these events with GazeEntered and GazeExited from GazeInputSourcePreview class

            UpKey.PointerEntered += UpKey_PointerEntered;
            UpKey.PointerExited += UpKey_PointerExited;
            DownKey.PointerEntered += DownKey_PointerEntered;
            DownKey.PointerExited += DownKey_PointerExited;
            LeftKey.PointerEntered += LeftKey_PointerEntered;
            LeftKey.PointerExited += LeftKey_PointerExited;
            RightKey.PointerEntered += RightKey_PointerEntered;
            RightKey.PointerExited += RightKey_PointerExited;
        }

        public void right() {
            Debug.WriteLine("right");
        }

        private void RightKey_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("exited");
            joystickStateX = 1;
            NotifyPropertyChanged();
        }

        private void RightKey_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("entered");
            joystickStateX = 2;
            NotifyPropertyChanged();
            right();
        }

        private void LeftKey_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void LeftKey_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DownKey_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DownKey_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UpKey_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void UpKey_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
