using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace avantgarde.Events
{
    public class FileSelectEventArgs : EventArgs
    {
        public String FileName;
        public StorageFolder Folder;
        public FileSelectEventArgs(StorageFolder folder, String fileName)
        {
            Folder = folder;
            FileName = fileName;
        }
    }
}
