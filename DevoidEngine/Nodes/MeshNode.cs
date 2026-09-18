using DevoidEngine.Core;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Nodes
{
    public class MeshNode : Node3D
    {
        public override NodeTickMode TickMode => NodeTickMode.All;

        public Mesh? Mesh
        {
            get => mesh;
            set
            {
                if (value == null)
                    return;

                mesh = value;
                instance_id = Scene.World.CreateMeshInstance(mesh);
                Scene.World.InstanceSetTransform(instance_id, Transform.WorldMatrix);
                just_spawned = true;
            }
        }
        public bool IsStatic
        {
            get => is_static;
            set
            {
                if (value == is_static || mesh == null)
                    return;

                is_static = value;
                Scene.World.InstanceSetStatic(instance_id, is_static);
            }
        }

        internal Mesh? mesh;
        internal bool is_static = false;

        private RID instance_id;
        private bool has_moved_current_frame = true;
        private bool just_spawned = false;

        protected override void OnAttach()
        {
            Transform.TransformChanged += VisualTransformChanged;
        }

        private void VisualTransformChanged()
        {
            has_moved_current_frame = true;
        }

        protected override void OnStart()
        {
            if (mesh == null)
                return;

            if (!instance_id.IsValid)
            {
                instance_id = Scene.World.CreateMeshInstance(mesh);
                Scene.World.InstanceSetTransform(instance_id, Transform.WorldMatrix);
                Scene.World.InstanceSetStatic(instance_id, is_static);
                just_spawned = true;

            }

            has_moved_current_frame = true;
        }

        protected override void OnRender()
        {
            //if (Engine.Instance.FrameCount == 0)
            //    return;

            if (mesh == null || !IsInitialized)
                return;

            bool interpolate = Engine.Instance.UseInterpolation && !IsStatic;

            if (!interpolate && !has_moved_current_frame)
                return;


            Matrix4x4 worldMatrixInterpolated;
            if (just_spawned)
            {
                worldMatrixInterpolated = Transform.WorldMatrix;
                just_spawned = false;
            }
            else if (Engine.Instance.UseInterpolation && !IsStatic)
            {
                worldMatrixInterpolated = Transform.GetGlobalTransformInterpolated(Engine.Instance.FrameCount, Engine.Instance.InterpolationAlpha);
            }
            else
            {
                worldMatrixInterpolated = Transform.WorldMatrix;
            }
            Scene.World.InstanceSetTransform(instance_id, worldMatrixInterpolated);
            has_moved_current_frame = false;
        }

        protected override void OnDestroy()
        {
            if (mesh != null && instance_id.IsValid)
            {
                Scene.World.FreeMeshInstance(instance_id);
                Engine.Instance.AssetManager.Unload(mesh);
            }
        }
    }
}
