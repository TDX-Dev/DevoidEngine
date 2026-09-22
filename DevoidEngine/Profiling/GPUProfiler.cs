using DevoidGPU;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Profiling
{
    public sealed class GPUProfiler
    {
        private const int FrameCount = 7;
        private const int MaxScopes = 128;
        private const int MaxTimestamps = MaxScopes * 2 + 2;

        private readonly GPUProfileFrame[] frames;
        private readonly int[] stack;

        private int currentFrame;
        private int stackTop;
        private int resolvedFrame = -1;

        public int ScopeCount =>
            resolvedFrame >= 0
                ? frames[resolvedFrame].ScopeCount
                : 0;

        public ReadOnlySpan<GPUProfileScope> Scopes
        {
            get
            {
                if (resolvedFrame < 0)
                    return [];

                GPUProfileFrame frame = frames[resolvedFrame];

                return frame.Scopes.AsSpan(0, frame.ScopeCount);
            }
        }

        public GPUProfiler(IGraphicsDevice device)
        {
            frames = new GPUProfileFrame[FrameCount];
            stack = new int[MaxScopes];

            for (int i = 0; i < FrameCount; i++)
                frames[i] = new GPUProfileFrame(device);
        }

        public void BeginFrame(ICommandList cmd)
        {
            stackTop = 0;

            ResolveFrame();

            GPUProfileFrame frame = frames[currentFrame];

            frame.ScopeCount = 0;
            frame.TimestampCount = 0;

            cmd.BeginTimestampDisjoint(frame.Disjoint);

            WriteTimestamp(cmd, frame);
        }

        public void EndFrame(ICommandList cmd)
        {
            GPUProfileFrame frame = frames[currentFrame];

            WriteTimestamp(cmd, frame);

            cmd.EndTimestampDisjoint(frame.Disjoint);

            frame.Submitted = true;

            currentFrame++;

            if (currentFrame == FrameCount)
                currentFrame = 0;
        }

        private void ResolveFrame()
        {
            int frameIndex = currentFrame - 2;

            if (frameIndex < 0)
                frameIndex += FrameCount;

            GPUProfileFrame frame = frames[frameIndex];

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
                ref GPUProfileScope scope =
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

            resolvedFrame = frameIndex;
            frame.Submitted = false;
        }

        private static void WriteTimestamp(
            ICommandList cmd,
            GPUProfileFrame frame)
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
            GPUProfileFrame frame = frames[currentFrame];

            int parent = stackTop > 0
                ? stack[stackTop - 1]
                : -1;

            int scopeIndex = frame.ScopeCount++;

            ref GPUProfileScope scope =
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

            stack[stackTop++] = scopeIndex;
        }

        [Conditional("PROFILING")]
        public void EndScope(ICommandList cmd)
        {
            GPUProfileFrame frame = frames[currentFrame];

            int scopeIndex = stack[--stackTop];

            ref GPUProfileScope scope =
                ref frame.Scopes[scopeIndex];

            scope.EndTimestamp = frame.TimestampCount;

            WriteTimestamp(cmd, frame);
        }

        private sealed class GPUProfileFrame
        {
            public readonly IGPUTimestampDisjoint Disjoint;
            public readonly IGPUTimestamp[] Timestamps;
            public readonly GPUProfileScope[] Scopes;

            public int TimestampCount;
            public int ScopeCount;
            public bool Submitted;

            public GPUProfileFrame(IGraphicsDevice device)
            {
                Disjoint = device.CreateGPUTimestampDisjoint();

                Timestamps = new IGPUTimestamp[MaxTimestamps];

                Scopes = new GPUProfileScope[MaxScopes];

                for (int i = 0; i < MaxTimestamps; i++)
                    Timestamps[i] = device.CreateGPUTimestamp();
            }
        }
    }
}