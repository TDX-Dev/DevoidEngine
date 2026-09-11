using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Tools.EditorServices
{
    public sealed class EditorServiceRegistry
    {
        private readonly Dictionary<Type, IEditorService> services;

        public EditorServiceRegistry()
        {
            services = [];
        }

        public void Register<T>(T service) where T : class, IEditorService
        {
            services.Add(typeof(T), service);
        }
        public T Get<T>() where T : class, IEditorService
        {
            return (T)services[typeof(T)];
        }
    }
}
