using DevoidGPU;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Profiling
{
    public sealed class GpuProfiler
    {
        private const int FrameCount = 7;
        private const int MaxScopes = 128;
        private const int MaxTimestamps = MaxScopes * 2 + 2;

        private readonly GpuProfileFrame[] _frames;
        private readonly int[] _stack;

        private int _currentFrame;
        private int _stackTop;
        private int _resolvedFrame = -1;

        public int ScopeCount =>
            _resolvedFrame >= 0
                ? _frames[_resolvedFrame].ScopeCount
                : 0;

        public ReadOnlySpan<GpuProfileScope> Scopes
        {
            get
            {
                if (_resolvedFrame < 0)
                    return [];

                GpuProfileFrame frame = _frames[_resolvedFrame];

                return frame.Scopes.AsSpan(0, frame.ScopeCount);
            }
        }

        public GpuProfiler(IGraphicsDevice device)
        {
            _frames = new GpuProfileFrame[FrameCount];
            _stack = new int[MaxScopes];

            for (int i = 0; i < FrameCount; i++)
                _frames[i] = new GpuProfileFrame(device);
        }

        public void BeginFrame(ICommandList cmd)
        {
            _stackTop = 0;

            ResolveFrame();

            GpuProfileFrame frame = _frames[_currentFrame];

            frame.ScopeCount = 0;
            frame.TimestampCount = 0;

            cmd.BeginTimestampDisjoint(frame.Disjoint);

            WriteTimestamp(cmd, frame);
        }

        public void EndFrame(ICommandList cmd)
        {
            GpuProfileFrame frame = _frames[_currentFrame];

            WriteTimestamp(cmd, frame);

            cmd.EndTimestampDisjoint(frame.Disjoint);

            frame.Submitted = true;

            _currentFrame++;

            if (_currentFrame == FrameCount)
                _currentFrame = 0;
        }

        private void ResolveFrame()
        {
            int frameIndex = _currentFrame - 2;

            if (frameIndex < 0)
                frameIndex += FrameCount;

            GpuProfileFrame frame = _frames[frameIndex];

            if (!frame.Submitted)
                return;

            if (!frame.Disjoint.TryGetResult(
                out long frequency,
                out bool disjoint))
            {
                return;
            }

            if (disjoint || frequency <= 0)
                return;

            for (int i = 0; i < frame.ScopeCount; i++)
            {
                ref GpuProfileScope scope =
                    ref frame.Scopes[i];

                if (!frame.Timestamps[scope.BeginTimestamp]
                    .TryGetTimestamp(out ulong begin))
                {
                    return;
                }

                if (!frame.Timestamps[scope.EndTimestamp]
                    .TryGetTimestamp(out ulong end))
                {
                    return;
                }

                scope.DurationMilliseconds =
                    (end - begin) * 1000.0 / frequency;

                scope.HasResult = true;
            }

            _resolvedFrame = frameIndex;
            frame.Submitted = false;
        }

        private static void WriteTimestamp(
            ICommandList cmd,
            GpuProfileFrame frame)
        {
            cmd.WriteTimestamp(
                frame.Timestamps[frame.TimestampCount++]);
        }

        [Conditional("PROFILING")]
        public void BeginScope(
            ICommandList cmd,
            string? customName = null,
            [CallerFilePath] string file = "",
            [CallerLineNumber] int line = 0,
            [CallerMemberName] string member = "")
        {
            GpuProfileFrame frame = _frames[_currentFrame];

            int parent = _stackTop > 0
                ? _stack[_stackTop - 1]
                : -1;

            int scopeIndex = frame.ScopeCount++;

            ref GpuProfileScope scope =
                ref frame.Scopes[scopeIndex];

            scope.DurationMilliseconds = 0;
            scope.HasResult = false;
            scope.Parent = parent;

            scope.CallerFileName = file;
            scope.CallerLineNumber = line;
            scope.CallerMemberName = member;
            scope.CustomName = customName;

            scope.BeginTimestamp = frame.TimestampCount;

            WriteTimestamp(cmd, frame);

            _stack[_stackTop++] = scopeIndex;
        }

        [Conditional("PROFILING")]
        public void EndScope(ICommandList cmd)
        {
            GpuProfileFrame frame = _frames[_currentFrame];

            int scopeIndex = _stack[--_stackTop];

            ref GpuProfileScope scope =
                ref frame.Scopes[scopeIndex];

            scope.EndTimestamp = frame.TimestampCount;

            WriteTimestamp(cmd, frame);
        }

        private sealed class GpuProfileFrame
        {
            public readonly IGpuTimestampDisjoint Disjoint;
            public readonly IGpuTimestamp[] Timestamps;
            public readonly GpuProfileScope[] Scopes;

            public int TimestampCount;
            public int ScopeCount;
            public bool Submitted;

            public GpuProfileFrame(IGraphicsDevice device)
            {
                Disjoint = device.CreateGpuTimestampDisjoint();

                Timestamps = new IGpuTimestamp[MaxTimestamps];

                Scopes = new GpuProfileScope[MaxScopes];

                for (int i = 0; i < MaxTimestamps; i++)
                    Timestamps[i] = device.CreateGpuTimestamp();
            }
        }
    }
}