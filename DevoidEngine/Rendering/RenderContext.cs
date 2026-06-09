using DevoidEngine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public struct RenderContext
    {
        public Renderer Renderer;
        public Viewport Viewport;
        public Camera Camera;

        public RenderResourceCache Resources;

        public ICommandList CommandList;
    }
}
