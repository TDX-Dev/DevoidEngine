using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.UI.Theme.Styleboxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Theme
{
    class ThemeTypeData
    {
        public Dictionary<string, Vector4> Colors = new();
        // Painful to look at.
        public Dictionary<string, object> Constants = new();
        public Dictionary<string, FontInternal> Fonts = new();
        public Dictionary<string, int> FontSizes = new();
        public Dictionary<string, Texture2D> Icons = new();
        public Dictionary<string, StyleBox> StyleBoxes = new();
    }
}
