using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11VertexBuffer : IVertexBuffer
    {
        public ulong Size { get; }
        public VertexInfo Layout { get; }
        public int Slot { get; }
        public int Stride { get; }
        public ResourceUsage Usage { get; }

        public Buffer Buffer { get; private set; } = null!;

        private readonly Device _device;
        private readonly DeviceContext _deviceContext;

        public Dx11VertexBuffer(Device device, DeviceContext context, VertexBufferDescription description)
        {
            this._device = device;
            this._deviceContext = context;

            Size = description.Size;
            Layout = description.Layout;
            Slot = description.Slot;
            Usage = description.Usage;

            Stride = Layout.SizeInBytes;

            SharpDX.Direct3D11.BufferDescription dxDescription = new()
            {
                SizeInBytes = (int)description.Size,
                BindFlags = BindFlags.VertexBuffer,
                Usage = Dx11StateMapper.ToDxResourceUsage(description.Usage),
                CpuAccessFlags = description.Usage == ResourceUsage.Dynamic ? CpuAccessFlags.Write : Dx11StateMapper.ToDxCpuAccess(description.CpuAccess),
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
                var box = _deviceContext.MapSubresource(
                    Buffer,
                    0,
                    SharpDX.Direct3D11.MapMode.WriteDiscard,
                    MapFlags.None);

                System.Buffer.MemoryCopy(
                    (void*)data,
                    (void*)box.DataPointer,
                    sizeInBytes,
                    sizeInBytes);

                _deviceContext.UnmapSubresource(Buffer, 0);
            }
            else
            {
                _deviceContext.UpdateSubresource(
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
