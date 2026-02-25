using DevoidEngine.Engine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering.GPUResource
{
    public class SamplerManager
    {
        private uint _nextSamplerHandleID = 0;
        private Dictionary<uint, ISampler> _samplers = new Dictionary<uint, ISampler>();

        public void BindSampler(SamplerHandle handle, int slot)
        {
            Debug.Assert(RenderThread.IsRenderThread());
            _samplers[handle.Id].Bind(slot);
        }

        public SamplerHandle CreateSampler(SamplerDescription samplerDescription)
        {
            uint id = ++_nextSamplerHandleID;
            SamplerHandle samplerHandle = new SamplerHandle(id);

            RenderThread.Enqueue(() =>
            {


                _samplers[samplerHandle.Id]
                    = Renderer.graphicsDevice.CreateSampler(
                        samplerDescription
                    );
            });

            return samplerHandle;
        }


    }
}
