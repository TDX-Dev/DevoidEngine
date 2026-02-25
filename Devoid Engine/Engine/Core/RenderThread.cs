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
        public static void Execute() 
        { 
            while (_queue.TryDequeue(out var action)) 
                action(); 
        }

    }
}
