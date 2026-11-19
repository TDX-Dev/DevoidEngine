using ImGuiNET;
using System.Numerics;

namespace Elemental.Tools.Themes
{
    static class DefaultTheme
    {
        public static void Apply()
        {
            ImGuiStylePtr style = ImGui.GetStyle();

            // ------------------------------------------------------------
            // Global
            // ------------------------------------------------------------

            style.Alpha = 1.0f;
            style.DisabledAlpha = 0.55f;

            // ------------------------------------------------------------
            // Window
            // ------------------------------------------------------------

            style.WindowPadding = new Vector2(4, 4);
            style.WindowRounding = 0.0f;
            style.WindowBorderSize = 0.0f;
            style.WindowMinSize = new Vector2(32, 32);
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;

            // ------------------------------------------------------------
            // Child
            // ------------------------------------------------------------

            style.ChildRounding = 0.0f;
            style.ChildBorderSize = 0.0f;

            // ------------------------------------------------------------
            // Popup
            // ------------------------------------------------------------

            style.PopupRounding = 0.0f;
            style.PopupBorderSize = 1.0f;

            // ------------------------------------------------------------
            // Frame / controls
            // ------------------------------------------------------------

            style.FramePadding = new Vector2(5, 3);
            style.FrameRounding = 0.0f;
            style.FrameBorderSize = 1.0f;

            // ------------------------------------------------------------
            // Spacing
            // ------------------------------------------------------------

            style.ItemSpacing = new Vector2(6, 4);
            style.ItemInnerSpacing = new Vector2(4, 4);
            style.CellPadding = new Vector2(4, 2);
            style.TouchExtraPadding = new Vector2(0, 0);

            style.IndentSpacing = 14.0f;
            style.ColumnsMinSpacing = 6.0f;

            // ------------------------------------------------------------
            // Scrollbars
            // ------------------------------------------------------------

            style.ScrollbarSize = 12.0f;
            style.ScrollbarRounding = 0.0f;

            // ------------------------------------------------------------
            // Slider / drag handles
            // ------------------------------------------------------------

            style.GrabMinSize = 10.0f;
            style.GrabRounding = 0.0f;
            style.LogSliderDeadzone = 4.0f;

            // ------------------------------------------------------------
            // Tabs
            // ------------------------------------------------------------

            style.TabRounding = 0.0f;
            style.TabBorderSize = 0.0f;
            //style.TabMinWidthBase = 0.0f;
            //style.TabMinWidthShrink = 0.0f;

            style.TabBarBorderSize = 1.0f;
            style.TabBarOverlineSize = 1.0f;

            // ------------------------------------------------------------
            // Alignment
            // ------------------------------------------------------------

            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.5f);

            // ------------------------------------------------------------
            // Separators
            // ------------------------------------------------------------

            style.SeparatorTextBorderSize = 1.0f;
            style.SeparatorTextAlign = new Vector2(0.0f, 0.5f);
            style.SeparatorTextPadding = new Vector2(4, 3);

            // ------------------------------------------------------------
            // Docking
            // ------------------------------------------------------------

            style.DockingSeparatorSize = 1.0f;

            // ------------------------------------------------------------
            // Display
            // ------------------------------------------------------------

            style.DisplayWindowPadding = new Vector2(19, 19);
            style.DisplaySafeAreaPadding = new Vector2(3, 3);

            // ------------------------------------------------------------
            // Rendering
            // ------------------------------------------------------------

            style.MouseCursorScale = 1.0f;
            style.AntiAliasedLines = true;
            style.AntiAliasedLinesUseTex = true;
            style.AntiAliasedFill = true;

            // ------------------------------------------------------------
            // Colors
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.Text] =
                new Vector4(0.82f, 0.82f, 0.82f, 1.0f);

            style.Colors[(int)ImGuiCol.TextDisabled] =
                new Vector4(0.50f, 0.50f, 0.50f, 1.0f);

            style.Colors[(int)ImGuiCol.WindowBg] =
                new Vector4(0.227f, 0.227f, 0.227f, 1.0f); // #3A3A3A

            style.Colors[(int)ImGuiCol.ChildBg] =
                new Vector4(0.184f, 0.184f, 0.184f, 1.0f); // #2F2F2F

            style.Colors[(int)ImGuiCol.PopupBg] =
                new Vector4(0.184f, 0.184f, 0.184f, 1.0f);

            style.Colors[(int)ImGuiCol.Border] =
                new Vector4(0.31f, 0.31f, 0.31f, 1.0f); // ~#4F4F4F

