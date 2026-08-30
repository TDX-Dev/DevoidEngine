using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Rendering;
using DevoidEngine.Rendering.ProbeGI;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class ProbeGI : Component, IGizmoProviderComponent
    {
        public override string Type =>
            nameof(ProbeGI);

        public override ComponentTickMode TickMode =>
            ComponentTickMode.All;

        readonly GizmoMaterial gizmoMaterial = new();
        readonly GizmoMaterial gridMaterial = new();
        readonly GizmoMaterial octreeMaterial = new();

        public bool MarkAllGeometryStatic = false;
        public bool GenerateExtents = false;
        public bool RebuildProbeLayout = false;
        public bool BakeGI = false;

        public Vector3 Min;
        public Vector3 Max;

        public float ProbeSpacing = 2.0f;
        public float MaxProbeDistance = 4.0f;
        public int MaxProbes = 1500;
        public int BounceCount = 3;

        public bool DrawGrid = false;
        public bool DrawProbes = false;

        ProbeGISystem ProbeGISystem => Engine.Renderer.ProbeGI;

        public ProbeGI()
        {
            gizmoMaterial.Color =
                new Vector4(0, 1, 0, 1);

            gridMaterial.Color =
                new Vector4(0.5f, 0.5f, 0.5f, 1);

            octreeMaterial.Color =
                new Vector4(0, 1, 1, 1);
        }

        public override void OnStart()
        {
            ApplyProbeSettings();

            ProbeGISystem.RegenerateVisualPoints();
        }

        void ApplyProbeSettings()
        {
            ProbeGISystem.SetVolume(
                Min,
                Max);

            ProbeGISystem.SetProbeParameters(
                ProbeSpacing,
                MaxProbeDistance,
                MaxProbes);

            ProbeGISystem.BounceCount =
                BounceCount;
        }

        void GenerateWorldExtents()
        {
            List<RenderMeshData> meshes = [];
            gameObject.Scene.World.GetStaticMeshes(meshes);
            if (meshes.Count == 0)
                return;
            BoundingBox worldBounds = BoundingBox.CreateEmptyBounds();
            foreach (RenderMeshData mesh in meshes)
            {
                BoundingBox localBounds = mesh.render_mesh.LocalBounds;
                BoundingBox.TransformAABB(localBounds.min, localBounds.max, mesh.render_transform, out Vector3 worldMin, out Vector3 worldMax);
                BoundingBox transformedBounds = new(worldMin, worldMax); worldBounds = BoundingBox.Union(worldBounds, transformedBounds);
            }
            Min = worldBounds.min;
            Max = worldBounds.max;
        }

        public void MarkAllGeometry() 
        { 
            for (int i = 0; i < gameObject.Scene.GameObjects.Count; i++) 
            { 
                GameObject obj = gameObject.Scene.GameObjects[i]; 
                MeshRenderer? meshRenderer = obj.GetComponent<MeshRenderer>(); 
                if (meshRenderer != null) 
                    meshRenderer.IsStatic = true; 
            } 
        }

        public void Bake()
        {
            ApplyProbeSettings();

            BVH? bvh = gameObject.Scene.World.StaticBVH;

            if (bvh == null)
                return;

            ProbeGISystem.Bake(gameObject.Scene.World);

            ProbeGISystem.Upload();
        }

        public void BuildProbeLayout()
        {
            BVH? bvh =
                    gameObject.Scene.World.StaticBVH;

            if (bvh != null)
            {
                ApplyProbeSettings();

                ProbeGISystem.Regenerate(bvh);

                ProbeGISystem.RegenerateVisualPoints();
            }
        }

        void DrawProbesGizmo(GizmoContext context)
        {
            if (!DrawProbes)
                return;
            ProbeGISystem.DrawProbes(
                context,
                gridMaterial);
        }

        void DrawGridGizmo(GizmoContext context)
        {
            if (!DrawGrid)
                return;

            ProbeGISystem.DrawGrid(
                context,
                gridMaterial);
        }

        public void OnDrawGizmos(GizmoContext context)
        {
            if (Engine.Instance.FrameCount == 1)
            {
                GenerateWorldExtents();
                //Min = new Vector3(-1.3f, 14.2f, -1.8f);
                //Max = new Vector3(1.8f, 15.4f, 1.3f);
                BuildProbeLayout();
                Bake();
            }

            if (RebuildProbeLayout)
            {
                BuildProbeLayout();
                RebuildProbeLayout = false;
            }

            if (BakeGI)
            {
                Bake();
                BakeGI = false;
            }

            if (MarkAllGeometryStatic)
            {
                MarkAllGeometry();
                MarkAllGeometryStatic = false;
            }

            if (GenerateExtents)
            {
                GenerateWorldExtents();
                GenerateExtents = false;
            }

            if (DrawGrid)
                context.DrawList.AddWireBox(Min, Max, gizmoMaterial);

            DrawProbesGizmo(context);
            DrawGridGizmo(context);
        }
    }
}