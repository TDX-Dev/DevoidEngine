using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public enum GizmoDepthMode
    {
        Test,
        Always
    }

    public enum GizmoOutlineMode
    {
        None,
        Geometry,
        ScreenSpace
    }

    public struct GizmoStyle
    {
        public Vector4 Color;

        public GizmoDepthMode Depth;

        public GizmoOutlineMode Outline;

        public Vector4 OutlineColor;

        public float OutlineWidth;
    }
}
