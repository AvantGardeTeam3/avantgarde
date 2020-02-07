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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace avantgarde.Menus
{
    public sealed partial class UI : UserControl, INotifyPropertyChanged
    
    {
        private int WIDTH { get; set; }
        private int HEIGHT { get; set; }

        public bool drawState { get; set; }

        public String drawStateIcon { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            WIDTH = (int)Window.Current.Bounds.Width;
            HEIGHT = (int)Window.Current.Bounds.Height;
        }

        public UI()
        {
            getWindowAttributes();
            this.InitializeComponent();
            drawState = false;
            drawStateIcon = "/Assets/icons/icon_play.png";

        }

        private void updateDrawStateIcon() {
            if (drawState)
            {
                drawStateIcon = "/Assets/icons/icon_pause.png";
            }
            else 
            {
                drawStateIcon = "/Assets/icons/icon_play.png";
            }
        }

        private void changeDrawState(object sender, RoutedEventArgs e)
        {
            drawState = !drawState;
            updateDrawStateIcon();
            NotifyPropertyChanged();
        }


        public event EventHandler undoButtonClicked;
        public event EventHandler redoButtonClicked;

        private void undo(object sender, RoutedEventArgs e)
        {
            undoButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private void redo(object sender, RoutedEventArgs e)
        {
            redoButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
