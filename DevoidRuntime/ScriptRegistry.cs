using DevoidEngine.Engine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidRuntime
{
    public static class ScriptRegistry
    {
        static Dictionary<string, Type> scripts = new();

        public static void Register(Type type)
        {
            scripts[type.Name] = type;
        }

        public static Component Create(string name)
        {
            if (!scripts.TryGetValue(name, out var type))
                throw new Exception("Script not found: " + name);

            return (Component)Activator.CreateInstance(type)!;
        }

        public static IEnumerable<Type> GetScripts()
        {
            return scripts.Values;
        }
    }
}