            style.Colors[(int)ImGuiCol.BorderShadow] =
                new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.FrameBg] =
                new Vector4(0.188f, 0.188f, 0.188f, 1.0f); // #303030

            style.Colors[(int)ImGuiCol.FrameBgHovered] =
                new Vector4(0.235f, 0.235f, 0.235f, 1.0f);

            style.Colors[(int)ImGuiCol.FrameBgActive] =
                new Vector4(0.275f, 0.275f, 0.275f, 1.0f);

            style.Colors[(int)ImGuiCol.TitleBg] =
                new Vector4(0.275f, 0.275f, 0.282f, 1.0f); // #464648

            style.Colors[(int)ImGuiCol.TitleBgActive] =
                new Vector4(0.275f, 0.275f, 0.282f, 1.0f);

            style.Colors[(int)ImGuiCol.TitleBgCollapsed] =
                new Vector4(0.275f, 0.275f, 0.282f, 1.0f);

            style.Colors[(int)ImGuiCol.MenuBarBg] =
                new Vector4(0.275f, 0.275f, 0.282f, 1.0f);

            // ------------------------------------------------------------
            // Buttons
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.Button] =
                new Vector4(0.188f, 0.188f, 0.188f, 1.0f);

            style.Colors[(int)ImGuiCol.ButtonHovered] =
                new Vector4(0.275f, 0.275f, 0.275f, 1.0f);

            style.Colors[(int)ImGuiCol.ButtonActive] =
                new Vector4(0.325f, 0.325f, 0.325f, 1.0f);

            // ------------------------------------------------------------
            // Headers
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.Header] =
                new Vector4(0.275f, 0.275f, 0.282f, 1.0f);

            style.Colors[(int)ImGuiCol.HeaderHovered] =
                new Vector4(0.325f, 0.325f, 0.325f, 1.0f);

            style.Colors[(int)ImGuiCol.HeaderActive] =
                new Vector4(0.365f, 0.365f, 0.365f, 1.0f);

            // ------------------------------------------------------------
            // Tabs
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.Tab] =
                new Vector4(0.275f, 0.275f, 0.282f, 1.0f);

            style.Colors[(int)ImGuiCol.TabHovered] =
                new Vector4(0.33f, 0.33f, 0.33f, 1.0f);

            style.Colors[(int)ImGuiCol.TabSelected] =
                new Vector4(0.227f, 0.227f, 0.227f, 1.0f);

            style.Colors[(int)ImGuiCol.TabSelectedOverline] =
                new Vector4(0.584f, 0.663f, 0.690f, 1.0f); // ~#95A9B0

            style.Colors[(int)ImGuiCol.TabDimmed] =
                new Vector4(0.22f, 0.22f, 0.22f, 1.0f);

            style.Colors[(int)ImGuiCol.TabDimmedSelected] =
                new Vector4(0.25f, 0.25f, 0.25f, 1.0f);

            // ------------------------------------------------------------
            // Scrollbar
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.ScrollbarBg] =
                new Vector4(0.13f, 0.13f, 0.13f, 1.0f);

            style.Colors[(int)ImGuiCol.ScrollbarGrab] =
                new Vector4(0.30f, 0.30f, 0.30f, 1.0f);

            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] =
                new Vector4(0.38f, 0.38f, 0.38f, 1.0f);

            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] =
                new Vector4(0.45f, 0.45f, 0.45f, 1.0f);

            // ------------------------------------------------------------
            // Checkboxes / sliders
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.CheckMark] =
                new Vector4(0.65f, 0.70f, 0.72f, 1.0f);

            style.Colors[(int)ImGuiCol.SliderGrab] =
                new Vector4(0.50f, 0.55f, 0.57f, 1.0f);

            style.Colors[(int)ImGuiCol.SliderGrabActive] =
                new Vector4(0.65f, 0.70f, 0.72f, 1.0f);

            // ------------------------------------------------------------
            // Separators
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.Separator] =
                new Vector4(0.31f, 0.31f, 0.31f, 1.0f);

            style.Colors[(int)ImGuiCol.SeparatorHovered] =
                new Vector4(0.45f, 0.50f, 0.52f, 1.0f);

            style.Colors[(int)ImGuiCol.SeparatorActive] =
                new Vector4(0.58f, 0.66f, 0.69f, 1.0f);

            // ------------------------------------------------------------
            // Resize grips
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.ResizeGrip] =
                new Vector4(0.30f, 0.30f, 0.30f, 0.25f);

            style.Colors[(int)ImGuiCol.ResizeGripHovered] =
                new Vector4(0.58f, 0.66f, 0.69f, 0.50f);

            style.Colors[(int)ImGuiCol.ResizeGripActive] =
                new Vector4(0.58f, 0.66f, 0.69f, 0.80f);

            // ------------------------------------------------------------
            // Tables
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.TableHeaderBg] =
                new Vector4(0.275f, 0.275f, 0.282f, 1.0f);

            style.Colors[(int)ImGuiCol.TableBorderStrong] =
                new Vector4(0.31f, 0.31f, 0.31f, 1.0f);

            style.Colors[(int)ImGuiCol.TableBorderLight] =
                new Vector4(0.25f, 0.25f, 0.25f, 1.0f);

            style.Colors[(int)ImGuiCol.TableRowBg] =
                new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.TableRowBgAlt] =
                new Vector4(1.0f, 1.0f, 1.0f, 0.015f);

            // ------------------------------------------------------------
            // Text selection / drag-drop
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.TextSelectedBg] =
                new Vector4(0.40f, 0.48f, 0.52f, 0.45f);

            style.Colors[(int)ImGuiCol.DragDropTarget] =
                new Vector4(0.58f, 0.66f, 0.69f, 0.90f);

            // ------------------------------------------------------------
            // Navigation
            // ------------------------------------------------------------

            //style.Colors[(int)ImGuiCol.NavHighlight] =
            //    new Vector4(0.58f, 0.66f, 0.69f, 1.0f);

            style.Colors[(int)ImGuiCol.NavWindowingHighlight] =
                new Vector4(1.0f, 1.0f, 1.0f, 0.70f);

            style.Colors[(int)ImGuiCol.NavWindowingDimBg] =
                new Vector4(0.80f, 0.80f, 0.80f, 0.20f);

            // ------------------------------------------------------------
            // Modal dimming
            // ------------------------------------------------------------

            style.Colors[(int)ImGuiCol.ModalWindowDimBg] =
                new Vector4(0.0f, 0.0f, 0.0f, 0.35f);
        }
    }
}
