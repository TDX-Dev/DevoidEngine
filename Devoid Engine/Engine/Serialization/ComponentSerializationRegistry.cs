using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Serialization
{
    public static class ComponentSerializationRegistry
    {
        private static readonly Dictionary<Type, Func<Component, byte[]>> engineSerializers = new();
        private static readonly Dictionary<string, Func<byte[], Component>> engineDeserializers = new();

        private static readonly Dictionary<string, Func<Component, byte[]>> scriptSerializers = new();
        private static readonly Dictionary<string, Func<byte[], Component>> scriptDeserializers = new();

        public static Component? FindComponent(GameObject go, string typeName)
        {
            foreach (var c in go.Components)
            {
                if (c.GetType().AssemblyQualifiedName == typeName)
                    return c;
            }

            return null;
        }

        public static void Register(
            Type type,
            Func<Component, byte[]> serialize,
            Func<byte[], Component> deserialize)
        {
            bool isScript = type.Assembly.GetName().Name == "GameScripts";

            if (isScript)
            {
                
                scriptSerializers[type.FullName!] = serialize;
                scriptDeserializers[type.FullName!] = deserialize;
            }
            else
            {
                engineSerializers[type] = serialize;
                engineDeserializers[type.FullName!] = deserialize;
            }
        }

        public static byte[] Serialize(Component component)
        {
            var type = component.GetType();

            if (scriptSerializers.TryGetValue(type.FullName!, out var s))
                return s(component);

            if (engineSerializers.TryGetValue(type, out var e))
                return e(component);

            throw new Exception("Serializer missing for " + type.FullName);
        }

        public static Component? Deserialize(string type, byte[] data)
        {
            if (scriptDeserializers.TryGetValue(type, out var s))
                return s(data);

            if (engineDeserializers.TryGetValue(type, out var e))
                return e(data);

            Console.WriteLine("Deserializer missing for " + type);
            return null;
        }

        public static void ClearScriptSerializers()
        {
            scriptSerializers.Clear();
            scriptDeserializers.Clear();
        }
    }
}
