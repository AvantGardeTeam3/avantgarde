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
    public sealed partial class VerticalJoystick : UserControl, INotifyPropertyChanged
    {
        public int joystickStateY { get; set; }

        public VerticalJoystick()
        {
            this.InitializeComponent();
            joystickStateY = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Action<object, StateChangedEventArgs>> GazeStateChangeHandler { get; set; }

        public event EventHandler UpKeyInvoked;

        public event EventHandler DownKeyInvoked;

        public event EventHandler MiddleKeyInvoked;

        public void displayEndPointCommands() {
            UpKey.Content = "New Line";
            DownKey.Content = "Move";
            NotifyPropertyChanged();
        }

        public void displayMidPointCommands() {
            UpKey.Content = "Curve";
            DownKey.Content = "Delete";
            NotifyPropertyChanged();
        }

        void JoystickUI_Loaded(object sender, RoutedEventArgs e)
        {
            UpKey.PointerEntered += KeyPointerEntered;
            MidKey.PointerEntered += KeyPointerEntered;
            DownKey.PointerEntered += KeyPointerEntered;

            UpKey.PointerExited += KeyPointerExited;
            MidKey.PointerExited += KeyPointerExited;
            DownKey.PointerExited += KeyPointerExited;
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
                case "MidKey":
                    BallToCentre();
                    break;
            }
        }

        private void KeyPointerExited(object sender, PointerRoutedEventArgs e)
        {
            BallToCentre();
        }

        private void BallToCentre()
        {
            joystickStateY = 1;
            NotifyPropertyChanged();
        }

        private void BallToUp()
        {
            joystickStateY = 0;
            NotifyPropertyChanged();
        }

        private void BallToDown()
        {
            joystickStateY = 2;
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
                case "MidKey":
                    MiddleKeyInvoked(this, EventArgs.Empty);
                    break;
            }
        }
    }
}
