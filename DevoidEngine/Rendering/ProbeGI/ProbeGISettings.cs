using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering.ProbeGI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ProbeGISettings
    {
        public Vector3 ProbeCount;
        public uint ProbeColorResolution;

        public Vector3 ProbeCountLog2;
        public uint ProbeVisibilityResolution;

        public Vector3 ProbeCountRCP;
        public float IrradianceBorderWidth;

        public float IrradianceBorderWidthRCP;
        public float VisibilityBorderWidth;
        public float VisibilityBorderWidthRCP;
        public float MaxVisibilityDistance;

        public Vector3 VolumeMin;
        public uint ProbeTraceResolution;

        public Vector3 VolumeMax;
        public float _padding1;

        public Vector3 VolumeSize;
        public float _padding2;

        public Vector3 ProbeSpacing;
        public float _padding3;

        public Vector3 ProbeSpacingRCP;
        public float _padding4;

        public Vector3 ProbeGridOrigin;
        public float _padding5;
    }
}
