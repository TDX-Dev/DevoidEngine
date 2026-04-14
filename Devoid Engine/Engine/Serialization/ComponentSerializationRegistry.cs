using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Serialization
{
    public static class ComponentSerializationRegistry
    {
        private static readonly Dictionary<Type, Func<Component, byte[]>> serializers = new();
        private static readonly Dictionary<string, Func<byte[], Component>> deserializers = new();
        static readonly HashSet<Type> scriptTypes = new();
        public static void Initialize()
        {

        }

        public static void Register<T>(
            Func<T, byte[]> serialize,
            Func<byte[], T> deserialize,
            bool isScript = false)
            where T : Component
        {
            serializers[typeof(T)] = c => serialize((T)c);
            deserializers[typeof(T).FullName!] = data => deserialize(data);

            if (isScript)
                scriptTypes.Add(typeof(T));
        }

        public static byte[] Serialize(Component component)
        {
            if (!serializers.TryGetValue(component.GetType(), out var serializer))
            {
                Console.WriteLine(
                    $"[Serialization] Missing serializer for {component.GetType().FullName}");

                return Array.Empty<byte>();
            }

            return serializer(component);
        }

        public static Component? Deserialize(string type, byte[] data)
        {
            if (!deserializers.TryGetValue(type, out var fn))
            {
                Console.WriteLine($"[Serialization] Unknown component type: {type}");
                return null;
            }

            try
            {
                return fn(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Serialization] Failed to deserialize {type}: {e.Message}");
                return null;
            }
        }

        public static Component? FindComponent(GameObject go, string typeName)
        {
            foreach (var c in go.Components)
            {
                if (c.GetType().AssemblyQualifiedName == typeName)
                    return c;
            }

            return null;
        }

        public static void ClearScriptSerializers()
        {
            foreach (var type in scriptTypes)
            {
                serializers.Remove(type);
                deserializers.Remove(type.FullName!);
            }

            scriptTypes.Clear();
        }

        public static void Unregister(string typeName)
        {
            deserializers.Remove(typeName);

            var toRemove = serializers
                .Where(p => p.Key.FullName == typeName)
                .Select(p => p.Key)
                .ToList();

            foreach (var t in toRemove)
                serializers.Remove(t);
        }
    }
}
