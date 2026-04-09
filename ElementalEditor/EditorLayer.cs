using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using ElementalEditor.Panels;
using ImGuiNET;

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
            panels.Add(new SceneViewportPanel());
            panels.Add(new HierarchyPanel());
            panels.Add(new InspectorPanel());

            var scene = new Scene();
            SceneManager.LoadScene(scene);
            scene.Play(true);

            scene.AddGameObject("Camera").AddComponent<CameraComponent3D>().IsDefault = true;
            scene.AddGameObject("Skybox").AddComponent<SkyboxComponent>();


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