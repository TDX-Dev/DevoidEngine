using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public class RenderState
    {
        public BlendMode BlendMode { get; set; } = BlendMode.Opaque;
        public DepthTest DepthTest { get; set; } = DepthTest.LessEqual;
        public bool DepthWrite { get; set; } = true;
        public CullMode CullMode { get; set; } = CullMode.Back;
        public FillMode FillMode { get; set; } = FillMode.Solid;
        public PrimitiveType PrimitiveType { get; set; } = PrimitiveType.Triangles;

        public static RenderState DefaultRenderState => new RenderState()
        {
            BlendMode = BlendMode.Opaque,
            DepthTest = DepthTest.LessEqual,
            DepthWrite = true,
            CullMode = CullMode.Back,
            FillMode = FillMode.Wireframe,
            PrimitiveType = PrimitiveType.Triangles,
        };
    }
}
