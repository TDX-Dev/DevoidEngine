using DevoidEngine.AssetPipeline;
using DevoidEngine.Assets;
using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Rendering;
using DevoidGPU;
using Elemental.Panels;
using ImGuiNET;
using MessagePack;
using System;
using System.Numerics;

namespace Elemental
{
    public class EditorLayer : Layer
    {
        private ImFontPtr _editorFont;
        private PanelManager _panelManager = null!;
        private Scene _activeScene = null!;

        private EditorContext _editorContext = null!;

        private SceneViewPanel _sceneViewPanel = null!;
        private GameViewPanel _gameViewPanel = null!;

        public override void OnAttach()
        {
            ApplyImGuiStyling();
            EditorIcons.Initialize();

            _editorFont = Application.ImguiRenderer.AddFontFromFile("./Assets/Fonts/JBM.ttf", 16);
            Application.ImguiRenderer.LoadIconFont("./Assets/Fonts/lucide.ttf", 16, (LucideIconFont.FontUnicodeMin, LucideIconFont.FontUnicodeMax));

            Application.ImguiRenderer.SetDefaultFont(_editorFont);

            _editorContext = new EditorContext();

            _panelManager = new PanelManager();
            _sceneViewPanel = new SceneViewPanel(null!);
            _gameViewPanel = new GameViewPanel(null!);

            _panelManager.AddPanel(_sceneViewPanel);
            _panelManager.AddPanel(_gameViewPanel);
            _panelManager.AddPanel(new AssetBrowserPanel());
            _panelManager.AddPanel(new InspectorPanel(_editorContext));
            _panelManager.AddPanel(new HierarchyPanel(_editorContext));

            SetupSandbox();
        }

        public override void OnGUIRender()
        {
            DrawMenuBar();
            _panelManager.OnImGuiRender();
        }

        public override void OnUpdate(float deltaTime)
        {
            var currentScene = Engine.Instance.SceneTree.CurrentScene;
            if (currentScene != null && currentScene != _activeScene)
            {
                _activeScene = currentScene;
                _sceneViewPanel.Viewport.TargetScene = _activeScene;
                _gameViewPanel.Viewport.TargetScene = _activeScene;

                // Keep active scene in context updated
                _editorContext.ActiveScene = _activeScene;
            }

            _panelManager.OnUpdate(deltaTime);
        }

        public override void OnPostRender(ICommandList cmd)
        {
        }

        public override void OnDetach()
        {
            EditorIcons.Shutdown();
            _panelManager.Clear();
        }

        private void SetupSandbox()
        {
            _activeScene = Asset.Load<PackedScene>("models/sh.gltf")!.Instantiate();

            Engine.Instance.SceneTree.LoadScene(_activeScene);
            _sceneViewPanel.Viewport.TargetScene = _activeScene;
            _gameViewPanel.Viewport.TargetScene = _activeScene;

            _editorContext.ActiveScene = _activeScene;

            SetupEnvironment();
            ConfigureInputBindings();
        }

        private void SetupEnvironment()
        {
            Engine.Renderer.SkyRenderer.Sky = new HDRISky
            {
                PanoramaTexture = Asset.Load<Texture>("HDRIs/soil_puresky.hdr")!
            };
        }

        private void ConfigureInputBindings()
        {
            var map = Engine.InputSystem.Map;

            // Keyboard Movement
            map.Bind("Forward", new InputBinding { DeviceType = InputDeviceType.Keyboard, Control = (ushort)Keys.W });
            map.Bind("Backward", new InputBinding { DeviceType = InputDeviceType.Keyboard, Control = (ushort)Keys.S });
            map.Bind("Left", new InputBinding { DeviceType = InputDeviceType.Keyboard, Control = (ushort)Keys.A });
            map.Bind("Right", new InputBinding { DeviceType = InputDeviceType.Keyboard, Control = (ushort)Keys.D });
            map.Bind("Jump", new InputBinding { DeviceType = InputDeviceType.Keyboard, Control = (ushort)Keys.Space });
            map.Bind("Sprint", new InputBinding { DeviceType = InputDeviceType.Keyboard, Control = (ushort)Keys.LeftShift });

            // Actions
            map.Bind("Grab", new InputBinding { DeviceType = InputDeviceType.Keyboard, Control = (ushort)Keys.G });
            map.Bind("Pickup", new InputBinding { DeviceType = InputDeviceType.Keyboard, Control = (ushort)Keys.E });

            // Mouse Look & Navigation
            map.Bind("LookX", new InputBinding { DeviceType = InputDeviceType.Mouse, Control = (ushort)MouseAxis.DeltaX, IsClamped = false });
            map.Bind("LookY", new InputBinding { DeviceType = InputDeviceType.Mouse, Control = (ushort)MouseAxis.DeltaY, IsClamped = false });
            map.Bind("Orbit", new InputBinding { DeviceType = InputDeviceType.Mouse, Control = (ushort)MouseButton.Middle });
            map.Bind("Zoom", new InputBinding { DeviceType = InputDeviceType.Mouse, Control = (ushort)MouseAxis.ScrollY });
        }

