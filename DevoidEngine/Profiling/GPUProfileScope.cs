namespace DevoidEngine.Profiling
{
    public struct GPUProfileScope
    {
        public double DurationMilliseconds;
        public bool HasResult;

        public int Parent;

        internal int BeginTimestamp;
        internal int EndTimestamp;

        public string CallerFileName;
        public int CallerLineNumber;
        public string CallerMemberName;

        public string? CustomName;
    }
}