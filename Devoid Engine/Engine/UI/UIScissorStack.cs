using System.Numerics;

namespace DevoidEngine.Engine.UI
{

    public static class UIScissorStack
    {
        static Stack<Vector4> stack = new();

        public static bool HasClip => stack.Count > 0;

        public static Vector4 Current => stack.Peek();

        public static void Push(float x, float y, float width, float height)
        {
            // Convert from UI top-left origin → GPU bottom-left origin
            //float gpuY = Renderer.Height - (y + height);

            Vector4 rect = new Vector4(x, y, width, height);

            if (stack.Count > 0)
            {
                rect = Intersect(rect, stack.Peek());
            }

            stack.Push(rect);

            //DebugRenderSystem.DrawRectUI(
            //    UISystem.BuildModel(new UITransform(new Vector2(rect.X, rect.Y), new Vector2(rect.Z, rect.W)))
            //);
        }

        public static void Pop()
        {
            if (stack.Count > 0)
                stack.Pop();
        }

        static Vector4 Intersect(Vector4 a, Vector4 b)
        {
            float ax1 = a.X;
            float ay1 = a.Y;
            float ax2 = a.X + a.Z;
            float ay2 = a.Y + a.W;

            float bx1 = b.X;
            float by1 = b.Y;
            float bx2 = b.X + b.Z;
            float by2 = b.Y + b.W;

            float x1 = MathF.Max(ax1, bx1);
            float y1 = MathF.Max(ay1, by1);
            float x2 = MathF.Min(ax2, bx2);
            float y2 = MathF.Min(ay2, by2);

            if (x2 <= x1 || y2 <= y1)
                return Vector4.Zero;

            return new Vector4(
                x1,
                y1,
                x2 - x1,
                y2 - y1
            );
        }
    }
}