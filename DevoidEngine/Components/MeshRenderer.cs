using DevoidEngine.Core;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Components
{
    public class MeshRenderer : Component
    {
        public override string Type => nameof(MeshRenderer);

        private RID instance_id;
        private bool has_moved_current_frame = true;

        public override void OnAttach()
        {
            gameObject.Transform.TransformChanged += VisualTransformChanged;

            Console.WriteLine("Mesh renderer attached");
            Mesh MESH = PrimitiveMeshes.GetCube();

            instance_id = Engine.Renderer.World.CreateMeshInstance(MESH);

            Console.WriteLine("Renderer assigned ID: " + instance_id.Index);
        }

        private void VisualTransformChanged()
        {
            has_moved_current_frame = true;
        }

        public override void OnRender()
        {
            if (!has_moved_current_frame)
                return;
            Matrix4x4 worldMatrixInterpolated = gameObject.Transform.GetGlobalTransformInterpolated(Engine.Instance.FrameCount, Engine.Instance.InterpolationAlpha);

            Engine.Renderer.World.InstanceSetTransform(instance_id, worldMatrixInterpolated);
            has_moved_current_frame = false;
        }
    }
}
