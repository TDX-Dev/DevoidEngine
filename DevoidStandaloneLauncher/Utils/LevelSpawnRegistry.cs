using Assimp;

namespace DevoidStandaloneLauncher.Utils
{
    public static class LevelSpawnRegistry
    {
        private static readonly Dictionary<string, Action<Node, Scene>> _registry = new();
        private static readonly List<Action<Node, Assimp.Light>> _lightHooks = new();

        private static Action<Node, Scene> _fallback;

        public static void Register(string key, Action<Node, Scene> action)
        {
            _registry[key] = action;
        }

        public static void RegisterLight(Action<Node, Assimp.Light> action)
        {
            _lightHooks.Add(action);
        }

        public static void RegisterFallBack(Action<Node, Scene> action)
        {
            _fallback = action;
        }

        public static void Execute(string key, Node node, Scene scene)
        {
            string normalized = ExtractSpawnKey(key);

            if (_registry.TryGetValue(normalized, out var action))
            {
                action(node, scene);
                return;
            }

            // fallback
            _fallback?.Invoke(node, scene);
        }

        public static void ExecuteLight(string key, Node node, Assimp.Light light)
        {
            foreach (var hook in _lightHooks)
                hook(node, light);
        }

        private static string ExtractSpawnKey(string name)
        {
            int dotIndex = name.LastIndexOf('.');
            if (dotIndex > 0)
            {
                string suffix = name.Substring(dotIndex + 1);
                if (int.TryParse(suffix, out _))
                    name = name.Substring(0, dotIndex);
            }

            int colonIndex = name.IndexOf(':');
            if (colonIndex >= 0 && colonIndex < name.Length - 1)
            {
                return name.Substring(colonIndex + 1);
            }

            return name;
        }
    }
}