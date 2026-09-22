namespace DevoidGPU
{
    public interface IGPUTimer : IDisposable
    {
        bool TryGetResult(out double milliseconds);
    }
}