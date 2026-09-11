namespace DevoidEngine.Logging
{
    public interface ILogSink
    {
        void Write(LogEntry entry);
    }
}
