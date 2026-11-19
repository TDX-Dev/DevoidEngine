using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using MapMode = SharpDX.Direct3D11.MapMode;

namespace DevoidGPU.DX11
{
    internal sealed class Dx11IndexBuffer : IIndexBuffer
    {
        public ulong Size { get; }
        public IndexFormat Format { get; }
        public ResourceUsage Usage { get; }

        public Buffer Buffer { get; private set; } = null!;

        private readonly Device _device;
        private readonly DeviceContext _deviceContext;

        public Dx11IndexBuffer(Device device, DeviceContext deviceContext, IndexBufferDescription description)
        {
            this._device = device;
            this._deviceContext = deviceContext;
            Size = description.Size;
            Format = description.Format;
            Usage = description.Usage;

            //var dxFormat = DX11StateMapper.ToDXGIFormat(desc.Format);

            SharpDX.Direct3D11.BufferDescription dxDescription = new()
            {
                SizeInBytes = (int)description.Size,
                Usage = Dx11StateMapper.ToDxResourceUsage(description.Usage),
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = description.Usage == ResourceUsage.Dynamic ? CpuAccessFlags.Write : Dx11StateMapper.ToDxCpuAccess(description.CpuAccess),
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
                var box = _deviceContext.MapSubresource(Buffer, 0, SharpDX.Direct3D11.MapMode.WriteDiscard, MapFlags.None);

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
