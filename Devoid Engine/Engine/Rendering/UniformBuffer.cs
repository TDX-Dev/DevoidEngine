using DevoidEngine.Engine.Rendering;
using DevoidGPU;

public class UniformBuffer
{
    private readonly IUniformBuffer _buffer;

    public int SizeInBytes => _buffer.SizeInBytes;

    public UniformBuffer(int sizeInBytes, BufferUsage usage = BufferUsage.Dynamic)
    {
        _buffer = Renderer.GraphicsDevice
            .BufferFactory
            .CreateUniformBuffer(sizeInBytes, usage);
    }

    public void SetData<T>(T data) where T : struct
    {
        _buffer.SetData(data);
    }
    public void SetData(byte[] data)
    {
        _buffer.SetData(data);
    }
    public void SetData(ReadOnlySpan<byte> data)
    {
        _buffer.SetData(data);
    }
    public void SetData(IntPtr ptr, int size)
    {
        _buffer.SetData(ptr, size);
    }
    public void Bind(int slot = 0, ShaderStage stage = ShaderStage.Fragment)
    {
        _buffer.Bind(slot, stage);
    }
}