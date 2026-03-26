namespace DevoidGPU
{
    public interface ISampler : IDisposable
    {
        SamplerDescription Description { get; }
        void Bind(int slot = 0);
    }
}