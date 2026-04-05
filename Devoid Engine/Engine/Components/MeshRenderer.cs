using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class MeshRenderer : Component, IRenderComponent
    {
        public override string Type => nameof(MeshRenderer);

        private Mesh? mesh;
        public MaterialInstance? material;

        public override void OnStart()
        {
            if (material != null)
                material = RenderingDefaults.GetMaterial();
        }

        public void AddMaterial(MaterialInstance material)
        {
            this.material = material;
        }

        public void AddMesh(Mesh mesh)
        {
            this.mesh = mesh;
        }

        public Mesh? GetMesh()
        {
            return mesh;
        }

        public override void OnRender(float dt)
        {
            //DebugRenderSystem.DrawCube(mesh.LocalBounds.min, mesh.LocalBounds.max, gameObject.Transform.WorldMatrix);
            //DebugRenderSystem.DrawMesh(mesh, gameObject.transform.WorldMatrix);
        }

        public void Collect(CameraComponent3D camera, CameraRenderContext viewData)
        {
            if (mesh == null || !gameObject.Enabled || material == null)
                return;

            Vector3 worldMin, worldMax;

            BoundingBox.TransformAABB(
                mesh.LocalBounds.min,
                mesh.LocalBounds.max,
                gameObject.Transform.WorldMatrix,
                out worldMin,
                out worldMax
            );

            if (!camera.Camera.IntersectsAABB(worldMin, worldMax))
            {
                return;
            }

            bool isTransparent = material.BaseMaterial.BlendMode == DevoidGPU.BlendMode.AlphaBlend;

            RenderItem renderItem = new RenderItem()
            {
                Material = material,
                Mesh = mesh
            };

            if (EngineSingleton.Instance.UseInterpolation)
            {
                renderItem.Model = gameObject.Transform.GetGlobalTransformInterpolated(EngineSingleton.Instance.FrameCount, EngineSingleton.Instance.InterpolationAlpha);
            }
            else
            {
                renderItem.Model = gameObject.Transform.WorldMatrix;
            }

            viewData.renderItems3D.Add(renderItem);

            //if (isTransparent)
            //{

            //} else
            //{
            //    viewData.renderItems3D.Add(new RenderItem()
            //    {
            //        Material = material,
            //        Mesh = mesh,
            //        Model = gameObject.transform.WorldMatrix
            //    });
            //}
        }
    }
}