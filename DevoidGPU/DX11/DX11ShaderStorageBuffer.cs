using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DevoidGPU.DX11
{
    internal sealed class DX11ShaderStorageBuffer : IShaderStorageBuffer
    {
        public ulong Size { get; }
        public ResourceUsage Usage { get; }
        public Buffer Buffer { get; private set; } = null!;

        public ShaderResourceView? SRV { get; private set; }
        public UnorderedAccessView? UAV { get; private set; }

        private readonly Device device;
        private readonly DeviceContext deviceContext;

        public DX11ShaderStorageBuffer(Device device, DeviceContext context, BufferDescription description)
        {
            this.device = device;
            this.deviceContext = context;

            this.Size = description.Size;
            Usage = description.Usage;

            BindFlags bindFlags = BindFlags.ShaderResource;

            if (description.Bind.HasFlag(BufferBind.StorageWritable))
                bindFlags |= BindFlags.UnorderedAccess;

            SharpDX.Direct3D11.BufferDescription dxDescription = new()
            {
                SizeInBytes = (int)Size,
                BindFlags = bindFlags,
                Usage = DX11StateMapper.ToDXBufferUsage(description.Usage),
                CpuAccessFlags = DX11StateMapper.ToDXCpuAccess(description.CpuAccess),
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = description.Stride
            };

            if (description.InitialData != IntPtr.Zero)
            {
                Buffer = new Buffer(device, description.InitialData, dxDescription);
            }
            else
            {
                Buffer = new Buffer(device, dxDescription);
            }

            SRV = new ShaderResourceView(
                device,
                Buffer,
                new ShaderResourceViewDescription
                {
                    Format = SharpDX.DXGI.Format.Unknown,
                    Dimension = ShaderResourceViewDimension.Buffer,
                    Buffer = new ShaderResourceViewDescription.BufferResource
                    {
                        ElementOffset = 0,
                        ElementCount = ((int)Size / description.Stride)
                    }
                });

            if (description.Bind.HasFlag(BufferBind.StorageWritable))
            {
                UAV = new UnorderedAccessView(
                    device,
                    Buffer,
                    new UnorderedAccessViewDescription
                    {
                        Format = SharpDX.DXGI.Format.Unknown,
                        Dimension = UnorderedAccessViewDimension.Buffer,
                        Buffer = new UnorderedAccessViewDescription.BufferResource
                        {
                            FirstElement = 0,
                            ElementCount = ((int)Size / description.Stride)
                        }
                    });
            }
        }

        public void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {

            int totalSize = Unsafe.SizeOf<T>() * data.Length;

            if ((ulong)totalSize > Size)
                throw new InvalidOperationException("Update data exceeds shader storage buffer size.");

            if ((Usage & ResourceUsage.Dynamic) != 0)
            {
                var box = deviceContext.MapSubresource(
                    Buffer,
                    0,
                    MapMode.WriteDiscard,
                    MapFlags.None);

                unsafe
                {
                    fixed (T* src = data)
                    {
                        System.Buffer.MemoryCopy(
                            src,
                            (void*)box.DataPointer,
                            (long)Size,
                            totalSize);
                    }
                }

                deviceContext.UnmapSubresource(Buffer, 0);
                return;
            }

            unsafe
            {
                fixed (T* src = data)
                {
                    deviceContext.UpdateSubresource(
                        new DataBox((IntPtr)src, 0, 0),
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
