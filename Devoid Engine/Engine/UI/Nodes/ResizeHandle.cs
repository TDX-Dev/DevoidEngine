using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.UI.Nodes
{
    class ResizeHandle : ContainerNode
    {
        public Action<Vector2> OnResize;

        public override void OnDrag(Vector2 mouse, Vector2 delta)
        {
            OnResize?.Invoke(delta);
        }
    }
}
