using DevoidEngine.AssetPipeline;
using DevoidEngine.Assets;
using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Rendering;
using DevoidGPU;
using Elemental.Panels;
using Elemental.ProjectSettings;
using Elemental.Util;
using ImGuiNET;
using MessagePack;
using System;
using System.IO;
using System.Numerics;

namespace Elemental
{
    public class EditorLayer : Layer
    {
        private ImFontPtr editorFont;
        private PanelManager panelManager = null!;
        private EditorInputLayer editorInputLayer = null!;
        private Scene activeScene = null!;
        private Scene? editScene;
        private Scene? playScene;
        private bool isPlaying;

        private EditorContext editorContext = null!;
        private ProjectSettingsWindow projectSettings = null!;

        private SceneViewPanel sceneViewPanel = null!;
        private GameViewPanel gameViewPanel = null!;

        private bool openProjectSettingsRequested = false;

        private string? activeScenePath;

        private bool saveScenePopupRequested;
        private string saveSceneName = "";
        private string saveSceneDirectory = "";
        private string? pendingScenePath;
        private bool loadPendingSceneAfterSave;
        private bool unsavedScenePopupRequested;

        private string saveSceneError = "";

        private string DefaultSceneDirectory => Engine.Instance.ProjectSystem.AssetPath;

        public override void OnAttach()
        {
            editorInputLayer = new EditorInputLayer();
            Engine.InputSystem.Router.Push(editorInputLayer);

            ApplyImGuiStyling();

            editorFont = Application.ImguiRenderer.AddFontFromFile("./Assets/Fonts/JBM.ttf", 16);
            Application.ImguiRenderer.LoadIconFont("./Assets/Fonts/lucide.ttf", 16, (LucideIconFont.FontUnicodeMin, LucideIconFont.FontUnicodeMax));

            Application.ImguiRenderer.SetDefaultFont(editorFont);

            editorContext = new EditorContext();
            projectSettings = new ProjectSettingsWindow();

            panelManager = new PanelManager();
            sceneViewPanel = new SceneViewPanel(null!, editorContext);
            gameViewPanel = new GameViewPanel(null!);

            sceneViewPanel.SceneDropRequested = HandleSceneDrop;

            panelManager.AddPanel(sceneViewPanel);
            panelManager.AddPanel(gameViewPanel);
            panelManager.AddPanel(new AssetBrowserPanel());
            panelManager.AddPanel(new InspectorPanel(editorContext));
            panelManager.AddPanel(new HierarchyPanel(editorContext));

            ProjectSettingsRegistry.Register(new InputSettingsProvider());

            SetupSandbox();

            Application.MainWindow.Window.Title = $"Elemental Editor - Editing Scene: {editScene?.SceneName ?? "Empty Scene"}";
        }

        public override void OnGUIRender()
        {
            DrawMenuBar();
            HandlePopupOpen();
            panelManager.OnImGuiRender();
            projectSettings.Draw();

            if (saveScenePopupRequested)
            {
                ImGui.OpenPopup("Save Scene");
                saveScenePopupRequested = false;
            }

            if (unsavedScenePopupRequested)
            {
                ImGui.OpenPopup("Unsaved Changes");
                unsavedScenePopupRequested = false;
            }

            DrawSaveSceneModal();
            DrawUnsavedSceneModal();

            EditorUI.DrawMaterialInstanceEditors();
        }

        private void HandlePopupOpen()
        {
            if (openProjectSettingsRequested)
            {
                projectSettings.Open();
                openProjectSettingsRequested = false;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            HandleSaveShortcuts();
            editorInputLayer.SceneViewFocused = editorContext.IsSceneViewFocused;

            var currentScene = Engine.Instance.SceneTree.CurrentScene;
            if (currentScene != null && currentScene != activeScene)
            {
                activeScene = currentScene;
                sceneViewPanel.Viewport.TargetScene = activeScene;
                gameViewPanel.Viewport.TargetScene = activeScene;

                editorContext.ActiveScene = activeScene;
            }

            panelManager.OnUpdate(deltaTime);
        }

        public override void OnDetach()
        {
            panelManager.Clear();
        }

        private void SetupSandbox()
        {
            activeScene = Asset.Load<Scene>("BaseLevel.scene")!;/*Asset.Load<PackedScene>("models/sh.gltf")!.Instantiate();*/

            Engine.Instance.SceneTree.LoadScene(activeScene);
            sceneViewPanel.Viewport.TargetScene = activeScene;
            gameViewPanel.Viewport.TargetScene = activeScene;

            editorContext.ActiveScene = activeScene;

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
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.09f, 0.09f, 0.09f, 1.00f));

