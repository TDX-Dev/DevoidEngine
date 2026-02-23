using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public sealed class GraphicsFence
    {
        private readonly ManualResetEventSlim _event = new(false);

        internal void Signal()
        {
            _event.Set();
        }

        public void Wait()
        {
            _event.Wait();
        }
    }
}
