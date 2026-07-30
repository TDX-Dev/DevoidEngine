using DevoidGPU;

namespace DevoidEngine.Core
{
    public class ShaderStage
    {
        public IShader GPU { get; }
        public ShaderReflectionData ShaderReflectionData { get; }

        public ShaderStage(IShader shader)
        {
            GPU = shader;
            ShaderReflectionData = shader.ReflectionData;
        }
    }
}
