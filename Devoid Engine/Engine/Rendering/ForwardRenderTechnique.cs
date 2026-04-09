using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.Utilities;
using SharpFont;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Engine.Rendering
{
    public class ForwardRenderTechnique : IRenderTechnique
    {
        Framebuffer finalOutputBuffer = null!;

        // Lights
        const int MAX_POINTLIGHTS = 100;
        const int MAX_SPOTLIGHTS = 20;
        const int MAX_DIRECTIONALLIGHTS = 1;

        StorageBuffer<GPUPointLight> pointLightBuffer = null!;
        StorageBuffer<GPUDirectionalLight> directionalLightBuffer = null!;
        StorageBuffer<GPUSpotLight> spotLightBuffer = null!;

        UniformBuffer sceneDataBuffer = null!;

        SceneData sceneData;

        List<RenderItem> visibleItems = null!;


        public RenderState renderStateOverride = RenderState.DefaultRenderState;
        public void Dispose()
        {

        }

        public unsafe void Initialize(int width, int height)
        {
            pointLightBuffer = new StorageBuffer<GPUPointLight>(MAX_POINTLIGHTS, DevoidGPU.BufferUsage.Dynamic, false);
            directionalLightBuffer = new StorageBuffer<GPUDirectionalLight>(MAX_DIRECTIONALLIGHTS, DevoidGPU.BufferUsage.Dynamic, false);
            spotLightBuffer = new StorageBuffer<GPUSpotLight>(MAX_SPOTLIGHTS, DevoidGPU.BufferUsage.Dynamic, false);
            sceneDataBuffer = new UniformBuffer(Unsafe.SizeOf<SceneData>(), DevoidGPU.BufferUsage.Dynamic);

            visibleItems = new List<RenderItem>();

            finalOutputBuffer = new Framebuffer();

            finalOutputBuffer.AttachRenderTexture(new Texture2D(new DevoidGPU.TextureDescription()
            {
                Width = width,
                Height = height,
                Format = DevoidGPU.TextureFormat.RGBA16_Float,
                GenerateMipmaps = false,
                IsDepthStencil = false,
                IsRenderTarget = true,
                IsMutable = false,
            }));
            finalOutputBuffer.AttachDepthTexture(new Texture2D(new DevoidGPU.TextureDescription()
            {
                Width = width,
                Height = height,
                Format = DevoidGPU.TextureFormat.Depth24_Stencil8,
                GenerateMipmaps = false,
                IsDepthStencil = true,
                IsRenderTarget = false,
                IsMutable = false
            }));
        }

        public Framebuffer Render(CameraRenderContext ctx)
        {
            finalOutputBuffer.Bind();
            finalOutputBuffer.Clear();

            Renderer.SkyboxRenderer.Render(ctx);

            UploadLights(ctx);
            UploadSceneData(ctx);

            Renderer.SetupCamera(ctx.cameraData);

            BuildVisibleList(ctx);

            Renderer.ExecuteDrawList(visibleItems, renderStateOverride);

            return finalOutputBuffer;
        }

        void BuildVisibleList(CameraRenderContext ctx)
        {
            int culledCount = 0;
            visibleItems.Clear();

            var camera = ctx.camera;

            foreach (var item in ctx.renderItems3D)
            {
                if (item.Mesh == null)
                    continue;

                Vector3 worldMin, worldMax;

                BoundingBox.TransformAABB(
                    item.Mesh.LocalBounds.min,
                    item.Mesh.LocalBounds.max,
                    item.Model,
                    out worldMin,
                    out worldMax
                );

                if (!camera.IntersectsAABB(worldMin, worldMax))
                {
                    culledCount++;
                    continue;
                }

                visibleItems.Add(item);
            }
        }

        void UploadSceneData(CameraRenderContext ctx)
        {

            sceneData = new SceneData();
            sceneData.pointLightCount = (uint)ctx.pointLights.Count;
            sceneData.directionalLightCount = (uint)ctx.directionalLights.Count;
            sceneData.spotLightCount = (uint)ctx.spotLights.Count;

            sceneDataBuffer.SetData(sceneData);
            sceneDataBuffer.Bind(RenderBindConstants.SceneDataBindSlot, DevoidGPU.ShaderStage.Fragment);
        }

        void UploadLights(CameraRenderContext ctx)
        {
            // Use the existing List overload instead of .ToArray()
            pointLightBuffer.SetData(ctx.pointLights, ctx.pointLights.Count, 0);
            pointLightBuffer.Bind(RenderBindConstants.PointLightBufferBindSlot, DevoidGPU.ShaderStage.Fragment);

            directionalLightBuffer.SetData(ctx.directionalLights, ctx.directionalLights.Count, 0);
            directionalLightBuffer.Bind(RenderBindConstants.DirLightBufferBindSlot, DevoidGPU.ShaderStage.Fragment);

            spotLightBuffer.SetData(ctx.spotLights, ctx.spotLights.Count, 0);
            spotLightBuffer.Bind(RenderBindConstants.SpotLightBufferBindSlot, DevoidGPU.ShaderStage.Fragment);
        }

        public void Resize(int width, int height)
        {
            finalOutputBuffer.Resize(width, height);
        }
    }
}