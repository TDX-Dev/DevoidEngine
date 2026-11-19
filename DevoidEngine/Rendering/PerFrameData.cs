using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerFrameData
    {
        public int FrameIndex;
    }
}
