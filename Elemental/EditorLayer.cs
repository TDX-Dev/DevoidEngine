using DevoidEngine.Core;
using DevoidGPU;
using Elemental.Panels;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Elemental
{
    public class EditorLayer : Layer
    {
        ImFontPtr editorFont;

        private PanelManager panelManager = null!;
        private Scene activeScene = null!;

        public override void OnAttach()
        {
            SetStyling();
            editorFont = Application.ImguiRenderer.AddFontFromFile("./Assets/Fonts/JBM.ttf", 16);
            Application.ImguiRenderer.SetDefaultFont(editorFont);


            panelManager = new PanelManager();
            panelManager.AddPanel(new SceneViewPanel(activeScene));
            panelManager.AddPanel(new GameViewPanel(activeScene));
        }

        public override void OnGUIRender()
        {
            DrawMenuBar();
            panelManager.OnImGuiRender();

        }

        public override void OnUpdate(float deltaTime)
        {
            // Update active scene on change
            if (Engine.Instance.SceneTree.CurrentScene != activeScene)
            {
                activeScene = Engine.Instance.SceneTree.CurrentScene!;
                foreach (var vpPanel in new ViewportPanel[] { panelManager.GetPanel<SceneViewPanel>()!, panelManager.GetPanel<GameViewPanel>()! })
                {
                    if (vpPanel != null) vpPanel.Viewport.TargetScene = activeScene!;
                }
            }

            panelManager.OnUpdate(deltaTime);
        }

        public override void OnPostRender(ICommandList cmd)
        {
            cmd.SetFramebuffer(Application.MainWindow.Framebuffer);
            cmd.SetViewport(0, 0, Application.MainWindow.Window.ClientSize.X, Application.MainWindow.Window.ClientSize.Y);
            Engine.Renderer.API.RenderToScreen(cmd, Engine.Instance.SceneTree.RootViewport.OutputTexture!);
        }

        public override void OnDetach()
        {
            panelManager.Clear();
            //EditorServices.Clear();
        }

        void DrawMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Scene"))
                    {
                        //RequestNewScene();
                    }

                    if (ImGui.MenuItem("Open Scene")) { }
                    if (ImGui.MenuItem("Save Scene"))
                    {
                        //saveSceneRequested = true;
                    }

                    if (ImGui.MenuItem("Save Scene As"))
                    {
                        //saveSceneAsRequested = true;
                    }

                    ImGui.Separator();

                    if (ImGui.MenuItem("Exit")) { }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Project"))
                {
                    if (ImGui.MenuItem("Project Settings"))
                    {
                        //openProjectSettingsRequested = true;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Edit"))
                {
                    if (ImGui.MenuItem("Undo")) { }
                    if (ImGui.MenuItem("Redo")) { }
                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }

        void SetStyling()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;


            style.WindowPadding = new Vector2(10, 10);
            style.FramePadding = new Vector2(8, 5);
            style.ItemSpacing = new Vector2(8, 6);
            style.ItemInnerSpacing = new Vector2(6, 4);
            style.ItemSpacing = new Vector2(8, 6);
            style.TouchExtraPadding = new Vector2(0, 0);
            style.IndentSpacing = 20;
            style.ScrollbarSize = 14;
            style.GrabMinSize = 10;


            style.WindowBorderSize = 1;
            style.ChildBorderSize = 1;
            style.PopupBorderSize = 1;
            style.FrameBorderSize = 0;
            style.TabBorderSize = 0;


            style.WindowRounding = 6;
            style.ChildRounding = 6;
            style.FrameRounding = 4;
            style.PopupRounding = 4;
            style.ScrollbarRounding = 9;
            style.GrabRounding = 4;
            style.TabRounding = 4;


            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0, 0);


            colors[(int)ImGuiCol.Text] = new Vector4(0.95f, 0.96f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.36f, 0.42f, 0.47f, 1.00f);

            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.11f, 0.11f, 0.12f, 1.00f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.14f, 0.14f, 0.15f, 1f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);

            colors[(int)ImGuiCol.Border] = new Vector4(0.20f, 0.20f, 0.21f, 1.00f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0, 0, 0, 0);

            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.18f, 0.18f, 0.19f, 1f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.23f, 0.23f, 0.24f, 1f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.28f, 0.28f, 0.30f, 1f);

            colors[(int)ImGuiCol.TitleBg] = new Vector4(0.09f, 0.09f, 0.09f, 1.00f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.12f, 0.12f, 0.12f, 1.00f);
            colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0, 0, 0, 0.51f);

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

            colors[(int)ImGuiCol.Header] = new Vector4(0.20f, 0.20f, 0.21f, 1f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.30f, 0.30f, 0.32f, 1f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.35f, 0.35f, 0.37f, 1f);

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
            //colors[(int)ImGuiCol.act] = new Vector4(0.20f, 0.20f, 0.21f, 1.00f);
            //colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.15f, 0.15f, 0.16f, 1.00f);
            //colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.18f, 0.18f, 0.19f, 1.00f);

            colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.28f, 0.56f, 1.00f, 0.7f);
            colors[(int)ImGuiCol.DockingEmptyBg] = new Vector4(0.12f, 0.12f, 0.13f, 1.00f);

            colors[(int)ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
            colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);

            colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.19f, 0.19f, 0.20f, 1.00f);
            colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.31f, 0.31f, 0.35f, 1.00f);
            colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.23f, 0.23f, 0.25f, 1.00f);

            colors[(int)ImGuiCol.TableRowBg] = new Vector4(0, 0, 0, 0);
            colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(1, 1, 1, 0.03f);

            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.28f, 0.56f, 1.00f, 0.35f);
        }
    }
}
