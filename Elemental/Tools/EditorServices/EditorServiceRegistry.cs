using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Tools.EditorServices
{
    public sealed class EditorServiceRegistry
    {
        private readonly Dictionary<Type, IEditorService> _services;

        public EditorServiceRegistry()
        {
            _services = [];
        }

        public void Register<T>(T service) where T : class, IEditorService
        {
            _services.Add(typeof(T), service);
        }
        public T Get<T>() where T : class, IEditorService
        {
            return (T)_services[typeof(T)];
        }
    }
}
