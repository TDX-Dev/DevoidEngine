using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.DebugTools
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ConsoleVariableAttribute : Attribute
    {
        public string Path { get; }

        public ConsoleVariableAttribute(string path)
        {
            Path = path;
        }
    }
}
