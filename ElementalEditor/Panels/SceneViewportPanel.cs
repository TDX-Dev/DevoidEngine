using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Panels
{
    public class SceneViewportPanel : IEditorPanel
    {
        private Vector2 lastViewportSize;
        public Vector2 ViewportSize;
        public Vector2 ViewportOffset;
        public float ViewportScale;

        public Vector2 GameMousePosition;
        public bool MouseInsideViewport;

        public void Draw(EditorContext context)
        {
            if (!ImGui.Begin("Scene"))
            {
                ImGui.End();
                return;
            }

            if (context.SceneViewportTarget == null)
            {
                ImGui.End();
                return;
            }

            Vector2 panelSize = ImGui.GetContentRegionAvail();
            ViewportSize = panelSize;

            var texture = context.SceneViewportTarget;
            Vector2 renderSize = new(texture.Width, texture.Height);

            // Calculate scale
            float scale = MathF.Min(
                panelSize.X / renderSize.X,
                panelSize.Y / renderSize.Y
            );

            ViewportScale = scale;

            Vector2 finalSize = renderSize * scale;
            Vector2 offset = (panelSize - finalSize) * 0.5f;

            ViewportOffset = offset;

            // Content region origin in screen space
            Vector2 contentMin = ImGui.GetCursorScreenPos();
            Vector2 contentMax = contentMin + panelSize;

            var drawList = ImGui.GetWindowDrawList();

            // Draw background (letterbox bars)
            drawList.AddRectFilled(
                contentMin,
                contentMax,
                ImGui.GetColorU32(new Vector4(0, 0, 0, 1))
            );

            // Move cursor relative to window
            ImGui.SetCursorPos(ImGui.GetCursorPos() + offset);

            ImGui.Image(
                texture.GetDeviceTexture().GetHandle(),
                finalSize
            );

            // --- Mouse conversion ---
            Vector2 mousePos = ImGui.GetMousePos();

            Vector2 viewportMin = contentMin + offset;
            Vector2 viewportMax = viewportMin + finalSize;

            MouseInsideViewport =
                mousePos.X >= viewportMin.X &&
                mousePos.Y >= viewportMin.Y &&
                mousePos.X <= viewportMax.X &&
                mousePos.Y <= viewportMax.Y;

            if (MouseInsideViewport)
            {
                Vector2 local = mousePos - viewportMin;
                GameMousePosition = local / scale;
            }

            ImGui.End();
        }
    }
}
