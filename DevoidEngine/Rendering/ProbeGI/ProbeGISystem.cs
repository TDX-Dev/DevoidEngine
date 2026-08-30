using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Util;
using DevoidGPU;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Rendering.ProbeGI
{
    public sealed class ProbeGISystem
    {
        public static readonly Vector3[] ProbeDirections =
        [
            Vector3.UnitX,
            -Vector3.UnitX,
            Vector3.UnitY,
            -Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitZ
        ];

        public ProbeVolume Volume { get; private set; }

        public IReadOnlyList<Probe> Probes => probes;

        public IReadOnlyList<(int x, int y, int z)> ValidCells => validCells;

        public int ProbeCount => probes.Count;

        readonly List<Probe> probes = [];

        readonly List<(int x, int y, int z)> validCells = [];

        readonly ProbeVolumeBuilder probeVolumeBuilder = new();

        readonly SH9Projector sh9Projector;

        Vector3 volumeMin;
        Vector3 volumeMax;

        bool probeLayoutDirty = true;

        public ShaderStorageBuffer<DiffuseProbe> BakedProbes => previousBounce;
        public UniformBuffer ProbeGIData => probeGIDataBuffer;

        public float ProbeSpacing { get; set; } = 2.0f;

        public float MaxProbeDistance { get; set; } = 4.0f;

        public int MaxProbes { get; set; } = 2000;

        public int BounceCount { get; set; } = 1;

        public bool HasProbes { get; private set; }

        public bool HasBakedData { get; private set; }

        const int ProbeCubemapResolution = 32;

        readonly Texture probeCubemap;
        readonly Texture probeCubemapDepth;
        readonly RenderTarget probeCaptureRenderTarget;

        readonly CubemapCapture probeCapture;

        ShaderStorageBuffer<DiffuseProbe> previousBounce = null!;
        ShaderStorageBuffer<DiffuseProbe> currentBounce = null!;

        readonly UniformBuffer probeGIDataBuffer;

        bool bakeRequested = false;
        RenderWorld bakeWorld = null!;

        readonly MaterialInstance probeCaptureMaterial;
        readonly DiffuseProbePacker diffuseProbePacker;

        public ProbeGISystem()
        {
            diffuseProbePacker = new DiffuseProbePacker();

            probeCubemap = Texture.CreateCube(ProbeCubemapResolution, TextureFormat.RGBA16_Float, TextureUsage.RenderTarget | TextureUsage.ShaderResource);
            probeCubemapDepth = Texture.Create2D(ProbeCubemapResolution, ProbeCubemapResolution, TextureFormat.Depth24_Stencil8, TextureUsage.DepthStencil);

            probeCaptureRenderTarget = RenderTarget.Create(1);
            probeCaptureRenderTarget.SetDepthAttachment(probeCubemapDepth);

            probeCapture = new CubemapCapture(ProbeCubemapResolution);

            sh9Projector = new SH9Projector(ProbeCubemapResolution, 6);

            probeCaptureMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/probe_gi_capture.dsd"))));

            probeGIDataBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<ProbeGIData>());

            probeCaptureMaterial.DescriptorSet.SetUniformBuffer(6, probeGIDataBuffer.GPU);
        }

        public void SetVolume(Vector3 min, Vector3 max)
        {
            Vector3 newMin = Vector3.Min(min, max);

            Vector3 newMax = Vector3.Max(min, max);

            if (volumeMin == newMin && volumeMax == newMax)
                return;

            volumeMin = newMin;
            volumeMax = newMax;

            probeLayoutDirty = true;
            HasBakedData = false;
        }

        public void SetProbeParameters(float spacing, float maxProbeDistance, int maxProbes)
        {
            if (ProbeSpacing != spacing ||
                MaxProbeDistance != maxProbeDistance ||
                MaxProbes != maxProbes)
            {
                probeLayoutDirty = true;
                HasBakedData = false;
            }

            ProbeSpacing = spacing;
            MaxProbeDistance = maxProbeDistance;
            MaxProbes = maxProbes;
        }

        public void Regenerate(BVH bvh)
        {
            probes.Clear();
            validCells.Clear();

            probeVolumeBuilder.ProbeSpacing = ProbeSpacing;

            probeVolumeBuilder.MaxProbeDistance = MaxProbeDistance;

            probeVolumeBuilder.MaxProbes =
                MaxProbes;

            Volume = probeVolumeBuilder.Build(volumeMin, volumeMax, bvh, probes, validCells);

            previousBounce?.Dispose();
            currentBounce?.Dispose();

            previousBounce = ShaderStorageBuffer<DiffuseProbe>.Create(ResourceUsage.Default, (uint)probes.Count, BufferBind.StorageWritable);

            currentBounce = ShaderStorageBuffer<DiffuseProbe>.Create(ResourceUsage.Default, (uint)probes.Count, BufferBind.StorageWritable);

            probeGIDataBuffer.Update(new ProbeGIData
            {
                GridMin = Volume.GridMin,
                Spacing = Volume.Spacing,
                ResolutionX = (uint)Volume.ResolutionX,
                ResolutionY = (uint)Volume.ResolutionY,
                ResolutionZ = (uint)Volume.ResolutionZ,
                ProbeCount = (uint)probes.Count
            });


            HasProbes = true;
            HasBakedData = false;
            probeLayoutDirty = false;

            Console.WriteLine("Regenerate Was Called");
        }

        public void RegenerateVisualPoints()
        {
            // This only rebuilds the visual representation.
            //
            // It does NOT:
            // - build probes
            // - query the BVH
            // - modify probe positions
            // - modify probe weights
        }

        public void Bake(RenderWorld world)
        {
            if (BounceCount < 1)
            {
                throw new InvalidOperationException("Bounce count must be at least one.");
            }

            if (probeLayoutDirty || !HasProbes)
            {
                if (world.StaticBVH == null)
                    throw new InvalidOperationException("Cannot bake Probe GI without a static BVH.");

                Regenerate(world.StaticBVH);
            }

            RegenerateVisualPoints();

            bakeRequested = true;
            bakeWorld = world;
            HasBakedData = false;
        }
        public void Upload()
        {
            //if (!HasBakedData)
            //{
            //    throw new InvalidOperationException("Cannot upload Probe GI before baking.");
            //}

            // GPU upload implementation.
        }

        public void Render(ICommandList cmd)
        {
            if (!bakeRequested)
                return;

            List<RenderMeshData> staticMeshes = [];

            bakeWorld.GetStaticMeshes(staticMeshes);

            // Bounce 0.
            for (int i = 0; i < probes.Count; i++)
            {
                if (probes[i].Weight <= 0.0f)
                    continue;

                BakeProbe(cmd, i, staticMeshes, null, previousBounce);
            }

            //// Additional bounces.
            for (int bounce = 1; bounce < BounceCount; bounce++)
            {
                for (int i = 0; i < probes.Count; i++)
                {
                    if (probes[i].Weight <= 0.0f)
                        continue;

                    BakeProbe(cmd, i, staticMeshes, previousBounce, currentBounce);
                }

                (previousBounce, currentBounce) = (currentBounce, previousBounce);
            }

            bakeRequested = false;
            HasBakedData = true;
        }

        void RenderProbeCubemap(ICommandList cmd, Vector3 position, List<RenderMeshData> staticMeshes, ShaderStorageBuffer<DiffuseProbe>? previousLighting)
        {
            Engine.Renderer.PushViewport(cmd, new()
            {
                Width = ProbeCubemapResolution,
                Height = ProbeCubemapResolution,
                X = 0,
                Y = 0
            });

            probeCapture.CameraData.CameraPosition = position;

            if (previousLighting != null)
            {
                probeCaptureMaterial.DescriptorSet.SetShaderStorageBuffer(0, previousLighting.GPU);
            }

            for (int face = 0; face < 6; face++)
            {
                probeCapture.CameraData.View = probeCapture.GetView(face, position);
                Matrix4x4.Invert(probeCapture.CameraData.View, out probeCapture.CameraData.InverseView);
                Matrix4x4.Invert(probeCapture.CameraData.View * probeCapture.CameraData.Projection, out probeCapture.CameraData.InverseViewProjection);

                Engine.Renderer.UpdateCameraBuffer(probeCapture.CameraData);

                probeCaptureRenderTarget.SetColorAttachment(0, probeCubemap, 0, face);

                cmd.SetFramebuffer(probeCaptureRenderTarget.GPU);
                cmd.ClearColor(0, Colors.Transparent);
                cmd.ClearDepthStencil(1, 0);

                Engine.Renderer.Execute(cmd, staticMeshes, previousLighting == null ? null : probeCaptureMaterial);
            }

            cmd.SetFramebuffer(null);

            Engine.Renderer.PopViewport(cmd);
        }

        void BakeProbe(ICommandList cmd, int probeIndex, List<RenderMeshData> staticMeshes, ShaderStorageBuffer<DiffuseProbe>? previousLighting, ShaderStorageBuffer<DiffuseProbe> outputLighting)
        {
            Probe probe = probes[probeIndex];

            RenderProbeCubemap(cmd, probe.Position, staticMeshes, previousLighting);

            sh9Projector.ProjectAndReduce(cmd, probeCubemap, ProbeCubemapResolution);

            Console.WriteLine("Probe Weight: " + probe.Weight + " At Index: " + probeIndex);

            diffuseProbePacker.Pack(cmd, sh9Projector.Output, outputLighting, (uint)probeIndex, probe.Weight);
        }

        #region GIZMO_STUFF
        public void DrawProbes(GizmoContext context, GizmoMaterial material)
        {
            for (int i = 0;
                 i < probes.Count;
                 i++)
            {
                Probe probe =
                    probes[i];

                if (probe.Weight <= 0.0f)
                    continue;

                context.DrawList.AddCircle(
                    probe.Position,
                    0.5f,
                    Vector3.UnitY,
                    material);
            }
        }

        public void DrawGrid(GizmoContext context, GizmoMaterial material)
        {
            for (int i = 0;
                 i < validCells.Count;
                 i++)
            {
                (int x, int y, int z) =
                    validCells[i];

                Vector3 cellMin =
                    new(
                        Volume.GridMin.X +
                            x * Volume.Spacing,

                        Volume.GridMin.Y +
                            y * Volume.Spacing,

                        Volume.GridMin.Z +
                            z * Volume.Spacing);

                Vector3 cellMax =
                    cellMin +
                    new Vector3(
                        Volume.Spacing);

                context.DrawList.AddWireBox(
                    cellMin,
                    cellMax,
                    material);
            }
        }
        #endregion
    }
}