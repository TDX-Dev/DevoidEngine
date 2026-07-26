using DevoidEngine.Rendering;
using DevoidEngine.UI.UINodes;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    public class UIContext
    {
        public Viewport Viewport = null!;

        public List<CanvasNode> Canvases = [];

        public UINode? Hovered;
        public UINode? Focused;
        public UINode? Pressed;

        internal bool IsDragging;
        internal Vector2 DragStartMouse;

        public Vector2 MousePosition;
        public Vector2 PrevMousePosition;

        public float UIScale = 1.0f;

        public UIDrawList DrawList = new();
    }
}
