using DevoidEngine.Gizmos;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Components
{
    public enum BVHDebugMode
    {
        Level,
        All,
        Leaves
    }

    public class VisualizeBVH : Component, IGizmoProviderComponent
    {
        public override string Type => nameof(VisualizeBVH);
        public override ComponentTickMode TickMode => ComponentTickMode.All;

        public bool ShowBVH = false;
        public int BVHLevel = 0;
        public BVHDebugMode Mode = BVHDebugMode.Level;

        GizmoMaterial GizmoMaterialBVH = new();

        public override void OnStart()
        {
            GizmoMaterialBVH.Color = new Vector4(1, 0, 0, 1);
        }

        public void OnDrawGizmos(GizmoContext context)
        {
            if (!ShowBVH)
                return;

            BVH? bvh = gameObject.Scene.World.StaticBVH;

            if (bvh == null)
                return;

            switch (Mode)
            {
                case BVHDebugMode.Level:
                    {
                        int maxDepth = bvh.GetMaxDepth();
                        int level = Math.Clamp(BVHLevel, 0, maxDepth);

                        bvh.DrawGizmos(
                            context,
                            GizmoMaterialBVH,
                            level);

                        break;
                    }

                case BVHDebugMode.All:
                    bvh.DrawAllGizmos(
                        context,
                        GizmoMaterialBVH);
                    break;

                case BVHDebugMode.Leaves:
                    bvh.DrawLeafGizmos(
                        context,
                        GizmoMaterialBVH);
                    break;
            }
        }

    }
}
