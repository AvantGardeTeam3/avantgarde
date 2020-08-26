using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avantgarde.Events
{
    class LoadEventArgs : EventArgs
    {
        public Utils.Save Save;
        public LoadEventArgs(Utils.Save save)
        {
            this.Save = save;
        }
    }
}
