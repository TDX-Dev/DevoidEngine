namespace DevoidGPU
{
    [Flags]
    public enum BufferBind
    {
        None = 0,

        Vertex = 1 << 0,
        Index = 1 << 1,
        Uniform = 1 << 2,

        Storage = 1 << 3,
        StorageWritable = 1 << 4,

        Indirect = 1 << 5,
        TransferSrc = 1 << 6,
        TransferDst = 1 << 7
    }
}
