using DevoidEngine.Core;

namespace DevoidEngine.Profiling
{
    public class Profiler
    {
        public CPUProfiler CPU { get; private set; } = null!;
        public GPUProfiler GPU { get; private set; } = null!;

        public Profiler()
        {
        }

        public void Initialize()
        {
            CPU = new CPUProfiler();
            GPU = new GPUProfiler(Engine.GraphicsDevice);
        }

        public void BeginFrame()
        {
            CPU.BeginFrame();

        }

    }
}
