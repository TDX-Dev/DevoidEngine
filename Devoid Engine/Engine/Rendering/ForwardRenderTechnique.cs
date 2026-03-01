using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Utilities;
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

        UniformBuffer sceneDataBuffer;

        SceneData sceneData;


        public RenderState renderStateOverride = RenderState.DefaultRenderState;
        public void Dispose()
        {

        }

        public unsafe void Initialize(int width, int height)
        {
            Console.WriteLine($"{width}:{height}");
            pointLightBuffer = new StorageBuffer<GPUPointLight>(MAX_POINTLIGHTS, DevoidGPU.BufferUsage.Dynamic, false);
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

            UIRenderer.CreateCameraData(width, height);
        }

        public Texture2D Render(CameraRenderContext ctx)
        {

            finalOutputBuffer.Bind();
            finalOutputBuffer.Clear();

            Renderer.graphicsDevice.SetViewport(0, 0, Renderer.Width, Renderer.Height);

            UploadLights(ctx);
            UploadSceneData(ctx);

            RenderBase.SetupCamera(ctx.cameraData);
            RenderBase.Execute(ctx.renderItems3D, renderStateOverride);

            RenderBase.SetupCamera(UIRenderer.ScreenData);
            RenderBase.Execute(ctx.renderItemsUI, UIRenderer.renderState);


            return finalOutputBuffer.GetRenderTexture(0);
        }

        void UploadSceneData(CameraRenderContext ctx)
        {
            sceneData = new SceneData();
            sceneData.pointLightCount = (uint)ctx.pointLights.Count;

            sceneDataBuffer.SetData(sceneData);
            sceneDataBuffer.Bind(RenderBindConstants.SceneDataBindSlot, DevoidGPU.ShaderStage.Fragment);
        }

        void UploadLights(CameraRenderContext ctx)
        {
            pointLightBuffer.SetData(ctx.pointLights.ToArray(), 0);
            pointLightBuffer.Bind(RenderBindConstants.PointLightBufferBindSlot, DevoidGPU.ShaderStage.Fragment);

        }

        public void Resize(int width, int height)
        {
            finalOutputBuffer.Resize(width, height);
            UIRenderer.CreateCameraData(width, height);
        }
    }
}
