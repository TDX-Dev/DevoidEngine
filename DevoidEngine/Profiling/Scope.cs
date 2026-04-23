using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Profiling
{
    public struct Scope
    {
        public long BeginTick;
        public long EndTick;

        public int Parent;

        public string CallerFileName;
        public int CallerLineNumber;
        public string CallerMemberName;

        public string? CustomName;
    }
}