            ImGui.SetNextWindowPos(Vector2.Zero, ImGuiCond.Always);
            ImGui.SetNextWindowSize(new Vector2(ImGui.GetIO().DisplaySize.X, height), ImGuiCond.Always);

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

            ToolbarButton("New", LucideIconFont.IconFile, "New");
            ImGui.SameLine();

            ToolbarButton("Open", LucideIconFont.IconFolderOpen, "Open");
            ImGui.SameLine();

            if (ToolbarButton("Save", LucideIconFont.IconSave, "Save"))
            {
                SaveScene();
            }

            ImGui.SameLine();

            if (ToolbarButton("SaveAs", LucideIconFont.IconFilePen, "Save As"))
            {
                SaveSceneAs();
            }

            EndToolbarSection();

            Separator();

            BeginToolbarSection("Actions");

            ToolbarButton("Undo", LucideIconFont.IconUndo2, "Undo");
            ImGui.SameLine();

            ToolbarButton("Redo", LucideIconFont.IconRedo2, "Redo");

            EndToolbarSection();

            Separator();

            BeginToolbarSection("");

            ToolbarButton("Select", LucideIconFont.IconMousePointer2, "Select");
            ImGui.SameLine();

            ToolbarButton("Move", LucideIconFont.IconMove3d, "Move");
            ImGui.SameLine();

            ToolbarButton("Rotate", LucideIconFont.IconRotate3d, "Rotate");
            ImGui.SameLine();

            ToolbarButton("Scale", LucideIconFont.IconScale3d, "Scale");
            ImGui.SameLine();

            ToolbarButton("Spline", LucideIconFont.IconSpline, "Spline");

            EndToolbarSection();

            Separator();

            BeginToolbarSection("Primitives");

            ToolbarButton("Cube", LucideIconFont.IconCuboid, "Create Cube");
            ImGui.SameLine();

            ToolbarButton("Sphere", LucideIconFont.IconCircle, "Create Sphere");
            ImGui.SameLine();

            ToolbarButton("Cylinder", LucideIconFont.IconCylinder, "Create Cylinder");
            ImGui.SameLine();

            ToolbarButton("Mesh", LucideIconFont.IconBox, "Create Mesh");

            EndToolbarSection();

            Separator();

            BeginToolbarSection("Game Tools");

            if (!isPlaying)
            {
                if (ToolbarButton("Play", LucideIconFont.IconPlay, "Play"))
                {
                    PlayScene();
                }
            }
            else
            {
                if (ToolbarButton("Stop", LucideIconFont.IconSquare, "Stop"))
                {
                    StopScene();
                }
            }

            ImGui.SameLine();

            ToolbarButton("Open In Runtime", LucideIconFont.IconAppWindow, "Open In Runtime");

            EndToolbarSection();

            Separator();

            BeginToolbarSection("Project");

            if (ToolbarButton("Project Settings", LucideIconFont.IconFileCog, "Project Settings"))
            {
                openProjectSettingsRequested = true;
            }

            EndToolbarSection();

            Separator();

            ImGui.End();

            ImGui.PopStyleColor(2);
            ImGui.PopStyleVar(2);
        }

        private void PlayScene()
        {
            if (isPlaying)
                return;

            editScene = activeScene;
            playScene = SceneTools.Copy(editScene);

            Engine.Instance.SceneTree.LoadScene(playScene, disposeOld: false);

            playScene.Play();

            sceneViewPanel.Viewport.TargetScene = playScene;
            gameViewPanel.Viewport.TargetScene = playScene;

            editorContext.ActiveScene = playScene;

            isPlaying = true;

            Application.MainWindow.Window.Title = $"Elemental Editor - Playing Scene: {playScene.SceneName ?? "Empty Scene"}";
        }

