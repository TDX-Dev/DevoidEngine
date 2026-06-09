using DevoidEngine.Core;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public class RenderWorld
    {
        private readonly RIDOwner<RenderMeshData> meshIdAllocator;


        public RenderWorld()
        {
            meshIdAllocator = new RIDOwner<RenderMeshData>();
        }

        public RID CreateMeshInstance(Mesh mesh)
        {
            RenderMeshData data = new(mesh);

            RID instanceId = meshIdAllocator.MakeRID(data);

            return instanceId;
        }

        public void InstanceSetTransform(RID instance_id, Matrix4x4 transform)
        {
            RenderMeshData data = meshIdAllocator.Get(instance_id);
            data.render_transform = transform;
        }

        public void InstanceSetMesh(
            RID instance_id,
            Mesh mesh)
        {
            RenderMeshData data = meshIdAllocator.Get(instance_id);
            data.render_mesh = mesh;
        }
        public RenderMeshData GetMeshInstance(RID rid)
        {
            return meshIdAllocator.Get(rid);
        }

        public void BuildView(
            Camera camera,
            RenderView view)
        {
            view.Objects.Clear();

            foreach ((RID rid, RenderMeshData mesh) in meshIdAllocator.Enumerate())
            {
                if (!camera.IntersectsAABB(
                        mesh.render_mesh.LocalBounds.min,
                        mesh.render_mesh.LocalBounds.max))
                {
                    continue;
                }

                view.Objects.Add(meshIdAllocator.Get(rid));
            }
        }
    }
}
