namespace DevoidGPU
{
    public interface IInputLayout
    {
        VertexInfo VertexInfo { get; }
        void Bind();
    }
}