        private void DrawMenuBar()
        {
            float height = Application.ImguiRenderer.ToolbarHeight;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0));

            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
            ImGui.SetNextWindowSize(
                new Vector2(ImGui.GetIO().DisplaySize.X, height),
                ImGuiCond.Always);

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
                ImGuiWindowFlags.NoNav);

            BeginToolbarSection("Scene Tools");

            ToolbarButton(
                "New",
                LucideIconFont.IconFile,
                "New");

            ImGui.SameLine();

            ToolbarButton(
                "Open",
                LucideIconFont.IconFolderOpen,
                "Open");

            ImGui.SameLine();

            ToolbarButton(
                "Save",
                LucideIconFont.IconSave,
                "Save");

            ImGui.SameLine();

            ToolbarButton(
                "SaveAs",
                LucideIconFont.IconFilePen,
                "Save As");

            EndToolbarSection();

            Separator();

            BeginToolbarSection("Actions");

            ToolbarButton(
                "Undo",
                LucideIconFont.IconUndo2,
                "Undo");

            ImGui.SameLine();

            ToolbarButton(
                "Redo",
                LucideIconFont.IconRedo2,
                "Redo");

            EndToolbarSection();

            Separator();

            BeginToolbarSection("");

            ToolbarButton(
                "Select",
                LucideIconFont.IconMousePointer2,
                "Select");

            ImGui.SameLine();

            ToolbarButton(
                "Move",
                LucideIconFont.IconMove3d,
                "Move");

            ImGui.SameLine();

            ToolbarButton(
                "Rotate",
                LucideIconFont.IconRotate3d,
                "Rotate");

            ImGui.SameLine();

            ToolbarButton(
                "Scale",
                LucideIconFont.IconScale3d,
                "Scale");

            ImGui.SameLine();

            ToolbarButton(
                "Spline",
                LucideIconFont.IconSpline,
                "Spline");

            EndToolbarSection();

            Separator();

            BeginToolbarSection("Primitives");

            ToolbarButton(
                "Cube",
                LucideIconFont.IconCuboid,
                "Create Cube");

            ImGui.SameLine();

            ToolbarButton(
                "Sphere",
                LucideIconFont.IconCircle,
                "Create Sphere");

            ImGui.SameLine();

            ToolbarButton(
                "Cylinder",
                LucideIconFont.IconCylinder,
                "Create Cylinder");

            ImGui.SameLine();

            ToolbarButton(
                "Mesh",
                LucideIconFont.IconBox,
                "Create Mesh");

            EndToolbarSection();

            Separator();

            BeginToolbarSection("Game Tools");

            ToolbarButton(
                "Play",
                LucideIconFont.IconPlay,
                "Play");

            ImGui.SameLine();

            ToolbarButton(
                "Open In Runtime",
                LucideIconFont.IconAppWindow,
                "Open In Runtime");


            EndToolbarSection();

            Separator();

            // ---------------------------------------------------------------------
            // Push everything after this to the right
            // ---------------------------------------------------------------------

            float remaining = ImGui.GetContentRegionAvail().X;

            if (remaining > 0)
            {
                ImGui.SameLine();
                ImGui.Dummy(new Vector2(remaining, 0));
                ImGui.SameLine();
            }

            // ---------------------------------------------------------------------
            // Scene / editor selector
            // ---------------------------------------------------------------------

            ImGui.SetNextItemWidth(220);

            //if (ImGui.BeginCombo(
            //    "##CurrentScene",
            //    CurrentSceneName))
            //{
            //    foreach (string scene in OpenScenes)
            //    {
            //        bool selected = scene == CurrentSceneName;

            //        if (ImGui.Selectable(scene, selected))
            //            CurrentSceneName = scene;

            //        if (selected)
            //            ImGui.SetItemDefaultFocus();
            //    }

            //    ImGui.EndCombo();
            //}

            ImGui.End();

            ImGui.PopStyleColor();
            ImGui.PopStyleVar(2);
        }

        static bool ToolbarButton(string id, string icon, string? tooltip = null)
        {
            bool pressed = ImGui.Button($"{icon}##{id}");

            if (tooltip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(tooltip);

            return pressed;
        }

        static void Separator()
        {
            //ImGui.SameLine();
            //ImGui.Dummy(new Vector2(4, 0));
            ImGui.SameLine();

            VerticalSeparator();

            ////ImGui.SeparatorEx(ImGuiSeparatorFlags.Vertical, new Vector2(1, 20));
            //ImGui.Separator();

            ImGui.SameLine();
        }
        static void BeginToolbarSection(string label)
        {
            ImGui.BeginGroup();

            ImGui.Text(label);

            ImGui.BeginGroup();
        }

        static void EndToolbarSection()
        {
            ImGui.EndGroup();
            ImGui.EndGroup();
        }

        private void ApplyImGuiStyling()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // Padding & Spacing
            style.WindowPadding = new Vector2(10, 10);
            style.FramePadding = new Vector2(8, 5);
            style.ItemSpacing = new Vector2(8, 6);
            style.ItemInnerSpacing = new Vector2(6, 4);
            style.TouchExtraPadding = Vector2.Zero;
            style.IndentSpacing = 20;
            style.ScrollbarSize = 14;
            style.GrabMinSize = 10;

            // Borders
            style.WindowBorderSize = 1;
            style.ChildBorderSize = 1;
            style.PopupBorderSize = 1;
            style.FrameBorderSize = 0;
            style.TabBorderSize = 0;

            // Rounding
            style.WindowRounding = 6;
            style.ChildRounding = 0;
            style.FrameRounding = 0;
            style.PopupRounding = 0;
            style.ScrollbarRounding = 9;
            style.GrabRounding = 4;
            style.TabRounding = 4;

            // Alignments
            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = Vector2.Zero;

            // Colors
            colors[(int)ImGuiCol.Text] = new Vector4(0.95f, 0.96f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.36f, 0.42f, 0.47f, 1.00f);

            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.11f, 0.11f, 0.12f, 1.00f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.14f, 0.14f, 0.15f, 1.00f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);

            colors[(int)ImGuiCol.Border] = new Vector4(0.25f, 0.25f, 0.26f, 1.00f);
            colors[(int)ImGuiCol.BorderShadow] = Vector4.Zero;

            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.18f, 0.18f, 0.19f, 1.00f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.23f, 0.23f, 0.24f, 1.00f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.28f, 0.28f, 0.30f, 1.00f);

            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.09f, 0.09f, 0.09f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.12f, 0.12f, 0.12f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 0.51f);

            colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.15f, 1.00f);

            colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.02f, 0.02f, 0.02f, 0.53f);
            colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.31f, 0.31f, 0.31f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.41f, 0.41f, 0.41f, 1.00f);
            colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.51f, 0.51f, 0.51f, 1.00f);

            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.90f, 0.90f, 0.90f, 1.00f);

            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.28f, 0.56f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.37f, 0.61f, 1.00f, 1.00f);

            colors[(int)ImGuiCol.Button] = new Vector4(0.20f, 0.20f, 0.21f, 1.00f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.28f, 0.56f, 1.00f, 1.00f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.06f, 0.53f, 0.98f, 1.00f);

            colors[(int)ImGuiCol.Header] = new Vector4(0.20f, 0.20f, 0.21f, 1.00f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.30f, 0.30f, 0.32f, 1.00f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.35f, 0.35f, 0.37f, 1.00f);

            colors[(int)ImGuiCol.Separator] = new Vector4(0.28f, 0.28f, 0.29f, 1.00f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.47f, 1.00f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);

            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.56f, 1.00f, 0.25f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.28f, 0.56f, 1.00f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.28f, 0.56f, 1.00f, 0.95f);

            colors[(int)ImGuiCol.Tab] = new Vector4(0.15f, 0.15f, 0.16f, 1.00f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.22f, 0.22f, 0.23f, 1.00f);

            colors[(int)ImGuiCol.TabDimmed] = new Vector4(0.12f, 0.12f, 0.13f, 1.00f);
            colors[(int)ImGuiCol.TabDimmedSelected] = new Vector4(0.18f, 0.18f, 0.19f, 1.00f);
            colors[(int)ImGuiCol.TabDimmedSelectedOverline] = new Vector4(0.35f, 0.35f, 0.36f, 1.00f);

            colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.28f, 0.56f, 1.00f, 0.70f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.12f, 0.12f, 0.13f, 1.00f);

            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);

            colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);

            colors[(int)ImGuiCol.TableRowBg] = Vector4.Zero;
            colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.03f);

            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.28f, 0.56f, 1.00f, 0.35f);
        }

        public static void VerticalSeparator(float thickness = 1.0f, float widthPadding = 4.0f)
        {
            // 1. Get the screen position where the cursor currently sits
            Vector2 p = ImGui.GetCursorScreenPos();

            // 2. Fetch the current window's draw list 
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // 3. Determine the height of the line based on the remaining content region
            float height = ImGui.GetContentRegionAvail().Y;

            // 4. Calculate the line's exact placement offset by padding
            float lineX = p.X + widthPadding;

            // 5. Draw the vertical line using the theme's built-in border color
            uint color = ImGui.GetColorU32(ImGuiCol.Border);
            drawList.AddLine(new Vector2(lineX, p.Y), new Vector2(lineX, p.Y + height), color, thickness);

            // 6. Advance the layout cursor so subsequent elements render to its right
            float totalWidth = (widthPadding * 2.0f) + thickness;
            ImGui.Dummy(new Vector2(totalWidth, height));
        }
    }
}