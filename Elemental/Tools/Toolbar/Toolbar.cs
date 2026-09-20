using ImGuiNET;
using System.Numerics;

namespace Elemental.Tools.Toolbar
{
    public sealed class Toolbar
    {
        public int ToolbarHeight = 40;

        private readonly List<ToolbarGroup> groups = [];

        public ToolbarGroup AddGroup(string title)
        {
            ToolbarGroup group = new(title);
            groups.Add(group);
            return group;
        }

        public void OnImguiRender(EditorContext context)
        {
            ImGui.SetNextWindowPos(new Vector2(0, context.Menu.MainMenuSize), ImGuiCond.Always);

            ImGui.SetNextWindowSize(new Vector2(ImGui.GetIO().DisplaySize.X, ToolbarHeight), ImGuiCond.Always);

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1294117647f, 0.12156862745f, 0.11372549019f, 1));

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));

            ImGui.Begin("##Toolbar",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBringToFrontOnFocus |
                ImGuiWindowFlags.NoFocusOnAppearing |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoNav
            );

            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].Draw(context);

                if (i < groups.Count - 1)
                    ImGui.SameLine(0, 6);
            }

            ImGui.End();
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
    }
}