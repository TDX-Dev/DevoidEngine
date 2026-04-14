using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.ProjectSystem;
using DevoidEngine.Engine.Serialization;
using ElementalEditor.Scripting;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

public static class ScriptAssemblyLoader
{
    static ScriptLoadContext? loadContext;
    static Assembly? assembly;

    static readonly List<Type> scriptComponentTypes = new();
    public static List<string> ScriptComponentTypeNames { get; } = new();

    public static Assembly? Assembly => assembly;
    public static WeakReference weakRef;

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

        ExecuteLoad(path);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void ExecuteLoad(string path)
    {
        loadContext = new ScriptLoadContext();

        using FileStream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );

        assembly = loadContext.LoadFromStream(stream);

        weakRef = new WeakReference(loadContext, trackResurrection: true);

        RegisterGeneratedSerializers();
        CacheScriptTypes();
    }

    public static void Unload()
    {
        if (assembly == null)
            return;

        var scriptAsm = assembly;

        ClearGeneratedSerializerDelegates(scriptAsm);
        ComponentSerializationRegistry.ClearScriptSerializers();

        scriptComponentTypes.Clear();
        ScriptComponentTypeNames.Clear();

        var alc = loadContext;

        assembly = null;
        loadContext = null;

        WeakReference wr = weakRef;

        ExecuteUnload(alc, wr);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void ExecuteUnload(
    AssemblyLoadContext alc,
    WeakReference wr)
    {
        alc.Unload();

        for (int i = 0; wr.IsAlive && i < 10; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        bool scriptsStillLoaded =
    AppDomain.CurrentDomain
        .GetAssemblies()
        .Any(a => a.GetName().Name == "GameScripts");

        Console.WriteLine("Scripts still in AppDomain: " + scriptsStillLoaded);

        Console.WriteLine("ALC alive: " + wr.IsAlive);

        if (wr.IsAlive)
        {
            Console.WriteLine("Still loaded assemblies:");

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.FullName.Contains("GameScripts"))
                    Console.WriteLine("Still loaded: " + asm.FullName);
            }

            //Debugger.Break();
        }
    }

    static void FindEventReferences(Assembly scriptAsm)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                if (type == null)
                    continue;

                foreach (var evt in type.GetEvents(
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic))
                {
                    var field = type.GetField(
                        evt.Name,
                        BindingFlags.Static |
                        BindingFlags.NonPublic);

                    if (field == null)
                        continue;

                    var del = field.GetValue(null) as Delegate;

                    if (del == null)
                        continue;

                    foreach (var d in del.GetInvocationList())
                    {
                        if (d.Method.DeclaringType.Assembly == scriptAsm)
                        {
                            Console.WriteLine($"Event leak: {type.FullName}.{evt.Name}");
                        }
                    }
                }
            }
        }
    }

    static void FindTypeReferences(Assembly scriptAsm)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                if (type == null)
                    continue;

                foreach (var field in type.GetFields(
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic))
                {
                    object value;

                    try
                    {
                        value = field.GetValue(null);
                    }
                    catch
                    {
                        continue;
                    }

                    if (value == null)
                        continue;

                    var valueType = value.GetType();

                    // direct Type reference
                    if (value is Type t && t.Assembly == scriptAsm)
                    {
                        Console.WriteLine($"Type leak: {type.FullName}.{field.Name}");
                    }

                    // instance reference
                    if (valueType.Assembly == scriptAsm)
                    {
                        Console.WriteLine(
                            $"Instance leak: {type.FullName}.{field.Name} -> {valueType}");
                    }

                    // collections
                    if (value is IEnumerable enumerable &&
    valueType != typeof(string) &&
    !(value is IDictionary) &&
    valueType.Namespace != null &&
    !valueType.Namespace.StartsWith("System"))
                    {
                        IEnumerator? enumerator = null;

                        try
                        {
                            enumerator = enumerable.GetEnumerator();
                        }
                        catch
                        {
                            continue;
                        }

                        if (enumerator == null)
                            continue;

                        while (true)
                        {
                            object? item;

                            try
                            {
                                if (!enumerator.MoveNext())
                                    break;

                                item = enumerator.Current;
                            }
                            catch
                            {
                                break;
                            }

                            if (item == null)
                                continue;

                            if (item is Type it && it.Assembly == scriptAsm)
                                Console.WriteLine($"Collection type leak: {type.FullName}.{field.Name}");

                            if (item.GetType().Assembly == scriptAsm)
                                Console.WriteLine($"Collection instance leak: {type.FullName}.{field.Name}");
                        }
                    }
                }
            }
        }
    }

    static void PrintDelegateReferences(Assembly scriptAsm)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = asm.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(t => t != null).ToArray();
            }

            foreach (var type in types)
            {
                if (type == null)
                    continue;

                if (type.ContainsGenericParameters)
                    continue;

                foreach (var field in type.GetFields(
                    BindingFlags.Static |
                    BindingFlags.NonPublic |
                    BindingFlags.Public))
                {
                    if (field.FieldType.ContainsGenericParameters)
                        continue;

                    if (!typeof(Delegate).IsAssignableFrom(field.FieldType))
                        continue;

                    Delegate del;

                    try
                    {
                        del = field.GetValue(null) as Delegate;
                    }
                    catch
                    {
                        continue;
                    }

                    if (del == null)
                        continue;

                    foreach (var d in del.GetInvocationList())
                    {
                        if (d.Method.DeclaringType.Assembly == scriptAsm)
                        {
                            Console.WriteLine(
                                $"Delegate reference: {type.FullName}.{field.Name}");
                        }
                    }
                }
            }
        }
    }

    static void ClearGeneratedSerializerDelegates(Assembly scriptAsm)
    {
        var registryType =
            scriptAsm.GetType(
                "DevoidEngine.Engine.Serialization.Generated.GeneratedComponentRegistry");

        if (registryType == null)
            return;

        foreach (var nested in registryType.GetNestedTypes(
            BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (!nested.Name.StartsWith("<>O"))
                continue;

            foreach (var field in nested.GetFields(
                BindingFlags.Static |
                BindingFlags.NonPublic |
                BindingFlags.Public))
            {
                if (typeof(Delegate).IsAssignableFrom(field.FieldType))
                {
                    field.SetValue(null, null);
                    Console.WriteLine($"Cleared delegate: {nested.Name}.{field.Name}");
                }
            }
        }
    }

    static void CacheScriptTypes()
    {
        ScriptComponentTypeNames.Clear();

        if (assembly == null)
            return;

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsSubclassOf(typeof(Component)))
                continue;

            if (type.IsAbstract)
                continue;

            ScriptComponentTypeNames.Add(type.FullName!);
        }
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
}