namespace DevoidGPU
{

    public sealed class GraphicsDeviceInfo
    {
        public string Name { get; init; } = "";

        public ulong DedicatedVideoMemory { get; init; }
        public ulong DedicatedSystemMemory { get; init; }
        public ulong SharedSystemMemory { get; init; }
        public ulong VideoMemoryUsage { get; internal set; }
        public ulong VideoMemoryBudget { get; internal set; }

        public ulong SystemMemoryUsage { get; internal set; }
        public ulong SystemMemoryBudget { get; internal set; }

        public int VendorId { get; init; }
        public int DeviceId { get; init; }

        public int FeatureLevel { get; init; }
    }
}
