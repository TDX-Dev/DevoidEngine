using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU.DX11
{
    internal sealed class DX11CommandList : ICommandList
    {
        private readonly DeviceContext deviceContext;

        // binding cache;
        private DX11Framebuffer? currentFramebuffer;


        public DX11CommandList(DeviceContext context) { deviceContext = context; }

        public void Begin() { /* No Op */ }
        public void End() { /* No Op */ }

        public void SetFramebuffer(IFrameBuffer framebuffer)
        {
            DX11Framebuffer dx11Fb = (DX11Framebuffer)framebuffer;
            if (ReferenceEquals(currentFramebuffer, dx11Fb))
                return;

            currentFramebuffer = dx11Fb;

            deviceContext.OutputMerger.SetRenderTargets(dx11Fb.DSV, dx11Fb.RTVs);
        }
    }
}
