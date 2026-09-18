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

        private Vector3 position;
        private Vector3 rotation;
        private bool wasHovering = false;

        public SceneViewPanel() : base("Scene View")
        {
            EditorViewCamera = new EditorCamera();

            this.Viewport.CameraOverride = EditorViewCamera.Camera;

            position = EditorViewCamera.Camera.Position;

        }

        public override void OnUpdate(EditorContext context, float deltaTime)
        {



            EditorViewCamera.Update(context, deltaTime);
        }

        protected override void OnViewportOverlayRender()
        {
            Vector2 windowPosition = ImGui.GetWindowPos();
            Vector2 windowSize = ImGui.GetWindowSize();
            Vector2 mousePosition = ImGui.GetMousePos();
            bool windowHovered = ImGui.IsWindowHovered();
            bool isMiddleDragging = ImGui.IsMouseDragging(ImGuiMouseButton.Middle);
            bool isRightDragging = ImGui.IsMouseDragging(ImGuiMouseButton.Right);

            if (isMiddleDragging || isRightDragging)
            {
                if (windowHovered)
                    wasHovering = true;

                if (!wasHovering)
                    return;

                // Orbital camera when dragging with middle mouse button, similar to godot.
                if (isMiddleDragging)
                {



                }
                // Camera look when right button dragging
                else if (isRightDragging)
                {

                }


                // Apply mouse wrap
                Vector2 finalMousePosition = mousePosition;

                if (mousePosition.X <= windowPosition.X)
                    finalMousePosition.X = windowPosition.X + windowSize.X;

                if (mousePosition.Y <= windowPosition.Y)
                    finalMousePosition.Y = windowPosition.Y + windowSize.Y;

                if (mousePosition.X > windowPosition.X + windowSize.X)
                    finalMousePosition.X = windowPosition.X;

                if (mousePosition.Y > windowPosition.Y + windowSize.Y)
                    finalMousePosition.Y = windowPosition.Y;

                Engine.Cursor.SetCursorPosition(finalMousePosition);
            } else
            {
                wasHovering = false;
            }


                ImGui.Begin("Editor Camera");

            Camera camera = EditorViewCamera.Camera;

            Vector3 position = camera.Position;
            Vector3 front = camera.Front;
            Vector3 up = camera.Up;

            bool positionChanged = ImGui.DragFloat3("Position", ref position, 0.1f);

            bool frontChanged = ImGui.DragFloat3("Front", ref front, 0.01f);

            bool upChanged = ImGui.DragFloat3("Up", ref up, 0.01f);

            if (positionChanged || frontChanged || upChanged)
            {
                camera.UpdateView(position, front, up);
            }

            ImGui.End();
        }
    }
}
