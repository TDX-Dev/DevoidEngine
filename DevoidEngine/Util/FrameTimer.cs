using System.Diagnostics;

namespace DevoidEngine.Util
{
    internal class FrameTimer
    {
        private readonly Stopwatch _stopwatch;
        private double _lastTime;

        public FrameTimer()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _lastTime = _stopwatch.Elapsed.TotalSeconds;
        }

        public double GetElapsedSeconds()
        {
            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            double elapsed = currentTime - _lastTime;
            _lastTime = currentTime;
            return elapsed;
        }
    }
}
