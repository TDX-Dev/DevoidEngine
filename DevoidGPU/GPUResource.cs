namespace DevoidGPU
{
    public abstract class GPUResource : IDisposable
    {
        // Size of GPU Resource in bytes
        public ulong Size { get; protected set; } = 0;

        public string DebugName { get; set; } = "";

        public abstract void Dispose();
    }
}
