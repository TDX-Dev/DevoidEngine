using DevoidEngine.Engine.ProjectSystem;

namespace ElementalEditor.Scripting
{
    public static class ScriptWatcher
    {
        static FileSystemWatcher watcher;
        static bool reloadRequested;
        static DateTime lastChange;

        public static void Initialize()
        {
            string assetPath = ProjectManager.Current.AssetPath;

            watcher = new FileSystemWatcher(ProjectManager.Current.AssetPath);

            watcher.Filter = "*.cs";
            watcher.IncludeSubdirectories = true;

            watcher.NotifyFilter =
                NotifyFilters.LastWrite |
                NotifyFilters.FileName |
                NotifyFilters.DirectoryName;

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;

            watcher.EnableRaisingEvents = true;
        }

        static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (!e.FullPath.EndsWith(".cs"))
                return;

            // debounce
            if ((DateTime.Now - lastChange).TotalMilliseconds < 500)
                return;

            lastChange = DateTime.Now;
            reloadRequested = true;

            Console.WriteLine("[Scripts] Change detected: " + e.Name);
        }

        public static bool ConsumeReload()
        {
            if (!reloadRequested)
                return false;

            reloadRequested = false;
            return true;
        }
    }

}