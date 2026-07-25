using DevoidEngine.Core;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    public struct UICommand
    {
        public UICommandType Type;

        public QuadCommand Quad;
        public ClipCommand Clip;
    }

    public struct QuadCommand
    {
        public Rect Rect;
        public Vector4 Color;
        public Texture Texture;
    }

    public struct ClipCommand
    {
        public Rect Rect;
    }
}
