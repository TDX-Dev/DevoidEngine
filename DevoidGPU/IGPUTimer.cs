namespace DevoidGPU
{
    public interface IGpuTimer : IDisposable
    {
        bool TryGetResult(out double milliseconds);
    }
}