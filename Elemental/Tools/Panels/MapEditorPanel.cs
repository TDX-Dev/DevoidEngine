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

        public float HorizontalSplit = 0.5f;
        public float VerticalSplit = 0.5f;
        public float SplitterSize = 8f;

        public Viewport CameraViewport;
        public Viewport TopViewport;
        public Viewport LeftViewport;
        public Viewport BackViewport;



        public MapEditorPanel() : base("Map Editor Panel")
        {
            CameraViewport = new Viewport();
            TopViewport = new Viewport();
            LeftViewport = new Viewport();
            BackViewport = new Viewport();
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

            float splitSizeX = availableSize.X * HorizontalSplit;
            float splitSizeY = availableSize.Y * VerticalSplit;

            float leftWidth = splitSizeX - (SplitterSize / 2);
            float rightWidth = availableSize.X - splitSizeX - (SplitterSize / 2);

            float topHeight = splitSizeY - (SplitterSize / 2);
            float bottomHeight = availableSize.Y - splitSizeY - (SplitterSize / 2);

            Vector2 origin = ImGui.GetCursorPos();


            ImGui.SetCursorPos(origin);
            ImGui.BeginChild("3DCameraView", new Vector2(leftWidth, topHeight));
            ImGui.EndChild();

            ImGui.SetCursorPos(origin + new Vector2(SplitterSize + leftWidth, 0));
            ImGui.BeginChild("TopView", new Vector2(rightWidth, topHeight));
            ImGui.EndChild();

            ImGui.SetCursorPos(origin + new Vector2(0, SplitterSize + topHeight));
            ImGui.BeginChild("LeftView", new Vector2(leftWidth, bottomHeight));
            ImGui.EndChild();

            ImGui.SetCursorPos(origin + new Vector2(SplitterSize + leftWidth, SplitterSize + topHeight));
            ImGui.BeginChild("BackView", new Vector2(rightWidth, bottomHeight));
            ImGui.EndChild();

            ImGui.SetCursorPos(origin + new Vector2(leftWidth, 0));
            ImGui.InvisibleButton("##splittervertical", new Vector2(SplitterSize, availableSize.Y));

            if (ImGui.IsItemActive())
            {
                HorizontalSplit += ImGui.GetIO().MouseDelta.X / availableSize.X;
                HorizontalSplit = Math.Clamp(HorizontalSplit, 0.1f, 0.9f);
            }

            ImGui.SetCursorPos(origin + new Vector2(0, topHeight));

            ImGui.InvisibleButton("##splitterhorizontal", new Vector2(availableSize.X, SplitterSize));

            if (ImGui.IsItemActive())
            {
                VerticalSplit += ImGui.GetIO().MouseDelta.Y / availableSize.Y;
                VerticalSplit = Math.Clamp(VerticalSplit, 0.1f, 0.9f);
            }
        }
    }
}
