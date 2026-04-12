using System.Reflection;
using DevoidEngine.Engine.ProjectSystem;

namespace ElementalEditor.Scripting;

public static class ScriptAssemblyLoader
{
    static Assembly assembly;

    public static void Load()
    {
        var project = ProjectManager.Current;

        string path = Path.Combine(
            project.TempPath,
            "Scripts",
            "GameScripts.dll"
        );

        if (!File.Exists(path))
        {
            Console.WriteLine("[Scripts] No compiled scripts found.");
            return;
        }

        Console.WriteLine("[Scripts] Loading assembly: " + path);

        assembly = Assembly.LoadFrom(path);
    }
}