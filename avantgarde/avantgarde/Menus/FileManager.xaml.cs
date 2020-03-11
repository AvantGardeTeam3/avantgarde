using avantgarde.Drawing;
using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace avantgarde.Menus
{
    public sealed partial class FileManager : UserControl, INotifyPropertyChanged
    {
        public int selectedSlot = 0;

        private Fleur page;
        private List<StrokeData> strokeData;
        private List<BezierCurve> curveData;

        public static int SAVING = 0;
        public static int LOADING = 1;

        public int mode = 0;
        private bool presetsLoaded;

        StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        public int[,] colourPalette = new int[5,3];
        private String slot1State { get; set; }
        private String slot2State { get; set; }
        private String slot3State { get; set; }
        public String title { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int horizontalOffset { get; set; }
        public int verticalOffset { get; set; }

        private string newBackground;

        private string imgSrc1 { get; set; }
        private string imgSrc2 { get; set; }
        private string imgSrc3 { get; set; }


        public FileManager()
        {
            //loadPresets();
            imgSrc1 = "";
            imgSrc2 = "";
            imgSrc3 = "";
            title = "";
            selectedSlot = 1;
            slot1State = "Visible";
            slot2State = "Collapsed";
            slot3State = "Collapsed";
            getWindowAttributes();
            this.InitializeComponent();

        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void getWindowAttributes()
        {
            width = 750;
            height = 550;
            horizontalOffset = (int)(Window.Current.Bounds.Width - width) / 2;
            verticalOffset = (int)(Window.Current.Bounds.Height - height) / 2;
        }



        private String serialize()
        {
            StringBuilder content = new StringBuilder();

            var bg = Controller.ControllerFactory.gazeController.colourManager.backgroundSelection;
            content.Append(bg.ToString() + "\n");

            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 3; j++) {
                    content.Append(colourPalette[i,j].ToString() + ",");
                }
            }
            content.Length--;
            content.Append("\n");


            foreach (StrokeData s in strokeData) {
                content.Append(s.p0.X.ToString() + "," + s.p0.Y.ToString()+",");
                content.Append(s.p1.X.ToString() + "," + s.p1.Y.ToString() + ",");
                content.Append(s.p2.X.ToString() + "," + s.p2.Y.ToString() + ",");
                content.Append(s.p3.X.ToString() + "," + s.p3.Y.ToString() + ",");
                content.Append(s.midpoint.X.ToString() + "," + s.midpoint.Y.ToString() + ",");
                content.Append(s.halfpoint.X.ToString() + "," + s.halfpoint.Y.ToString() + ",");
                content.Append(s.size.Width.ToString() + "," + s.size.Height.ToString() + ",");
                content.Append(s.modified.ToString() + ",");
                content.Append(s.colourProfile.ToString() + ",");
                content.Append(s.brightness.ToString() + ",");
                content.Append(s.opacity.ToString() + ",");
                content.Append(s.brush + ",");
                content.Append(s.reflections.ToString());
                content.Append("\n");
            }
            
            return content.ToString();
        }

        private List<StrokeData> deserialize(String content) {
            List<StrokeData> newData = new List<StrokeData>();

            string[] lines = content.Split("\n");
            StrokeData data;

            bool first = true;

            foreach (var line in lines)
            {
                if (first) {
                    newBackground = line.Remove(line.Length - 1);
                    first = false;
                    continue;
                }
                string[] vals = line.Split(",");
                data = new StrokeData();
                data.p0 = new Point(Double.Parse(vals[0]), Double.Parse(vals[1]));
                data.p1 = new Point(Double.Parse(vals[2]), Double.Parse(vals[3]));
                data.p2 = new Point(Double.Parse(vals[4]), Double.Parse(vals[5]));
                data.p3 = new Point(Double.Parse(vals[6]), Double.Parse(vals[7]));
                data.midpoint = new Point(Double.Parse(vals[8]), Double.Parse(vals[9]));
                data.halfpoint = new Point(Double.Parse(vals[10]), Double.Parse(vals[11]));
                data.size = new Size(Double.Parse(vals[12]), Double.Parse(vals[13]));
                data.modified = Boolean.Parse(vals[14]);
                data.colourProfile = Int32.Parse(vals[15]);
                data.brightness = Int32.Parse(vals[16]);
                data.opacity = Int32.Parse(vals[17]);
                data.brush = vals[18];
                data.reflections = Int32.Parse(vals[19]);

                newData.Add(data);
            }


            return newData;
        }

        private async Task saveFileAsync() {
            StorageFile file = await localFolder.CreateFileAsync("slot" + selectedSlot.ToString() + ".txt",
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, serialize());

        }

        private void getCanvasData() {
            strokeData = Fleur.getStrokeData();
            curveData = Controller.ControllerFactory.gazeController.drawingModel.getCurves();

            if (strokeData.Count != curveData.Count) {
                throw new Exception("Saved Data Corrupted");
            }

            StrokeData tmpSD;
            BezierCurve tmpBC;

            for (int i = 0; i < strokeData.Count; i++) {
                tmpSD = strokeData[i];
                tmpBC = curveData[i];

                tmpSD.p0 = tmpBC.P0;
                tmpSD.p1 = tmpBC.P1;
                tmpSD.p2 = tmpBC.P2;
                tmpSD.p3 = tmpBC.P3;
                tmpSD.midpoint = tmpBC.MidPoint;
                tmpSD.halfpoint = new Point(10, 10); // TO DO
                tmpSD.modified = tmpBC.Modified;
            }

        }

        public async void save() {
            getCanvasData();
            await saveFileAsync();
            await saveImageAsync();
        }

        private async Task saveImageAsync() {
            InkCanvas inkCanvas = Controller.ControllerFactory.gazeController.inkCanvas;
            Color background = Controller.ControllerFactory.gazeController.colourManager.backgroundSelection;

            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasRenderTarget renderTarget = new CanvasRenderTarget(device, (int)inkCanvas.ActualWidth, (int)inkCanvas.ActualHeight, 96);

            StorageFile file = await localFolder.CreateFileAsync("img" + selectedSlot.ToString() + ".jpg", CreationCollisionOption.ReplaceExisting);
            using (var ds = renderTarget.CreateDrawingSession())
            {
                ds.Clear(background);
                ds.DrawInk(inkCanvas.InkPresenter.StrokeContainer.GetStrokes());
            }
            if (file != null)
            {
                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await renderTarget.SaveAsync(fileStream, CanvasBitmapFileFormat.Jpeg, 1f);
                }


            }

            if (selectedSlot == 1)
            {
                imgSrc1 = file.Path;
            }
            else if (selectedSlot == 2) {

                imgSrc2 = file.Path;
            }
            else if (selectedSlot == 3)
            {

                imgSrc3 = file.Path;
            }

            NotifyPropertyChanged();

        }


        private async void loadPresets()
        {
            checkSlotsEmpty();
            if (!presetsLoaded) {
                for (int i = 1; i < 4; i++)
                {
                    selectedSlot = i;
                    await loadFileAsync(true);
                    // generate canvas from file 
                    save();
                }
            }
           
        }

        private async void checkSlotsEmpty() {
            var slot1 = await ApplicationData.Current.LocalFolder.TryGetItemAsync("slot1.txt");
            var slot2 = await ApplicationData.Current.LocalFolder.TryGetItemAsync("slot2.txt");
            var slot3 = await ApplicationData.Current.LocalFolder.TryGetItemAsync("slot3.txt");
            var img1 = await ApplicationData.Current.LocalFolder.TryGetItemAsync("img1.jpg");
            var img2 = await ApplicationData.Current.LocalFolder.TryGetItemAsync("img2.jpg");
            var img3 = await ApplicationData.Current.LocalFolder.TryGetItemAsync("img3.jpg");

            presetsLoaded = (slot1 != null) && (slot2 != null) && (slot3 != null) 
                && (img1 != null) && (img2 != null) && (img3 != null);

        }

        public async Task loadFileAsync(bool fromPresets) {
            
            StorageFile file;

            if (fromPresets)
            {
                file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\slot" + selectedSlot.ToString() + ".txt");
            }
            else 
            {
                file = await localFolder.GetFileAsync("slot" + selectedSlot.ToString() + ".txt");
            }

           
            var content = await FileIO.ReadTextAsync(file);
            strokeData = deserialize(content);
        }

        public async void load() {
            await loadFileAsync(false);
            //generate new Canvas from updated strokeData
        }

        public void open(int m) {
            mode = m;
            if (mode == SAVING)
            {
                title = "Save Canvas...";
            }
            else if (mode == LOADING)
            {
                title = "Load Canvas...";
            }
            NotifyPropertyChanged();
            if (!FileManagerMenu.IsOpen) { FileManagerMenu.IsOpen = true; }

        }

        public event EventHandler loadRequested;
        public event EventHandler saveRequested;

        private void confirm(object sender, RoutedEventArgs e) {
            if (mode == SAVING)
            {
                saveRequested?.Invoke(this, EventArgs.Empty);
            }
            else if (mode == LOADING)
            {
                loadRequested?.Invoke(this, EventArgs.Empty);
            }
            if (FileManagerMenu.IsOpen) { FileManagerMenu.IsOpen = false; }
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            close();
        }

        public void close() {
            if (FileManagerMenu.IsOpen) { FileManagerMenu.IsOpen = false; }
        }

        private void selectSlot1(object sender, RoutedEventArgs e)
        {
            selectedSlot = 1;
            slot1State = "Visible";
            slot2State = "Collapsed";
            slot3State = "Collapsed";
            
            NotifyPropertyChanged();
        }

        private void selectSlot2(object sender, RoutedEventArgs e)
        {
            selectedSlot = 2;
            slot1State = "Collapsed";
            slot2State = "Visible";
            slot3State = "Collapsed";
            
            NotifyPropertyChanged();
        }

        private void selectSlot3(object sender, RoutedEventArgs e)
        {
            selectedSlot = 3;
            slot1State = "Collapsed";
            slot2State = "Collapsed";
            slot3State = "Visible";
            
            NotifyPropertyChanged();
        }
    }
}
