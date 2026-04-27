namespace DevoidGPU
{
    public interface IShader
    {
        ShaderDescription Description { get; }
        ShaderStage Stage { get; }
        ShaderReflectionData ReflectionData { get; }
        string Name { get; }
    }
}
