using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public class SkyboxRenderer
    {
        public TextureCube SkyboxTexture { get; private set; }
        public TextureCube IrradianceTexture { get; private set; }
        public TextureCube PreFilterTexture { get; private set; }
        public Texture2D BrdfLutTexture { get; private set; }

        private Shader skyboxShader;
        private Shader panoramaConvertShader;
        private Shader irradianceShader;
        private Shader prefilterShader;
        private Shader brdfLutShader;

        private Framebuffer cubemapCreationFB;
        private Framebuffer irradianceMapFB;
        private Framebuffer prefilterMapFB;
        private Framebuffer brdfLutFB;

        private Mesh cubeMesh;

        private Texture2D panorama;

        private Matrix4x4 captureProjection;
        private Matrix4x4[] captureViews;

        private bool dirty = true;

        private const int SKY_SIZE = 1024;
        private const int IRR_SIZE = 32;
        private const int PREF_SIZE = 128;
        private const int BRDF_SIZE = 512;
        private const int PREF_MIPS = 5;

        private RenderState skyboxRenderState;

        private MaterialInstance panoramaConvertMaterial;
        private MaterialInstance skyboxMaterial;
        private MaterialInstance irradianceMaterial;
        private MaterialInstance prefilterMaterial;
        private MaterialInstance brdfMaterial;

        private CameraData conversionCameraData;

        public void Initialize()
        {
            skyboxRenderState = new RenderState()
            {
                BlendMode = BlendMode.Opaque,
                CullMode = CullMode.Front,
                DepthTest = DepthTest.Disabled,
                DepthWrite = false,
                FillMode = FillMode.Solid,
                PrimitiveType = PrimitiveType.Triangles
            };

            cubeMesh = RenderConstants.Cube;

            cubemapCreationFB = new Framebuffer();
            brdfLutFB = new Framebuffer();
            prefilterMapFB = new Framebuffer();
            irradianceMapFB = new Framebuffer();

            panoramaConvertShader = new Shader("Engine/Content/Shaders/Common/base_3d.vert.hlsl",
                                              "Engine/Content/Shaders/Skybox/panoramaConvert.frag.hlsl");

            skyboxShader = new Shader("Engine/Content/Shaders/Screen/renderToScreen.vert.hlsl",
                                  "Engine/Content/Shaders/Skybox/skybox_hdri.frag.hlsl");

            irradianceShader = new Shader("Engine/Content/Shaders/Common/base_3d.vert.hlsl",
                                         "Engine/Content/Shaders/Skybox/irradiance.frag.hlsl");

            prefilterShader = new Shader("Engine/Content/Shaders/Common/base_3d.vert.hlsl",
                                        "Engine/Content/Shaders/Skybox/prefilter.frag.hlsl");

            //skyboxShader = new Shader("Engine/Content/Shaders/fullscreen.vert",
            //                       "Engine/Content/Shaders/skybox.frag");

            brdfLutShader = new Shader("Engine/Content/Shaders/Screen/renderToScreen.vert.hlsl",
                                        "Engine/Content/Shaders/Skybox/brdf.frag.hlsl");

            panoramaConvertMaterial = new MaterialInstance(new Material(panoramaConvertShader));

            irradianceMaterial = new MaterialInstance(new Material(irradianceShader));

            prefilterMaterial = new MaterialInstance(new Material(prefilterShader));

            // capture projection
            captureProjection = Matrix4x4.CreatePerspectiveFieldOfView(
                MathF.PI * 0.5f, 1f, 0.1f, 1000f);

            captureViews = new[]
            {
                Matrix4x4.CreateLookAt(Vector3.Zero,  Vector3.UnitX,  -Vector3.UnitY),
                Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitX,  -Vector3.UnitY),
                Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitY,   -Vector3.UnitZ),
                Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitY,  Vector3.UnitZ),
                Matrix4x4.CreateLookAt(Vector3.Zero,  Vector3.UnitZ,  -Vector3.UnitY),
                Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ,  -Vector3.UnitY)
            };

            conversionCameraData = new CameraData()
            {
                Projection = captureProjection,
                View = captureViews[0],
                ScreenSize = new Vector2(SKY_SIZE),
                FarClip = 1000,
                NearClip = 0.1f,
                Position = new Vector3(0)
            };

            CreateTextures();

            Texture2D monoStudio = Helper.LoadHDRI("Engine/Content/HDRIs/suburban_garden_4k.hdr");
            SetPanorama(monoStudio);
        }

        public void SetPanorama(Texture2D hdr)
        {
            panorama = hdr;
            dirty = true;
        }
        public void Update()
        {
            if (!dirty || panorama == null)
                return;

            ConvertPanoramaToCubemap();
            GenerateIrradiance();
            GeneratePrefilter();
            GenerateBRDF();

            dirty = false;
        }

        public void Render(CameraRenderContext ctx)
        {
            if (SkyboxTexture == null) return;

            Update();

            Renderer.SetupCamera(ctx.cameraData);
            Renderer.ApplyRenderState(skyboxRenderState);

            skyboxShader.Use();
            SkyboxTexture.Bind(0);
            Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);
            RenderAPI.RenderFullScreen(skyboxShader);
        }

        public void BindIBL(Material material)
        {
            material.SetTexture("ENV_Irradiance", IrradianceTexture);
            material.SetTexture("ENV_Prefilter", PreFilterTexture);
            material.SetTexture("ENV_BRDF", BrdfLutTexture);
        }

        private void CreateTextures()
        {
            SkyboxTexture = new TextureCube(new TextureDescription()
            {
                Width = SKY_SIZE,
                Height = SKY_SIZE,
                Format = TextureFormat.RGBA16_Float,
                Type = TextureType.TextureCube,
                IsRenderTarget = true,
                GenerateMipmaps = true,
                MipLevels = 0
            });

            IrradianceTexture = new TextureCube(new TextureDescription()
            {
                Width = IRR_SIZE,
                Height = IRR_SIZE,
                Format = TextureFormat.RGBA16_Float,
                Type = TextureType.TextureCube,
                MipLevels = 1,
                GenerateMipmaps = false,
                IsRenderTarget = true
            });

            PreFilterTexture = new TextureCube(new TextureDescription()
            {
                Width = PREF_SIZE,
                Height = PREF_SIZE,
                Format = TextureFormat.RGBA16_Float,
                Type = TextureType.TextureCube,
                GenerateMipmaps = true,
                MipLevels = PREF_MIPS,
                IsRenderTarget = true
            });

            PreFilterTexture.SetFilter(TextureFilter.Linear, TextureFilter.Linear);

            BrdfLutTexture = new Texture2D(new TextureDescription()
            {
                Width = BRDF_SIZE,
                Height = BRDF_SIZE,
                Format = TextureFormat.RG16_Float,
                Type = TextureType.Texture2D,
                MipLevels = 1,
                GenerateMipmaps = false,
                IsRenderTarget = true
            });
        }

        // PASSES

        private void ConvertPanoramaToCubemap()
        {
            (int,int,int,int) prevViewportSize = Renderer.GraphicsDevice.GetViewport();
            Renderer.GraphicsDevice.SetViewport(0, 0, SKY_SIZE, SKY_SIZE);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);
            Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);

            panoramaConvertMaterial.SetTexture("MAT_PANORAMA_TEX", panorama);

            for (int face = 0; face < 6; face++)
            {
                conversionCameraData.View = captureViews[face];
                Renderer.SetupCamera(conversionCameraData);
                cubemapCreationFB.SetRenderTexture(SkyboxTexture, (CubeFace)face, 0, 0);
                cubemapCreationFB.Bind();

                panoramaConvertMaterial.Bind();

                IInputLayout inputLayout = Renderer.GetInputLayout(cubeMesh, panoramaConvertShader);
                inputLayout.Bind();

                cubeMesh.Bind();
                cubeMesh.Draw();

            }

            SkyboxTexture.GenerateMipmaps();
            Renderer.GraphicsDevice.MainSurface.Bind();
            Renderer.GraphicsDevice.SetViewport(0, 0, prevViewportSize.Item3, prevViewportSize.Item4);
        }

        private void GenerateIrradiance()
        {
            (int, int, int, int) prevViewportSize = Renderer.GraphicsDevice.GetViewport();
            Renderer.GraphicsDevice.SetViewport(0, 0, IRR_SIZE, IRR_SIZE);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);
            Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);

            irradianceMaterial.SetTexture("MAT_ENVIRONMENT_MAP", SkyboxTexture);

            IInputLayout inputLayout = Renderer.GetInputLayout(cubeMesh, irradianceShader);
            inputLayout.Bind();

            for (int face = 0; face < 6; face++)
            {
                conversionCameraData.View = captureViews[face];
                Renderer.SetupCamera(conversionCameraData);

                irradianceMapFB.SetRenderTexture(IrradianceTexture, (CubeFace)face, 0, 0);
                irradianceMapFB.Bind();

                irradianceMaterial.Bind();

                cubeMesh.Bind();
                cubeMesh.Draw();
            }

            Renderer.GraphicsDevice.MainSurface.Bind();
            Renderer.GraphicsDevice.SetViewport(prevViewportSize.Item1, prevViewportSize.Item2, prevViewportSize.Item3, prevViewportSize.Item4);
        }

        private void GeneratePrefilter()
        {
            (int, int, int, int) prevViewportSize = Renderer.GraphicsDevice.GetViewport();
            Renderer.GraphicsDevice.SetViewport(0, 0, PREF_SIZE, PREF_SIZE);
            Renderer.GraphicsDevice.SetPrimitiveType(PrimitiveType.Triangles);
            Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);

            PreFilterTexture.GenerateMipmaps();

            prefilterMaterial.SetTexture("MAT_ENVIRONMENT_MAP", SkyboxTexture);

            IInputLayout inputLayout = Renderer.GetInputLayout(cubeMesh, prefilterShader);
            inputLayout.Bind();

            for (int mip = 0; mip < PREF_MIPS; mip++)
            {
                int size = PREF_SIZE >> mip;
                Renderer.GraphicsDevice.SetViewport(0, 0, size, size);

                float roughness = (float)mip / (PREF_MIPS - 1);
                prefilterMaterial.SetFloat("Roughness", roughness);

                for (int face = 0; face < 6; face++)
                {
                    conversionCameraData.View = captureViews[face];
                    Renderer.SetupCamera(conversionCameraData);

                    prefilterMapFB.SetRenderTexture(PreFilterTexture, (CubeFace)face, mip, 0);
                    prefilterMapFB.Bind();

                    prefilterMaterial.Bind();

                    cubeMesh.Bind();
                    cubeMesh.Draw();
                }
            }

            Renderer.GraphicsDevice.MainSurface.Bind();
            Renderer.GraphicsDevice.SetViewport(prevViewportSize.Item1, prevViewportSize.Item2, prevViewportSize.Item3, prevViewportSize.Item4);
        }

        private void GenerateBRDF()
        {
            (int, int, int, int) prevViewportSize = Renderer.GraphicsDevice.GetViewport();
            Renderer.GraphicsDevice.SetViewport(0, 0, BRDF_SIZE, BRDF_SIZE);
            Renderer.GraphicsDevice.SetDepthState(DepthTest.Disabled, false);
            Renderer.GraphicsDevice.SetRasterizerState(CullMode.None);
            Renderer.GraphicsDevice.SetBlendState(BlendMode.Opaque);

            brdfLutFB.SetRenderTexture(BrdfLutTexture, 0);
            brdfLutFB.Bind();
            brdfLutShader.Use();
            RenderAPI.RenderFullScreen(brdfLutShader);

            Renderer.GraphicsDevice.MainSurface.Bind();
            Renderer.GraphicsDevice.SetViewport(prevViewportSize.Item1, prevViewportSize.Item2, prevViewportSize.Item3, prevViewportSize.Item4);
        }
    }
}
