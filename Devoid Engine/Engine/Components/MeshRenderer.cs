using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class MeshRenderer : Component, IRenderComponent
    {
        public override string Type => nameof(MeshRenderer);

        private Mesh mesh;
        public MaterialInstance material;

        public override void OnStart()
        {
            material = RenderingDefaults.GetMaterial();
        }

        public void AddMesh(Mesh mesh)
        {
            this.mesh = mesh;
        }

        public void Collect(CameraComponent3D camera, CameraRenderContext viewData)
        {
            if (mesh == null || !gameObject.Enabled)
                return;

            viewData.renderItems3D.Add(new RenderItem()
            {
                Material = material,
                Mesh = mesh,
                Model = gameObject.transform.WorldMatrix
            });
        }
    }
}
