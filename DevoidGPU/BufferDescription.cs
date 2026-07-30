namespace DevoidGPU
{
    public struct BufferDescription
    {
        public ulong Size;
        public int Stride;

        public BufferBind Bind;
        public ResourceUsage Usage;

        public CpuAccess CpuAccess;

        public IntPtr InitialData;
    }
}
