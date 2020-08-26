using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace avantgarde.Utils
{
    class File
    {
        public static StorageFolder LocalFolder = ApplicationData.Current.LocalFolder;
        public async Task SaveAsync(Utils.Save save, String fileName)
        {
            StorageFile file = await LocalFolder.CreateFileAsync(fileName,
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, save.ToString());
            Saved(this, EventArgs.Empty);
        }
        public async void LoadAsync(String fileName)
        {
            StorageFile file = await LocalFolder.GetFileAsync(fileName);
            var content = await FileIO.ReadTextAsync(file);
            Utils.Save save = new Utils.Save(content);
            Loaded(this, new Events.LoadEventArgs(save));
        }
        public EventHandler<EventArgs> Saved;
        public EventHandler<Events.LoadEventArgs> Loaded;
    }
}