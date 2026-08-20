using DevoidEngine.Core;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class RenderWorld
    {
        private readonly RIDOwner<RenderMeshData> meshIdAllocator;

        private readonly RIDOwner<GPUPointLight> pointLights;
        private readonly RIDOwner<GPUSpotLight> spotLights;
        private readonly RIDOwner<GPUDirectionalLight> directionalLights;


        public RenderWorld()
        {
            meshIdAllocator = new RIDOwner<RenderMeshData>();

            pointLights = new RIDOwner<GPUPointLight>();
            spotLights = new RIDOwner<GPUSpotLight>();
            directionalLights = new RIDOwner<GPUDirectionalLight>();
        }

        public RID CreateMeshInstance(Mesh mesh)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            RenderMeshData data = new(mesh);

            RID instanceId = meshIdAllocator.MakeRID(data);

            return instanceId;
        }

        public void InstanceSetTransform(RID instance_id, Matrix4x4 transform)
        {
            RenderMeshData data = meshIdAllocator.Get(instance_id);
            data.render_transform = transform;
        }

        public void InstanceSetMesh(RID instance_id, Mesh mesh)
        {
            RenderMeshData data = meshIdAllocator.Get(instance_id);
            data.render_mesh = mesh;
        }
        public void InstanceSetMaterial(RID instance_id, MaterialInstance material)
        {
            RenderMeshData data = meshIdAllocator.GetRef(instance_id);
            data.render_material = material;
        }
        public void InstanceSetStatic(RID instance_id, bool is_static)
        {
            RenderMeshData data = meshIdAllocator.GetRef(instance_id);
            data.is_static = is_static;
        }
        public RenderMeshData GetMeshInstance(RID rid)
        {
            return meshIdAllocator.Get(rid);
        }

        public void FreeMeshInstance(RID rid)
        {
            meshIdAllocator.Free(rid);
        }

        public RID CreatePointLight()
        {
            return pointLights.MakeRID(new GPUPointLight());
        }

        public RID CreateSpotLight()
        {
            return spotLights.MakeRID(new GPUSpotLight());
        }

        public RID CreateDirectionalLight()
        {
            return directionalLights.MakeRID(new GPUDirectionalLight());
        }

        public void FreePointLight(RID rid)
        {
            pointLights.Free(rid);
        }

        public void FreeSpotLight(RID rid)
        {
            spotLights.Free(rid);
        }

        public void FreeDirectionalLight(RID rid)
        {
            directionalLights.Free(rid);
        }

        public void PointLightSetPosition(RID rid, Vector3 position)
        {
            ref GPUPointLight light =
                ref pointLights.GetRef(rid);

            light.position.X = position.X;
            light.position.Y = position.Y;
            light.position.Z = position.Z;
        }

        public void PointLightSetEnabled(RID rid, bool enabled)
        {
            ref GPUPointLight light =
                ref pointLights.GetRef(rid);

            light.position.W = enabled ? 1f : 0f;
        }

        public void PointLightSetColor(RID rid, Vector3 color)
        {
            ref GPUPointLight light =
                ref pointLights.GetRef(rid);

            light.color.X = color.X;
            light.color.Y = color.Y;
            light.color.Z = color.Z;
        }

        public void PointLightSetIntensity(RID rid, float intensity)
        {
            ref GPUPointLight light =
                ref pointLights.GetRef(rid);

            light.color.W = intensity;
        }

        public void PointLightSetRange(RID rid, float radius)
        {
            ref GPUPointLight light =
                ref pointLights.GetRef(rid);

            light.range.X = radius;
        }

        public void PointLightSetAttenuation(RID rid, LightAttenuationType attenuationType, float linear, float quadratic)
        {
            ref GPUPointLight light =
                ref pointLights.GetRef(rid);

            light.range.Y = (float)attenuationType;
            light.range.Z = linear;
            light.range.W = quadratic;
        }

        public void SpotLightSetPosition(RID rid, Vector3 position)
        {
            ref GPUSpotLight light =
                ref spotLights.GetRef(rid);

            light.position.X = position.X;
            light.position.Y = position.Y;
            light.position.Z = position.Z;
        }

        public void SpotLightSetEnabled(RID rid, bool enabled)
        {
            ref GPUSpotLight light =
                ref spotLights.GetRef(rid);

            light.position.W = enabled ? 1f : 0f;
        }

        public void SpotLightSetColor(RID rid, Vector3 color)
        {
            ref GPUSpotLight light =
                ref spotLights.GetRef(rid);

            light.color.X = color.X;
            light.color.Y = color.Y;
            light.color.Z = color.Z;
        }

        public void SpotLightSetIntensity(RID rid, float intensity)
        {
            ref GPUSpotLight light =
                ref spotLights.GetRef(rid);

            light.color.W = intensity;
        }

        public void SpotLightSetDirection(RID rid, Vector3 direction)
        {
            ref GPUSpotLight light =
                ref spotLights.GetRef(rid);

            light.direction.X = direction.X;
            light.direction.Y = direction.Y;
            light.direction.Z = direction.Z;
        }

        public void SpotLightSetRadius(RID rid, float radius)
        {
            ref GPUSpotLight light =
                ref spotLights.GetRef(rid);

            light.direction.W = radius;
        }

        public void SpotLightSetCutoffs(RID rid, float innerCutoff, float outerCutoff)
        {
            ref GPUSpotLight light =
                ref spotLights.GetRef(rid);

            light.innerCutoff = innerCutoff;
            light.outerCutoff = outerCutoff;
        }

        public void SpotLightSetShadowIndex(RID rid, int shadowIndex)
        {
            ref GPUSpotLight light =
                ref spotLights.GetRef(rid);

            light.shadowIndex = shadowIndex;
        }

        public void DirectionalLightSetDirection(RID rid, Vector3 direction)
        {
            ref GPUDirectionalLight light =
                ref directionalLights.GetRef(rid);

            light.Direction.X = direction.X;
            light.Direction.Y = direction.Y;
            light.Direction.Z = direction.Z;
        }

        public void DirectionalLightSetEnabled(RID rid, bool enabled)
        {
            ref GPUDirectionalLight light =
                ref directionalLights.GetRef(rid);

            light.Direction.W = enabled ? 1f : 0f;
        }

        public void DirectionalLightSetColor(RID rid, Vector3 color)
        {
            ref GPUDirectionalLight light =
                ref directionalLights.GetRef(rid);

            light.Color.X = color.X;
            light.Color.Y = color.Y;
            light.Color.Z = color.Z;
        }

        public void DirectionalLightSetIntensity(RID rid, float intensity)
        {
            ref GPUDirectionalLight light =
                ref directionalLights.GetRef(rid);

            light.Color.W = intensity;
        }
        
        
        public void BuildView(Camera camera, ref RenderView view)
        {
            var meshEntries = meshIdAllocator.AsSpan();

            for (int i = 0; i < meshEntries.Length; i++)
            {
                ref readonly var slot = ref meshEntries[i];

                if (!slot.Occupied)
                    continue;

                ref readonly var mesh = ref slot.Value;

                BoundingBox.TransformAABB(
                    mesh.render_mesh.LocalBounds.min,
                    mesh.render_mesh.LocalBounds.max,
                    mesh.render_transform,
                    out Vector3 worldMin,
                    out Vector3 worldMax
                );

                if (!camera.IntersectsAABB(worldMin, worldMax))
                {
                    continue;
                }

                view.Objects.Add(mesh);
            }

            var pointEntries = pointLights.AsSpan();

            for (int i = 0; i < pointEntries.Length; i++)
            {
                if (i >= Renderer.MAX_POINT_LIGHTS)
                    break;

                ref readonly var slot = ref pointEntries[i];

                if (!slot.Occupied)
                    continue;

                ref readonly var light = ref slot.Value;

                if (light.position.W == 0.0f)
                    continue;

                //float radius = light.range.X;

                //Vector3 pos = new(
                //    light.position.X,
                //    light.position.Y,
                //    light.position.Z);

                //Vector3 min = pos - new Vector3(radius);
                //Vector3 max = pos + new Vector3(radius);

                //if (!camera.IntersectsAABB(min, max))
                //    continue;

                view.PointLights[view.PointLightCount++] = light;
            }

            var spotEntries = spotLights.AsSpan();

            for (int i = 0; i < spotEntries.Length; i++)
            {
                if (i >= Renderer.MAX_SPOT_LIGHTS)
                    break;

                ref readonly var slot = ref spotEntries[i];

                if (!slot.Occupied)
                    continue;

                ref readonly var light = ref slot.Value;

                if (light.position.W == 0.0f)
                    continue;

                //float radius = light.direction.W;

                //Vector3 pos = new(
                //    light.position.X,
                //    light.position.Y,
                //    light.position.Z);

                //Vector3 min = pos - new Vector3(radius);
                //Vector3 max = pos + new Vector3(radius);

                //if (!camera.IntersectsAABB(min, max))
                //    continue;

                view.SpotLights[view.SpotLightCount++] = light;
            }

            var directionalEntries = directionalLights.AsSpan();

            for (int i = 0; i < directionalEntries.Length; i++)
            {
                if (i >= Renderer.MAX_DIRECTIONAL_LIGHTS)
                    break;
                ref readonly var slot = ref directionalEntries[i];

                if (!slot.Occupied)
                    continue;

                ref readonly var light = ref slot.Value;

                if (light.Direction.W == 0.0f)
                    continue;

                view.DirectionalLights[view.DirectionalLightCount++] = light;
            }
        }
        public void GetStaticMeshes(List<RenderMeshData> output)
        {
            output.Clear();

            ReadOnlySpan<Slot<RenderMeshData>> entries =
                meshIdAllocator.AsSpan();

            for (int i = 0; i < entries.Length; i++)
            {
                ref readonly Slot<RenderMeshData> slot = ref entries[i];

                if (!slot.Occupied)
                    continue;

                ref readonly RenderMeshData data = ref slot.Value;

                if (!data.is_static)
                    continue;

                output.Add(data);
            }
        }

    }
}
