using DevoidEngine.Rendering;
using DevoidEngine.UI.UINodes;
using System.Numerics;

namespace DevoidEngine.UI
{
    public class UIContext
    {
        public Viewport Viewport = null!;

        public List<CanvasNode> Canvases = [];
        internal readonly HashSet<UINode> DirtyLayoutRoots = [];

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
