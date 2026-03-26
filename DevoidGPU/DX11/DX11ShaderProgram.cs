using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    public class DX11ShaderProgram : IShaderProgram
    {
        private readonly DeviceContext deviceContext;

        private DX11Shader vertexShader;
        private DX11Shader pixelShader;
        private DX11Shader geometryShader;

        public DX11ShaderProgram(DeviceContext device)
        {
            this.deviceContext = device;
        }

        public void AttachShader(IShader shader)
        {
            if (shader is not DX11Shader dX11Shader)
            {
                throw new InvalidOperationException("DX11 Program can only attach DX11 Shaders");
            }

            switch (dX11Shader.Type)
            {
                case ShaderType.Vertex: vertexShader = dX11Shader; break;
                case ShaderType.Fragment: pixelShader = dX11Shader; break;
                case ShaderType.Geometry: geometryShader = dX11Shader; break;
                default: throw new NotSupportedException("Unsupported shader type for this program");
            }


        }

        public void Bind()
        {
            deviceContext.VertexShader.Set(vertexShader?.VertexShader);
            deviceContext.PixelShader.Set(pixelShader?.PixelShader);
            deviceContext.GeometryShader.Set(geometryShader?.GeometryShader);
        }

        public void Link()
        {
            // DX11 does not link shaders.
        }
    }
}