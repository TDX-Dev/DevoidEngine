using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Nodes
{
    public class WindowNode : ContainerNode
    {
        ResizeHandle resize;

        public WindowNode()
        {
            resize = new ResizeHandle();

            resize.Size = new Vector2(10, 10);
            resize.Offset = new Vector2(-10, -10); // bottom right
            resize.Pivot = new Vector2(1, 1);

            resize.OnResize = Resize;

            Add(resize);
        }

        void Resize(Vector2 delta)
        {
            Size += delta;

            Size = new Vector2(
                MathF.Max(Size?.X ?? 0, 100),
                MathF.Max(Size?.Y ?? 0, 80)
            );
        }
    }
}
