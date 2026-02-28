using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Utils
{
    public static class LevelSpawnRegistry
    {
        private static readonly Dictionary<string, Action<Node, Scene>> _registry
                = new();

        private static readonly List<Action<Node, Assimp.Light>> _lightHooks
            = new();

        public static void Register(string key, Action<Node, Scene> action)
        {
            _registry[key] = action;
        }

        public static void RegisterLight(Action<Node, Assimp.Light> action)
        {
            _lightHooks.Add(action);
        }

        public static void Execute(string key, Node node, Scene scene)
        {
            string normalized = ExtractSpawnKey(key);

            if (_registry.TryGetValue(normalized, out var action))
                action(node, scene);
        }

        public static void ExecuteLight(string key, Node node, Assimp.Light light)
        {
            foreach (var hook in _lightHooks)
                hook(node, light);
        }

        private static string ExtractSpawnKey(string name)
        {
            // Remove Blender numeric suffix (.001, .002 etc.)
            int dotIndex = name.LastIndexOf('.');
            if (dotIndex > 0)
            {
                string suffix = name.Substring(dotIndex + 1);
                if (int.TryParse(suffix, out _))
                    name = name.Substring(0, dotIndex);
            }

            // Extract everything AFTER ':'
            int colonIndex = name.IndexOf(':');
            if (colonIndex >= 0 && colonIndex < name.Length - 1)
            {
                return name.Substring(colonIndex + 1);
            }

            return name; // fallback if no colon
        }
    }
}
