using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avantgarde.Utils
{
    public class AGColor
    {
        private static String[,] presetColorHex = new string[13, 11] {
            { "000000", "330000", "660000","990000", "CC0000", "FF0000", "FF3333", "FF6666", "FF9999", "FFCCCC", "FFFFFF"},
            { "000000", "331900", "663300","994C00", "CC6600", "FF8000", "FF9933", "FFB266", "FFCC99", "FFE5CC", "FFFFFF"},
            { "000000", "333300", "666600","999900", "CCCC00", "FFFF00", "FFFF33", "FFFF66", "FFFF99", "FFFFCC", "FFFFFF"},
            { "000000", "193300", "336600","4C9900", "66CC00", "80FF00", "99FF33", "B2FF66", "CCFF99", "E5FFCC", "FFFFFF"},
            { "000000", "003300", "006600","009900", "00CC00", "00FF00", "33FF33", "66FF66", "99FF99", "CCFFCC", "FFFFFF"},
            { "000000", "003319", "006633","00994C", "00CC66", "00FF80", "33FF99", "66FFB2", "99FFCC", "CCFFE5", "FFFFFF"},
            { "000000", "003333", "006666","009999", "00CCCC", "00FFFF", "33FFFF", "66FFFF", "99FFFF", "CCFFFF", "FFFFFF"},
            { "000000", "001933", "003366","004C99", "0066CC", "0080FF", "3399FF", "66B2FF", "99CCFF", "CCE5FF", "FFFFFF"},
            { "000000", "000033", "000066","000099", "0000CC", "0000FF", "3333FF", "6666FF", "9999FF", "CCCCFF", "FFFFFF"},
            { "000000", "190033", "330066","4C0099", "6600CC", "7F00FF", "9933FF", "B266FF", "CC99FF", "E5CCFF", "FFFFFF"},
            { "000000", "330033", "660066","990099", "CC00CC", "FF00FF", "FF33FF", "FF66FF", "FF99FF", "FFCCFF", "FFFFFF"},
            { "000000", "330019", "660033","99004C", "CC0066", "FF007F", "FF3399", "FF66B2", "FF99CC", "FFCCE5", "FFFFFF"},
            { "000000", "000000", "202020","404040", "606060", "808080", "A0A0A0", "C0C0C0", "E0E0E0", "FFFFFF", "FFFFFF"}
        };
        private static String paraToHex(int profile, int brightness, int opacity)
        {
            int newOpacity = (int)(opacity * 0.01 * 255);
            String newOpacityStr = Convert.ToString(newOpacity, 16).ToUpper();

            if (newOpacityStr.Length == 1)
            {
                newOpacityStr = newOpacityStr.Insert(0, "0");
            }
            return newOpacityStr + presetColorHex[profile, brightness];
        }
        private static Windows.UI.Color hexToColor(string hex)
        {
            if (hex.IndexOf('#') != -1)
                hex = hex.Replace("#", "");

            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));

            return Windows.UI.Color.FromArgb(a, r, g, b);
        }
        public static Windows.UI.Color paraToColor(int profile, int brightness, int opacity)
        {
            return hexToColor(paraToHex(profile, brightness, opacity));
        }
        private Windows.UI.Color _color;
        public Windows.UI.Color Color
        {
            get { return _color; }
            private set { _color = value; }
        }
        private String _hex;
        public String Hex
        {
            get { return _hex; }
            private set { _hex = value; }
        }
        private int _profile;
        public int Profile
        {
            get { return _profile; }
            private set { _profile = value; }
        }
        public int _brightness;
        public int Brightness
        {
            get { return _brightness; }
            private set { _brightness = value; }
        }
        private int _opacity;
        public int Opacity
        {
            get { return _opacity; }
            private set { _opacity = value; }
        }
        public AGColor(String hex)
        {
            _hex = hex;
            _color = hexToColor(hex);
        }
        public AGColor(int profile, int brightness, int opacity)
        {
            _hex = paraToHex(profile, brightness, opacity);
            _color = hexToColor(_hex);
            _profile = profile;
            _opacity = opacity;
            _brightness = brightness;
        }
        public static Windows.UI.Color MakeColor(int profile, int brightness, int opacity)
        {
            String hex = paraToHex(profile, brightness, opacity);
            return hexToColor(hex);
        }
        public static Windows.UI.Color MakeColor(String hex)
        {
            return hexToColor(hex);
        }
    }
}
