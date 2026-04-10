using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using ElementalEditor.Panels;
using ElementalEditor.Utils;
using ImGuiNET;
using System.Numerics;

namespace ElementalEditor
{
    class EditorLayer : Layer
    {
        EditorContext context;
        EditorCamera editorCamera;
        EditorInputLayer inputLayer;

        List<IEditorPanel> panels;

        public override void OnAttach()
        {
            editorCamera = new EditorCamera(1280, 720);
            inputLayer = new EditorInputLayer(editorCamera);
            Input.Router.Push(inputLayer);

            panels = new List<IEditorPanel>();
            context = new EditorContext();


            panels.Add(new SceneViewportPanel());
            panels.Add(new HierarchyPanel());
            panels.Add(new InspectorPanel());
            panels.Add(new AssetBrowserPanel());



            var scene = new Scene();
            SceneManager.LoadScene(scene);
            scene.Play(true);

            //scene.AddGameObject("Camera").AddComponent<CameraComponent3D>().IsDefault = true;
            scene.AddGameObject("Skybox").AddComponent<SkyboxComponent>();
            Asset.Load<Model>("platform/platform.gltf").Instantiate(scene);
            Asset.Load<Model>("cubey_boi/cubey_boi.gltf").Instantiate(scene);

        }

        public override void OnUpdate(float deltaTime)
        {
            inputLayer.ViewportActive = context.ViewportFocused;
            inputLayer.Update(deltaTime);
            SceneManager.CurrentScene?.Update(deltaTime);
        }
        public override void OnFixedUpdate(float deltaTime)
        {
            SceneManager.CurrentScene?.FixedUpdate(deltaTime);
        }

        public override void OnRender()
        {
            SceneManager.CurrentScene?.Render();
            var cam = editorCamera.Camera;

            CameraRenderContext ctx = new CameraRenderContext();
            ctx.Clear();

            ctx.camera = cam;
            ctx.cameraData = cam.GetCameraData();
            ctx.cameraTargetSurface = cam.RenderTarget;

            var renderables = SceneManager.CurrentScene.GetRenderables();

            foreach (var r in renderables)
            {
                r.Collect(cam, ctx);
            }

            Renderer.Render(ctx);
        }

        public override void OnResize(int width, int height)
        {
            base.OnResize(width, height);
        }

        public override void OnGUIRender()
        {
            var io = ImGui.GetIO();

            if (inputLayer.IsNavigating) // RMB held
            {
                io.ConfigFlags |= ImGuiConfigFlags.NoMouse;
            }
            else
            {
                io.ConfigFlags &= ~ImGuiConfigFlags.NoMouse;
            }

            if (SceneManager.CurrentScene != null)
            {

                context.Scene = SceneManager.CurrentScene;
                context.EditorCamera = editorCamera;
            }

            foreach (var panel in panels)
                panel.Draw(context);
        }
    }
}