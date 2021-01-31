using System;
using System.Collections.Generic;
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
    public sealed partial class ExportTypeTool : UserControl
   {
   


        private int width { get; set; }
        private int height { get; set; }
        private int horizontalOffset { get; set; }
        private int verticalOffset { get; set; }
        public String message { get; set; }

        public EventHandler confirmExportType;

        public bool isSquare;
        public bool cancelled;

       
        public ExportTypeTool()
        {
            message = "Would you like to export the canvas as a \n square or rectangle?";
            getWindowAttributes();
            isSquare = true;
            cancelled = false;
            this.InitializeComponent();
        }

        private void getWindowAttributes()
        {
            width = 500;
            height = 250;
            horizontalOffset = (int)(Window.Current.Bounds.Width - width) / 2;
            verticalOffset = (int)(Window.Current.Bounds.Height - height) / 2;
        }



        public bool isOpen()
        {
            return exportTypeTool.IsOpen;
        }


        public void openExportTypeTool()
        {
            if (!exportTypeTool.IsOpen) { exportTypeTool.IsOpen = true; }
        }

        public void closeExportTypeTool()
        {
            if (exportTypeTool.IsOpen) { exportTypeTool.IsOpen = false; }
        }


        private void square(object sender, RoutedEventArgs e)
        {
            isSquare = true;
            closeExportTypeTool();
            confirmExportType?.Invoke(this, EventArgs.Empty);
        }

        private void rectangle(object sender, RoutedEventArgs e)
        {
            isSquare = false;
            closeExportTypeTool();
            confirmExportType?.Invoke(this, EventArgs.Empty);
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            cancelled = true;
            confirmExportType?.Invoke(this, EventArgs.Empty);
            closeExportTypeTool();
        }


    }
}
