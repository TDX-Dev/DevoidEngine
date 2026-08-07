using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    struct ReduceData
    {
        public uint InputCount;
        public uint FinalPass;
        public uint Padding1;
        public uint Padding2;
    }

    public class SkyRenderer
    {
        public int SkyResolution = 1024;
        public int IrradianceResolution = 64;
        public int PrefilterResolution = 512;
        public int BRDFLutResolution = 512;

        public int PrefilterMipLevels = 9;

        public ISky Sky = null!;

        public EnvironmentLighting Environment;

        private readonly RenderMeshData skyMeshData;

        // Here lie ze clutter

        private readonly RenderTarget IrradianceRenderTarget;

        private readonly Texture IrradianceCubeTexture;

        private readonly Texture PrefilterTexture;

        private readonly Texture BRDFLutTexture;

        private readonly RenderTarget PanoramaRenderTarget;

        private readonly MaterialInstance PanoramaToCubemapMaterial;

        private readonly Texture SkyboxTexture;

        private readonly MaterialInstance ProjectToSHMaterial;
        private readonly MaterialInstance ReduceSHMaterial;
        private readonly MaterialInstance PrefilterMaterial;
        private readonly MaterialInstance BRDFLutMaterial;

        private readonly MaterialInstance SkyCubemapMaterial;

        private readonly IComputePipeline ProjectToSHPipeline;

        private readonly IComputePipeline ReduceSHPipeline;

        private readonly ShaderStorageBuffer<SH9> EnvironmentSH;

        private readonly ShaderStorageBuffer<SH9> PartialSH;

        private readonly ShaderStorageBuffer<SH9> PartialSH2;

        private readonly UniformBuffer ReduceInputBuffer;

        private readonly uint partialCount;

        private readonly Texture BlueNoisePrefilter;

        private readonly Texture DebugCube;

        // End of ze clutter


        private readonly Matrix4x4[] CubemapCaptureViews;

        private readonly Mesh CubeMesh;
        private readonly Mesh PlaneMesh;

        private readonly RenderMeshData ConversionRenderData;

        private CameraData ConversionCameraData;

        public SkyRenderer()
        {
            CubeMesh = PrimitiveMeshes.GetInvertedUVCube();
            PlaneMesh = PrimitiveMeshes.GetFullscreenPlane();

            Environment = new();
            Sky = new HDRISky();

            skyMeshData = new RenderMeshData
            {
                render_mesh = CubeMesh,
                //render_material = Sky.Material,
            };

            BlueNoisePrefilter = Texture.CreateFromImage2D(TextureUtil.LoadImage("Content/Noise/LDR_RG01_0.png"), TextureUsage.ShaderResource, TextureFormat.RG8_UNorm);

            SkyboxTexture = Texture.CreateCube(SkyResolution, TextureFormat.RGBA16_Float, TextureUsage.RenderTarget | TextureUsage.ShaderResource, (int)(Math.Log2(SkyResolution) + 1));

            IrradianceCubeTexture = Texture.CreateCube(IrradianceResolution, TextureFormat.RGBA16_Float, TextureUsage.RenderTarget | TextureUsage.ShaderResource);

            IrradianceRenderTarget = RenderTarget.Create(1);

            ProjectToSHMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/sh9_project.dsd"))));
            ReduceSHMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/sh9_reduce.dsd"))));

            PrefilterMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/prefilter.dsd"))));

            BRDFLutMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/brdfLUT.dsd"))));

            SkyCubemapMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/sky_cubemap.dsd"))));

            SkyCubemapMaterial.SetTexture("MAT_Skybox", SkyboxTexture);

            PrefilterTexture = Texture.CreateCube(
                PrefilterResolution,
                TextureFormat.RGBA16_Float,
                TextureUsage.RenderTarget | TextureUsage.ShaderResource,
                PrefilterMipLevels);

            BRDFLutTexture = Texture.Create2D(BRDFLutResolution, BRDFLutResolution, TextureFormat.RG16_Float, TextureUsage.RenderTarget | TextureUsage.ShaderResource);

            PanoramaRenderTarget = RenderTarget.Create(1);

            PanoramaToCubemapMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, Path.Combine(Engine.BasePath, "Content/DevoidShaderDescriptors/sky_panoramatocubemap.dsd"))));

            DebugCube =
                Texture.CreateCube(
                32,
                TextureFormat.RGBA16_Float,
                TextureUsage.UnorderedAccess |
                TextureUsage.ShaderResource);

            ProjectToSHPipeline = ProjectToSHMaterial.BaseMaterial.DefaultPass.ComputePipeline!;
            ReduceSHPipeline = ReduceSHMaterial.BaseMaterial.DefaultPass.ComputePipeline!;

            uint groupsX = (uint)SkyResolution / 8;
            uint groupsY = (uint)SkyResolution / 8;
            partialCount = groupsX * groupsY * 6;

            PartialSH = ShaderStorageBuffer<SH9>.Create(
                ResourceUsage.Default,
                partialCount, BufferBind.StorageWritable);

            EnvironmentSH = ShaderStorageBuffer<SH9>.Create(
                ResourceUsage.Default,
                1, BufferBind.StorageWritable);

            PartialSH2 = ShaderStorageBuffer<SH9>.Create(
                ResourceUsage.Default,
                partialCount, BufferBind.StorageWritable);

            ReduceInputBuffer = UniformBuffer.Create(ResourceUsage.Dynamic, (uint)Unsafe.SizeOf<ReduceData>());
            CubemapCaptureViews = [
                Matrix4x4.CreateLookAtLeftHanded(Vector3.Zero, Vector3.UnitX,  Vector3.UnitY),   // +X
                Matrix4x4.CreateLookAtLeftHanded(Vector3.Zero, -Vector3.UnitX, Vector3.UnitY),   // -X

                Matrix4x4.CreateLookAtLeftHanded(Vector3.Zero, Vector3.UnitY,  -Vector3.UnitZ),  // +Y
                Matrix4x4.CreateLookAtLeftHanded(Vector3.Zero, -Vector3.UnitY,  Vector3.UnitZ),  // -Y

                Matrix4x4.CreateLookAtLeftHanded(Vector3.Zero, Vector3.UnitZ,  Vector3.UnitY),   // +Z
                Matrix4x4.CreateLookAtLeftHanded(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY),   // -Z

            ];

            Matrix4x4 captureProjection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded(
                MathF.PI * 0.5f, 1f, 0.1f, 1000f);


            ConversionCameraData = new CameraData()
            {
                Projection = captureProjection,
                View = CubemapCaptureViews[0],
                ScreenSize = new Vector2(SkyResolution),
                FarClip = 1000,
                NearClip = 0.1f,
                CameraPosition = Vector3.Zero
            };

            ConversionRenderData = new RenderMeshData()
            {
                render_mesh = CubeMesh,
                render_transform = Matrix4x4.Identity
            };

            PrefilterMaterial.SetTexture("BlueNoise", BlueNoisePrefilter);
        }
        public void Render(RenderContext ctx)
        {
            if (Sky == null)
                return;

            if (Sky.Dirty)
            {
                Sky.BuildEnvironment(ctx, Environment);

                ProcessCubemap(ctx.CommandList, Sky);

                Sky.Dirty = false;
            }

        }

        public void RenderSkybox(RenderContext ctx)
        {


            ConversionRenderData.render_material = SkyCubemapMaterial;
            ConversionRenderData.render_mesh = CubeMesh;
            //ConversionRenderData.render_transform = Matrix4x4.CreateTranslation(ctx.Camera.Position);

            Engine.Renderer.Execute(ctx.CommandList, ConversionRenderData);
        }


        void ProcessCubemap(ICommandList cmd, ISky sky)
        {

            cmd.GenerateMipmaps(SkyboxTexture.GPU);

            ConversionRenderData.render_mesh = CubeMesh;
            ConversionRenderData.render_material = PrefilterMaterial;

            ProjectToSH(cmd);
            ReduceSH(cmd);

            GeneratePrefilter(cmd);

            ConversionRenderData.render_mesh = PlaneMesh;

            GenerateBRDFLUT(cmd);

            Environment.Skybox = SkyboxTexture;
            Environment.Irradiance = IrradianceCubeTexture;
            Environment.Prefilter = PrefilterTexture;
            Environment.BrdfLut = BRDFLutTexture;
            Environment.SH9 = EnvironmentSH;

            Engine.Renderer.UpdateEnvironment(Environment);
        }

        public void ConvertPanoramaToCubemap(ICommandList cmd, Texture panoramicTexture)
        {
            Engine.Renderer.PushViewport(cmd, new ViewportRect()
            {
                Height = SkyResolution,
                Width = SkyResolution,
                X = 0,
                Y = 0
            });

            PanoramaToCubemapMaterial.SetTexture("MAT_Panorama", panoramicTexture);
            ConversionRenderData.render_material = PanoramaToCubemapMaterial;

            for (int face = 0; face < 6; face++)
            {
                ConversionCameraData.View = CubemapCaptureViews[face];

                Engine.Renderer.UpdateCameraBuffer(ConversionCameraData);

                PanoramaRenderTarget.SetColorAttachment(0, SkyboxTexture, 0, face);
                cmd.SetFramebuffer(PanoramaRenderTarget.GPU);

                Engine.Renderer.Execute(cmd, ConversionRenderData);

            }

            //cmd.ClearColor(0, new Vector4(0, 0, 0, 1));

            Engine.Renderer.PopViewport(cmd);
        }

        void ProjectToSH(ICommandList cmd)
        {
            ProjectToSHMaterial.SetTexture("MAT_Skybox", SkyboxTexture);

            cmd.SetComputePipeline(ProjectToSHPipeline);

            ProjectToSHMaterial.DescriptorSet.SetRWTexture(1, DebugCube.GPU);
            ProjectToSHMaterial.DescriptorSet.SetRWShaderStorageBuffer(0, PartialSH.GPU);

            ProjectToSHMaterial.SetFloat("EnvironmentMapResolution", SkyResolution);

            cmd.SetDescriptorSet(0, ProjectToSHMaterial.DescriptorSet);

            //cmd.Dispatch(
            //    (uint)IrradianceResolution / 8,
            //    (uint)IrradianceResolution / 8,
            //    6);

            cmd.Dispatch(
                (uint)SkyResolution / 8,
                (uint)SkyResolution / 8,
            6);
        }

        void ReduceSH(ICommandList cmd)
        {
            uint count = partialCount;

            ShaderStorageBuffer<SH9> input = PartialSH;
            ShaderStorageBuffer<SH9> output = PartialSH2;

            while (count > 1)
            {
                uint outputCount = (count + 1) / 2;

                ReduceSHMaterial.DescriptorSet.SetShaderStorageBuffer(
                    0,
                    input.GPU);

                bool finalPass = outputCount == 1;

                ShaderStorageBuffer<SH9> destination =
                    finalPass ? EnvironmentSH : output;

                ReduceSHMaterial.DescriptorSet.SetRWShaderStorageBuffer(
                    0,
                    destination.GPU);

                // TODO upload InputCount here

                ReduceInputBuffer.Update(new ReduceData
                {
                    InputCount = count,
                    FinalPass = finalPass ? 1u : 0u
                });
                ReduceSHMaterial.DescriptorSet.SetUniformBuffer(
                    0,
                ReduceInputBuffer.GPU);

                cmd.SetComputePipeline(ReduceSHPipeline);
                cmd.SetDescriptorSet(0, ReduceSHMaterial.DescriptorSet);

                uint groups = (outputCount + 63) / 64;

                cmd.Dispatch(groups, 1, 1);

                if (!finalPass)
                {
                    (input, output) = (output, input);
                }

                count = outputCount;
            }
        }

        void GeneratePrefilter(ICommandList cmd)
        {
            PrefilterMaterial.SetTexture("MAT_Skybox", SkyboxTexture);
            PrefilterMaterial.SetFloat("MaxPrefilterMipLevel", PrefilterMipLevels - 1);
            PrefilterMaterial.SetFloat("EnvironmentMapResolution", SkyResolution);

            ConversionRenderData.render_material = PrefilterMaterial;
            for (int mip = 0; mip < PrefilterMipLevels; mip++)
            {
                int size = PrefilterResolution >> mip;

                Engine.Renderer.PushViewport(cmd, new ViewportRect()
                {
                    Width = size,
                    Height = size,
                    X = 0,
                    Y = 0,
                });

                float roughness = (float)mip / (PrefilterMipLevels - 1);

                PrefilterMaterial.SetFloat("Roughness", roughness);

                for (int face = 0; face < 6; face++)
                {
                    ConversionCameraData.View = CubemapCaptureViews[face];

                    Engine.Renderer.UpdateCameraBuffer(ConversionCameraData);

                    PanoramaRenderTarget.SetColorAttachment(
                        0,
                        PrefilterTexture,
                        mip,
                        face);

                    cmd.SetFramebuffer(PanoramaRenderTarget.GPU);

                    Engine.Renderer.Execute(cmd, ConversionRenderData);

                }

                Engine.Renderer.PopViewport(cmd);

            }
        }

        void GenerateBRDFLUT(ICommandList cmd)
        {
            Engine.Renderer.PushViewport(cmd, new ViewportRect()
            {
                Width = BRDFLutResolution,
                Height = BRDFLutResolution,
                X = 0,
                Y = 0,
            });

            CameraData lutCamera = new()
            {
                View = Matrix4x4.Identity,
                Projection = Matrix4x4.CreateOrthographic(
                    2.0f,
                    2.0f,
                    -1.0f,
                    1.0f),
                CameraPosition = Vector3.Zero,
                ScreenSize = new Vector2(BRDFLutResolution)
            };

            Matrix4x4.Invert(lutCamera.View, out lutCamera.InverseView);
            Matrix4x4.Invert(lutCamera.Projection, out lutCamera.InverseProjection);

            Engine.Renderer.UpdateCameraBuffer(lutCamera);

            ConversionRenderData.render_material = BRDFLutMaterial;

            PanoramaRenderTarget.SetColorAttachment(0, BRDFLutTexture);
            cmd.SetFramebuffer(PanoramaRenderTarget.GPU);
            Engine.Renderer.Execute(cmd, ConversionRenderData);

            Engine.Renderer.PopViewport(cmd);
        }

    }
}
