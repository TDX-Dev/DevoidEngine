using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.DebugTools
{
    public sealed class ConsoleCommand : ConsoleEntry
    {
        public Type[] ParameterTypes { get; }
        public Type ReturnType { get; }

        private readonly Func<object?[], object?> _invoke;

        public ConsoleCommand(
            string path,
            Type[] parameters,
            Type returnType,
            Func<object?[], object?> invoke,
            string description = "")
            : base(path, description)
        {
            ParameterTypes = parameters;
            ReturnType = returnType;
            _invoke = invoke;
        }

        public object? Execute(object?[] args)
        {
            return _invoke(args);
        }
    }
}
