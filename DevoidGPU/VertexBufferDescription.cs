namespace DevoidGPU
{
    public struct VertexBufferDescription
    {
        public ulong Size;
        public VertexInfo Layout;
        public int Slot;
        public BufferBind Bind;
        public ResourceUsage Usage;
        public CpuAccess CpuAccess;
        public IntPtr InitialData; // I've kept this field for immutable vertex buffers?, stuff that needs the data upfront, like an immutable buffer.
    }
}
