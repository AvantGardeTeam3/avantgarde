using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Input.Inking;

using avantgarde.Drawing;
using avantgarde.Menus;

namespace avantgarde
{
    public class StrokeData
    {
        public Point p0;
        public Point p1;
        public Point p2;
        public Point p3;
        public Point midpoint;
        public Point halfpoint;
        public int ColorProfile;
        public int Brightness;
        public int Opactiy;
        public bool modified;
        public String brush;
        public Size size;
        public int reflections;

        // add bezier curve constructor. 
        // create list only when loading and saving from the list of curves 
        //
    }
}
