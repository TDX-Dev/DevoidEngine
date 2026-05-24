using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Device = SharpDX.Direct3D11.Device;

namespace DevoidGPU.DX11
{
    internal sealed class DX11VertexBuffer : IVertexBuffer
    {
        public ulong Size { get; }
        public VertexInfo Layout { get; }
        public int Slot { get; }
        public int Stride { get; }
        public BufferUsage Usage { get; }

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

            BufferDescription dxDescription = new()
            {
                SizeInBytes = (int)description.Size,
                BindFlags = BindFlags.VertexBuffer,
                Usage = DX11StateMapper.ToDXBufferUsage(description.Usage),
                CpuAccessFlags = DX11StateMapper.ToDXCpuAccess(description.Usage),
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

        public void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            int elementSize = Unsafe.SizeOf<T>();
            int totalSize = elementSize * data.Length;

            if ((ulong)totalSize > Size)
                throw new InvalidOperationException("Update data exceeds buffer size.");

            if (elementSize != Stride)
                throw new InvalidOperationException($"Stride mismatch. Expected {Stride}, got {elementSize}");

            if (Usage.HasFlag(BufferUsage.Dynamic))
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
                return;
            }

            unsafe
            {
                fixed (T* src = data)
                {
                    IntPtr ptr = (IntPtr)src;

                    deviceContext.UpdateSubresource(
                        ref ptr,
                        Buffer,
                        0
                    );
                }
            }
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }

}
