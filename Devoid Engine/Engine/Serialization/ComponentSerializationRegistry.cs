using DevoidEngine.Engine.Components;
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

        public static Component Deserialize(string type, byte[] data)
        {
            return deserializers[type](data);
        }
    }
}
