using DevoidEngine.Core;
using ImGuiNET;

namespace Elemental.Panels
{
    public class SceneViewPanel : ViewportPanel
    {
        public EditorCamera EditorCamera { get; }

        public SceneViewPanel(Scene activeScene) : base("Scene View", activeScene)
        {
            EditorCamera = new EditorCamera();

            // Direct assignment to Viewport
            Viewport.CameraOverride = EditorCamera.Camera;
        }

        public override void OnUpdate(float deltaTime)
        {
            // Keep aspect ratio aligned with window size
            //if (Viewport.Width > 0 && Viewport.Height > 0)
            //{
            //    EditorCamera.SetAspectRatio((float)Viewport.Width / Viewport.Height);
            //}

            //// Update flight controls
            //EditorCamera.OnUpdate(deltaTime, IsHovered);
        }

        protected override void OnViewportOverlayRender()
        {
            // Render Editor Gizmos
            // Engine.GizmoSystem.RenderGizmos(Viewport, LocalMousePos);
        }
    }
}