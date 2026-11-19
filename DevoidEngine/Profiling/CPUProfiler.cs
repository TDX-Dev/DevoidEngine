using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Profiling
{
    public class CpuProfiler
    {

        private readonly List<Scope> _scopes = new(128);
        private readonly int[] _stack = new int[128];
        private int _stackTop = 0;

        public IReadOnlyList<Scope> Scopes => _scopes;

        public void BeginFrame()
        {
            _scopes.Clear();
            _stackTop = 0;
        }

        [Conditional("PROFILING")]
        public void BeginScope(
            string? customName = null,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            int parent = _stackTop > 0 ? _stack[_stackTop - 1] : -1;

            int index = _scopes.Count;

            _scopes.Add(new Scope
            {
                BeginTick = Stopwatch.GetTimestamp(),
                Parent = parent,

                CallerFileName = file,
                CallerLineNumber = line,
                CallerMemberName = member,
                CustomName = customName
            });

            _stack[_stackTop++] = index;
        }

        [Conditional("PROFILING")]
        public void EndScope()
        {
            int index = _stack[--_stackTop];

            var scope = _scopes[index];
            scope.EndTick = Stopwatch.GetTimestamp();

            _scopes[index] = scope;
        }
    }
}
