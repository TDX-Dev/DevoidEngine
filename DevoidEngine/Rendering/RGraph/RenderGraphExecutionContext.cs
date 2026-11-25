using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph
{
    public readonly struct RenderGraphExecutionContext
    {
        private readonly RenderGraph _graph;

        public ICommandList CommandList { get; }

        public Viewport Viewport { get; }

        public RenderGraphExecutionContext(RenderGraph graph, ICommandList commandList, Viewport viewport)
        {
            _graph = graph;
            CommandList = commandList;
            Viewport = viewport;
        }

        public readonly Texture GetTexture(RID handle)
        {
            return _graph.GetTexture(handle);
        }
    }
}
