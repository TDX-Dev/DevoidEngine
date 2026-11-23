using DevoidEngine.Logging;
using DevoidGPU;

namespace DevoidEngine.Core
{
    public sealed class Shader : IDisposable
    {
        private readonly OrderedDictionary<ShaderDefineSet, ShaderPass> _shaderPasses = [];
        private int _currentShaderPass = 0;

        public Shader(string name, string vertexShader, string fragmentShader)
        {
            if (!File.Exists(vertexShader))
            {
                DevoidLog.Error(LogCategory.Core, $"Vertex shader not found at path: {vertexShader}");
                throw new ArgumentException("", nameof(vertexShader));
            }
            if (!File.Exists(fragmentShader))
            {
                DevoidLog.Error(LogCategory.Core, $"Vertex shader not found at path: {fragmentShader}");
                throw new ArgumentException("", nameof(fragmentShader));
            }

            IShader vShader = LoadProgram($"{name}_VS", vertexShader, "VSMain", [], ShaderStage.Vertex);
            IShader fShader = LoadProgram($"{name}_FS", fragmentShader, "PSMain", [], ShaderStage.Fragment);

            ShaderPass pass = new(vShader, fShader);
            ShaderDefineSet shaderDefineSet = new();
            _shaderPasses.Add(shaderDefineSet, pass);
        }

        IShader LoadProgram(string name, string path, string entryPoint, Dictionary<string, string> defines, ShaderStage stage)
        {
            string source = string.Empty;
            using (StreamReader sr = new(path))
            {
                source = sr.ReadToEnd();
            }

            ShaderDescription shaderDescription = new()
            {
                Name = name,
                EntryPoint = entryPoint,
                Defines = defines,
                Stage = stage,
                Source = source,
                FilePath = path,
            };

            return Engine.GraphicsDevice.CreateShader(shaderDescription);
        }
        public void SetActiveShaderPass(int pass)
        {
            _currentShaderPass = pass;
        }
        public ShaderPass GetActivePass()
        {
            return _shaderPasses.GetAt(_currentShaderPass).Value;
        }
        public int GetOrCreateVariant(Dictionary<string, string> defines)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            foreach (var shaderPass in _shaderPasses.Values)
            {
                shaderPass.Dispose();
            }
            _shaderPasses.Clear();
        }
    }
}
