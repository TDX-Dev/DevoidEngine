using DevoidEngine.Core;
using DevoidEngine.UI.Theme.Styleboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI.Theme
{
    class ThemeTypeData
    {
        public Dictionary<string, Vector4> Colors = [];
        // Painful to look at.
        public Dictionary<string, object> Constants = [];
        public Dictionary<string, string> Fonts = [];
        public Dictionary<string, int> FontSizes = [];
        public Dictionary<string, Texture> Icons = [];
        public Dictionary<string, StyleBox> StyleBoxes = [];
    }
}
