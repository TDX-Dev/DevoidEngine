using ImGuiNET;
using System.Numerics;

namespace Elemental.Tools.Toolbar
{
    public class Toolbar
    {
        public int ToolbarHeight = 40;

        public void OnImguiRender(EditorContext context)
        {
            ImGui.SetNextWindowPos(new Vector2(0, context.Menu.MainMenuSize), ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(ImGui.GetIO().DisplaySize.X, ToolbarHeight), ImGuiCond.Always);

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1294117647f, 0.12156862745f, 0.11372549019f, 1));

            ImGui.Begin(
                "##Toolbar",
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

            ImGui.End();
            ImGui.PopStyleColor();
        }

    }
}
