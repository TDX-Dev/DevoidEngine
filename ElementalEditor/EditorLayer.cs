using DevoidEngine.Engine.AssetPipeline;
using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using ElementalEditor.Panels;
using ElementalEditor.Utils;
using ImGuiNET;
using System.Numerics;

namespace ElementalEditor
{
    class EditorLayer : Layer
    {
        EditorContext context;
        EditorCamera editorCamera;
        EditorInputLayer inputLayer;

        List<IEditorPanel> panels;

        ImFontPtr editorFont;

        public override void OnAttach()
        {
            SetStyling();
            editorFont = Application.ImGuiBackend.AddFontFromFile("./Content/Font/JetBrainsMono-Regular.ttf", 16);
            Application.ImGuiBackend.LoadIconFont("./Content/Font/bootstrap_icons.ttf", 16, (BootstrapIconFont.IconMin, BootstrapIconFont.IconMax16));


            editorCamera = new EditorCamera(1280, 720);
            inputLayer = new EditorInputLayer(editorCamera);
            Input.Router.Push(inputLayer);

            panels = new List<IEditorPanel>();
            context = new EditorContext();


            panels.Add(new SceneViewportPanel());
            panels.Add(new HierarchyPanel());
            panels.Add(new InspectorPanel());
            panels.Add(new AssetBrowserPanel());
            panels.Add(new ProjectSettingsPanel());



            var scene = new Scene();
            SceneManager.LoadScene(scene);
            scene.Play(true);

            //scene.AddGameObject("Camera").AddComponent<CameraComponent3D>().IsDefault = true;
            //scene.AddGameObject("Skybox").AddComponent<SkyboxComponent>();
            //Asset.Load<Model>("platform/platform.gltf").Instantiate(scene);
            //Asset.Load<Model>("cubey_boi/cubey_boi.gltf").Instantiate(scene);

        }

        public override void OnUpdate(float deltaTime)
        {
            inputLayer.ViewportActive = context.ViewportFocused;
            inputLayer.Update(deltaTime);
            SceneManager.CurrentScene?.Update(deltaTime);
        }
        public override void OnFixedUpdate(float deltaTime)
        {
            SceneManager.CurrentScene?.FixedUpdate(deltaTime);
        }

        public override void OnRender()
        {
            SceneManager.CurrentScene?.Render();
            var cam = editorCamera.Camera;

            CameraRenderContext ctx = new CameraRenderContext();
            ctx.Clear();

            ctx.camera = cam;
            ctx.cameraData = cam.GetCameraData();
            ctx.cameraTargetSurface = cam.RenderTarget;

            var renderables = SceneManager.CurrentScene.GetRenderables();

            foreach (var r in renderables)
            {
                r.Collect(cam, ctx);
            }

            Renderer.Render(ctx);
        }

        public override void OnResize(int width, int height)
        {
            base.OnResize(width, height);
        }



        public override void OnGUIRender()
        {
            ImGui.PushFont(editorFont);
            DrawMenuBar();
            DrawToolbar();

            var io = ImGui.GetIO();

            if (inputLayer.IsNavigating) // RMB held
            {
                io.ConfigFlags |= ImGuiConfigFlags.NoMouse;
            }
            else
            {
                io.ConfigFlags &= ~ImGuiConfigFlags.NoMouse;
            }

            if (SceneManager.CurrentScene != null)
            {

                context.Scene = SceneManager.CurrentScene;
                context.EditorCamera = editorCamera;
            }

            foreach (var panel in panels)
                panel.Draw(context);
            DrawFooter();
            ImGui.PopFont();
        }
        public float ToolbarHeight = 0f;
        void DrawToolbar()
        {
            var viewport = ImGui.GetMainViewport();

            ImGui.SetNextWindowPos(new Vector2(
                viewport.WorkPos.X,
                viewport.WorkPos.Y));

            // Only force width
            ImGui.SetNextWindowSize(new Vector2(
                viewport.WorkSize.X,
                0)); // height auto

            ImGui.SetNextWindowViewport(viewport.ID);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(6, 4));

            ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoSavedSettings;

            ImGui.Begin("Toolbar", flags);

            if (ImGui.Button("Play")) { }
            ImGui.SameLine();
            if (ImGui.Button("Pause")) { }
            ImGui.SameLine();
            if (ImGui.Button("Stop")) { }

            ToolbarHeight = ImGui.GetWindowHeight();

