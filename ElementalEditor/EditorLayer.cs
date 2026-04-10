using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using ElementalEditor.Panels;
using ImGuiNET;
using System.Numerics;

namespace ElementalEditor
{
    class EditorLayer : Layer
    {
        EditorContext context;

        List<IEditorPanel> panels;

        public override void OnAttach()
        {
            panels = new List<IEditorPanel>();
            context = new EditorContext();

            context.EditorCameraTransform = new Transform3D();
            context.EditorCameraTransform.Position = new Vector3(0, 2, 5);

            context.EditorCamera = new Camera();
            context.EditorCamera.FovY = MathHelper.DegToRad(60);


            panels.Add(new SceneViewportPanel());
            panels.Add(new HierarchyPanel());
            panels.Add(new InspectorPanel());
            panels.Add(new AssetBrowserPanel());



            var scene = new Scene();
            SceneManager.LoadScene(scene);
            scene.Play(true);

            scene.AddGameObject("Camera").AddComponent<CameraComponent3D>().IsDefault = true;
            scene.AddGameObject("Skybox").AddComponent<SkyboxComponent>();


        }

        void DrawToolbar()
        {

            ImGui.SetNextWindowSize(new Vector2(100, 30));

            ImGui.Begin("##Toolbar",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse);

            ImGui.SetCursorPosX(ImGui.GetWindowWidth() * 0.5f - 60);

            bool playing = context.PlayState == ScenePlayState.Play;
            bool paused = context.PlayState == ScenePlayState.Pause;

            if (ImGui.Button(playing ? "Stop" : "Play"))
            {
                if (playing)
                {
                    context.PlayState = ScenePlayState.Edit;
                    context.Scene.Play(false);
                }
                else
                {
                    context.PlayState = ScenePlayState.Play;
                    context.Scene.Play(true);
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Pause"))
            {
                if (context.PlayState == ScenePlayState.Play)
                {
                    context.PlayState = ScenePlayState.Pause;
                }
            }

            ImGui.End();
        }

        public override void OnUpdate(float deltaTime)
        {
            SceneManager.CurrentScene?.Update(deltaTime);
        }

        public override void OnRender()
        {
            SceneManager.CurrentScene?.Render();
        }

        public override void OnResize(int width, int height)
        {
            base.OnResize(width, height);
        }

        public override void OnGUIRender()
        {
            if (SceneManager.CurrentScene != null)
            {
                context.SceneViewportTarget = (Texture2D)SceneManager.CurrentScene.GetDefaultCamera3D()?.Camera.RenderTarget.GetRenderTexture(0);

                context.Scene = SceneManager.CurrentScene;
            }

            foreach (var panel in panels)
                panel.Draw(context);
        }
    }
}