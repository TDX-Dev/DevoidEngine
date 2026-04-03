using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.DebugTools
{
    public abstract class ConsoleEntry
    {
        public string Path { get; }
        public string Description { get; }

        protected ConsoleEntry(string path, string description = "")
        {
            Path = path;
            Description = description;
        }
    }
}
