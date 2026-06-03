using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public class RenderMeshData
    {
        public Mesh render_mesh;
        public Matrix4x4 render_transform;

        public RenderMeshData(Mesh mesh)
        {
            render_mesh = mesh;
        }
    }
}
