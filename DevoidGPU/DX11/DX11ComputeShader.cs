using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace DevoidGPU.DX11
{
    public class DX11ComputeShader : IComputeShader
    {
        public string Name { get; private set; }
        public ComputeShader ComputeShader { get; private set; }
        public byte[] ByteCode { get; private set; }

        private readonly Device device;
        private readonly DeviceContext deviceContext;

        public DX11ComputeShader(Device device, DeviceContext context, string name = null)
        {
            this.device = device;
            this.deviceContext = context;
            this.Name = name ?? "UnnamedComputeShader";
        }

        public void Compile(string source, string entryPoint = "CSMain")
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Shader source cannot be empty.", nameof(source));

            if (string.IsNullOrWhiteSpace(entryPoint))
                throw new ArgumentException("Entry point cannot be empty.", nameof(entryPoint));

            ShaderFlags flags = ShaderFlags.None;
#if DEBUG
            flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization;
#endif

            // compile compute shader
            var result = ShaderBytecode.Compile(source, entryPoint, "cs_5_0", flags);

            if (result.HasErrors)
                throw new Exception($"Compute shader compile error ({Name}): {result.Message}");

            ByteCode = result.Bytecode;

            CreateShaderFromBytecode();
        }

        public void LoadPrecompiled(byte[] bytecode)
        {
            if (bytecode == null || bytecode.Length == 0)
                throw new ArgumentException("Bytecode cannot be empty.", nameof(bytecode));

            ByteCode = bytecode;
            CreateShaderFromBytecode();
        }

        private void CreateShaderFromBytecode()
        {
            ComputeShader?.Dispose();

            using (var shaderBytecode = new SharpDX.D3DCompiler.ShaderBytecode(ByteCode))
            {
                ComputeShader = new ComputeShader(device, shaderBytecode);
            }
        }

        public void Dispatch(int x, int y, int z)
        {
            if (deviceContext == null)
                throw new ArgumentNullException(nameof(deviceContext));

            deviceContext.Dispatch(x, y, z);
        }

        public void Wait()
        {
            // DX11 Handles pipeline sync
        }

        public void Dispose()
        {
            ComputeShader.Dispose();
        }

        public void Use()
        {
            deviceContext.ComputeShader.Set(ComputeShader);
        }
    }
}