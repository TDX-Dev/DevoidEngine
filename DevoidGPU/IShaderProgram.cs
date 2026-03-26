namespace DevoidGPU
{
    public interface IShaderProgram
    {
        void AttachShader(IShader shader);
        void Link();
        void Bind();
    }
}