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

        public void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {


            int elementSize = Unsafe.SizeOf<T>();
            int totalSize = elementSize * data.Length;

            if ((ulong)totalSize > Size)
                throw new InvalidOperationException("Update exceeds buffer size");

            if ((Usage & ResourceUsage.Dynamic) != 0)
            {
                var box = deviceContext.MapSubresource(Buffer, 0, MapMode.WriteDiscard, MapFlags.None);

                unsafe
                {
                    fixed (T* src = data)
                    {
                        System.Buffer.MemoryCopy(
                            src,
                            (void*)box.DataPointer,
                            (long)Size,
                            totalSize
                        );
                    }
                }

                deviceContext.UnmapSubresource(Buffer, 0);
            }
            else
            {
                unsafe
                {
                    fixed (T* src = data)
                    {
                        IntPtr ptr = (IntPtr)src;
                        deviceContext.UpdateSubresource(ref ptr, Buffer, 0);
                    }
                }
            }
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
