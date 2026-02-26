using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Core
{
    public static class RenderThread
    {
        public static int mainThreadID = -1;
        public static volatile bool MainThreadStarted = false;

        private static ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action>();
        private static ConcurrentQueue<Action> _queueFrameEnd = new ConcurrentQueue<Action>();


        // We add atleast a 3 frame buffer to ensure objects are not being used after queueing for deletion. 
        private static readonly int DeleteDelayFrames = 3;
        private static readonly Queue<Action>[] _deleteBuckets = new Queue<Action>[DeleteDelayFrames];
        private static int _frameIndex = 0;

        static RenderThread()
        {
            for (int i = 0; i < DeleteDelayFrames; i++)
                _deleteBuckets[i] = new Queue<Action>();
        }

        public static bool IsRenderThread() => Thread.CurrentThread.ManagedThreadId == mainThreadID;

        public static void Enqueue(Action action)
        {
            if (IsRenderThread()) 
            { 
                action(); 
                return; 
            } 
            _queue.Enqueue(action); 
        }

        public static void EnqueueDelayedDelete(Action action)
        {
            int targetFrame = (_frameIndex + DeleteDelayFrames - 1) % DeleteDelayFrames;
            _deleteBuckets[targetFrame].Enqueue(action);
        }

        public static void EnqueueFrameEnd(Action action)
        {
            if (IsRenderThread())
            {
                action();
                return;
            }
            _queueFrameEnd.Enqueue(action);
        }


        public static void Execute() 
        { 
            while (_queue.TryDequeue(out var action)) 
                action(); 

        }

        public static void ExecuteFrameEnd()
        {
            while (_queueFrameEnd.TryDequeue(out var action))
                action();

            int bucket = _frameIndex % DeleteDelayFrames;

            while (_deleteBuckets[bucket].Count > 0)
            {
                _deleteBuckets[bucket].Dequeue().Invoke();
            }

            _frameIndex++;
        }

    }
}
