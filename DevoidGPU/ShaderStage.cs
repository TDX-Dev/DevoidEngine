namespace DevoidGPU
{
    [Flags]
    public enum ShaderStage
    {
        None = 0,

        Vertex = 1 << 0,
        Fragment = 1 << 1,
        Geometry = 1 << 2,
        Compute = 1 << 3,
    }
}
