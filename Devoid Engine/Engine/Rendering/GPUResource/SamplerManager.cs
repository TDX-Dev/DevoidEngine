using DevoidEngine.Engine.Core;
using DevoidGPU;
using System.Diagnostics;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class SamplerManager
    {
        private uint _nextSamplerHandleID = 0;

        internal Dictionary<uint, ISampler> _samplers = new();

        private RenderCommandPool<CreateSamplerCommand> _createPool;
        private RenderCommandPool<DeleteSamplerCommand> _deletePool;

        public SamplerManager()
        {
            _createPool = new();
            _deletePool = new();
        }


        public void BindSampler(SamplerHandle handle, int slot)
        {
            Debug.Assert(RenderThread.IsRenderThread());

            _samplers[handle.Id].Bind(slot);
        }


        public SamplerHandle CreateSampler(SamplerDescription samplerDescription)
        {
            uint id = ++_nextSamplerHandleID;
            SamplerHandle handle = new(id);

            var cmd = _createPool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;
            cmd.Description = samplerDescription;

            RenderThread.Enqueue(cmd);

            return handle;
        }


        public void DeleteSampler(SamplerHandle handle)
        {
            var cmd = _deletePool.Get();

            cmd.Manager = this;
            cmd.Handle = handle;

            RenderThread.Enqueue(cmd);
        }
    }
}