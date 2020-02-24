using avantgarde.Menus;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input.Preview;
using Windows.UI.Xaml.Controls;

namespace avantgarde
{
    interface IDrawMode
    {
        UI GetUI();
        InkCanvas GetInkCanvas();
        RadialProgressBar GetRadialProgressBar();
        DrawingModel GetDrawingModel();
        GazeInputSourcePreview GetGazeInputSourcePreview();
        Canvas GetCanvas();
            
    }
}
