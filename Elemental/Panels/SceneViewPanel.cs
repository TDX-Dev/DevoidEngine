using DevoidEngine.AssetPipeline;
using DevoidEngine.Assets;
using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Gizmos.DevoidEngine.Gizmos;
using DevoidEngine.UI.Text;
using DevoidEngine.UI.UINodes;
using ImGuiNET;
using System.Text;

namespace Elemental.Panels
{
    public class SceneViewPanel : ViewportPanel
    {
        private bool _isNavigating;
        private readonly EditorContext _context;

        public Action<string>? AssetDropRequested;



        public SceneViewPanel(Scene activeScene, EditorContext context) : base("Scene View", activeScene)
        {
            _context = context;
            _context.EditorCamera = new EditorCamera();

            // Direct assignment to Viewport
            Viewport.CameraOverride = context.EditorCamera!.Camera;
        }

        public override void OnUpdate(float deltaTime)
        {
            if (Viewport.Width > 0 && Viewport.Height > 0)
            {
                _context.EditorCamera!.SetAspectRatio(
                    (float)Viewport.Width / Viewport.Height);
            }

            bool isRmb = ImGui.IsMouseDown(ImGuiMouseButton.Right);
            bool isMmb = ImGui.IsMouseDown(ImGuiMouseButton.Middle);

            // Start navigation session if clicking inside the viewport
            if (IsHovered &&
                (ImGui.IsMouseClicked(ImGuiMouseButton.Right) ||
                 ImGui.IsMouseClicked(ImGuiMouseButton.Middle)))
            {
                _isNavigating = true;
                ImGui.SetWindowFocus();
            }

            // End navigation session when both camera buttons are released
            if (!isRmb && !isMmb)
            {
                _isNavigating = false;
            }

            // Scene View owns input while focused or actively navigating.
            _context.IsSceneViewFocused =
                IsFocused || _isNavigating;

            Engine.Cursor.SetCursorState(
                (isRmb || isMmb)
                    ? OpenTK.Windowing.Common.CursorState.Grabbed
                    : OpenTK.Windowing.Common.CursorState.Normal);

            if (IsFocused ||
                _isNavigating ||
                _context.EditorCamera!.IsFocusing)
            {
                if (_isNavigating)
                {
                    ImGui.SetWindowFocus();
                }

                _context.EditorCamera!.OnUpdate(
                    deltaTime,
                    IsHovered || _isNavigating);
            }
        }

        protected override void OnViewportOverlayRender()
        {
            if (!ImGui.BeginDragDropTarget())
                return;

            var payload = ImGui.AcceptDragDropPayload("ASSET_PATH");

            unsafe
            {
                if (payload.NativePtr != null)
                {
                    string path = Encoding.UTF8.GetString(
                        (byte*)payload.Data,
                        payload.DataSize);

                    HandleAssetDrop(path);
                }
            }

            ImGui.EndDragDropTarget();
        }

        private void HandleAssetDrop(string relativePath)
        {
            string ext = Path.GetExtension(relativePath);

            if (ext.Equals(".scene", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".gltf", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".glb", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".fbx", StringComparison.OrdinalIgnoreCase) ||
                ext.Equals(".obj", StringComparison.OrdinalIgnoreCase))
            {
                AssetDropRequested?.Invoke(relativePath);
            }
        }
    }
}