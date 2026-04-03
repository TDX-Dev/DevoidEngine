using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.DebugTools
{
    public static class ConsoleExecutor
    {
        public static object? Execute(string path, object?[] args)
        {
            if (!ConsoleRegistry.Instance.TryGet(path, out var entry))
                throw new Exception($"Console command not found: {path}");

            if (entry is ConsoleCommand cmd)
                return cmd.Execute(args);

            if (entry is ConsoleVariable cvar)
            {
                if (args.Length == 0)
                    return cvar.Get();

                cvar.Set(args[0]);
                return null;
            }

            return null;
        }
    }
}
