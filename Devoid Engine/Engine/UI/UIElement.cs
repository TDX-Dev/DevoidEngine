using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI.Nodes;
using System.Numerics;

namespace DevoidEngine.Engine.UI
{
    public abstract class UIElement
    {
        public UITransform Rect;
        public bool Visible = true;
        public bool Interactable = true;

        public UINode Root;

        public virtual void Setup()
        {

        }

        public virtual void Update()
        {
            Root.Measure(new Vector2(Screen.Size.X, Screen.Size.Y));
            Root.Arrange(new UITransform(Vector2.Zero, Screen.Size));
        }
    }
}