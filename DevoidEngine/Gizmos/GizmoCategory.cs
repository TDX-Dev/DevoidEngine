using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    [Flags]
    public enum GizmoCategory
    {
        None = 0,
        Lighting = 1 << 0,
        Physics = 1 << 1,
        Cameras = 1 << 2,
        Audio = 1 << 3,
        AI = 1 << 4,
        Gameplay = 1 << 5,
        Navigation = 1 << 6,
        Custom = 1 << 7,

        All = ~0
    }

    public static class GizmoCategoryColors
    {
        public static Vector4 Get(GizmoCategory category)
        {
            return category switch
            {
                GizmoCategory.Lighting => new Vector4(1.0f, 0.8f, 0.2f, 1.0f),
                GizmoCategory.Physics => new Vector4(0.2f, 1.0f, 0.2f, 1.0f),
                GizmoCategory.Cameras => new Vector4(0.2f, 0.6f, 1.0f, 1.0f),
                GizmoCategory.Audio => new Vector4(0.8f, 0.2f, 1.0f, 1.0f),
                GizmoCategory.AI => new Vector4(1.0f, 0.3f, 0.8f, 1.0f),
                GizmoCategory.Gameplay => new Vector4(1.0f, 0.4f, 0.2f, 1.0f),
                GizmoCategory.Navigation => new Vector4(0.2f, 0.9f, 0.9f, 1.0f),
                GizmoCategory.Custom => new Vector4(1.0f, 1.0f, 1.0f, 1.0f),

                _ => Vector4.One
            };
        }
    }
}
