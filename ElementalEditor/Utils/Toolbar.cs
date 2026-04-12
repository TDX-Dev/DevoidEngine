using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ElementalEditor.Utils
{
    public enum ToolbarAlign
    {
        Left,
        Center,
        Right
    }

    class ToolbarGroup
    {
        public ToolbarAlign Align;
        public Action Draw;
    }

    public class Toolbar
    {
        private readonly List<ToolbarGroup> groups = new();

        public void Add(ToolbarAlign align, Action draw)
        {
            groups.Add(new ToolbarGroup
            {
                Align = align,
                Draw = draw
            });
        }

        public void Draw()
        {
            float width = ImGui.GetContentRegionAvail().X;

            float left = 0f;
            float right = width;

            foreach (var group in groups)
            {
                float size = Measure(group.Draw);

                switch (group.Align)
                {
                    case ToolbarAlign.Left:
                        ImGui.SetCursorPosX(left);
                        group.Draw();
                        left += size + ImGui.GetStyle().ItemSpacing.X;
                        break;

                    case ToolbarAlign.Right:
                        right -= size;
                        ImGui.SetCursorPosX(right);
                        group.Draw();
                        right -= ImGui.GetStyle().ItemSpacing.X;
                        break;

                    case ToolbarAlign.Center:
                        ImGui.SetCursorPosX(width * 0.5f - size * 0.5f);
                        group.Draw();
                        break;
                }
            }
        }

        float Measure(Action draw)
        {
            Vector2 start = ImGui.GetCursorPos();

            ImGui.BeginGroup();
            draw();
            ImGui.EndGroup();

            Vector2 end = ImGui.GetCursorPos();

            ImGui.SetCursorPos(start);

            return end.X - start.X;
        }
    }
}
