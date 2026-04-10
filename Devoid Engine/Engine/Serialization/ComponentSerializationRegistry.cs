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

        public static void Initialize()
        {

        }

        public static void Register<T>(
            Func<T, byte[]> serialize,
            Func<byte[], T> deserialize)
            where T : Component
        {
            serializers[typeof(T)] = c => serialize((T)c);
            deserializers[typeof(T).FullName!] = data => deserialize(data);
        }

        public static byte[] Serialize(Component component)
        {
            return serializers[component.GetType()](component);
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
    }
}
