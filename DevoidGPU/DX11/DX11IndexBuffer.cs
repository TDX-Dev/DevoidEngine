using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DevoidGPU.DX11
{
    internal sealed class DX11IndexBuffer : IIndexBuffer
    {
        public ulong Size { get; }
        public IndexFormat Format { get; }
        public ResourceUsage Usage { get; }

        public Buffer Buffer { get; private set; } = null!;

        private readonly Device device;
        private readonly DeviceContext deviceContext;

        public DX11IndexBuffer(Device device, DeviceContext deviceContext, IndexBufferDescription description)
        {
            this.device = device;
            this.deviceContext = deviceContext;
            Size = description.Size;
            Format = description.Format;
            Usage = description.Usage;

            //var dxFormat = DX11StateMapper.ToDXGIFormat(desc.Format);

            SharpDX.Direct3D11.BufferDescription dxDescription = new()
            {
                SizeInBytes = (int)description.Size,
                Usage = DX11StateMapper.ToDXBufferUsage(description.Usage),
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = description.Usage == ResourceUsage.Dynamic ? CpuAccessFlags.Write : DX11StateMapper.ToDXCpuAccess(description.CpuAccess),
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };


            if (description.InitialData != IntPtr.Zero)
            {
                Buffer = new Buffer(device, description.InitialData, dxDescription);
            }
            else
            {
                Buffer = new Buffer(device, dxDescription);
            }
        }

        public unsafe void Update(nint data, int sizeInBytes)
        {
            if ((ulong)sizeInBytes > Size)
                throw new InvalidOperationException("Update exceeds buffer size.");

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

        public unsafe void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            int sizeInBytes = Unsafe.SizeOf<T>() * data.Length;

            fixed (T* ptr = data)
            {
                Update((nint)ptr, sizeInBytes);
            }
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
