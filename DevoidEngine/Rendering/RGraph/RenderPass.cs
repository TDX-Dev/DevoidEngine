using DevoidGPU;

namespace DevoidEngine.Rendering.RGraph
{
    public class RenderPass
    {
        public string Name { get; protected set; } = "N/A";
        public virtual void CreateResources(RenderGraphBuilder builder) { }
        public virtual void Setup(RenderGraphBuilder builder) { }
        public virtual void Execute(ICommandList cmd) { }

    }
}
