using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class SkyRenderer
    {
        public int SkyResolution = 1024;
        public int IrradianceResolution = 32;
        public int PrefilterResolution = 256;
        public int BRDFLutResolution = 512;

        public int PrefilterMipLevels = 8;

        public ISky Sky = null!;

        public EnvironmentLighting Environment;

        private readonly RenderMeshData skyMeshData;

        // Here lie ze clutter

        private readonly RenderTarget IrradianceRenderTarget;

        private readonly Texture IrradianceCubeTexture;

        private readonly MaterialInstance IrradianceMaterial;

        private readonly RenderTarget PanoramaRenderTarget;

        private readonly MaterialInstance PanoramaToCubemapMaterial;

        private readonly Texture SkyboxTexture;

        // End of ze clutter


        private readonly Matrix4x4[] CubemapCaptureViews;

        private readonly Mesh CubeMesh;

        private readonly RenderMeshData ConversionRenderData;

        private CameraData ConversionCameraData;

        public SkyRenderer()
        {
            CubeMesh = PrimitiveMeshes.GetCube();

            Environment = new();
            Sky = new HDRISky();

            skyMeshData = new RenderMeshData
            {
                render_mesh = CubeMesh,
                //render_material = Sky.Material,
            };

            SkyboxTexture = Texture.CreateCube(SkyResolution, TextureFormat.RGBA16_Float, TextureUsage.RenderTarget | TextureUsage.ShaderResource, (int)(Math.Log2(SkyResolution) + 1));

            IrradianceCubeTexture = Texture.CreateCube(IrradianceResolution, TextureFormat.RGBA16_Float, TextureUsage.RenderTarget | TextureUsage.ShaderResource);

            IrradianceRenderTarget = RenderTarget.Create(1);

            IrradianceMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/sky_irradiance.dsd")));

            PanoramaRenderTarget = RenderTarget.Create(1);

            PanoramaToCubemapMaterial = new MaterialInstance(new Material(Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/sky_panoramatocubemap.dsd")));


            CubemapCaptureViews =
            [
                Matrix4x4.CreateLookAt(Vector3.Zero,  Vector3.UnitX,  -Vector3.UnitY),
                Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitX,  -Vector3.UnitY),
                Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitY,   -Vector3.UnitZ),
                Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitY,  Vector3.UnitZ),
                Matrix4x4.CreateLookAt(Vector3.Zero,  Vector3.UnitZ,  -Vector3.UnitY),
                Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ,  -Vector3.UnitY)
            ];

            Matrix4x4 captureProjection = Matrix4x4.CreatePerspectiveFieldOfView(
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

        }


        void ProcessCubemap(ICommandList cmd, ISky sky)
        {
            cmd.GenerateMipmaps(SkyboxTexture.GPU);

            Engine.Renderer.PushViewport(cmd, new ViewportRect()
            {
                Height = IrradianceResolution,
                Width = IrradianceResolution,
                X = 0,
                Y = 0
            });
            GenerateIrradiance(cmd, SkyboxTexture, IrradianceCubeTexture);
            Engine.Renderer.PopViewport(cmd);


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
                cmd.SetFramebuffer(PanoramaRenderTarget.GPU, 0, face);

                Engine.Renderer.Execute(cmd, ConversionRenderData);

            }

            Engine.Renderer.PopViewport(cmd);
        }

        void GenerateIrradiance(ICommandList cmd, Texture SkyboxTexture, Texture IrradianceTextureTargetCube)
        {
            ConversionRenderData.render_material = IrradianceMaterial;

            IrradianceMaterial.SetTexture("MAT_Skybox", SkyboxTexture);
            ConversionRenderData.render_material = IrradianceMaterial;
            for (int face = 0; face < 6; face++)
            {
                ConversionCameraData.View = CubemapCaptureViews[face];

                Engine.Renderer.UpdateCameraBuffer(ConversionCameraData);

                IrradianceRenderTarget.SetColorAttachment(0, IrradianceTextureTargetCube, 0, face);
                cmd.SetFramebuffer(IrradianceRenderTarget.GPU, 0, face);

                Engine.Renderer.Execute(cmd, ConversionRenderData);
            }
        }

        //public void Render(RenderContext ctx)
        //{
        //    if (Sky == null)
        //        return;

        //    skyMeshData.render_transform = Matrix4x4.CreateScale(2) * Matrix4x4.CreateTranslation(ctx.Camera.Position);

        //    ctx.Renderer.Execute(ctx.CommandList, skyMeshData);
        //}
    }
}
