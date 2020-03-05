using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avantgarde.Controller
{
    static class ControllerFactory
    {
        public static avantgarde.Controller.GazeController gazeController { get; set; }
        public static void MakeAController(IDrawMode page)
        {
            gazeController = new GazeController(page);
        }
    }
}
