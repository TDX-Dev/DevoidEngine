using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Utilities
{
    internal class FrameTimer
    {
        Stopwatch stopwatch;
        double lastTime;

        public FrameTimer()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            lastTime = stopwatch.Elapsed.TotalSeconds;
        }

        public double GetElapsedSeconds()
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            double elapsed =  currentTime - lastTime;
            lastTime = currentTime;
            return elapsed;
        }
    }
}
