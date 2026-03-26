using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public struct RenderItem
    {
        public Mesh Mesh;
        public Matrix4x4 Model;
        public MaterialInstance Material;
        public RenderState RenderState;
    }
}
