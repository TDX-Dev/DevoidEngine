using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DevoidRuntime
{
    public static class ScriptLoader
    {
        static Assembly? scriptAssembly;

        public static void LoadScripts()
        {
            var project = ProjectManager.Current!;

            string path = Path.Combine(
                project.TempPath,
                "Scripts",
                "GameScripts.dll"
            );

            if (!File.Exists(path))
            {
                Console.WriteLine("[Scripts] No script assembly found.");
                return;
            }

            Console.WriteLine("[Scripts] Loading: " + path);

            scriptAssembly = Assembly.LoadFrom(path);
            RegisterGeneratedSerializers();
            RegisterScripts();
        }

        static void RegisterScripts()
        {
            foreach (var type in scriptAssembly!.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Component)))
                    continue;

                if (type.IsAbstract)
                    continue;

                Console.WriteLine("[Scripts] Found script: " + type.Name);

                ScriptRegistry.Register(type);
            }
        }

        static void RegisterGeneratedSerializers()
        {
            var registryType =
                scriptAssembly!.GetType(
                    "DevoidEngine.Engine.Serialization.Generated.GeneratedComponentRegistry");

            registryType?
                .GetMethod(
                    "RegisterAll",
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                ?.Invoke(null, null);
        }
    }
}
