namespace DevoidEngine.Profiling
{
    public class Profiler
    {
        public CPUProfiler CPU { get; }

        public Profiler()
        {
            CPU = new CPUProfiler();
        }

        public void BeginFrame()
        {
            CPU.BeginFrame();
        }

    }
}