            ImGui.End();

            ImGui.PopStyleVar(3);

            Application.ImGuiBackend.SetCustomToolbarHeight(ToolbarHeight);
        }

        void DrawMenuBar()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("New Scene")) { }
                    if (ImGui.MenuItem("Open Scene")) { }
                    if (ImGui.MenuItem("Save Scene")) { }
                    ImGui.Separator();
                    if (ImGui.MenuItem("Exit")) { }

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

        void DrawFooter()
        {
            var viewport = ImGui.GetMainViewport();

            float height = 24.0f;

            ImGui.SetNextWindowPos(new Vector2(
                viewport.WorkPos.X,
                viewport.WorkPos.Y + viewport.WorkSize.Y - height));

            ImGui.SetNextWindowSize(new Vector2(
                viewport.WorkSize.X,
                height));

            ImGui.SetNextWindowViewport(viewport.ID);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 4));

            ImGuiWindowFlags flags =
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoDocking |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoSavedSettings;

            ImGui.Begin("FooterBar", flags);

            ImGui.Text("Ready");

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - 120);
            ImGui.Text("FPS: " + ImGui.GetIO().Framerate.ToString("0"));

            ImGui.End();

            ImGui.PopStyleVar(3);
        }

        public static void SetStyling()
        {
            var style = ImGui.GetStyle();
            var colors = style.Colors;

            // --------------------------------------------------
            // Layout
            // --------------------------------------------------

            style.WindowPadding = new Vector2(10, 10);
            style.FramePadding = new Vector2(6, 4);
            style.CellPadding = new Vector2(6, 4);
            style.ItemSpacing = new Vector2(8, 6);
            style.ItemInnerSpacing = new Vector2(6, 4);
            style.TouchExtraPadding = new Vector2(0, 0);
            style.IndentSpacing = 20;
            style.ScrollbarSize = 14;
            style.GrabMinSize = 10;

            // --------------------------------------------------
            // Borders
            // --------------------------------------------------

            style.WindowBorderSize = 1;
            style.ChildBorderSize = 1;
            style.PopupBorderSize = 1;
            style.FrameBorderSize = 0;
            style.TabBorderSize = 0;

            // --------------------------------------------------
            // Rounding
            // --------------------------------------------------

            style.WindowRounding = 6;
            style.ChildRounding = 6;
            style.FrameRounding = 4;
            style.PopupRounding = 4;
            style.ScrollbarRounding = 9;
            style.GrabRounding = 4;
            style.TabRounding = 4;

            // --------------------------------------------------
            // Alignment
            // --------------------------------------------------

            style.WindowTitleAlign = new Vector2(0.0f, 0.5f);
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0, 0);

            // --------------------------------------------------
            // Dark Grey Theme
            // --------------------------------------------------

            colors[(int)ImGuiCol.Text] = new Vector4(0.95f, 0.96f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.36f, 0.42f, 0.47f, 1.00f);

            colors[(int)ImGuiCol.WindowBg] = new Vector4(0.11f, 0.11f, 0.12f, 1.00f);
            colors[(int)ImGuiCol.ChildBg] = new Vector4(0.15f, 0.15f, 0.16f, 1.00f);
            colors[(int)ImGuiCol.PopupBg] = new Vector4(0.08f, 0.08f, 0.08f, 0.94f);

            colors[(int)ImGuiCol.Border] = new Vector4(0.20f, 0.20f, 0.21f, 1.00f);
            colors[(int)ImGuiCol.BorderShadow] = new Vector4(0, 0, 0, 0);

            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.16f, 0.16f, 0.17f, 1.00f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.20f, 0.20f, 0.21f, 1.00f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.25f, 0.25f, 0.26f, 1.00f);

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

            colors[(int)ImGuiCol.Header] = new Vector4(0.20f, 0.20f, 0.21f, 1.00f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.28f, 0.56f, 1.00f, 0.80f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.28f, 0.56f, 1.00f, 1.00f);

            colors[(int)ImGuiCol.Separator] = new Vector4(0.28f, 0.28f, 0.29f, 1.00f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.47f, 1.00f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);

            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.56f, 1.00f, 0.25f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.28f, 0.56f, 1.00f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.28f, 0.56f, 1.00f, 0.95f);

            colors[(int)ImGuiCol.Tab] = new Vector4(0.15f, 0.15f, 0.16f, 1.00f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.28f, 0.56f, 1.00f, 0.80f);
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