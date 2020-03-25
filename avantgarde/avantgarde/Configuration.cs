using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avantgarde
{
    class Configuration
    {
        public static int DrawingDwellTimeMs = 600;
        public static double GazeSnapDistance = 50.0;
        public static int JoystickMoveDistance = 10;
        public static double StokeDuration = 3.0;
        public static long ReplayFPS = 60L;
        public static Fleur fleur;
        public static avantgarde.Menus.UI ui;
    }
}
