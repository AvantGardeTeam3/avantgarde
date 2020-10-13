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

            PointerEnteredHandler = new List<Action<object, PointerRoutedEventArgs>>();
            PointerExitedHandler = new List<Action<object, PointerRoutedEventArgs>>();

            GazeStateChangeHandler = new List<Action<object, StateChangedEventArgs>>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<Action<object, PointerRoutedEventArgs>> PointerEnteredHandler { get; set; }
        public List<Action<object, PointerRoutedEventArgs>> PointerExitedHandler { get; set; }
        public List<Action<object, StateChangedEventArgs>> GazeStateChangeHandler { get; set; }

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

            //replace these events with GazeEntered and GazeExited from GazeInputSourcePreview class

            UpKey.PointerEntered += KeyPointerEntered;
            DownKey.PointerEntered += KeyPointerEntered;

            UpKey.PointerExited += KeyPointerExited;
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
            }
            foreach (Action<object, PointerRoutedEventArgs> action in PointerEnteredHandler)
            {
                action.Invoke(sender, e);
            }
        }

        private void KeyPointerExited(object sender, PointerRoutedEventArgs e)
        {
            BallToCentre();
            foreach (Action<object, PointerRoutedEventArgs> action in PointerExitedHandler)
            {
                action.Invoke(sender, e);
            }
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
            foreach (Action<object, StateChangedEventArgs> action in GazeStateChangeHandler.ToArray())
            {
                action.Invoke(sender, e);
            }
        }
    }
}
