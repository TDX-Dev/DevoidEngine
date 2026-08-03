using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.PostProcessing
{
    public abstract class PostProcessPass
    {
        internal readonly List<string> Reads = [];
        internal readonly List<string> Writes = [];

        protected void Read(string name)
            => Reads.Add(name);

        protected void Write(string name)
            => Writes.Add(name);

        public abstract void Setup();

        public abstract void Execute(PostProcessContext ctx);

        public virtual void Resize(int width, int height)
        {
        }
    }
}
