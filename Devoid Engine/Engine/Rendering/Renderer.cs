using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering.GPUResource;
using DevoidEngine.Engine.Rendering.PostProcessing;
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
        public static IGraphicsDevice GraphicsDevice { get; internal set; }
        public static IRenderTechnique ActiveRenderTechnique;
        public static PostProcessor PostProcessor;

        internal static ResourceManager ResourceManager = new ResourceManager();
        internal static InputLayoutCache inputLayoutCache = new InputLayoutCache();

        static MeshRenderData meshRenderData;
        static UniformBuffer meshRenderDataBuffer;
        static UniformBuffer cameraDataBuffer;

        public static IInputLayout GetInputLayout(Mesh mesh, Shader shader) => inputLayoutCache.Get(GraphicsDevice, mesh.VertexBuffer.GetVertexInfo(), shader.vShader);

        public static void SetupCamera(CameraData cameraData)
        {
            // Per camera data goes here
            cameraDataBuffer.SetData(cameraData);
            cameraDataBuffer.Bind(RenderBindConstants.CameraDataBindSlot, ShaderStage.Vertex | ShaderStage.Fragment);
        }

        public unsafe static void Initialize(int width, int height)
        {
            RenderingDefaults.Initialize();

            meshRenderData = new MeshRenderData();
            meshRenderDataBuffer = new UniformBuffer(sizeof(MeshRenderData));

            cameraDataBuffer = new UniformBuffer(sizeof(CameraData));

            // TESTING
            ActiveRenderTechnique = new ForwardRenderTechnique();

            ActiveRenderTechnique?.Initialize(width, height);

            PostProcessor = new PostProcessor();
            var bloomPass = new BloomPass(width, height);
            var tonemapPass = new TonemapPass(width, height);
            PostProcessor.AddPass(bloomPass);
            PostProcessor.AddPass(tonemapPass);
        }

        public static void Render(CameraRenderContext ctx)
        {
            if (ActiveRenderTechnique == null)
                Console.WriteLine("[Renderer]: Render technique was not set. No Object rendered.");
            Framebuffer activeFrameBuffer = ActiveRenderTechnique?.Render(ctx);
            Renderer.GraphicsDevice.UnbindAllShaderResources();
            //DebugRenderSystem.Render(ctx.cameraData, activeFrameBuffer);

            var Output = activeFrameBuffer.GetRenderTexture(0);
            Texture2D finalColor = PostProcessor.Run(Output);
            RenderAPI.RenderToBuffer(finalColor, ctx.cameraTargetSurface);
        }

        public static void ExecuteDrawList(List<RenderItem> items, RenderState renderState)
        {
            if (items.Count == 0) { return; }

            MaterialInstance currentMaterial = null;
            Shader currentShader = null;
            Mesh currentMesh = null;

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

                if (item.Mesh != currentMesh) { currentMesh = item.Mesh; }
                if (currentMesh == null) continue;

                UpdatePerObjectData(item.Model);

                currentMesh.Bind();
                GetInputLayout(currentMesh, currentShader).Bind();

                currentMesh.Draw();
            }
        }
        static void UpdatePerObjectData(Matrix4x4 model)
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
        static void ApplyRenderState(RenderState renderState)
        {
            Renderer.GraphicsDevice.SetBlendState(renderState.BlendMode);
            Renderer.GraphicsDevice.SetDepthState(renderState.DepthTest, renderState.DepthWrite);
            Renderer.GraphicsDevice.SetRasterizerState(renderState.CullMode, renderState.FillMode);
            Renderer.GraphicsDevice.SetPrimitiveType(renderState.PrimitiveType);
        }

    }
}
