using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    public class DX11VertexBuffer : IVertexBuffer
    {
        public int VertexCount { get; private set; }

        public int Stride { get; private set; }

        public VertexInfo Layout { get; private set; }

        public BufferUsage Usage { get; private set; }

        public SharpDX.Direct3D11.Device device;
        public DeviceContext deviceContext;

        public SharpDX.Direct3D11.Buffer buffer;

        public DX11VertexBuffer(SharpDX.Direct3D11.Device device, DeviceContext deviceContext, VertexInfo layout, int vertexCount, BufferUsage usage)
        {
            this.device = device;
            this.deviceContext = deviceContext;
            this.Layout = layout;
            this.VertexCount = vertexCount;
            this.Usage = usage;
            this.Stride = layout.SizeInBytes;

            BufferDescription bufferDescription = new BufferDescription()
            {
                SizeInBytes = Stride * VertexCount,
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = GetCpuAccessFlags(usage),
                Usage = GetDXUsage(usage),
                OptionFlags = ResourceOptionFlags.None,
                StructureByteStride = 0 // <-- not Stride, set to 0
            };

            buffer = new SharpDX.Direct3D11.Buffer(device, bufferDescription);
        }

        public void SetData<T>(T[] vertices, int startVertex = 0) where T : struct
        {
            if (Usage == BufferUsage.Dynamic)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);
                stream.WriteRange(vertices);
                deviceContext.UnmapSubresource(buffer, 0);
                stream.Dispose();
            }
            else
            {
                deviceContext.UpdateSubresource(vertices, buffer);
            }

        }

        public void UpdatePartial<T>(T[] vertices, int startVertex, int vertexCount) where T : struct
        {
            int offsetInBytes = startVertex * Stride;

            if (Usage == BufferUsage.Dynamic)
            {
                // Map/Unmap for dynamic buffers
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);

                // Advance to correct offset
                stream.Position = offsetInBytes;

                // Write only the range
                stream.WriteRange(vertices, startVertex, vertexCount);

                deviceContext.UnmapSubresource(buffer, 0);
                stream.Dispose();
            }
            else
            {
                // Default buffer: can use UpdateSubresource
                deviceContext.UpdateSubresource(vertices, buffer, 0, offsetInBytes, 0);
            }
        }

        public void UpdatePartial(nint dataPtr, int startVertex, int vertexCount)
        {
            int sizeInBytes = vertexCount * Stride;
            int offsetInBytes = startVertex * Stride;

            if (Usage == BufferUsage.Dynamic)
            {
                DataStream stream;
                deviceContext.MapSubresource(buffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out stream);

                stream.Position = offsetInBytes;
                stream.WriteRange(dataPtr, sizeInBytes);

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


        public void Bind(int slot = 0, int offset = 0)
        {
            VertexBufferBinding binding = new VertexBufferBinding(buffer, Stride, offset);
            deviceContext.InputAssembler.SetVertexBuffers(slot, binding);
        }

        // REMOVE
        public void Draw()
        {
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.Draw(VertexCount, 0);
        }

        public void Dispose()
        {
            buffer?.Dispose();
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

        private static ResourceUsage GetDXUsage(BufferUsage usage)
        {
            return usage switch
            {
                BufferUsage.Static => ResourceUsage.Immutable,
                BufferUsage.Dynamic => ResourceUsage.Dynamic,
                BufferUsage.Staging => ResourceUsage.Staging,
                BufferUsage.Default => ResourceUsage.Default,
                _ => ResourceUsage.Default
            };
        }
    }
}