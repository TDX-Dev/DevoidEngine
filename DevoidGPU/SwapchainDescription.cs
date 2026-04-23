using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public struct SwapchainDescription
    {
        public IntPtr WindowHandle;
        public int Width;
        public int Height;

        public TextureFormat Format;
        public int BufferCount;
        public bool VSync;

        // This tells DX11 or other api (if implemented) what refresh rate to switch when
        // entering exclusive fullscreen mode.
        public Vector2 RefreshRate; // X defines digits before ., Y defines digits after

        public bool Windowed;

        public TextureSampleDescription Samples; // X defines sample count, Y defines quality per sample.


    }
}
