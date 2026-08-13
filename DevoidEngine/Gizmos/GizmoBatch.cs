using DevoidEngine.Rendering;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoBatch
    {
        public GizmoBatchState State;

        public readonly List<RenderMeshData> RenderData = [];

        public void Reset()
        {
            State = default;
            RenderData.Clear();
        }
    }
}
