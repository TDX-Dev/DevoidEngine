namespace DevoidEngine.Rendering.RG
{
    internal readonly record struct RenderGraphBarrier(RenderGraphHandle Resource, RenderGraphAccessType PreviousAccess, RenderGraphAccessType CurrentAccess);
}
