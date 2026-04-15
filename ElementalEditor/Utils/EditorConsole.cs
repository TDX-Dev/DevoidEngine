using System.Collections.Generic;
using System;

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
    static readonly List<LogEntry> entries = new();

    public static IReadOnlyList<LogEntry> Entries => entries;

    public static void Log(string message)
    {
        entries.Add(new LogEntry
        {
            Message = message,
            Type = LogType.Info,
            Time = DateTime.Now
        });
    }

    public static void Warn(string message)
    {
        entries.Add(new LogEntry
        {
            Message = message,
            Type = LogType.Warning,
            Time = DateTime.Now
        });
    }

    public static void Error(string message)
    {
        entries.Add(new LogEntry
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