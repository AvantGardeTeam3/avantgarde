using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avantgarde.Controller
{
    static class ControllerFactory
    {
        private static avantgarde.Controller.GazeController _gazeController = null;
        public static avantgarde.Controller.GazeController gazeController
        {
            get { return _gazeController; }
            set { _gazeController = value; }
        }
        public static void MakeAController(IDrawMode page)
        {
            _gazeController = new GazeController(page);
        }
    }
}
