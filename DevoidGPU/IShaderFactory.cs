namespace DevoidGPU
{
    public interface IShaderFactory
    {
        IShader CreateShader(ShaderType shaderType, string source, string entrypoint, string path = "");
        IShaderProgram CreateShaderProgram();

        IComputeShader CreateComputeShader(string source, string entrypoint);

    }
}