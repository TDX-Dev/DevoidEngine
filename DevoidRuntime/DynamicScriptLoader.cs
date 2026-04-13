#if SCRIPT_DYNAMIC
using System.Reflection;

static class DynamicScriptLoader
{
    public static void Load()
    {
        var project = ProjectManager.Current;

        string path = Path.Combine(
            project.TempPath,
            "Scripts",
            "GameScripts.dll");

        if (!File.Exists(path))
            return;

        var assembly = Assembly.LoadFrom(path);

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(Component)))
                continue;

            if (type.IsAbstract)
                continue;

            ScriptRegistry.Register(type);
        }

        GeneratedComponentRegistry.RegisterAll();
    }
}
#endif