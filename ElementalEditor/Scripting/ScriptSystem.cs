using DevoidEngine.Engine.ProjectSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Scripting
{
    public static class ScriptSystem
    {
        public static void Initialize()
        {
            var project = ProjectManager.Current!;

            string dll = Path.Combine(
                project.TempPath,
                "Scripts",
                "GameScripts.dll"
            );

            if (!File.Exists(dll))
                return;

            Console.WriteLine("[Scripts] Loading assembly: " + dll);

            Assembly asm = Assembly.LoadFrom(dll);

            RegisterScripts(asm);
        }

        static void RegisterScripts(Assembly asm)
        {
            foreach (var type in asm.GetTypes())
            {
                if (!type.IsSubclassOf(typeof(Component)))
                    continue;

                if (type.IsAbstract)
                    continue;

                Console.WriteLine("[Scripts] Registered: " + type.FullName);

                ScriptRegistry.Register(type);
            }
        }

    }
}