        private void StopScene()
        {
            if (!isPlaying)
                return;

            Engine.Instance.SceneTree.LoadScene(editScene!);

            activeScene = editScene!;
            playScene = null;
            isPlaying = false;

            sceneViewPanel.Viewport.TargetScene = activeScene;
            gameViewPanel.Viewport.TargetScene = activeScene;

            editorContext.ActiveScene = activeScene;

            Application.MainWindow.Window.Title = $"Elemental Editor - Editing Scene: {editScene?.SceneName ?? "Empty Scene"}";
        }

        private bool SaveScene()
        {
            if (isPlaying || activeScene == null)
                return false;

            if (string.IsNullOrWhiteSpace(activeScenePath))
            {
                OpenSaveSceneDialog();
                return false;
            }

            return SaveSceneToPath(activeScenePath);
        }
        private void LoadPendingScene()
        {
            if (string.IsNullOrWhiteSpace(pendingScenePath))
                return;

            string path = pendingScenePath;
            pendingScenePath = null;

            LoadSceneFromPath(path);
        }

        private bool LoadSceneFromPath(string relativePath)
        {
            if (isPlaying)
                return false;

            try
            {
                Scene? scene = Asset.Load<Scene>(relativePath, false);

                if (scene == null)
                {
                    Console.WriteLine(
                        $"[Editor] Failed to load scene: {relativePath}");

                    return false;
                }

                activeScene = scene;

                Engine.Instance.SceneTree.LoadScene(activeScene);

                sceneViewPanel.Viewport.TargetScene = activeScene;
                gameViewPanel.Viewport.TargetScene = activeScene;

                editorContext.ActiveScene = activeScene;
                editorContext.SceneDirty = false;

                activeScenePath = Path.GetFullPath(
                    Path.Combine(
                        Engine.Instance.ProjectSystem.AssetPath,
                        relativePath));

                Application.MainWindow.Window.Title =
                    $"Elemental Editor - Editing Scene: {activeScene.SceneName ?? "Empty Scene"}";

                Console.WriteLine(
                    $"[Editor] Loaded scene: {activeScenePath}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[Editor] Failed to load scene '{relativePath}':\n{ex}");

                return false;
            }
        }
        private bool SaveSceneToPath(string path)
        {
            try
            {
                string? directory = Path.GetDirectoryName(path);

                if (string.IsNullOrWhiteSpace(directory))
                {
                    saveSceneError = "Invalid scene directory.";
                    return false;
                }

                Directory.CreateDirectory(directory);

                byte[] data = SceneTools.Serialize(activeScene);
                File.WriteAllBytes(path, data);

                activeScenePath = Path.GetFullPath(path);
                activeScene.SceneName = Path.GetFileNameWithoutExtension(path);

                editorContext.SceneDirty = false;

                Application.MainWindow.Window.Title =
                    $"Elemental Editor - Editing Scene: {activeScene.SceneName}";

                Console.WriteLine($"[Editor] Saved scene: {activeScenePath}");

                if (loadPendingSceneAfterSave)
                {
                    loadPendingSceneAfterSave = false;
                    LoadPendingScene();
                }

                return true;
            }
            catch (Exception ex)
            {
                saveSceneError = ex.Message;
                Console.WriteLine($"[Editor] Failed to save scene:\n{ex}");
                return false;
            }
        }

        private void SaveSceneAs()
        {
            if (isPlaying)
                return;

            OpenSaveSceneDialog();
        }

        private void OpenSaveSceneDialog()
        {
            saveScenePopupRequested = true;

            saveSceneName = string.IsNullOrWhiteSpace(activeScene?.SceneName)
                ? "Scene"
                : activeScene.SceneName;

            saveSceneDirectory = string.IsNullOrWhiteSpace(activeScenePath)
                ? DefaultSceneDirectory
                : Path.GetDirectoryName(activeScenePath) ?? DefaultSceneDirectory;

            saveSceneError = "";
        }

