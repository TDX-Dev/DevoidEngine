using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public interface ISwapchain
    {
        int Width { get; }
        int Height { get; }

        ITexture GetCurrentBackBuffer();

        void Present();
        void Resize(int width, int height);
        void Dispose();
    }
}
