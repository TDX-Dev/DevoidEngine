using System.Collections.Generic;
using System;

namespace ElementalEditor.Utils
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }

    public struct LogEntry
    {
        public string Message;
        public LogType Type;
        public DateTime Time;
    }

    public static class EditorConsole
    {
        const int MaxEntries = 100;

        static readonly List<LogEntry> entries = new();

        public static IReadOnlyList<LogEntry> Entries => entries;

        static void Add(LogEntry entry)
        {
            if (entries.Count >= MaxEntries)
                entries.RemoveAt(0); // remove oldest

            entries.Add(entry);
        }

        public static void Log(string message)
        {
            Add(new LogEntry
            {
                Message = message,
                Type = LogType.Info,
                Time = DateTime.Now
            });
        }

        public static void Warn(string message)
        {
            Add(new LogEntry
            {
                Message = message,
                Type = LogType.Warning,
                Time = DateTime.Now
            });
        }

        public static void Error(string message)
        {
            Add(new LogEntry
            {
                Message = message,
                Type = LogType.Error,
                Time = DateTime.Now
            });
        }

        public static void Clear()
        {
            entries.Clear();
        }
    }
}