using DevoidEngine.Logging;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Logging
{
    public sealed class LogEntry
    {
        public LogLevel Level { get; }
        public LogCategory Category { get; }
        public string Message { get; }

        public DateTime Timestamp { get; }

        public string File { get; }
        public int Line { get; }
        public string Member { get; }

        public int ThreadId { get; }
        public string? ThreadName { get; }

        public LogEntry(
            LogLevel level,
            LogCategory category,
            string message,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            Level = level;
            Category = category;
            Message = message;

            Timestamp = DateTime.UtcNow;

            File = file;
            Line = line;
            Member = member;

            ThreadId = Environment.CurrentManagedThreadId;
            ThreadName = Thread.CurrentThread.Name;
        }
    }
}