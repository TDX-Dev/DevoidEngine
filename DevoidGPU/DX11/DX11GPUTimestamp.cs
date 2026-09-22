using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    internal sealed class DX11GPUTimestamp : IGPUTimestamp
    {
        private readonly DeviceContext context;

        internal Query Query { get; }

        public DX11GPUTimestamp(
            Device device,
            DeviceContext context)
        {
            this.context = context;

            QueryDescription description = new()
            {
                Type = QueryType.Timestamp,
                Flags = QueryFlags.None
            };

            Query = new Query(device, description);
        }

        public bool TryGetTimestamp(out ulong timestamp)
        {
            return context.GetData(Query, out timestamp);
        }


        public void Dispose()
        {
            Query.Dispose();
        }
    }
}