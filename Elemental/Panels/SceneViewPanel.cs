using DevoidEngine.AssetPipeline;
using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Gizmos.DevoidEngine.Gizmos;
using DevoidEngine.UI.Text;
using DevoidEngine.UI.UINodes;
using ImGuiNET;

namespace Elemental.Panels
{
    public class SceneViewPanel : ViewportPanel
    {
        private bool _isNavigating;
        private readonly EditorContext _context;


        public SceneViewPanel(Scene activeScene, EditorContext context) : base("Scene View", activeScene)
        {
            _context = context;
            _context.EditorCamera = new EditorCamera();

            // Direct assignment to Viewport
            Viewport.CameraOverride = context.EditorCamera!.Camera;

            BoundsGizmo transformGizmo = new()
            {

            };

            BoundsGizmo transformGizmo1 = new()
            {

            };

            TranslateGizmo translateGizmo = new() { };

            Viewport.GizmoContext.Gizmos.Add(transformGizmo);
            Viewport.GizmoContext.Gizmos.Add(transformGizmo1);
            Viewport.GizmoContext.Gizmos.Add(translateGizmo);
        }

        public override void OnUpdate(float deltaTime)
        {
            // Keep aspect ratio aligned with window size
            if (Viewport.Width > 0 && Viewport.Height > 0)
            {
                _context.EditorCamera!.SetAspectRatio((float)Viewport.Width / Viewport.Height);
            }

            bool isRmb = ImGui.IsMouseDown(ImGuiMouseButton.Right);
            bool isMmb = ImGui.IsMouseDown(ImGuiMouseButton.Middle);

            // Start navigation session if clicking inside the viewport
            if (IsHovered && (ImGui.IsMouseClicked(ImGuiMouseButton.Right) || ImGui.IsMouseClicked(ImGuiMouseButton.Middle)))
            {
                _isNavigating = true;
                ImGui.SetWindowFocus();
            }

            Engine.Cursor.SetCursorState(((isRmb || isMmb)) ? OpenTK.Windowing.Common.CursorState.Grabbed : OpenTK.Windowing.Common.CursorState.Normal);

            // End navigation session when both camera buttons are released
            if (!isRmb && !isMmb)
            {
                _isNavigating = false;
            }

            // Update camera if the panel is focused OR currently mid-drag navigation
            if (IsFocused || _isNavigating || _context.EditorCamera!.IsFocusing)
            {
                if (_isNavigating)
                {
                    ImGui.SetWindowFocus(); // Keep ImGui window focused during mouse drag
                }

                _context.EditorCamera!.OnUpdate(deltaTime, IsHovered || _isNavigating);
            }
        }

        protected override void OnViewportOverlayRender()
        {
            // Render Editor Gizmos
            // Engine.GizmoSystem.RenderGizmos(Viewport, LocalMousePos);
        }
    }
}