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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Devices.Input.Preview;

using Microsoft.Toolkit.Uwp.Input.GazeInteraction;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace avantgarde
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GazeInputSourcePreview gazeInputSource;
        private GazeDeviceWatcherPreview gazeDeviceWatcher;
        private int deviceCounter = 0;
        private DispatcherTimer timerGaze = new DispatcherTimer();
        private bool timerStarted = false;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StartGazeDeviceWatcher();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            StopGazeDeviceWatcher();
        }
        private void StartGazeDeviceWatcher()
        {
            if (gazeDeviceWatcher != null) return;
            gazeDeviceWatcher = GazeInputSourcePreview.CreateWatcher();
            gazeDeviceWatcher.Added += this.DeviceAdded;
            gazeDeviceWatcher.Updated += this.DeviceUpdated;
            gazeDeviceWatcher.Removed += this.DeviceRemoved;
            gazeDeviceWatcher.Start();
        }

        private void StopGazeDeviceWatcher()
        {
            if (gazeDeviceWatcher == null) return;
            gazeDeviceWatcher.Stop();
            gazeDeviceWatcher.Added -= this.DeviceAdded;
            gazeDeviceWatcher.Updated -= this.DeviceUpdated;
            gazeDeviceWatcher.Removed -= this.DeviceRemoved;
            gazeDeviceWatcher = null;
        }

        private void GazeEntered(
            GazeInputSourcePreview sender,
            GazeEnteredPreviewEventArgs args)
        {
            args.Handled = true;
        }

        private void GazeMoved(
            GazeInputSourcePreview sender,
            GazeMovedPreviewEventArgs args)
        {
            args.Handled = true;
            if (args.CurrentPoint.EyeGazePosition == null) return;
            double gazePointX = args.CurrentPoint.EyeGazePosition.Value.X;
            double gazePointY = args.CurrentPoint.EyeGazePosition.Value.Y;
            Point gazePoint = new Point(gazePointX, gazePointY);
            /*if(DoesElementContainPoint(gazePoint, playButton.Name, playButton))
            {
                this.Frame.Navigate(typeof(Libre));
            }
            else if(DoesElementContainPoint(gazePoint, freeButton.Name, freeButton))
            {
                this.Frame.Navigate(typeof(Fleur));
            }*/
        }

        private void GazeExited(
            GazeInputSourcePreview sender,
            GazeExitedPreviewEventArgs args)
        {
            args.Handled = true;
        }
        /// <summary>
        /// Return whether the gaze point is over the progress bar.
        /// </summary>
        /// <param name="gazePoint">The gaze point screen location</param>
        /// <param name="elementName">The progress bar name</param>
        /// <param name="uiElement">The progress bar UI element</param>
        /// <returns></returns>
        private bool DoesElementContainPoint(   
            Point gazePoint, string elementName, UIElement uiElement)
        {
            // Use entire visual tree of progress bar.
            IEnumerable<UIElement> elementStack =
              VisualTreeHelper.FindElementsInHostCoordinates(gazePoint, uiElement, true);
            foreach (UIElement item in elementStack)
            {
                //Cast to FrameworkElement and get element name.
                if (item is FrameworkElement feItem)
                {
                    if (feItem.Name.Equals(elementName))
                    {
                        return true;
                    }
                }
            }

            // Stop gaze timer and reset progress bar if gaze leaves element.
            return false;
        }

        private async void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Libre));
        }
        private async void Free_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Fleur));
        }
        private async void Exit_Button_Click(object sender, RoutedEventArgs e)
        {

        }
        private void DeviceAdded(GazeDeviceWatcherPreview source,
            GazeDeviceWatcherAddedPreviewEventArgs args)
        {
            if (IsSupportedDevice(args.Device))
            {
                deviceCounter++;
            }
            TryEnableGazeTrackingAsync(args.Device);
        }

        private void DeviceUpdated(GazeDeviceWatcherPreview source,
            GazeDeviceWatcherUpdatedPreviewEventArgs args)
        {
            TryEnableGazeTrackingAsync(args.Device);
        }

        private void DeviceRemoved(GazeDeviceWatcherPreview source,
            GazeDeviceWatcherRemovedPreviewEventArgs args)
        {
            if (IsSupportedDevice(args.Device))
            {
                deviceCounter--;
                if(deviceCounter == 0)
                {
                    gazeInputSource.GazeEntered -= GazeEntered;
                    gazeInputSource.GazeMoved -= GazeMoved;
                    gazeInputSource.GazeExited -= GazeExited;
                }
            }
        }

        /// <summary>
        /// Initialize gaze tracking.
        /// </summary>
        /// <param name="gazeDevice"></param>
        private async void TryEnableGazeTrackingAsync(GazeDevicePreview gazeDevice)
        {
            // If eye-tracking device is ready, declare event handlers and start tracking.
            if (IsSupportedDevice(gazeDevice))
            {
                //timerGaze.Interval = new TimeSpan(0, 0, 0, 0, 20);
                //timerGaze.Tick += TimerGaze_Tick;

                // This must be called from the UI thread.
                gazeInputSource = GazeInputSourcePreview.GetForCurrentView();

                gazeInputSource.GazeEntered += GazeEntered;
                gazeInputSource.GazeMoved += GazeMoved;
                gazeInputSource.GazeExited += GazeExited;
            }
            // Notify if device calibration required.
            else if (gazeDevice.ConfigurationState ==
                     GazeDeviceConfigurationStatePreview.UserCalibrationNeeded ||
                     gazeDevice.ConfigurationState ==
                     GazeDeviceConfigurationStatePreview.ScreenSetupNeeded)
            {
                // Device isn't calibrated, so invoke the calibration handler.
                System.Diagnostics.Debug.WriteLine(
                    "Your device needs to calibrate. Please wait for it to finish.");
                await gazeDevice.RequestCalibrationAsync();
            }
            // Notify if device calibration underway.
            else if (gazeDevice.ConfigurationState ==
                GazeDeviceConfigurationStatePreview.Configuring)
            {
                // Device is currently undergoing calibration.  
                // A device update is sent when calibration complete.
                System.Diagnostics.Debug.WriteLine(
                    "Your device is being configured. Please wait for it to finish");
            }
            // Device is not viable.
            else if (gazeDevice.ConfigurationState == GazeDeviceConfigurationStatePreview.Unknown)
            {
                // Notify if device is in unknown state.  
                // Reconfigure/recalbirate the device.  
                System.Diagnostics.Debug.WriteLine(
                    "Your device is not ready. Please set up your device or reconfigure it.");
            }
        }

        /// <summary>
        /// Check if eye-tracking device is viable.
        /// </summary>
        /// <param name="gazeDevice">Reference to eye-tracking device.</param>
        /// <returns>True, if device is viable; otherwise, false.</returns>
        private bool IsSupportedDevice(GazeDevicePreview gazeDevice)
        {
            return (gazeDevice.CanTrackEyes &&
                     gazeDevice.ConfigurationState ==
                     GazeDeviceConfigurationStatePreview.Ready);
        }
    }
}
