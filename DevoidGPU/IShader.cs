namespace DevoidGPU
{
    public enum ShaderType
    {
        Vertex,
        Fragment,
        Geometry,
        Compute,
        TessControl,
        TessEval
    }
    public interface IShader
    {
        ShaderType Type { get; }
        ShaderReflectionData ReflectionData { get; }
        string Name { get; }

        void Compile(string source, string entryPoint, string path);

    }
}