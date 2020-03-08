using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input.Inking;
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
    public sealed partial class FileManager : UserControl, INotifyPropertyChanged
    {
        private int selectedSlot = 0;

        private Fleur page;
        private List<StrokeData> strokeData;
        


        private String slot1State { get; set; }
        private String slot2State { get; set; }
        private String slot3State { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int horizontalOffset { get; set; }
        public int verticalOffset { get; set; }

        public FileManager()
        {
            slot1State = "Collapsed";
            slot2State = "Collapsed";
            slot3State = "Collapsed";
            getWindowAttributes();
            this.InitializeComponent();   
        }

        private String serialize()
        {
            StringBuilder content = new StringBuilder();
            foreach (StrokeData s in strokeData) {
                content.Append(s.p0.X.ToString() + "," + s.p0.Y.ToString()+",");
                content.Append(s.p1.X.ToString() + "," + s.p1.Y.ToString() + ",");
                content.Append(s.p2.X.ToString() + "," + s.p2.Y.ToString() + ",");
                content.Append(s.p3.X.ToString() + "," + s.p3.Y.ToString() + ",");
                content.Append(s.midpoint.X.ToString() + "," + s.midpoint.Y.ToString() + ",");
                content.Append(s.halfpoint.X.ToString() + "," + s.halfpoint.Y.ToString() + ",");
                content.Append(s.colourProfile.ToString() + ",");
                content.Append(s.brightness.ToString() + ",");
                content.Append(s.opacity.ToString() + ",");
                content.Append(s.modified.ToString() + ",");
                content.Append(s.brush + ",");
                content.Append(s.size.Width.ToString() + "," + s.size.Height.ToString() + ",");
                content.Append(s.reflections.ToString() + ",");
                content.Append("\n");
            }
            return content.ToString();
        }

        private String pointToString(Point p) {
            return null;
        }

        private void getCanvasData() {
           
            strokeData = Controller.ControllerFactory.gazeController.page.getStrokeData();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            width = 600;
            height = 450;
            horizontalOffset = (int)(Window.Current.Bounds.Width - width) / 2;
            verticalOffset = (int)(Window.Current.Bounds.Height - height) / 2;
        }

        private void confirm(object sender, RoutedEventArgs e) { 
        
        }

        private void cancel(object sender, RoutedEventArgs e)
        {

        }

        private void selectSlot1(object sender, RoutedEventArgs e)
        {
            selectedSlot = 1;
            slot1State = "Visible";
            slot2State = "Collapsed";
            slot3State = "Collapsed";
            NotifyPropertyChanged();
        }

        private void selectSlot2(object sender, RoutedEventArgs e)
        {
            selectedSlot = 2;
            slot1State = "Collapsed";
            slot2State = "Visible";
            slot3State = "Collapsed";
            NotifyPropertyChanged();
        }

        private void selectSlot3(object sender, RoutedEventArgs e)
        {
            selectedSlot = 3;
            slot1State = "Collapsed";
            slot2State = "Collapsed";
            slot3State = "Visible";
            NotifyPropertyChanged();
        }
    }
}
