using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    public class DX11ShaderFactory : IShaderFactory
    {
        Device device;
        DeviceContext deviceContext;
        public DX11ShaderFactory(Device device, DeviceContext deviceContext)
        {
            this.device = device;
            this.deviceContext = deviceContext;
        }

        public IComputeShader CreateComputeShader(string source, string entrypoint)
        {
            DX11ComputeShader shader = new DX11ComputeShader(device, deviceContext);
            shader.Compile(source, entrypoint);
            return shader;
        }

        public IShader CreateShader(ShaderType shaderType, string source, string entrypoint, string path = "")
        {
            DX11Shader shader = new DX11Shader(device, shaderType);
            shader.Compile(source, entrypoint, path);
            return shader;
        }

        public IShaderProgram CreateShaderProgram()
        {
            return new DX11ShaderProgram(deviceContext);
        }
    }
}