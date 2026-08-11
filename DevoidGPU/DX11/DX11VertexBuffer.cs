using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DevoidGPU.DX11
{
    internal sealed class DX11VertexBuffer : IVertexBuffer
    {
        public ulong Size { get; }
        public VertexInfo Layout { get; }
        public int Slot { get; }
        public int Stride { get; }
        public ResourceUsage Usage { get; }

        public Buffer Buffer { get; private set; } = null!;

        private readonly Device device;
        private readonly DeviceContext deviceContext;

        public DX11VertexBuffer(Device device, DeviceContext context, VertexBufferDescription description)
        {
            this.device = device;
            this.deviceContext = context;

            Size = description.Size;
            Layout = description.Layout;
            Slot = description.Slot;
            Usage = description.Usage;

            Stride = Layout.SizeInBytes;

            SharpDX.Direct3D11.BufferDescription dxDescription = new()
            {
                SizeInBytes = (int)description.Size,
                BindFlags = BindFlags.VertexBuffer,
                Usage = DX11StateMapper.ToDXBufferUsage(description.Usage),
                CpuAccessFlags = description.Usage == ResourceUsage.Dynamic ? CpuAccessFlags.Write : DX11StateMapper.ToDXCpuAccess(description.CpuAccess),
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };


            if (description.InitialData != IntPtr.Zero)
            {
                Buffer = new Buffer(
                    device,
                    description.InitialData,
                    dxDescription
                );
            }
            else
            {
                Buffer = new Buffer(device, dxDescription);
            }

        }

        public unsafe void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            int stride = Unsafe.SizeOf<T>();

            if (stride != Stride)
                throw new InvalidOperationException(
                    $"Stride mismatch. Expected {Stride}, got {stride}");

            fixed (T* ptr = data)
            {
                Update((nint)ptr, stride * data.Length);
            }
        }

        public unsafe void Update(nint data, int sizeInBytes)
        {
            if ((ulong)sizeInBytes > Size)
                throw new InvalidOperationException("Update data exceeds buffer size.");

            if ((Usage & ResourceUsage.Dynamic) != 0)
            {
                var box = deviceContext.MapSubresource(
                    Buffer,
                    0,
                    MapMode.WriteDiscard,
                    MapFlags.None);

                System.Buffer.MemoryCopy(
                    (void*)data,
                    (void*)box.DataPointer,
                    sizeInBytes,
                    sizeInBytes);

                deviceContext.UnmapSubresource(Buffer, 0);
            }
            else
            {
                deviceContext.UpdateSubresource(
                    new DataBox(data, 0, 0),
                    Buffer,
                    0);
            }
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }

}
