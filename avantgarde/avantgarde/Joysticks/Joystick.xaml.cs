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

using Microsoft.Toolkit.Uwp.Input.GazeInteraction;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace avantgarde.Joysticks
{
    public sealed partial class Joystick : UserControl, INotifyPropertyChanged
    {
        public int joystickStateX { get; set; }
        public int joystickStateY { get; set; }

        public event EventHandler UpKeyInvoked;
        public event EventHandler DownKeyInvoked;
        public event EventHandler LeftKeyInvoked;
        public event EventHandler RightKeyInvoked;

        public event EventHandler MiddleKeyInvoked;

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

            UpKey.PointerEntered += KeyPointerEntered;
            DownKey.PointerEntered += KeyPointerEntered;
            LeftKey.PointerEntered += KeyPointerEntered;
            RightKey.PointerEntered += KeyPointerEntered;

            UpKey.PointerExited += KeyPointerExited;
            DownKey.PointerExited += KeyPointerExited;
            LeftKey.PointerExited += KeyPointerExited;
            RightKey.PointerExited += KeyPointerExited;
        }

        private void KeyPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "UpKey":
                    BallToUp();
                    break;
                case "DownKey":
                    BallToDown();
                    break;
                case "LeftKey":
                    BallToLeft();
                    break;
                case "RightKey":
                    BallToRight();
                    break;
            }
        }

        private void KeyPointerExited(object sender, PointerRoutedEventArgs e)
        {
            BallToCentre();
        }

        private void BallToCentre()
        {
            joystickStateX = 1;
            joystickStateY = 1;
            NotifyPropertyChanged();
        }

        private void BallToUp()
        {
            joystickStateX = 1;
            joystickStateY = 0;
            NotifyPropertyChanged();
        }

        private void BallToDown()
        {
            joystickStateX = 1;
            joystickStateY = 2;
            NotifyPropertyChanged();
        }

        private void BallToLeft()
        {
            joystickStateX = 0;
            joystickStateY = 1;
            NotifyPropertyChanged();
        }

        private void BallToRight()
        {
            joystickStateX = 2;
            joystickStateY = 1;
            NotifyPropertyChanged();
        }

        private void GazeElement_StateChanged(object sender, Microsoft.Toolkit.Uwp.Input.GazeInteraction.StateChangedEventArgs e)
        {
            if (e.PointerState == PointerState.Exit)
            {
                BallToCentre();
                return;
            }
            else
            {
                Button button = (Button)sender;
                switch (button.Name)
                {
                    case "UpKey":
                        BallToUp();
                        break;
                    case "DownKey":
                        BallToDown();
                        break;
                    case "LeftKey":
                        BallToLeft();
                        break;
                    case "RightKey":
                        BallToRight();
                        break;
                }
            }
        }
    
        private void OnKeyClick(object sender, RoutedEventArgs args)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "UpKey":
                    UpKeyInvoked(this, EventArgs.Empty);
                    break;
                case "DownKey":
                    DownKeyInvoked(this, EventArgs.Empty);
                    break;
                case "LeftKey":
                    LeftKeyInvoked(this, EventArgs.Empty);
                    break;
                case "RightKey":
                    RightKeyInvoked(this, EventArgs.Empty);
                    break;
                case "MidKey":
                    MiddleKeyInvoked(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
