using DevoidEngine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public struct RenderView
    {
        public Camera Camera = null!;

        public List<RenderMeshData> Objects;

        //public List<RenderLight> Lights;

        public RenderView()
        {
            Objects = [];
        }
    }
}
