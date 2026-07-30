using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace DevoidGPU.DX11
{
    internal sealed class DX11UniformBuffer : IUniformBuffer
    {
        public ulong Size { get; }
        public ResourceUsage Usage { get; }
        public Buffer Buffer { get; private set; } = null!;

        private readonly Device device;
        private readonly DeviceContext deviceContext;

        public DX11UniformBuffer(Device device, DeviceContext context, BufferDescription description)
        {
            this.device = device;
            this.deviceContext = context;

            this.Size = DX11StateMapper.Align16(description.Size);
            Usage = description.Usage;

            SharpDX.Direct3D11.BufferDescription dxDescription = new()
            {
                SizeInBytes = (int)Size,
                BindFlags = BindFlags.ConstantBuffer,
                Usage = DX11StateMapper.ToDXBufferUsage(description.Usage),
                CpuAccessFlags = DX11StateMapper.ToDXCpuAccess(description.CpuAccess),
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

        public void Update<T>(T data) where T : struct
        {
            if (this.Usage == ResourceUsage.Dynamic)
            {
                DataBox dataBox = deviceContext.MapSubresource(
                    Buffer,
                    0,
                    MapMode.WriteDiscard, // Use WriteDiscard or WriteNoOverwrite
                    MapFlags.None
                );

                // Copy the data
                Utilities.Write(dataBox.DataPointer, ref data);

                deviceContext.UnmapSubresource(Buffer, 0);
            }
            else
            {
                deviceContext.UpdateSubresource(ref data, Buffer);
            }
        }


        public void Update<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            int totalSize = Unsafe.SizeOf<T>() * data.Length;

            if ((ulong)totalSize > Size)
                throw new InvalidOperationException("Update data exceeds uniform buffer size.");

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
