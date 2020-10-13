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


namespace avantgarde.Menus
{
    //Mechanism to ensure user's intentions before executing a potentially destructive action
    //Eduardo Battistini
    public sealed partial class ConfirmTool : UserControl, INotifyPropertyChanged
    {
        private int width { get; set; }
        private int height { get; set; }
        private int horizontalOffset { get; set; }
        private int verticalOffset { get; set; }
        public String message { get; set; }

        public bool decision = true;

        public EventHandler confirmDecisionMade;

        public event PropertyChangedEventHandler PropertyChanged;

        public ConfirmTool()
        {
            message = "Are you sure?";
            getWindowAttributes();
            this.InitializeComponent();
        }
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes() {
            width = 400;
            height = 250;
            horizontalOffset = (int)(Window.Current.Bounds.Width - width) / 2;
            verticalOffset = (int)(Window.Current.Bounds.Height - height) / 2;
        }

       

        public bool isOpen()
        {
            return confirmTool.IsOpen;
        }

        public void setMessage(String s) {
            message = s;
            NotifyPropertyChanged();
        }

        public void openConfirmTool()
        {
            if (!confirmTool.IsOpen) { confirmTool.IsOpen = true; }
        }

        public void closeConfirmTool()
        {
            if (confirmTool.IsOpen) { confirmTool.IsOpen = false; }
        }

        private void reject(object sender, RoutedEventArgs e)
        {
            decision = false;
            closeConfirmTool();
            confirmDecisionMade?.Invoke(this, EventArgs.Empty);
        }

        private void confirm(object sender, RoutedEventArgs e)
        {
            decision = true;
            closeConfirmTool();
            confirmDecisionMade?.Invoke(this, EventArgs.Empty);
        }
    }
}
