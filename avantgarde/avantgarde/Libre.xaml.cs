using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace avantgarde
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Libre : Page
    {
        private Stack<InkStroke> redoStack = new Stack<InkStroke>();
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
    }
    
}
