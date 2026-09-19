using DevoidEngine.Core;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Elemental.Tools.Panels
{
    public class SceneViewPanel : ViewportPanel
    {
        public EditorCamera EditorViewCamera;

        private Vector2 windowPosition;
        private Vector2 windowSize;
        private Vector2 mousePosition;
        private bool windowHovered;
        private bool middleMouse;
        private bool rightMouse;
        private float mouseWheel;

        private bool wasHovering = false;

        public SceneViewPanel() : base("Scene View")
        {
            EditorViewCamera = new EditorCamera();

            this.Viewport.CameraOverride = EditorViewCamera.Camera;
        }

        public override void OnUpdate(EditorContext context, float deltaTime)
        {
            context.Camera = EditorViewCamera;

            bool interacting = middleMouse || rightMouse;

            if (windowHovered)
                wasHovering = true;

            if (!wasHovering)
            {
                EditorViewCamera.CanInteract = false;
                return;
            }

            EditorViewCamera.CanInteract = true;

            mouseWheel = windowHovered ? mouseWheel : 0.0f;

            EditorViewCamera.Update(context, deltaTime, mouseWheel);

            if (!interacting)
                return;

            Vector2 finalMousePosition = mousePosition;
            bool wrapped = false;

            if (mousePosition.X <= windowPosition.X)
            {
                finalMousePosition.X = windowPosition.X + windowSize.X - 1.0f;
                wrapped = true;
            }
            else if (mousePosition.X + 1 >= windowPosition.X + windowSize.X)
            {
                finalMousePosition.X = windowPosition.X + 1.0f;
                wrapped = true;
            }

            if (mousePosition.Y <= windowPosition.Y)
            {
                finalMousePosition.Y = windowPosition.Y + windowSize.Y - 1.0f;
                wrapped = true;
            }
            else if (mousePosition.Y >= windowPosition.Y + windowSize.Y)
            {
                finalMousePosition.Y = windowPosition.Y + 1.0f;
                wrapped = true;
            }

            if (wrapped)
            {
                Engine.Cursor.SetCursorPosition(finalMousePosition);
                EditorViewCamera.NotifyMouseWarp();
            }
        }

        protected override void OnViewportOverlayRender()
        {
            windowPosition = ImGui.GetWindowPos();
            windowSize = ImGui.GetWindowSize();
            mousePosition = ImGui.GetMousePos();

            windowHovered = ImGui.IsWindowHovered();

            middleMouse = ImGui.IsMouseDown(ImGuiMouseButton.Middle);
            rightMouse = ImGui.IsMouseDown(ImGuiMouseButton.Right);
            mouseWheel = ImGui.GetIO().MouseWheel;
        }
    }
}