        private void DrawSaveSceneModal()
        {
            if (!ImGui.BeginPopupModal("Save Scene", ImGuiWindowFlags.AlwaysAutoResize))
            {
                return;
            }

            ImGui.TextUnformatted("Name");
            ImGui.SetNextItemWidth(450);
            ImGui.InputText("##SceneName", ref saveSceneName, 256);

            ImGui.Spacing();

            ImGui.TextUnformatted("Location");
            ImGui.SetNextItemWidth(450);
            ImGui.InputText("##SceneDirectory", ref saveSceneDirectory, 1024);

            ImGui.Spacing();

            string filename = saveSceneName.Trim();
            if (!filename.EndsWith(".scene", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".scene";
            }

            string path = Path.Combine(saveSceneDirectory, filename);
            ImGui.TextDisabled(path);

            if (!string.IsNullOrEmpty(saveSceneError))
            {
                ImGui.Spacing();
                ImGui.TextColored(new Vector4(1f, 0.3f, 0.3f, 1f), saveSceneError);
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Save", new Vector2(-1, 0)))
            {
                if (string.IsNullOrWhiteSpace(saveSceneName))
                {
                    saveSceneError = "Scene name cannot be empty.";
                }
                else if (string.IsNullOrWhiteSpace(saveSceneDirectory))
                {
                    saveSceneError = "Scene location cannot be empty.";
                }
                else if (!Directory.Exists(saveSceneDirectory))
                {
                    saveSceneError = "The specified directory does not exist.";
                }
                else
                {
                    string finalPath = Path.Combine(saveSceneDirectory, filename);

                    if (SaveSceneToPath(finalPath))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            if (ImGui.Button("Cancel", new Vector2(-1, 0)))
            {
                loadPendingSceneAfterSave = false;
                pendingScenePath = null;

                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
        private void DrawUnsavedSceneModal()
        {
            if (!ImGui.BeginPopupModal(
                    "Unsaved Changes",
                    ImGuiWindowFlags.AlwaysAutoResize))
            {
                return;
            }

            ImGui.TextUnformatted(
                $"Scene '{activeScene?.SceneName ?? "Untitled"}' has unsaved changes.");

            ImGui.Spacing();

            ImGui.TextUnformatted(
                "Do you want to save your changes before loading the new scene?");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Save", new Vector2(120, 0)))
            {
                if (string.IsNullOrWhiteSpace(activeScenePath))
                {
                    loadPendingSceneAfterSave = true;
                    OpenSaveSceneDialog();
                    ImGui.CloseCurrentPopup();
                }
                else
                {
                    if (SaveScene())
                    {
                        LoadPendingScene();
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Don't Save", new Vector2(120, 0)))
            {
                LoadPendingScene();
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();

            if (ImGui.Button("Cancel", new Vector2(120, 0)))
            {
                pendingScenePath = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
        private void HandleSaveShortcuts()
        {
            if (!ImGui.GetIO().WantTextInput)
            {
                if (ImGui.IsKeyPressed(ImGuiKey.S, false) && ImGui.GetIO().KeyCtrl)
                {
                    if (ImGui.GetIO().KeyShift)
                        SaveSceneAs();
                    else
                        SaveScene();
                }
            }
        }
        private void HandleSceneDrop(string relativePath)
        {
            if (isPlaying)
                return;

            if (editorContext.SceneDirty)
            {
                pendingScenePath = relativePath;
                unsavedScenePopupRequested = true;
                return;
            }

            LoadSceneFromPath(relativePath);
        }
        private static bool ToolbarButton(string id, string icon, string? tooltip = null)
        {
            bool pressed = ImGui.Button($"{icon}##{id}");

            if (tooltip != null && ImGui.IsItemHovered())
                ImGui.SetTooltip(tooltip);

            return pressed;
        }

        private static void Separator()
        {
            ImGui.SameLine();
            VerticalSeparator();
            ImGui.SameLine();
        }

        private static void BeginToolbarSection(string label)
        {
            ImGui.BeginGroup();
            ImGui.Text(label);
            ImGui.BeginGroup();
        }

        private static void EndToolbarSection()
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
            Vector2 p = ImGui.GetCursorScreenPos();
            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            float height = ImGui.GetContentRegionAvail().Y;
            float lineX = p.X + widthPadding;

            uint color = ImGui.GetColorU32(ImGuiCol.Border);
            drawList.AddLine(new Vector2(lineX, p.Y), new Vector2(lineX, p.Y + height), color, thickness);

            float totalWidth = (widthPadding * 2.0f) + thickness;
            ImGui.Dummy(new Vector2(totalWidth, height));
        }
    }
}