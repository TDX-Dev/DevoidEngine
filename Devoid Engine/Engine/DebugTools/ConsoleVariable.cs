using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.DebugTools
{
    public sealed class ConsoleVariable<T> : ConsoleVariable
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;

        public ConsoleVariable(
            string path,
            Func<T> getter,
            Action<T> setter,
            string description = "")
            : base(path, description)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public override object? Get()
        {
            return getter();
        }

        public override void Set(object? value)
        {
            if (value is T v)
            {
                setter(v);
                return;
            }

            if (typeof(T) == typeof(float) && value is int i)
            {
                setter((T)(object)(float)i);
                return;
            }

            if (typeof(T) == typeof(int) && value is float f)
            {
                setter((T)(object)(int)f);
                return;
            }

            throw new Exception($"Invalid value for {Path}");
        }
    }

    public abstract class ConsoleVariable : ConsoleEntry
    {
        protected ConsoleVariable(string path, string description = "")
            : base(path, description)
        {
        }

        public abstract object? Get();
        public abstract void Set(object? value);
    }
}
