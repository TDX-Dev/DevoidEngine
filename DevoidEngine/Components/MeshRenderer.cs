using DevoidEngine.Core;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class MeshRenderer : Component
    {
        public override string Type => nameof(MeshRenderer);

        public Mesh? Mesh
        {
            get => mesh;
            set
            {
                if (value == null)
                    return;

                mesh = value;
                instance_id = Engine.Renderer.World.CreateMeshInstance(mesh);
            }
        }

        public MaterialInstance? Material
        {
            get => material;
            set
            {
                if (value == null)
                    return;

                material = value;
                Engine.Renderer.World.InstanceSetMaterial(instance_id, material);
            }
        }



        internal Mesh? mesh;
        internal MaterialInstance? material;
        private RID instance_id;
        private bool has_moved_current_frame = true;

        public override void OnAttach()
        {
            gameObject.Transform.TransformChanged += VisualTransformChanged;
        }

        private void VisualTransformChanged()
        {
            has_moved_current_frame = true;
        }

        public override void OnRender()
        {
            if (!has_moved_current_frame || mesh == null)
                return;
            Matrix4x4 worldMatrixInterpolated;
            if (Engine.Instance.UseInterpolation)
            {
                worldMatrixInterpolated = gameObject.Transform.GetGlobalTransformInterpolated(Engine.Instance.FrameCount, Engine.Instance.InterpolationAlpha);
            }
            else
            {
                worldMatrixInterpolated = gameObject.Transform.WorldMatrix;
            }
            Engine.Renderer.World.InstanceSetTransform(instance_id, worldMatrixInterpolated);
            has_moved_current_frame = false;
        }
    }
}
