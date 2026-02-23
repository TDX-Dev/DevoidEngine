using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public class RenderCommandQueue
    {
        private static readonly ConcurrentQueue<Action> actions = new();

        public static void Queue(Action action) => actions.Enqueue(action);

        public static void Execute()
        {
            while (actions.TryDequeue(out var action))
                action();
        }


    }
}
