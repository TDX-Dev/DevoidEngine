namespace DevoidEngine.Logging
{
    public static class DevoidLog
    {
        private static readonly List<ILogSink> Sinks = [];

        public static void AddSink(ILogSink sink)
        {
            Sinks.Add(sink);
        }

        public static void RemoveSink(ILogSink sink)
        {
            Sinks.Remove(sink);
        }

        public static void Log(LogLevel level, LogCategory category, string message)
        {
            var entry = new LogEntry(level, category, message);

            WriteSinks(entry);
        }

        public static void Info(LogCategory category, string message)
        {
            var entry = new LogEntry(LogLevel.Info, category, message);

            WriteSinks(entry);
        }

        public static void Warning(LogCategory category, string message)
        {
            var entry = new LogEntry(LogLevel.Warning, category, message);

            WriteSinks(entry);
        }

        public static void Error(LogCategory category, string message)
        {
            var entry = new LogEntry(LogLevel.Error, category, message);

            WriteSinks(entry);
        }

        public static void Debug(LogLevel level, LogCategory category, string message)
        {
            var entry = new LogEntry(level, category, message);

            WriteSinks(entry);
        }

        public static void Debug(string message)
        {
            var entry = new LogEntry(LogLevel.Debug, LogCategory.External, message);

            WriteSinks(entry);
        }

        static void WriteSinks(LogEntry entry)
        {
            foreach (var sink in Sinks)
                sink.Write(entry);
        }
    }
}
