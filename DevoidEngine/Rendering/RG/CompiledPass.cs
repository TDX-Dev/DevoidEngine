namespace DevoidEngine.Rendering.RG
{
    internal sealed class CompiledPass
    {
        public required RenderPass Pass;

        public List<RenderGraphBarrier> Barriers = [];
    }
}
