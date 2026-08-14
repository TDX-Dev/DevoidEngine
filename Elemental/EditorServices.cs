using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental
{
    public static class EditorServices
    {
        private static readonly Dictionary<Type, object> services = [];

        public static void Register<T>(T service) where T : class
        {
            Type type = typeof(T);
            if (services.ContainsKey(type))
            {
                services[type] = service;
            }
            else
            {
                services.Add(type, service);
            }
        }

        public static T Get<T>() where T : class
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out var service))
            {
                return (T)service;
            }

            throw new InvalidOperationException($"Editor service '{type.Name}' is not registered.");
        }

        public static bool TryGet<T>(out T? service) where T : class
        {
            Type type = typeof(T);
            if (services.TryGetValue(type, out var found))
            {
                service = (T)found;
                return true;
            }

            service = null;
            return false;
        }

        public static void Clear() => services.Clear();
    }
}
