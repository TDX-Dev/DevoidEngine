namespace DevoidEngine.Rendering.RG
{
    public readonly record struct PassDependency(RenderGraphHandle Resource, RenderGraphAccessType AccessType);
}
