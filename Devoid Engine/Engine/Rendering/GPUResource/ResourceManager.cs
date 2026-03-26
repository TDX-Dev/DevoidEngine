using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class ResourceManager
    {
        public TextureManager TextureManager { get; private set; } = new TextureManager();
        public SamplerManager SamplerManager { get; private set; } = new SamplerManager();
        public VertexBufferManager VertexBufferManager { get; private set; } = new VertexBufferManager();
        public IndexBufferManager IndexBufferManager { get; private set; } = new IndexBufferManager();
        public FramebufferManager FramebufferManager { get; private set; } = new FramebufferManager();
    }
}