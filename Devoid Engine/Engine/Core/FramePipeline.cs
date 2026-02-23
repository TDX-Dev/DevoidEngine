using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;

namespace DevoidEngine.Engine.Core
{


    public class CameraRenderContext
    {
        public CameraData cameraData;

        public List<RenderItem> renderItems3D = new();
        public List<RenderItem> renderItems2D = new();
        public List<RenderItem> renderItemsUI = new();

        public List<GPUPointLight> pointLights = new();
        public List<GPUSpotLight> spotLights = new();
        public List<GPUDirectionalLight> directionalLights = new();

        public void Clear()
        {
            renderItems3D.Clear(); renderItems2D.Clear(); renderItemsUI.Clear(); pointLights.Clear();
            spotLights.Clear(); directionalLights.Clear();
        }
    }

    public static class FramePipeline
    {
        //static Pool<CameraRenderContext> CameraContextPool = new Pool<CameraRenderContext>();

        static AsyncDoubleBuffer<List<CameraRenderContext>> SwapBuffer;

        static FramePipeline()
        {
            SwapBuffer = new AsyncDoubleBuffer<List<CameraRenderContext>>(new List<CameraRenderContext>(), new List<CameraRenderContext>());
        }

        public static void ExecuteUpdateThread(float deltaTime)
        {
            if (!SceneManager.IsSceneLoaded()) return;

            var backList = SwapBuffer.Back;
            Scene scene = SceneManager.CurrentScene;
            var cameras = scene.GetComponentsOfType<CameraComponent3D>();

            // CRITICAL FIX
            backList.Clear();

            for (int i = 0; i < cameras.Count; i++)
            {
                var cameraComponent = cameras[i];

                CameraRenderContext ctx = new CameraRenderContext();
                ctx.cameraData = cameraComponent.Camera.GetCameraData();

                foreach (var renderable in scene.Renderables)
                {
                    renderable.Collect(cameraComponent, ctx);
                }

                backList.Add(ctx);
            }

            SwapBuffer.Publish();
        }

        public static void ExecuteRenderThread(float deltaTime)
        {
            RenderCommandQueue.Execute();


            List<CameraRenderContext> cameraContextList = SwapBuffer.Front;

            // Frame level shader bindings go here

            for (int i = 0; i < cameraContextList.Count; i++)
            {
                CameraRenderContext ctx = cameraContextList[i];
                RenderBase.Render(ctx);
            }

        }


    }
}
