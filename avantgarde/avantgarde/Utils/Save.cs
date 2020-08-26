using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace avantgarde.Utils
{
    class Save
    {
        public AGColor BackgroundColor { get; private set; }
        public AGColor[] ColorPalettes { get; private set; }
        public List<StrokeData> Strokes { get; private set; }
        public Save(AGColor background, AGColor[] colorPalettes, List<StrokeData> strokes)
        {
            // need to clone instead of reference!!!
            BackgroundColor = background;
            ColorPalettes = new AGColor[ColorPalettes.Length];
            for(int i = 0; i < colorPalettes.Length; i++)
            {
                ColorPalettes[i] = colorPalettes[i];
            }
            Strokes = strokes;
        }
        public Save(String content)
        {
            string[] lines = content.Split("\n");

            // load the background color from the 1st line

            string[] backgroundColor_vals = lines[0].Split(",");
            int profile     = Int32.Parse(backgroundColor_vals[0]);
            int brightness  = Int32.Parse(backgroundColor_vals[1]);
            int opacity     = Int32.Parse(backgroundColor_vals[2]);
            BackgroundColor = new AGColor(profile, brightness, opacity);

            // load the color palettes from the second line
            string[] colorPalette_vals = lines[1].Split(",");
            ColorPalettes = new AGColor[5];
            for(int i = 0; i < 5; i++)
            {
                profile = Int32.Parse(colorPalette_vals[i * 3]);
                brightness = Int32.Parse(colorPalette_vals[i * 3 + 1]);
                opacity = Int32.Parse(colorPalette_vals[i * 3 + 2]);
                ColorPalettes[i] = new AGColor(profile, brightness, opacity);
            }

            // load the strokes from the following lines
            Strokes = new List<StrokeData>();
            for(int i = 2; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] vals = line.Split(",");
                if (line.Length <= 1) continue;
                StrokeData stroke = new StrokeData();
                stroke.p0 = new Point(Double.Parse(vals[0] + ".0"), Double.Parse(vals[1] + ".0"));
                stroke.p1 = new Point(Double.Parse(vals[2] + ".0"), Double.Parse(vals[3] + ".0"));
                stroke.p2 = new Point(Double.Parse(vals[4] + ".0"), Double.Parse(vals[5] + ".0"));
                stroke.p3 = new Point(Double.Parse(vals[6] + ".0"), Double.Parse(vals[7] + ".0"));
                stroke.midpoint = new Point(Double.Parse(vals[8] + ".0"), Double.Parse(vals[9] + ".0"));
                stroke.halfpoint = new Point(Double.Parse(vals[10] + ".0"), Double.Parse(vals[11] + ".0"));
                stroke.size = new Size(Double.Parse(vals[12] + ".0"), Double.Parse(vals[13] + ".0"));
                stroke.modified = "True" == vals[14];
                stroke.ColorProfile = Int32.Parse(vals[15]);
                stroke.Brightness = Int32.Parse(vals[16]);
                stroke.Opactiy = Int32.Parse(vals[17]);
                stroke.brush = vals[18];
                stroke.reflections = Int32.Parse(vals[19]);
                Strokes.Add(stroke);
            }
        }
        public override String ToString()
        {
            StringBuilder content = new StringBuilder();

            // stores the background color in the 1st line
            content.Append(BackgroundColor.Profile + "," + BackgroundColor.Brightness + "," + BackgroundColor.Opacity + "\n");

            // stores the color palette in the 2nd line
            for(int i = 0; i < 5; i++)
            {
                content.Append(ColorPalettes[i].Profile + ",");
                content.Append(ColorPalettes[i].Brightness + ",");
                content.Append(ColorPalettes[i].Opacity + ",");
            }
            content.Length--; // remove the last comma
            content.Append("\n");

            // stores the strokes in the following lines
            foreach(StrokeData stroke in Strokes)
            {
                content.Append(Convert.ToInt32(stroke.p0.X).ToString() + "," + Convert.ToInt32(stroke.p0.Y).ToString() + ",");
                content.Append(Convert.ToInt32(stroke.p1.X).ToString() + "," + Convert.ToInt32(stroke.p1.Y).ToString() + ",");
                content.Append(Convert.ToInt32(stroke.p2.X).ToString() + "," + Convert.ToInt32(stroke.p2.Y).ToString() + ",");
                content.Append(Convert.ToInt32(stroke.p3.X).ToString() + "," + Convert.ToInt32(stroke.p3.Y).ToString() + ",");
                content.Append(Convert.ToInt32(stroke.midpoint.X).ToString() + "," + Convert.ToInt32(stroke.midpoint.Y).ToString() + ",");
                content.Append(Convert.ToInt32(stroke.halfpoint.X).ToString() + "," + Convert.ToInt32(stroke.halfpoint.Y).ToString() + ",");
                content.Append(Convert.ToInt32(stroke.size.Width).ToString() + "," + Convert.ToInt32(stroke.size.Height).ToString() + ",");
                content.Append(stroke.modified.ToString() + ",");
                content.Append(stroke.ColorProfile.ToString() + ",");
                content.Append(stroke.Brightness.ToString() + ",");
                content.Append(stroke.Opactiy.ToString() + ",");
                content.Append(stroke.brush + ",");
                content.Append(stroke.reflections.ToString());
                content.Append("\n");
            }

            return content.ToString();
        }    
    }
}
