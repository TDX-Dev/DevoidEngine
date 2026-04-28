using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Profiling
{
    public class CPUProfiler
    {

        private readonly List<Scope> scopes = new(128);
        private readonly int[] stack = new int[128];
        private int stackTop = 0;

        public IReadOnlyList<Scope> Scopes => scopes;

        public void BeginFrame()
        {
            scopes.Clear();
            stackTop = 0;
        }

        [Conditional("PROFILING")]
        public void BeginScope(
            string? customName = null,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            int parent = stackTop > 0 ? stack[stackTop - 1] : -1;

            int index = scopes.Count;

            scopes.Add(new Scope
            {
                BeginTick = Stopwatch.GetTimestamp(),
                Parent = parent,

                CallerFileName = file,
                CallerLineNumber = line,
                CallerMemberName = member,
                CustomName = customName
            });

            stack[stackTop++] = index;
        }

        [Conditional("PROFILING")]
        public void EndScope()
        {
            int index = stack[--stackTop];

            var scope = scopes[index];
            scope.EndTick = Stopwatch.GetTimestamp();

            scopes[index] = scope;
        }
    }
}
