using SharpDX;
using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    public class DX11ShaderStorageBuffer : IShaderStorageBuffer
    {
        public BufferUsage Usage { get; }
        public int SizeInBytes { get; }

        private readonly Device device;
        private readonly DeviceContext deviceContext;
        private readonly SharpDX.Direct3D11.Buffer buffer;

        public ShaderResourceView ShaderResourceView { get; }
        public UnorderedAccessView UnorderedAccessView { get; }

        public DX11ShaderStorageBuffer(
            Device device,
            DeviceContext context,
            int sizeInBytes,
            int structureStride,
            BufferUsage usage,
            bool allowUAV)
        {
            this.device = device;
            this.deviceContext = context;
            this.Usage = usage;
            this.SizeInBytes = sizeInBytes;

            var desc = new BufferDescription()
            {
                Usage = usage == BufferUsage.Dynamic ? ResourceUsage.Dynamic : ResourceUsage.Default,
                SizeInBytes = sizeInBytes,
                BindFlags = BindFlags.ShaderResource |
                            (allowUAV && usage != BufferUsage.Dynamic ? BindFlags.UnorderedAccess : 0),
                OptionFlags = ResourceOptionFlags.BufferStructured,
                CpuAccessFlags = usage == BufferUsage.Dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
                StructureByteStride = structureStride,
            };

            buffer = new SharpDX.Direct3D11.Buffer(device, desc);

            ShaderResourceView = new ShaderResourceView(device, buffer);

            if (allowUAV)
                UnorderedAccessView = new UnorderedAccessView(device, buffer);
        }

        // ---------------------------
        // Binding
        // ---------------------------

        public void Bind(int slot, ShaderStage stages)
        {
            if ((stages & ShaderStage.Vertex) != 0)
                deviceContext.VertexShader.SetShaderResource(slot, ShaderResourceView);

            if ((stages & ShaderStage.Fragment) != 0)
                deviceContext.PixelShader.SetShaderResource(slot, ShaderResourceView);

            if ((stages & ShaderStage.Geometry) != 0)
                deviceContext.GeometryShader.SetShaderResource(slot, ShaderResourceView);

            if ((stages & ShaderStage.Compute) != 0)
                deviceContext.ComputeShader.SetShaderResource(slot, ShaderResourceView);
        }

        public void UnBind(int slot, ShaderStage stages)
        {
            if ((stages & ShaderStage.Vertex) != 0)
                deviceContext.VertexShader.SetShaderResource(slot, null);

            if ((stages & ShaderStage.Fragment) != 0)
                deviceContext.PixelShader.SetShaderResource(slot, null);

            if ((stages & ShaderStage.Geometry) != 0)
                deviceContext.GeometryShader.SetShaderResource(slot, null);

            if ((stages & ShaderStage.Compute) != 0)
                deviceContext.ComputeShader.SetShaderResource(slot, null);
        }

        public void BindMutable(int slot)
        {
            deviceContext.ComputeShader.SetUnorderedAccessView(slot, UnorderedAccessView);
        }

        public void UnBindMutable(int slot)
        {
            deviceContext.ComputeShader.SetUnorderedAccessView(slot, null);
        }

        // ---------------------------
        // Data Upload
        // ---------------------------

        public void SetData(ReadOnlySpan<byte> data)
        {
            if (data.Length > SizeInBytes)
                throw new ArgumentException("Data exceeds buffer capacity.");

            UpdatePartial(data, 0);
        }

        public void UpdatePartial(ReadOnlySpan<byte> data, int byteOffset)
        {
            if (byteOffset + data.Length > SizeInBytes)
                throw new ArgumentException("Partial update exceeds buffer bounds.");

            if (Usage == BufferUsage.Dynamic)
            {
                MapMode mode =
                    (byteOffset == 0 && data.Length == SizeInBytes)
                    ? MapMode.WriteDiscard
                    : MapMode.WriteNoOverwrite;

                DataStream stream;

                deviceContext.MapSubresource(
                    buffer,
                    0,
                    mode,
                    MapFlags.None,
                    out stream);

                stream.Position = byteOffset;

                unsafe
                {
                    fixed (byte* ptr = data)
                    {
                        stream.WriteRange((IntPtr)ptr, data.Length);
                    }
                }

                deviceContext.UnmapSubresource(buffer, 0);
                stream.Dispose();
            }
            else
            {
                unsafe
                {
                    fixed (byte* ptr = data)
                    {
                        deviceContext.UpdateSubresource(
                            new DataBox((IntPtr)ptr, 0, 0),
                            buffer,
                            0,
                            new ResourceRegion()
                            {
                                Left = byteOffset,
                                Right = byteOffset + data.Length,
                                Top = 0,
                                Front = 0,
                                Bottom = 1,
                                Back = 1
                            });
                    }
                }
            }
        }

    }
}