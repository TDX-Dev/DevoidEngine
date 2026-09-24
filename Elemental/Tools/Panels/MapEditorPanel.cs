using DevoidEngine.Rendering;
using ImGuiNET;
using System;
using System.Numerics;

namespace Elemental.Tools.Panels
{
    public class MapEditorPanel : Panel
    {
        public enum MapEditorView
        {
            CameraView,
            TopView,
            LeftView,
            BackView
        }

        public float horizontalSplit = 0.5f;
        public float verticalSplit = 0.5f;
        public float splitterSize = 8f;

        public Viewport cameraViewport;
        public Viewport topViewport;
        public Viewport leftViewport;
        public Viewport backViewport;



        public MapEditorPanel() : base("Map Editor Panel")
        {
            cameraViewport = new Viewport();
            topViewport = new Viewport();
            leftViewport = new Viewport();
            backViewport = new Viewport();
        }

        public void DrawView(MapEditorView view)
        {
            if (view == MapEditorView.CameraView)
            {
                

                return;
            }

            if (view == MapEditorView.TopView)
            {
                return;
            }

            if (view == MapEditorView.LeftView)
            {
                return;
            }

            if (view == MapEditorView.BackView)
            {
                return;
            }
        }

        protected override void OnImGuiRender(EditorContext context)
        {
            Vector2 availableSize = ImGui.GetContentRegionAvail();

            float splitSizeX = availableSize.X * horizontalSplit;
            float splitSizeY = availableSize.Y * verticalSplit;

            float leftWidth = splitSizeX - (splitterSize / 2);
            float rightWidth = availableSize.X - splitSizeX - (splitterSize / 2);

            float topHeight = splitSizeY - (splitterSize / 2);
            float bottomHeight = availableSize.Y - splitSizeY - (splitterSize / 2);

            Vector2 origin = ImGui.GetCursorPos();


            ImGui.SetCursorPos(origin);
            ImGui.BeginChild("3DCameraView", new Vector2(leftWidth, topHeight));
            ImGui.EndChild();

            ImGui.SetCursorPos(origin + new Vector2(splitterSize + leftWidth, 0));
            ImGui.BeginChild("TopView", new Vector2(rightWidth, topHeight));
            ImGui.EndChild();

            ImGui.SetCursorPos(origin + new Vector2(0, splitterSize + topHeight));
            ImGui.BeginChild("LeftView", new Vector2(leftWidth, bottomHeight));
            ImGui.EndChild();

            ImGui.SetCursorPos(origin + new Vector2(splitterSize + leftWidth, splitterSize + topHeight));
            ImGui.BeginChild("BackView", new Vector2(rightWidth, bottomHeight));
            ImGui.EndChild();

            ImGui.SetCursorPos(origin + new Vector2(leftWidth, 0));
            ImGui.InvisibleButton("##splittervertical", new Vector2(splitterSize, availableSize.Y));

            if (ImGui.IsItemActive())
            {
                horizontalSplit += ImGui.GetIO().MouseDelta.X / availableSize.X;
                horizontalSplit = Math.Clamp(horizontalSplit, 0.1f, 0.9f);
            }

            ImGui.SetCursorPos(origin + new Vector2(0, topHeight));

            ImGui.InvisibleButton("##splitterhorizontal", new Vector2(availableSize.X, splitterSize));

            if (ImGui.IsItemActive())
            {
                verticalSplit += ImGui.GetIO().MouseDelta.Y / availableSize.Y;
                verticalSplit = Math.Clamp(verticalSplit, 0.1f, 0.9f);
            }
        }
    }
}
