using DevoidEngine.Core;
using DevoidEngine.Nodes;
using DevoidEngine.Rendering;
using ImGuiNET;
using System.Numerics;

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
        private bool wasHovered;
        private bool IsControlling => middleMouse || rightMouse;

        public SceneViewPanel() : base("Scene View")
        {
            EditorViewCamera = new EditorCamera();

            this.Viewport.CameraOverride = EditorViewCamera.Camera;
        }

        public override void OnUpdate(EditorContext context, float deltaTime)
        {
            if (context.SceneService.SceneDocument != null)
            {
                MeshNode? node = HandleObjectPicking(context.SceneService.SceneDocument.Scene);
                if (node != null)
                    context.SelectedNode = node;
            }



            context.Camera = EditorViewCamera;

            bool interacting = middleMouse || rightMouse;

            if (!wasHovered && IsHovered && interacting)
            {
                wasHovered = true;
            }

            if (wasHovered && !interacting)
            {
                wasHovered = false;
            }

            EditorViewCamera.CanInteract = windowHovered;

            //if (!wasHovering)
            //{
            //    EditorViewCamera.CanInteract = false;
            //    return;
            //}

            //EditorViewCamera.CanInteract = true;

            mouseWheel = windowHovered ? mouseWheel : 0.0f;

            EditorViewCamera.Update(context, deltaTime, mouseWheel);

            if (!wasHovered)
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

            if (windowHovered && IsControlling)
            {
                ImGui.SetWindowFocus();
            }


            ShowRendererInfo();

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                if (!IsHovered)
                    return;

                Viewport.RequestObjectPick(LocalMousePosition);
            }
        }

        void ShowRendererInfo()
        {
            RenderResourceCache? cache = Engine.Renderer.GetViewportResources(Viewport);
            if (cache == null)
                return;

            ImGui.PushStyleColor(ImGuiCol.ChildBg, new Vector4(0));
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8));

            ImGui.BeginChild("#viewport_info", new Vector2(0), ImGuiChildFlags.AlwaysUseWindowPadding, ImGuiWindowFlags.NoInputs);

            ImGui.Text($"Video Memory: {Engine.GraphicsDevice.Info.VideoMemoryUsage / 1048576} / {Engine.GraphicsDevice.Info.DedicatedVideoMemory / 1048576}");
            ImGui.Text($"Total Shared Video Memory: {Engine.GraphicsDevice.Info.VideoMemoryBudget / 1048576} ");
            ImGui.Text($"Internal Textures: {cache.NumTextures}");

            ImGui.EndChild();

            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
        
        MeshNode? HandleObjectPicking(Scene scene)
        {

            if (Viewport.TryGetObjectPick(out ObjectPickResult result))
            {
                uint objectId = result.ObjectId;
                foreach (MeshNode meshNode in scene.GetNodes<MeshNode>())
                {
                    if (meshNode.Mesh?.UniqueIdentifier == objectId)
                        return meshNode;
                }
            }
            return null;
        }
    }
}