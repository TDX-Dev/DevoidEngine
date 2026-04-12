using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.ProjectSystem;
using DevoidEngine.Engine.Serialization;
using ElementalEditor.Scripting;
using System.Reflection;

public static class ScriptAssemblyLoader
{
    static ScriptLoadContext? loadContext;
    static Assembly? assembly;

    static readonly List<Type> scriptComponentTypes = new();

    public static Assembly? Assembly => assembly;

    public static void Load()
    {
        var project = ProjectManager.Current!;

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

        // remove previous script serializers
        UnregisterScriptSerializers();

        // unload previous scripts
        Unload();

        loadContext = new ScriptLoadContext();

        using FileStream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );

        assembly = loadContext.LoadFromStream(stream);

        RegisterGeneratedSerializers();
        CacheScriptTypes();
    }

    static void CacheScriptTypes()
    {
        scriptComponentTypes.Clear();

        if (assembly == null)
            return;

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(Component)))
                continue;

            if (type.IsAbstract)
                continue;

            scriptComponentTypes.Add(type);
        }
    }

    static void UnregisterScriptSerializers()
    {
        foreach (var type in scriptComponentTypes)
        {
            ComponentSerializationRegistry.Unregister(type);
        }

        scriptComponentTypes.Clear();
    }

    static void RegisterGeneratedSerializers()
    {
        if (assembly == null)
            return;

        var registryType =
            assembly.GetType(
                "DevoidEngine.Engine.Serialization.Generated.GeneratedComponentRegistry");

        registryType?
            .GetMethod(
                "RegisterAll",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic)?
            .Invoke(null, null);
    }

    public static void Unload()
    {
        if (loadContext == null)
            return;

        Console.WriteLine("[Scripts] Unloading previous scripts...");

        loadContext.Unload();
        loadContext = null;
        assembly = null;

        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}