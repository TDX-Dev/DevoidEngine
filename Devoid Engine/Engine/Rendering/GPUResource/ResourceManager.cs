using DevoidEngine.Engine.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class ResourceManager
    {
        public TextureManager TextureManager { get; private set; } = new TextureManager();
        public SamplerManager SamplerManager { get; private set; } = new SamplerManager();
        public VertexBufferManager VertexBufferManager { get; private set; } = new VertexBufferManager();
        public IndexBufferManager IndexBufferManager { get; private set; } = new IndexBufferManager();
        public FramebufferManager FramebufferManager { get; private set; } = new FramebufferManager();

        public GraphicsFence CreateFence()
        {
            var fence = new GraphicsFence();

            RenderThread.Enqueue(() =>
            {
                fence.Signal();
            });

            return fence;
        }
    }
}
