using SharpDX;
using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    public class DX11IndexBuffer : IIndexBuffer, IDisposable
    {
        public int IndexCount { get; private set; }
        public bool Is16Bit { get; private set; }
        public BufferUsage Usage { get; private set; }

        private readonly Device device;
        private readonly DeviceContext deviceContext;
        private SharpDX.Direct3D11.Buffer buffer;

        // NEW: accept is16Bit flag
        public DX11IndexBuffer(Device device, DeviceContext deviceContext, int indexCount, BufferUsage usage, bool is16Bit = true)
        {
            this.device = device;
            this.deviceContext = deviceContext;
            this.IndexCount = indexCount;
            this.Usage = usage;
            this.Is16Bit = is16Bit;

            int stride = Is16Bit ? sizeof(ushort) : sizeof(uint);

            var bufferDesc = new BufferDescription
            {
                SizeInBytes = stride * IndexCount,
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = GetCpuAccessFlags(usage),
                Usage = DX11StateMapper.ToDXResourceUsage(usage),
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0
            };

            buffer = new SharpDX.Direct3D11.Buffer(device, bufferDesc);
        }

        // Support ushort[] upload (preferred for ImGui)
        public void SetData(ushort[] indices, int startIndex = 0)
        {
            if (indices == null) throw new ArgumentNullException(nameof(indices));
            if (!Is16Bit) throw new InvalidOperationException("Buffer configured for 32-bit indices");

            int countToWrite = indices.Length - startIndex;
            if (countToWrite <= 0) return;

            int sizeInBytes = countToWrite * sizeof(ushort);

            if (Usage == BufferUsage.Dynamic)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                IntPtr dest = stream.DataPointer;
                Utilities.Write(dest, indices, startIndex, countToWrite);
                deviceContext.UnmapSubresource(buffer, 0);
                stream.Dispose();
            }
            else
            {
                deviceContext.UpdateSubresource(indices, buffer);
            }
        }

        // Support int[] upload for 32-bit buffers (keeps previous API)
        public void SetData(int[] indices, int startIndex = 0)
        {
            if (indices == null) throw new ArgumentNullException(nameof(indices));
            if (Is16Bit) throw new InvalidOperationException("Buffer configured for 16-bit indices");

            int countToWrite = indices.Length - startIndex;
            if (countToWrite <= 0) return;

            if (Usage == BufferUsage.Dynamic)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                stream.WriteRange(indices, startIndex, countToWrite);
                deviceContext.UnmapSubresource(buffer, 0);
                stream.Dispose();
            }
            else
            {
                deviceContext.UpdateSubresource(indices, buffer);
            }
        }

        // UpdatePartial for managed int[] (32-bit)
        public void UpdatePartial(int[] indices, int startIndex, int indexCount)
        {
            if (indices == null) throw new ArgumentNullException(nameof(indices));
            if (Is16Bit) throw new InvalidOperationException("Buffer configured for 16-bit indices");

            int offsetInBytes = startIndex * sizeof(int);

            if (Usage == BufferUsage.Dynamic)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out stream);
                stream.Position = offsetInBytes;
                stream.WriteRange(indices, 0, indexCount);
                deviceContext.UnmapSubresource(buffer, 0);
                stream.Dispose();
            }
            else
            {
                deviceContext.UpdateSubresource(indices, buffer, 0, offsetInBytes, 0);
            }
        }

        // UpdatePartial for unmanaged pointer (nint) — handles both 16/32-bit
        public void UpdatePartial(nint dataPtr, int startIndex, int count)
        {
            if (dataPtr == IntPtr.Zero) throw new ArgumentNullException(nameof(dataPtr));

            int stride = Is16Bit ? sizeof(ushort) : sizeof(uint);
            int sizeInBytes = count * stride;
            int offsetInBytes = startIndex * stride;

            if (Usage == BufferUsage.Dynamic)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, MapFlags.None, out stream);

                IntPtr dest = stream.DataPointer + offsetInBytes;
                Utilities.CopyMemory(dest, dataPtr, sizeInBytes);

                deviceContext.UnmapSubresource(buffer, 0);
                stream.Dispose();
            }
            else
            {
                var dataBox = new DataBox(dataPtr, 0, 0);
                deviceContext.UpdateSubresource(
                    dataBox,
                    buffer,
                    0,
                    new ResourceRegion
                    {
                        Left = offsetInBytes,
                        Right = offsetInBytes + sizeInBytes,
                        Top = 0,
                        Front = 0,
                        Bottom = 1,
                        Back = 1
                    });
            }
        }

        // Bind with correct format based on Is16Bit
        public void Bind()
        {
            var format = Is16Bit ? SharpDX.DXGI.Format.R16_UInt : SharpDX.DXGI.Format.R32_UInt;
            deviceContext.InputAssembler.SetIndexBuffer(buffer, format, 0);
        }

        public void Dispose()
        {
            buffer?.Dispose();
            GC.SuppressFinalize(this);
        }

        private static CpuAccessFlags GetCpuAccessFlags(BufferUsage usage)
        {
            return usage switch
            {
                BufferUsage.Dynamic => CpuAccessFlags.Write,
                BufferUsage.Staging => CpuAccessFlags.Write | CpuAccessFlags.Read,
                _ => CpuAccessFlags.None
            };
        }
    }
}