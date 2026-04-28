namespace DevoidGPU
{
    public struct IndexBufferDescription
    {
        public ulong Size;
        public IndexFormat Format;
        public BufferUsage Usage;
        public IntPtr InitialData;
    }
}
