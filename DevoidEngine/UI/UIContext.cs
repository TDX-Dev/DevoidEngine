using DevoidEngine.UI.UINodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.UI
{
    public class UIContext
    {
        public List<CanvasNode> Canvases = [];

        public UINode? Hovered;
        public UINode? Focused;
        public UINode? Pressed;

        public Vector2 MousePosition;

        public float UIScale = 1.0f;


    }
}
