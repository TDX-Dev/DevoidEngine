using DevoidEngine.Core;

namespace DevoidEngine.Profiling
{
    public class Profiler
    {
        public CpuProfiler Cpu { get; private set; } = null!;
        public GpuProfiler Gpu { get; private set; } = null!;

        public Profiler()
        {
        }

        public void Initialize()
        {
            Cpu = new CpuProfiler();
            Gpu = new GpuProfiler(Engine.GraphicsDevice);
        }

        public void BeginFrame()
        {
            Cpu.BeginFrame();

        }

    }
}
