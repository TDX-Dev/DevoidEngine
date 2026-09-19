using ImGuiNET;
using System.Numerics;

namespace Elemental.Tools.Themes
{
    static class DefaultTheme
    {


        public static void Apply()
        {
            ImGuiStylePtr styles = ImGui.GetStyle();

            styles.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.211764f, 0.211764f, 0.211764f, 1);
            styles.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.1294117647f, 0.1294117647f, 0.1294117647f, 1);
            styles.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.137254902f, 0.137254902f, 0.137254902f, 1);
            styles.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.137254902f, 0.137254902f, 0.137254902f, 1);
            styles.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.137254902f, 0.137254902f, 0.137254902f, 1);

            styles.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14901960784f, 0.14901960784f, 0.14901960784f, 1);

            styles.Colors[(int)ImGuiCol.Tab] = new Vector4(0);
            styles.Colors[(int)ImGuiCol.TabSelected] = new Vector4(0);
            styles.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0);
            styles.Colors[(int)ImGuiCol.TabDimmed] = new Vector4(0);
            styles.Colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0);
            styles.Colors[(int)ImGuiCol.TabSelectedOverline] = new Vector4(0);

            styles.WindowBorderSize = 0;
            styles.WindowPadding = new Vector2(0, 0);
        }
    }
}
