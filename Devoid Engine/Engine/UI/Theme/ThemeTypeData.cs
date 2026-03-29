using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DevoidEngine.Engine.UI.Theme
{
    class ThemeTypeData
    {
        public Dictionary<string, Vector4> Colors = new();
        public Dictionary<string, int> Constants = new();
        public Dictionary<string, Font> Fonts = new();
        public Dictionary<string, int> FontSizes = new();
        public Dictionary<string, Texture2D> Icons = new();
        public Dictionary<string, StyleBox> StyleBoxes = new();
    }
}
