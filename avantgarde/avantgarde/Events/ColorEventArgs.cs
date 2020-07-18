using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avantgarde.Events
{
    public class ColorEventArgs : EventArgs
    {
        public Utils.AGColor Color { get; set; }

        public ColorEventArgs(Utils.AGColor color)
        {
            this.Color = color;
        }
    }
}
