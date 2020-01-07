using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input.Inking;
using RoutedEventArgs = Windows.UI.Xaml.RoutedEventArgs;
using Windows.UI.Xaml.Shapes;
using Windows.Devices.Input;
using Windows.UI.Input.Inking.Analysis;




// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Libre : Page
    {
        private Stack<InkStroke> redoStack = new Stack<InkStroke>();
        private Point iniP;
        public Libre()
        {
            this.InitializeComponent();
            inkCanvas.InkPresenter.InputDeviceTypes =
                Windows.UI.Core.CoreInputDeviceTypes.Mouse |
                Windows.UI.Core.CoreInputDeviceTypes.Pen |
                Windows.UI.Core.CoreInputDeviceTypes.Touch;
        }
        private async void Redo_Button_Click(object sender, RoutedEventArgs e)
        {
            if (redoStack.Count() == 0)
            {
                return;
            }
            inkCanvas.InkPresenter.StrokeContainer.AddStroke(redoStack.Pop());
        }
        private async void UndoOne_Button_Click(object sender, RoutedEventArgs e)
        {
            int containerSize = 0;
            var strokes = inkCanvas.InkPresenter.StrokeContainer.GetStrokes();
            foreach (var stroke in strokes)
            {
                containerSize++;
            }
            int index = 0;
            foreach (var s in strokes)
            {
                index++;
                if (index == containerSize)
                {
                    s.Selected = true;
                    redoStack.Push(s.Clone());
                }
            }
            inkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }

        private void Shape_Button_Click(object sender, MouseEventArgs e)
        {
            
            if (shape_popup.IsOpen == false)
            {
                shape_popup.IsOpen = true;
            }
            else
            {
                shape_popup.IsOpen = false;
            }
        }

        private Point startP = new Point(50, 100);
        private Point endP = new Point(100, 200);

        private void Circle_Button_Click(object sender, MouseEventArgs e)
        {
            Ellipse ellipse = new Ellipse()
            {
                Height = 10,
                Width = 10
            };
            if (endP.X >= startP.X)
            {
                ellipse.SetValue(Canvas.LeftProperty, startP.X);
                ellipse.Width = endP.X - startP.X;
            }
            else
            {
                ellipse.SetValue(Canvas.LeftProperty, startP.X);
                ellipse.Width = startP.X - endP.X;
            }
            if (endP.Y >= startP.Y)
            {
                ellipse.SetValue(Canvas.TopProperty, startP.Y);
                ellipse.Height = endP.Y - startP.Y;
            }
            else
            {
                ellipse.SetValue(Canvas.TopProperty, startP.Y);
                ellipse.Height = startP.Y - endP.Y;
            }
            Canvas canvas = new Canvas();
            canvas.Children.Add(ellipse);
            

        }

        private void Square_Button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Triangle_Button_Click(object sender, RoutedEventArgs e)
        {
            Ellipse aaaa = new Ellipse();
           
            Point p = new Point(0, 1);

        }






    }
    
}
