using DevoidGPU;

namespace DevoidEngine.Rendering.RG
{
    public abstract class RenderPass
    {
        public string Name { get; }

        public RenderPass(string name)
        {
            Name = name;
        }

        public virtual void Setup(RenderGraphBuilder builder) { }
        public virtual void Execute(ICommandList cmd, RenderContext context) { }
        public virtual bool HasSideEffect() => false;
    }
}
