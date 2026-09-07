using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Rendering.ProbeGI;
using System.Numerics;

namespace DevoidEngine.Components
{
    internal class ProbeGIComponent : Component, IGizmoProviderComponent
    {
        public override string Type => nameof(ProbeGIComponent);
        public override ComponentTickMode TickMode => ComponentTickMode.All;

        private readonly GizmoMaterial gizmoMaterial = new()
        {
            Color = new Vector4(1, 1, 1, 1)
        };

        internal float probeDebugSphereSize = 0.1f;

        internal Vector3 probeCount = new(16, 16, 16);
        internal uint probeColorResolution = 8;
        internal uint probeVisibilityResolution = 16;

        internal float irradianceBorderWidth = 1.0f;
        internal float visibilityBorderWidth = 1.0f;
        internal float maxVisibilityDistance = 10.0f;

        internal Vector3 volumeMin = new(-5, -5, -5);
        internal Vector3 volumeMax = new(5, 5, 5);

        public float ProbeDebugSphereSize
        {
            get => probeDebugSphereSize;
            set
            {
                if (probeDebugSphereSize == value)
                    return;

                probeDebugSphereSize = value;
                Engine.Renderer.ProbeGISystem.ProbeDebugSize = value;
            }
        }

        public Vector3 ProbeCount
        {
            get => probeCount;
            set
            {
                if (probeCount == value)
                    return;

                probeCount = value;
                Apply();
            }
        }

        public uint ProbeColorResolution
        {
            get => probeColorResolution;
            set
            {
                if (probeColorResolution == value)
                    return;

                probeColorResolution = value;
                Apply();
            }
        }

        public uint ProbeVisibilityResolution
        {
            get => probeVisibilityResolution;
            set
            {
                if (probeVisibilityResolution == value)
                    return;

                probeVisibilityResolution = value;
                Apply();
            }
        }

        public float IrradianceBorderWidth
        {
            get => irradianceBorderWidth;
            set
            {
                if (irradianceBorderWidth == value)
                    return;

                irradianceBorderWidth = value;
                Apply();
            }
        }

        public float VisibilityBorderWidth
        {
            get => visibilityBorderWidth;
            set
            {
                if (visibilityBorderWidth == value)
                    return;

                visibilityBorderWidth = value;
                Apply();
            }
        }

        public float MaxVisibilityDistance
        {
            get => maxVisibilityDistance;
            set
            {
                if (maxVisibilityDistance == value)
                    return;

                maxVisibilityDistance = value;
                Apply();
            }
        }

        public Vector3 VolumeMin
        {
            get => volumeMin;
            set
            {
                if (volumeMin == value)
                    return;

                volumeMin = value;
                Apply();
            }
        }

        public Vector3 VolumeMax
        {
            get => volumeMax;
            set
            {
                if (volumeMax == value)
                    return;

                volumeMax = value;
                Apply();
            }
        }

        public bool Bake = false;
        public bool MarkAllStaticAction = false;

        public override void OnStart()
        {
            Apply();
        }

        public override void OnUpdate(float dt)
        {
            if (Bake)
            {
                Engine.Renderer.ProbeGISystem.Bake(Engine.Renderer.ProbeGISettings, gameObject.Scene.World);
                Bake = false;
            }

            if (MarkAllStaticAction)
            {
                MarkAllStatic();
                MarkAllStaticAction = false;
            }
        }

        void MarkAllStatic()
        {
            List<GameObject> gameObjects = gameObject.Scene.GameObjects;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                GameObject go = gameObjects[i];
                MeshRenderer? mr = go.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    mr.IsStatic = true;
                }
            }
        }

        private void Apply()
        {
            Engine.Renderer.ProbeGISettings = new ProbeGISettings
            {
                ProbeCount = probeCount,
                ProbeColorResolution = probeColorResolution,
                ProbeVisibilityResolution = probeVisibilityResolution,

                IrradianceBorderWidth = irradianceBorderWidth,
                VisibilityBorderWidth = visibilityBorderWidth,
                MaxVisibilityDistance = maxVisibilityDistance,

                VolumeMin = volumeMin,
                VolumeMax = volumeMax
            };

            Engine.Renderer.ProbeGISystem.ProbeDebugSize =
                probeDebugSphereSize;
        }

        public void OnDrawGizmos(GizmoContext context)
        {
            context.DrawList.AddWireBox(volumeMin, volumeMax, gizmoMaterial, GizmoCategory.Custom);
        }
    }
}