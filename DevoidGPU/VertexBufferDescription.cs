namespace DevoidGPU
{
    public struct VertexBufferDescription
    {
        public ulong Size;
        public VertexInfo Layout;
        public int Slot;
        public BufferUsage Usage;
        public IntPtr InitialData; // I've kept this field for immutable vertex buffers?, stuff that needs the data upfront, like an immutable buffer.
    }
}
