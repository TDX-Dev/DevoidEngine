using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    abstract class RenderCommand
    {
        internal Action<RenderCommand>? ReturnToPool;

        public abstract void Execute();

        internal void Release()
        {
            ReturnToPool?.Invoke(this);
        }
    }

    class BindVertexBufferCommand : RenderCommand
    {
        public VertexBufferHandle handle;
        public int slot;
        public int offset;
        public VertexBufferManager manager = null!;

        public override void Execute()
        {
            if (manager._vertexBuffers.ContainsKey(handle.Id))
                manager._vertexBuffers[handle.Id].Bind(slot, offset);
        }
    }
}
