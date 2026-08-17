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
                instance_id = gameObject.Scene.World.CreateMeshInstance(mesh);
                gameObject.Scene.World.InstanceSetTransform(instance_id, gameObject.Transform.WorldMatrix);
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
                gameObject.Scene.World.InstanceSetMaterial(instance_id, material);
            }
        }



        internal Mesh? mesh;
        internal MaterialInstance? material;

        private RID instance_id;
        private bool has_moved_current_frame = true;

        public override void OnAttach()
        {
            gameObject.Transform.TransformChanged += VisualTransformChanged;
            Console.WriteLine(gameObject.Scene.SceneName + " Attached");
        }

        private void VisualTransformChanged()
        {
            has_moved_current_frame = true;
        }

        public override void OnStart()
        {
            Console.WriteLine("Starting Mesh Renderer Component");
            if (mesh == null)
                return;

            if (!instance_id.IsValid)
            {
                instance_id =
                    gameObject.Scene.World.CreateMeshInstance(mesh);
                gameObject.Scene.World.InstanceSetTransform(
                    instance_id,
                    gameObject.Transform.WorldMatrix);
            }

            if (material != null)
            {
                gameObject.Scene.World.InstanceSetMaterial(
                    instance_id,
                    material);
            }

            has_moved_current_frame = true;
        }

        public override void OnRender()
        {
            if (Engine.Instance.FrameCount == 0)
                return;
            if ((!has_moved_current_frame || mesh == null || !IsInitialized))
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
            gameObject.Scene.World.InstanceSetTransform(instance_id, worldMatrixInterpolated);
            has_moved_current_frame = false;
        }

        public override void OnDestroy()
        {
            if (mesh != null && instance_id.IsValid)
            {
                gameObject.Scene.World.FreeMeshInstance(instance_id);
                Engine.Instance.AssetManager.Unload(mesh);
            }
            if (material != null)
            {
                Engine.Instance.AssetManager.Unload(material.BaseMaterial);
            }
        }
    }
}
