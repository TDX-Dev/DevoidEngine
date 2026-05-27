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

        private readonly Device device;
        private readonly DeviceContext deviceContext;

        public DX11ShaderStorageBuffer(Device device, DeviceContext context, BufferDescription description)
        {
            this.device = device;
            this.deviceContext = context;

            this.Size = DX11StateMapper.Align16(description.Size);
            Usage = description.Usage;

            SharpDX.Direct3D11.BufferDescription dxDescription = new()
            {
                SizeInBytes = (int)Size,
                BindFlags = BindFlags.ShaderResource,
                Usage = DX11StateMapper.ToDXBufferUsage(description.Usage),
                CpuAccessFlags = DX11StateMapper.ToDXCpuAccess(description.CpuAccess),
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
            int totalSize = Unsafe.SizeOf<T>() * data.Length;

            if ((ulong)totalSize > Size)
                throw new InvalidOperationException("Update data exceeds uniform buffer size.");
        }
    }
}
