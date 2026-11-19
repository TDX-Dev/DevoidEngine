using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11GpuTimestamp : IGpuTimestamp
    {
        private readonly DeviceContext _context;

        internal Query Query { get; }

        public Dx11GpuTimestamp(
            Device device,
            DeviceContext context)
        {
            this._context = context;

            QueryDescription description = new()
            {
                Type = QueryType.Timestamp,
                Flags = QueryFlags.None
            };

            Query = new Query(device, description);
        }

        public bool TryGetTimestamp(out ulong timestamp)
        {
            return _context.GetData(Query, out timestamp);
        }


        public void Dispose()
        {
            Query.Dispose();
        }
    }
}