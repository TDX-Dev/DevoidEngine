using System.Diagnostics;

namespace DevoidEngine.Util
{
    internal class FrameTimer
    {
        private readonly Stopwatch stopwatch;
        private double lastTime;

        public FrameTimer()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            lastTime = stopwatch.Elapsed.TotalSeconds;
        }

        public double GetElapsedSeconds()
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            double elapsed = currentTime - lastTime;
            lastTime = currentTime;
            return elapsed;
        }
    }
}
