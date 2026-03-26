using SharpDX;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DevoidGPU.DX11
{
    public class DX11UniformBuffer : IUniformBuffer
    {
        public BufferUsage Usage { get; private set; }
        public int SizeInBytes => bufferSize;

        private readonly Device device;
        private readonly DeviceContext deviceContext;
        private readonly SharpDX.Direct3D11.Buffer buffer;
        private readonly int bufferSize;



        public DX11UniformBuffer(Device device, DeviceContext deviceContext, int sizeInBytes, BufferUsage bufferUsage)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
            this.deviceContext = deviceContext ?? throw new ArgumentNullException(nameof(deviceContext));

            int alignedSize = (sizeInBytes + 15) / 16 * 16;

            var desc = new BufferDescription
            {
                Usage = DX11StateMapper.ToDXResourceUsage(bufferUsage),
                SizeInBytes = alignedSize,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = bufferUsage == BufferUsage.Dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            buffer = new SharpDX.Direct3D11.Buffer(device, desc);


            this.Usage = bufferUsage;

            this.bufferSize = alignedSize;
        }

        public void SetData<T>(T data) where T : struct
        {
            if (this.Usage == BufferUsage.Dynamic)
            {
                DataBox dataBox = deviceContext.MapSubresource(
                    buffer,
                    0,
                    MapMode.WriteDiscard, // Use WriteDiscard or WriteNoOverwrite
                    MapFlags.None
                );

                // Copy the data
                Utilities.Write(dataBox.DataPointer, ref data);

                deviceContext.UnmapSubresource(buffer, 0);
            }
            else
            {
                deviceContext.UpdateSubresource(ref data, buffer);
            }
        }

        public unsafe void SetData(ReadOnlySpan<byte> data)
        {
            if (data.Length > bufferSize)
                throw new ArgumentException("Data size exceeds constant buffer size.");

            if (Usage != BufferUsage.Dynamic)
            {
                // Fallback for non-dynamic buffers (rare for CBs)
                fixed (byte* srcPtr = data)
                {
                    deviceContext.UpdateSubresource(
                        new DataBox((IntPtr)srcPtr, 0, 0),
                        buffer,
                        0
                    );
                }
                return;
            }

            DataBox mapped = deviceContext.MapSubresource(
                buffer,
                0,
                MapMode.WriteDiscard,
                MapFlags.None
            );

            System.Buffer.MemoryCopy(
                source: Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)),
                destination: (void*)mapped.DataPointer,
                destinationSizeInBytes: bufferSize,
                sourceBytesToCopy: data.Length
            );

            deviceContext.UnmapSubresource(buffer, 0);
        }

        public unsafe void SetData(IntPtr dataPtr, int size)
        {
            if (size > bufferSize)
                throw new ArgumentException("Data exceeds constant buffer size.");

            if (Usage == BufferUsage.Dynamic)
            {
                var mapped = deviceContext.MapSubresource(
                    buffer,
                    0,
                    MapMode.WriteDiscard,
                    MapFlags.None);

                System.Buffer.MemoryCopy(
                    dataPtr.ToPointer(),
                    (void*)mapped.DataPointer,
                    bufferSize,
                    size);

                if (size < bufferSize)
                {
                    Span<byte> tail = new Span<byte>(
                        (void*)((byte*)mapped.DataPointer + size),
                        bufferSize - size);

                    tail.Clear();
                }

                deviceContext.UnmapSubresource(buffer, 0);
            }
            else
            {
                deviceContext.UpdateSubresource(ref dataPtr, buffer);
            }
        }


        public void Bind(int slot, ShaderStage stages)
        {
            if ((stages & ShaderStage.Vertex) != 0)
                deviceContext.VertexShader.SetConstantBuffer(slot, buffer);
            if ((stages & ShaderStage.Fragment) != 0)
                deviceContext.PixelShader.SetConstantBuffer(slot, buffer);
            if ((stages & ShaderStage.Geometry) != 0)
                deviceContext.GeometryShader.SetConstantBuffer(slot, buffer);
            if ((stages & ShaderStage.Compute) != 0)
                deviceContext.ComputeShader.SetConstantBuffer(slot, buffer);
        }

        public void Dispose()
        {
            buffer?.Dispose();
        }
    }
}