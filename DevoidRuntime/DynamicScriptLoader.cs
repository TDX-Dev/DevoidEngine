#if SCRIPT_DYNAMIC
using System.Reflection;
using System.IO;

using DevoidEngine.Engine.ProjectSystem;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Serialization;

namespace DevoidRuntime
{
    static class DynamicScriptLoader
    {
        public static void Load()
        {
            var project = ProjectManager.Current;

            string path = Path.Combine(
                project.TempPath,
                "Scripts",
                "GameScripts.dll");

            Console.WriteLine(path);

            if (!File.Exists(path))
                return;

            var assembly = Assembly.LoadFrom(path);

            var registerMethod = typeof(ScriptRegistry).GetMethod("Register");

            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Component)))
                    continue;

                if (type.IsAbstract)
                    continue;

                var genericRegister = registerMethod.MakeGenericMethod(type);
                genericRegister.Invoke(null, null);
            }

            var registryType = assembly.GetType(
                "DevoidEngine.Engine.Serialization.Generated.GeneratedComponentRegistry");

            var method = registryType?.GetMethod(
                "RegisterAll",
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            method?.Invoke(null, null);
        }
    }
}
#endif