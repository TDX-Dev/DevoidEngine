using System;
using System.IO;

public class FileReloader : IDisposable
{
    private readonly FileSystemWatcher watcher;
    private readonly Action onChanged;

    private DateTime lastReload = DateTime.MinValue;
    private const int debounceMs = 300;

    public FileReloader(string path, Action reloadAction)
    {
        onChanged = reloadAction;

        watcher = new FileSystemWatcher(
            Path.GetDirectoryName(path)!,
            Path.GetFileName(path)!
        );

        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += OnFileChanged;
        watcher.EnableRaisingEvents = true;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - lastReload).TotalMilliseconds < debounceMs)
            return;

        lastReload = DateTime.Now;

        Console.WriteLine("File change detected.");

        // ⚠ This runs on background thread
        // So just set a flag
        ReloadRequested = true;
    }

    public bool ReloadRequested { get; private set; }

    public void Consume()
    {
        if (!ReloadRequested) return;

        ReloadRequested = false;
        onChanged?.Invoke();
    }

    public void Dispose()
    {
        watcher?.Dispose();
    }
}