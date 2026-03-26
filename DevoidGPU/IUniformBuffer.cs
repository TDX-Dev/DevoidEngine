namespace DevoidGPU
{
    public enum ShaderStage
    {
        Vertex = 1 << 0,
        Fragment = 1 << 1,
        Geometry = 1 << 2,
        Compute = 1 << 3,
        All = Vertex | Fragment | Geometry | Compute
    }

    public interface IUniformBuffer
    {
        int SizeInBytes { get; }

        void SetData<T>(T data) where T : struct;
        void SetData(ReadOnlySpan<byte> data);
        void SetData(IntPtr data, int size);
        void Bind(int slot, ShaderStage stage);
    }

}