namespace DevoidEngine.Logging
{
    public sealed class ConsoleLogSink : ILogSink
    {
        public LogVerbosity Verbose { get; set; }

        public ConsoleLogSink(LogVerbosity verbosity  = LogVerbosity.Normal)
        {
            Verbose = verbosity;
        }

        public void Write(LogEntry entry)
        {
            ConsoleColor color = entry.Level switch
            {
                LogLevel.Debug => ConsoleColor.Cyan,
                LogLevel.Info => ConsoleColor.Gray,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            if (Verbose == LogVerbosity.Detailed || Verbose == LogVerbosity.Complete)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"[{entry.Timestamp:HH:mm:ss.fff}] ");

                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"[{entry.Member}]");

                Console.ForegroundColor = color;
                Console.Write($"[{entry.Level}]");
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{entry.Category}]: ");

            Console.ForegroundColor = color;
            Console.WriteLine(entry.Message);


            if (Verbose == LogVerbosity.Complete)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\tat {Path.GetFileName(entry.File)}:{entry.Line} ({entry.Member})");
                Console.WriteLine($"\tthreadID: {entry.ThreadId} ({entry.ThreadName})");
            }

            Console.ResetColor();
        }
    }
}