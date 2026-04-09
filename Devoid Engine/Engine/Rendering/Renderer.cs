using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidEngine.Engine.Rendering.PostProcessing;
using DevoidEngine.Engine.Rendering.Shadows;
using DevoidEngine.Engine.UI;
using DevoidGPU;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Rendering
{
    public static class Renderer
    {
        public static IGraphicsDevice GraphicsDevice { get; internal set; } = null!;
        public static IRenderTechnique? ActiveRenderTechnique;
        public static SkyboxRenderer SkyboxRenderer = null!;
        public static ShadowSystem ShadowSystem = null!;
        public static PostProcessor PostProcessor = null!;

        internal static ResourceManager ResourceManager = new ResourceManager();
        internal static InputLayoutCache inputLayoutCache = new InputLayoutCache();

        static MeshRenderData meshRenderData;
        static UniformBuffer meshRenderDataBuffer = null!;
        static UniformBuffer cameraDataBuffer = null!;

        static Framebuffer UIFramebuffer = null!;
        static Texture2D UIRenderOutput = null!;

        static Framebuffer GBufferFramebuffer = null!;
        //static Texture2D GBufferPosition = null!;
        //static Texture2D GBufferNormal = null!;
        static Texture2D GBufferDepth = null!;
        static RenderState GBufferRenderState = null!;
        static Shader GBufferShader = null!;

        static Stack<(int x, int y, int w, int h)> viewportStack = new Stack<(int, int, int, int)>();

        public static void SetViewport(int x, int y, int width, int height)
        {
            GraphicsDevice.SetViewport(x,y,width,height);
        }

        public static void PushViewport(int x, int y, int width, int height)
        {
            var current = GraphicsDevice.GetViewport();
            viewportStack.Push(current);

            GraphicsDevice.SetViewport(x, y, width, height);
        }

        public static void RestoreViewport()
        {
            if (viewportStack.Count == 0)
                throw new Exception("Viewport stack is empty");

            var vp = viewportStack.Peek();
            GraphicsDevice.SetViewport(vp.Item1, vp.Item2, vp.Item3, vp.Item4);
        }

        public static void PopViewport(bool popOnly = false)
        {
            if (viewportStack.Count == 0)
                throw new Exception("Viewport stack underflow");

            var vp = viewportStack.Pop();
            if (!popOnly)
                GraphicsDevice.SetViewport(vp.Item1, vp.Item2, vp.Item3, vp.Item4);
        }

        public static IInputLayout GetInputLayout(Mesh mesh, Shader shader)
        {
            if (mesh.VertexBuffer == null)
                throw new Exception("Mesh vertexbuffer was null");
            return inputLayoutCache.Get(GraphicsDevice, mesh.VertexBuffer.GetVertexInfo(), shader.vShader);
        }

        public static void SetupCamera(CameraData cameraData)
        {
            // Per camera data goes here
            cameraDataBuffer.SetData(cameraData);
            cameraDataBuffer.Bind(RenderBindConstants.CameraDataBindSlot, ShaderStage.Vertex | ShaderStage.Fragment);
        }

        public unsafe static void Initialize(int width, int height)
        {
            if (GraphicsDevice == null)
                throw new Exception("Graphics device has not been set.");

            GBufferRenderState = new RenderState()
            {
                BlendMode = BlendMode.Opaque,
                DepthTest = DepthTest.LessEqual,
                DepthWrite = true,
                CullMode = CullMode.Back,
                PrimitiveType = PrimitiveType.Triangles,
                FillMode = FillMode.Solid
            };

            GBufferShader = ShaderLibrary.GetShader("Screen/GBUFFER") ?? throw new Exception("GBuffer shader not loaded");

            meshRenderData = new MeshRenderData();
            meshRenderDataBuffer = new UniformBuffer(sizeof(MeshRenderData));

            cameraDataBuffer = new UniformBuffer(sizeof(CameraData));

            UIFramebuffer = new Framebuffer();

            UIRenderOutput = new Texture2D(new TextureDescription()
            {
                Width = width,
                Height = height,
                Format = TextureFormat.RGBA8_UNorm,
                GenerateMipmaps = false,
                MipLevels = 1,
                IsDepthStencil = false,
                IsRenderTarget = true,
                Type = TextureType.Texture2D,
            });

            UIFramebuffer.SetRenderTexture(UIRenderOutput, 0);

            GBufferFramebuffer = new Framebuffer();
            GBufferDepth = new Texture2D(new TextureDescription()
            {
                Width = width,
                Height = height,
                Format = TextureFormat.Depth32_Float,
                IsDepthStencil = true,
                GenerateMipmaps = false,
                MipLevels = 1,
                IsRenderTarget = false,
                Type = TextureType.Texture2D,
            });
            GBufferFramebuffer.AttachDepthTexture(GBufferDepth);

            // TESTING
            ActiveRenderTechnique = new ForwardRenderTechnique();

            ActiveRenderTechnique?.Initialize(width, height);

            SkyboxRenderer = new SkyboxRenderer();

            ShadowSystem = new ShadowSystem();
            ShadowSystem.Initialize();

            RenderingDefaults.Initialize();

            PostProcessor = new PostProcessor();
            var bloomPass = new BloomPass(width, height);
            var volumetricPass = new VolumetricLightPass(width, height);
            var tonemapPass = new TonemapPass(width, height);
            PostProcessor.AddPass(bloomPass);
            PostProcessor.AddPass(volumetricPass);
            PostProcessor.AddPass(tonemapPass);
        }

        public static void Render(CameraRenderContext ctx)
        {
            if (ActiveRenderTechnique == null)
            {
                Console.WriteLine("[Renderer]: Render technique was not set. No Object rendered.");
                return;
            }

            PushViewport(0, 0, (int)ctx.cameraData.ScreenSize.X, (int)ctx.cameraData.ScreenSize.Y);

            RenderUI(ctx.renderItemsUI);
            RenderGBuffer(ctx.renderItems3D, ctx);
            ShadowSystem.RenderShadowMaps(ctx);
            Framebuffer activeFrameBuffer = ActiveRenderTechnique.Render(ctx);

            DebugRenderSystem.Render(ctx.cameraData, activeFrameBuffer);

            var Output = (Texture2D)activeFrameBuffer.GetRenderTexture(0);
            Texture2D finalColor = PostProcessor.Run(Output, ctx);
            RenderAPI.RenderToBuffer(finalColor, ctx.cameraTargetSurface);

            Renderer.GraphicsDevice.UnbindAllShaderResources();

            RenderAPI.RenderToBuffer(UIRenderOutput, ctx.cameraTargetSurface);

            PopViewport();


            if (viewportStack.Count > 0)
                throw new Exception("Viewport stack was not popped within render frame");
        }

        public static void RenderUI(List<RenderItem> renderItems)
        {
            UIFramebuffer.Bind();
            UIFramebuffer.Clear(Vector4.Zero);
            GraphicsDevice.SetViewport(0, 0, (int)Screen.Size.X, (int)Screen.Size.Y);
            Renderer.SetupCamera(UISystem.ScreenData);
            Renderer.ExecuteDrawList(renderItems, UISystem.RenderState);
        }

        public static void RenderGBuffer(List<RenderItem> renderItems, CameraRenderContext ctx)
        {
            GBufferFramebuffer.Bind();
            GBufferFramebuffer.Clear();

            Renderer.SetupCamera(ctx.cameraData);
            ApplyRenderState(GBufferRenderState);

            GBufferShader.Use();

            Mesh? currentMesh = null;

            for (int i = 0; i < renderItems.Count; i++)
            {
                var item = renderItems[i];

                UpdatePerObjectData(item.Model);

                if (item.Mesh != currentMesh)
                {
                    currentMesh = item.Mesh;

                    currentMesh.Bind();
                    GetInputLayout(currentMesh, GBufferShader)?.Bind();
                }

                currentMesh.Draw();
            }
        }

        public static void ExecuteDrawList(List<RenderItem> items, RenderState renderState)
        {
            if (items.Count == 0) { return; }

            MaterialInstance? currentMaterial = null;
            Shader? currentShader = null;
            Mesh? currentMesh = null;

            bool currentClipState = false;
            Vector4 currentClipRect = default;

            ApplyRenderState(renderState);

            int currentObjectDataBindSlot = -1;

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.useClipping != currentClipState || item.ClipRegion != currentClipRect)
                {
                    currentClipState = item.useClipping;
                    currentClipRect = item.ClipRegion;

                    ApplyClipRegion(currentClipState, currentClipRect);
                }


                if (item.Material != currentMaterial)
                {
                    currentMaterial = item.Material;
                    if (item.Material.BaseMaterial.Shader != currentShader)
                    {
                        currentShader = item.Material.BaseMaterial.Shader;

                        int perObjectBindSlot = currentShader.vShader.ReflectionData.GetUniformBufferSlot("perObject");

                        if (perObjectBindSlot != currentObjectDataBindSlot)
                        {
                            currentObjectDataBindSlot = perObjectBindSlot;

                            if (currentObjectDataBindSlot == -1)
                            {
                                Console.WriteLine("Shader does not implement PerObject uniform buffer.\nObject data cannot be sent to shader.");
                                continue;
                            }

                            meshRenderDataBuffer.Bind(currentObjectDataBindSlot, ShaderStage.Vertex);
                        }

                        currentShader.Use();
                    }
                    currentMaterial.Bind();
                }

                if (item.Mesh != currentMesh)
                {
                    currentMesh = item.Mesh;

                    if (currentMesh != null)
                    {
                        currentMesh.Bind();
                        GetInputLayout(currentMesh, currentShader!)?.Bind();
                    }
                }
                if (currentMesh == null)
                    continue;

                UpdatePerObjectData(item.Model);

                currentMesh.Bind();

                currentMesh.Draw();
            }
        }

        public static void Resize(int width, int height)
        {
            GraphicsDevice.MainSurface.Resize(width, height);
            UIFramebuffer.Resize(width, height);
            ActiveRenderTechnique?.Resize(width, height);
            PostProcessor.Resize(width, height);
            UISystem.Resize(width, height);
        }
        public static void UpdatePerObjectData(Matrix4x4 model)
        {
            Matrix4x4.Invert(model, out var ModelMatrixInv);

            meshRenderData = new MeshRenderData()
            {
                ModelMatrix = model,
                ModelMatrixInv = ModelMatrixInv
            };


            meshRenderDataBuffer.SetData(meshRenderData);
        }

        static void ApplyClipRegion(bool state, Vector4 rect)
        {
            Renderer.GraphicsDevice.SetScissorState(state);

            if (state)
                Renderer.GraphicsDevice.SetScissorRectangle(
                    (int)rect.X, (int)rect.Y,
                    (int)rect.Z, (int)rect.W);
        }
        public static void ApplyRenderState(RenderState renderState)
        {
            Renderer.GraphicsDevice.SetBlendState(renderState.BlendMode);
            Renderer.GraphicsDevice.SetDepthState(renderState.DepthTest, renderState.DepthWrite);
            Renderer.GraphicsDevice.SetRasterizerState(renderState.CullMode, renderState.FillMode);
            Renderer.GraphicsDevice.SetPrimitiveType(renderState.PrimitiveType);
        }

    }
}
