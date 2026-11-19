using DevoidEngine.Core;
using DevoidEngine.Rendering.PostProcessing;
using DevoidEngine.Util;
using System.Diagnostics;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class RenderWorld
    {
        public Bvh? StaticBvh => _staticBvh;

        private readonly RidOwner<RenderMeshData> _meshIdAllocator;

        private readonly RidOwner<GpuPointLight> _pointLights;
        private readonly RidOwner<GpuSpotLight> _spotLights;
        private readonly RidOwner<GpuDirectionalLight> _directionalLights;

        private readonly Dictionary<Camera, PostProcessSettings> _postprocessingSettings;

        private Bvh? _staticBvh;
        private bool _staticBvhDirty = true;

        public RenderWorld()
        {
            _meshIdAllocator = new RidOwner<RenderMeshData>();

            _pointLights = new RidOwner<GpuPointLight>();
            _spotLights = new RidOwner<GpuSpotLight>();
            _directionalLights = new RidOwner<GpuDirectionalLight>();

            _postprocessingSettings = [];
        }
        public void UpdateAccelerationStructures()
        {
            if (!_staticBvhDirty)
                return;

            Stopwatch bvhTimer = Stopwatch.StartNew();
            BuildStaticBvh();
            bvhTimer.Stop();
            Console.WriteLine($"BVH Build time: {bvhTimer.Elapsed.TotalMilliseconds:F3} ms");

            _staticBvhDirty = false;
        }

        public void BuildStaticBvh()
        {
            ReadOnlySpan<Slot<RenderMeshData>> entries = _meshIdAllocator.AsSpan();

            Bvh bvh = new(12);

            int triangleId = 0;

            for (int i = 0; i < entries.Length; i++)
            {
                ref readonly Slot<RenderMeshData> slot = ref entries[i];

                if (!slot.Occupied)
                    continue;

                ref readonly RenderMeshData data = ref slot.Value;

                if (!data.IsStatic)
                    continue;

                Mesh mesh = data.RenderMesh;
                Matrix4x4 transform = data.RenderTransform;

                foreach (MeshSurface surface in mesh.Surfaces)
                {
                    if (surface.Indices == null)
                    {
                        Console.WriteLine("[BVH Build]: Surface was skipped due to not having indices.");
                        continue;
                    }

                    if (surface.Positions == null)
                    {
                        Console.WriteLine("[BVH Build]: Surface was skipped due to not having vertex positions.");
                        continue;
                    }

                    uint[] indices = surface.Indices;
                    Vector3[] positions = surface.Positions;

                    if (indices.Length % 3 != 0)
                    {
                        Console.WriteLine("[BVH Build]: Surface was skipped due to invalid index count.");
                        continue;
                    }

                    for (int j = 0; j < indices.Length; j += 3)
                    {
                        Vector3 v0 = Vector3.Transform(positions[indices[j]], transform);

                        Vector3 v1 = Vector3.Transform(positions[indices[j + 1]], transform);

                        Vector3 v2 = Vector3.Transform(positions[indices[j + 2]], transform);

                        bvh.AddTriangle(triangleId++, v0, v1, v2);
                    }
                }
            }

            bvh.Build();

            _staticBvh = bvh;
        }
        public Rid CreateMeshInstance(Mesh mesh)
        {
            ArgumentNullException.ThrowIfNull(mesh);
            RenderMeshData data = new(mesh);

            Rid instanceId = _meshIdAllocator.MakeRid(data);
            mesh.UniqueIdentifier = instanceId.Index + 1;

            return instanceId;
        }

        public void InstanceSetTransform(Rid instanceId, Matrix4x4 transform)
        {
            RenderMeshData data = _meshIdAllocator.Get(instanceId);
            data.RenderTransform = transform;

            if (data.IsStatic)
            {
                Console.WriteLine("[BVH Build]: A static marked object was moved, this can cause performance issues.");
                _staticBvhDirty = true;
            }
        }

        public void InstanceSetMesh(Rid instanceId, Mesh mesh)
        {
            RenderMeshData data = _meshIdAllocator.Get(instanceId);
            data.RenderMesh = mesh;
        }
        public void InstanceSetStatic(Rid instanceId, bool isStatic)
        {
            RenderMeshData data = _meshIdAllocator.GetRef(instanceId);
            if (data.IsStatic == isStatic)
                return;

            data.IsStatic = isStatic;
            _staticBvhDirty = true;
        }
        public RenderMeshData GetMeshInstance(Rid rid)
        {
            return _meshIdAllocator.Get(rid);
        }

        public void FreeMeshInstance(Rid rid)
        {
            if (_meshIdAllocator.Get(rid).IsStatic)
                _staticBvhDirty = true;
            _meshIdAllocator.Free(rid);
        }

        public Rid CreatePointLight()
        {
            return _pointLights.MakeRid(new GpuPointLight());
        }

        public Rid CreateSpotLight()
        {
            return _spotLights.MakeRid(new GpuSpotLight());
        }

        public Rid CreateDirectionalLight()
        {
            return _directionalLights.MakeRid(new GpuDirectionalLight());
        }

        public void FreePointLight(Rid rid)
        {
            _pointLights.Free(rid);
        }

        public void FreeSpotLight(Rid rid)
        {
            _spotLights.Free(rid);
        }

        public void FreeDirectionalLight(Rid rid)
        {
            _directionalLights.Free(rid);
        }

        public void PointLightSetPosition(Rid rid, Vector3 position)
        {
            ref GpuPointLight light =
                ref _pointLights.GetRef(rid);

            light.position.X = position.X;
            light.position.Y = position.Y;
            light.position.Z = position.Z;
        }

        public void PointLightSetEnabled(Rid rid, bool enabled)
        {
            ref GpuPointLight light =
                ref _pointLights.GetRef(rid);

            light.position.W = enabled ? 1f : 0f;
        }

        public void PointLightSetColor(Rid rid, Vector3 color)
        {
            ref GpuPointLight light =
                ref _pointLights.GetRef(rid);

            light.color.X = color.X;
            light.color.Y = color.Y;
            light.color.Z = color.Z;
        }

        public void PointLightSetIntensity(Rid rid, float intensity)
        {
            ref GpuPointLight light =
                ref _pointLights.GetRef(rid);

            light.color.W = intensity;
        }

        public void PointLightSetRange(Rid rid, float radius)
        {
            ref GpuPointLight light =
                ref _pointLights.GetRef(rid);

            light.range.X = radius;
        }

        public void PointLightSetAttenuation(Rid rid, LightAttenuationType attenuationType, float linear, float quadratic)
        {
            ref GpuPointLight light =
                ref _pointLights.GetRef(rid);

            light.range.Y = (float)attenuationType;
            light.range.Z = linear;
            light.range.W = quadratic;
        }

        public void SpotLightSetPosition(Rid rid, Vector3 position)
        {
            ref GpuSpotLight light =
                ref _spotLights.GetRef(rid);

            light.position.X = position.X;
            light.position.Y = position.Y;
            light.position.Z = position.Z;
        }

        public void SpotLightSetEnabled(Rid rid, bool enabled)
        {
            ref GpuSpotLight light =
                ref _spotLights.GetRef(rid);

            light.position.W = enabled ? 1f : 0f;
        }

        public void SpotLightSetColor(Rid rid, Vector3 color)
        {
            ref GpuSpotLight light =
                ref _spotLights.GetRef(rid);

            light.color.X = color.X;
            light.color.Y = color.Y;
            light.color.Z = color.Z;
        }

        public void SpotLightSetIntensity(Rid rid, float intensity)
        {
            ref GpuSpotLight light =
                ref _spotLights.GetRef(rid);

            light.color.W = intensity;
        }

        public void SpotLightSetDirection(Rid rid, Vector3 direction)
        {
            ref GpuSpotLight light =
                ref _spotLights.GetRef(rid);

            light.direction.X = direction.X;
            light.direction.Y = direction.Y;
            light.direction.Z = direction.Z;
        }

        public void SpotLightSetRadius(Rid rid, float radius)
        {
            ref GpuSpotLight light = ref _spotLights.GetRef(rid);

            light.direction.W = radius;
        }

        public void SpotLightSetCutoffs(Rid rid, float innerCutoff, float outerCutoff)
        {
            ref GpuSpotLight light =
                ref _spotLights.GetRef(rid);

            light.innerCutoff = innerCutoff;
            light.outerCutoff = outerCutoff;
        }

        public void SpotLightSetShadowIndex(Rid rid, int shadowIndex)
        {
            ref GpuSpotLight light =
                ref _spotLights.GetRef(rid);

            light.shadowIndex = shadowIndex;
        }

        public void DirectionalLightSetDirection(Rid rid, Vector3 direction)
        {
            ref GpuDirectionalLight light =
                ref _directionalLights.GetRef(rid);

            light.Direction.X = direction.X;
            light.Direction.Y = direction.Y;
            light.Direction.Z = direction.Z;
        }

        public void DirectionalLightSetEnabled(Rid rid, bool enabled)
        {
            ref GpuDirectionalLight light =
                ref _directionalLights.GetRef(rid);

            light.Direction.W = enabled ? 1f : 0f;
        }

        public void DirectionalLightSetColor(Rid rid, Vector3 color)
        {
            ref GpuDirectionalLight light =
                ref _directionalLights.GetRef(rid);

            light.Color.X = color.X;
            light.Color.Y = color.Y;
            light.Color.Z = color.Z;
        }

        public void DirectionalLightSetIntensity(Rid rid, float intensity)
        {
            ref GpuDirectionalLight light =
                ref _directionalLights.GetRef(rid);

            light.Color.W = intensity;
        }

        public void AddPostprocessSettings(Camera camera, PostProcessSettings settings)
        {
            _postprocessingSettings[camera] = settings;
        }
        public void RemovePostProcessSettings(Camera camera)
        {
            _postprocessingSettings.Remove(camera);
        }
        public PostProcessSettings? GetPostProcessSettings(Camera camera)
        {
            if (_postprocessingSettings.TryGetValue(camera, out var ps))
                return ps;
            return null;
        }

        public void BuildView(Camera camera, ref RenderView view)
        {
            var meshEntries = _meshIdAllocator.AsSpan();

            for (int i = 0; i < meshEntries.Length; i++)
            {
                ref readonly var slot = ref meshEntries[i];

                if (!slot.Occupied)
                    continue;

                ref readonly var mesh = ref slot.Value;

                BoundingBox.TransformAabb(
                    mesh.RenderMesh.LocalBounds.Min,
                    mesh.RenderMesh.LocalBounds.Max,
                    mesh.RenderTransform,
                    out Vector3 worldMin,
                    out Vector3 worldMax
                );

                if (!camera.IntersectsAabb(worldMin, worldMax))
                {
                    continue;
                }

                view.Objects.Add(mesh);
            }

            var pointEntries = _pointLights.AsSpan();

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

            var spotEntries = _spotLights.AsSpan();

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

            var directionalEntries = _directionalLights.AsSpan();

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
                _meshIdAllocator.AsSpan();

            for (int i = 0; i < entries.Length; i++)
            {
                ref readonly Slot<RenderMeshData> slot = ref entries[i];

                if (!slot.Occupied)
                    continue;

                ref readonly RenderMeshData data = ref slot.Value;

                if (!data.IsStatic)
                    continue;

                output.Add(data);
            }
        }

    }
}
