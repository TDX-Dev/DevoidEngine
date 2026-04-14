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

        RefreshScriptBehaviours();
    }

    static void CacheScriptTypes()
    {
        scriptComponentTypes.Clear();

        if (assembly == null)
            return;

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(ScriptBehaviour)))
                continue;

            if (type.IsAbstract)
                continue;

            scriptComponentTypes.Add(type);

            Console.WriteLine("[Script] " + type.FullName);
        }
    }

    static void RefreshScriptBehaviours()
    {
        if (assembly == null)
            return;

        foreach (var scriptComp in ScriptComponentRegistry.All)
        {
            var type = assembly.GetType(scriptComp.ScriptType);

            if (type == null)
            {
                Console.WriteLine($"[Scripts] Missing type: {scriptComp.ScriptType}");
                continue;
            }

            var behaviour = (ScriptBehaviour)Activator.CreateInstance(type)!;

            scriptComp.Bind(behaviour!);
        }
    }

    static void UnregisterScriptSerializers()
    {
        foreach (var type in scriptComponentTypes)
        {
            ComponentSerializationRegistry.Unregister(type.FullName!);
        }

        scriptComponentTypes.Clear();
    }

    static void RegisterGeneratedSerializers()
    {
        if (assembly == null)
            throw new Exception("[Scripts] Assembly not loaded.");

        var registryType = assembly.GetType(
            "DevoidEngine.Engine.Serialization.Generated.GeneratedComponentRegistry");

        if (registryType == null)
            throw new Exception(
                "[Scripts] GeneratedComponentRegistry not found. " +
                "Source generator likely did not run.");

        var method = registryType.GetMethod(
            "RegisterAll",
            BindingFlags.Static |
            BindingFlags.Public |
            BindingFlags.NonPublic);

        if (method == null)
            throw new Exception(
                "[Scripts] RegisterAll() missing from GeneratedComponentRegistry.");

        Console.WriteLine("[Scripts] Registering generated serializers...");
        method.Invoke(null, null);
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