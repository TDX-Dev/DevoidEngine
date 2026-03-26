using System.Numerics;

namespace DevoidEngine.Engine.UI
{
    public class UITransform
    {
        public Vector2 position;
        public Vector2 size;

        public UITransform() { }
        public UITransform(Vector2 start, Vector2 end)
        {
            this.position = start;
            this.size = end;
        }
    }
}