namespace DevoidGPU
{
    [Flags]
    public enum TextureUsage
    {
        ShaderResource = 1 << 0,
        RenderTarget = 1 << 1,
        DepthStencil = 1 << 2,
        UnorderedAccess = 1 << 3
    }
}
