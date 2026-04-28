namespace DevoidGPU
{
    [Flags]
    public enum BufferUsage
    {
        Vertex = 1 << 0,
        Index = 1 << 1,
        Uniform = 1 << 2,
        Storage = 1 << 3,

        Dynamic = 1 << 4,
        Staging = 1 << 5
    }
}
