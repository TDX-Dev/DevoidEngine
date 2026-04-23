using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
