using System.Numerics;

namespace DevoidEngine.Engine.UI
{
    public struct UITransform
    {
        public Vector2 Position;
        public Vector2 Size;

        public UITransform() { }
        public UITransform(Vector2 start, Vector2 end)
        {
            Position = start;
            Size = end;
        }
    }
}