using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11GpuTimestampDisjoint : IGpuTimestampDisjoint
    {
        private readonly DeviceContext _context;

        internal Query Query { get; }

        public Dx11GpuTimestampDisjoint(
            Device device,
            DeviceContext context)
        {
            this._context = context;

            QueryDescription description = new()
            {
                Type = QueryType.TimestampDisjoint,
                Flags = QueryFlags.None
            };

            Query = new Query(device, description);
        }

        public bool TryGetResult(out long frequency, out bool disjoint)
        {
            frequency = 0;
            disjoint = false;

            if (!_context.GetData(Query, out QueryDataTimestampDisjoint disjointData))
            {
                return false;
            }

            frequency = disjointData.Frequency;
            disjoint = disjointData.Disjoint;

            return true;
        }

        public void Dispose()
        {
            Query.Dispose();
        }
    }
}