using SharpDX.Direct3D11;
using System.Runtime.InteropServices;

namespace DevoidGPU.DX11
{
    public class DX11BufferFactory : IBufferFactory
    {
        Device device;
        DeviceContext deviceContext;
        public DX11BufferFactory(Device device, DeviceContext deviceContext)
        {
            this.device = device;
            this.deviceContext = deviceContext;

        }

        public IUniformBuffer CreateUniformBuffer<T>(BufferUsage bufferUsage) where T : struct
        {
            return new DX11UniformBuffer(device, deviceContext, Marshal.SizeOf<T>(), bufferUsage);
        }

        public IUniformBuffer CreateUniformBuffer(int size, BufferUsage bufferUsage)
        {
            return new DX11UniformBuffer(device, deviceContext, size, bufferUsage);
        }

        public IShaderStorageBuffer CreateShaderStorageBuffer(int sizeInBytes, int stride, BufferUsage usage, bool allowUAV = false)
        {
            return new DX11ShaderStorageBuffer(device, deviceContext, sizeInBytes, stride, usage, allowUAV);
        }

        public IIndexBuffer CreateIndexBuffer(int indexCount, BufferUsage usage, bool is16Bit = false)
        {
            return new DX11IndexBuffer(device, deviceContext, indexCount, usage, is16Bit);
        }

        public IVertexBuffer CreateVertexBuffer(BufferUsage bufferUsage, VertexInfo vertexInfo, int vertexCount)
        {
            DX11VertexBuffer dX11VertexBuffer = new DX11VertexBuffer(this.device, this.deviceContext, vertexInfo, vertexCount, bufferUsage);
            return dX11VertexBuffer;
        }

        public IFramebuffer CreateFramebuffer()
        {
            DX11Framebuffer framebuffer = new DX11Framebuffer(this.device, deviceContext);
            return framebuffer;
        }
    }
}