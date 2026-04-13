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
        static Dictionary<string, Func<Component>> scripts = new();

        public static void Register<T>()
            where T : Component, new()
        {
            scripts[typeof(T).Name] = () => new T();
        }

        public static Component Create(string name)
        {
            if (!scripts.TryGetValue(name, out var factory))
                throw new Exception("Script not found: " + name);

            return factory();
        }
    }
}
