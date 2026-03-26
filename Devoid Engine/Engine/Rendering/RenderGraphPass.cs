using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Rendering
{
    public abstract class RenderGraphPass
    {
        internal List<string> Reads = new();
        internal List<string> Writes = new();

        protected void Read(string resource)
        {
            Reads.Add(resource);
        }

        protected void Write(string resource)
        {
            Writes.Add(resource);
        }

        public virtual Texture2D OutputTexture => null;

        public abstract void Setup();
        public abstract void Execute(RenderGraphContext ctx);
        public abstract void Resize(int width, int height);
    }
}