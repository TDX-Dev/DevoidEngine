using System.Collections.Concurrent;
using DevoidEngine.Engine.Rendering.GPUResource;

namespace DevoidEngine.Engine.Core
{
    public static class RenderThread
    {
        public static int mainThreadID = -1;
        public static volatile bool MainThreadStarted = false;

        private static ConcurrentQueue<RenderCommand> _queue = new();
        private static ConcurrentQueue<RenderCommand> _queueFrameEnd = new();

        private static int uploadBudgetPerFrame = 7;
        private static ConcurrentQueue<RenderCommand> _gpuUploadQueue = new();

        private static readonly int DeleteDelayFrames = 3;
        private static readonly Queue<RenderCommand>[] _deleteBuckets = new Queue<RenderCommand>[DeleteDelayFrames];
        private static int _frameIndex = 0;

        static RenderThread()
        {
            for (int i = 0; i < DeleteDelayFrames; i++)
                _deleteBuckets[i] = new Queue<RenderCommand>();
        }

        public static bool IsRenderThread() => Thread.CurrentThread.ManagedThreadId == mainThreadID;

        public static void Enqueue(RenderCommand cmd)
        {
            if (IsRenderThread())
            {
                cmd.Execute();
                cmd.Release();
                return;
            }

            _queue.Enqueue(cmd);
        }

        public static void EnqueueDelayedDelete(RenderCommand cmd)
        {
            int targetFrame = (_frameIndex + DeleteDelayFrames - 1) % DeleteDelayFrames;
            _deleteBuckets[targetFrame].Enqueue(cmd);
        }

        public static void EnqueueFrameEnd(RenderCommand cmd)
        {
            if (IsRenderThread())
            {
                cmd.Execute();
                cmd.Release();
                return;
            }

            _queueFrameEnd.Enqueue(cmd);
        }

        public static void EnqueueUpload(RenderCommand cmd)
        {
            if (IsRenderThread())
            {
                cmd.Execute();
                cmd.Release();
                return;
            }

            _gpuUploadQueue.Enqueue(cmd);
        }

        public static void Execute()
        {
            while (_queue.TryDequeue(out var cmd))
            {
                cmd.Execute();
                cmd.Release();
            }

            for (int i = 0; i < uploadBudgetPerFrame; i++)
            {
                if (_gpuUploadQueue.TryDequeue(out var cmd))
                {
                    cmd.Execute();
                    cmd.Release();
                }
                else break;
            }
        }

        public static void ExecuteFrameEnd()
        {
            while (_queueFrameEnd.TryDequeue(out var cmd))
            {
                cmd.Execute();
                cmd.Release();
            }

            int bucket = _frameIndex % DeleteDelayFrames;

            while (_deleteBuckets[bucket].Count > 0)
            {
                var cmd = _deleteBuckets[bucket].Dequeue();
                cmd.Execute();
                cmd.Release();
            }

            _frameIndex++;
        }
    }
}