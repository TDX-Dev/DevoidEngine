using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.DebugTools
{
    public sealed class ConsoleRegistry
    {
        private readonly Dictionary<string, ConsoleEntry> _entries = new();

        public static ConsoleRegistry Instance { get; } = new();

        public void Register(ConsoleEntry entry)
        {
            _entries[entry.Path] = entry;
        }

        public bool TryGet(string path, out ConsoleEntry entry)
        {
            return _entries.TryGetValue(path, out entry!);
        }

        public IEnumerable<string> GetSuggestions(string prefix)
        {
            foreach (var key in _entries.Keys)
            {
                if (key.StartsWith(prefix))
                    yield return key;
            }
        }

        public IEnumerable<ConsoleEntry> GetAll()
        {
            return _entries.Values;
        }
    }
}
