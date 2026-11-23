using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph
{
    public class RenderGraphBuilder
    {
        public int RenderPassIndexCurrent { get; set; } = 0;

        private readonly RenderGraph graph;

        public RenderGraphBuilder(RenderGraph _graph)
        {
            graph = _graph;
        }

        public RID CreateTexture(string name, TextureDescription textureDescription)
        {
            return graph.CreateTexture(name, textureDescription);
        }

        public RID? FindTexture(string name)
        {
            return graph.FindTexture(name);
        }

        public void AddTextureDependency(RID handle, RGResourceAccessType accessType)
        {
            graph.AddTextureDependency(handle, RenderPassIndexCurrent, accessType);
        }
    }
}
