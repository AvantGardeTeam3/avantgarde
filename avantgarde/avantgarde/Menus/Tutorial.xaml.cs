using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde.Menus
{
    //Eduardo Battistini
    // Tutorial logic for switching between tutorial pages
    public sealed partial class Tutorial : Page, INotifyPropertyChanged
    {

        public int width { get; set; }
        public int height { get; set; }
        public int horizontalOffset { get; set; }
        public int verticalOffset { get; set; }

        private int pageID;

        private String tutorialPagePath { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler tutorialClosed;

        public Tutorial()
        {
            pageID = 1;
            getWindowAttributes();
            updatePage();
            this.InitializeComponent();
            left_button.Visibility = Visibility.Collapsed;

        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            width = 800;
            height = 600;
            horizontalOffset = (int)(Window.Current.Bounds.Width - width) / 2;
            verticalOffset = (int)(Window.Current.Bounds.Height - height) / 2;
        }

        private void left(object sender, RoutedEventArgs e)
        {
            pageID--;
            if (pageID == 1)
            {
                left_button.Visibility = Visibility.Collapsed;

            }
            if (pageID < 8)
            {
                right_button.Visibility = Visibility.Visible;

            }
            updatePage();
        }

        private void right(object sender, RoutedEventArgs e)
        {
            pageID++;
            if (pageID == 8)
            {
                right_button.Visibility = Visibility.Collapsed;
            }
            if (pageID > 1)
            {
                left_button.Visibility = Visibility.Visible;
            }
            updatePage();
        }

        private void close(object sender, RoutedEventArgs e)
        {
            close();
            tutorialClosed?.Invoke(this, EventArgs.Empty);
        }

        public void close() {
            if (tutorial.IsOpen) { tutorial.IsOpen = false; }
        }

        public void open(int id)
        {
            pageID = id;

            if (pageID != 1) {
                left_button.Visibility = Visibility.Visible;
            }
            if (pageID != 8)
            {
                right_button.Visibility = Visibility.Visible;
            }

            updatePage();
            if (!tutorial.IsOpen) { tutorial.IsOpen = true; }
        }

        private void updatePage()
        {
            tutorialPagePath = "/Assets/tutorial/page_" + pageID.ToString() + ".png";
            NotifyPropertyChanged();
        }

    }
}




