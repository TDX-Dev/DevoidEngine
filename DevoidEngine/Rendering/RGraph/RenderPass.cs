namespace DevoidEngine.Rendering.RGraph
{
    public class RenderPass
    {
        public string Name { get; protected set; } = "N/A";
        public virtual void CreateResources(RenderGraphBuilder builder) { }
        public virtual void Setup(RenderGraphContext ctx, RenderGraphBuilder builder) { }
        public virtual void Execute(RenderGraphExecutionContext context) { }
    }
}
