namespace DevoidGPU
{
    public interface IShader : IDisposable
    {
        ShaderDescription Description { get; }
        ShaderStage Stage { get; }
        ShaderReflectionData ReflectionData { get; }
        string Name { get; }
    }
}
