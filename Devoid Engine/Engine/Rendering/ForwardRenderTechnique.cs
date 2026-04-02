using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.Utilities;
using SharpFont;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Engine.Rendering
{
    public class ForwardRenderTechnique : IRenderTechnique
    {
        Framebuffer finalOutputBuffer;

        // Lights
        const int MAX_POINTLIGHTS = 100;
        const int MAX_SPOTLIGHTS = 20;
        const int MAX_DIRECTIONALLIGHTS = 1;

        StorageBuffer<GPUPointLight> pointLightBuffer;
        StorageBuffer<GPUDirectionalLight> directionalLightBuffer;
        StorageBuffer<GPUSpotLight> spotLightBuffer;

        UniformBuffer sceneDataBuffer;

        SceneData sceneData;


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

            Renderer.GraphicsDevice.SetViewport(0, 0, (int)Screen.Size.X, (int)Screen.Size.Y);

            Renderer.SkyboxRenderer.Render(ctx);

            UploadLights(ctx);
            UploadSceneData(ctx);

            Renderer.SetupCamera(ctx.cameraData);
            Renderer.ExecuteDrawList(ctx.renderItems3D, renderStateOverride);


            return finalOutputBuffer;
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