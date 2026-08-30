using DevoidEngine.AssetPipeline;
using DevoidEngine.Assets;
using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Rendering;
using DevoidGPU;
using Elemental.ContextMenu;
using Elemental.Dialogs;
using Elemental.Documents;
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
        private SceneDocument activeDocument = null!;

        private EditorContext editorContext = null!;
        private ProjectSettingsWindow projectSettings = null!;
        private ImportSettingsWindow importSettings = null!;

        private SceneViewPanel sceneViewPanel = null!;
        private GameViewPanel gameViewPanel = null!;

        private EditorDialogManager dialogs = null!;

        private bool openProjectSettingsRequested = false;
        bool openImportSettingsRequested;
        string importSettingsPath = string.Empty;
        private Scene? playScene;
        private bool IsPlaying => playScene != null;

        private string DefaultSceneDirectory => Engine.Instance.ProjectSystem.AssetPath;

        public override void OnAttach()
        {
            Engine.Instance.SceneTree.RootViewport.Render = false;
            AssetContextMenuRegistry.Register("*", path =>
            {
                var ext = Path.GetExtension(path).ToLower();

                if (ImporterRegistry.HasImporter(ext))
                {
                    if (ImGui.MenuItem("Open Import Settings"))
                    {
                        openImportSettingsRequested = true;
                        importSettingsPath = path;
                    }
                }
            });

            editorInputLayer = new EditorInputLayer();
            Engine.InputSystem.Router.Push(editorInputLayer);

            dialogs = new EditorDialogManager();


            ApplyImGuiStyling();

            editorFont = Application.ImguiRenderer.AddFontFromFile("./Assets/Fonts/JBM.ttf", 16);
            Application.ImguiRenderer.LoadIconFont("./Assets/Fonts/lucide.ttf", 16, (LucideIconFont.FontUnicodeMin, LucideIconFont.FontUnicodeMax));

            Application.ImguiRenderer.SetDefaultFont(editorFont);

            editorContext = new EditorContext();
            projectSettings = new ProjectSettingsWindow();
            importSettings = new ImportSettingsWindow();

            panelManager = new PanelManager();
            sceneViewPanel = new SceneViewPanel(null!, editorContext);
            gameViewPanel = new GameViewPanel(null!);

            sceneViewPanel.AssetDropRequested = HandleAssetDrop;

            panelManager.AddPanel(sceneViewPanel);
            panelManager.AddPanel(gameViewPanel);
            panelManager.AddPanel(new AssetBrowserPanel());
            panelManager.AddPanel(new InspectorPanel(editorContext));
            panelManager.AddPanel(new HierarchyPanel(editorContext));

            ProjectSettingsRegistry.Register(new InputSettingsProvider());

            SetupSandbox();
        }

        public override void OnGUIRender()
        {
            DrawMenuBar();

            panelManager.OnImGuiRender();
            projectSettings.Draw();
            importSettings.Draw();

            dialogs.Draw();

            if (openProjectSettingsRequested)
            {
                projectSettings.Open();
                openProjectSettingsRequested = false;
            }

            if (openImportSettingsRequested)
            {
                importSettings.Open(importSettingsPath);
                openImportSettingsRequested = false;
            }


            EditorUI.DrawMaterialInstanceEditors();
        }

        public override void OnUpdate(float deltaTime)
        {
            HandleSaveShortcuts();

            editorInputLayer.SceneViewFocused = editorContext.IsSceneViewFocused;

            panelManager.OnUpdate(deltaTime);
        }
        public override void OnRender(ICommandList cmd)
        {
            if (!sceneViewPanel.IsOpen)
                return;
            GizmoContext context = sceneViewPanel.Viewport.GizmoContext;
            foreach (GameObject gameObject in activeDocument.Scene.GameObjects)
            {
                foreach (Component component in gameObject.Components)
                {
                    if (component is IGizmoProviderComponent gizmoProvider)
                    {
                        gizmoProvider.OnDrawGizmos(context);
                    }
                }
            }
        }
        public override void OnDetach()
        {
            panelManager.Clear();
        }

        private void SetupSandbox()
        {
            const string relativePath = "PortalLevel.scene";

            Scene scene = Asset.Load<Scene>(relativePath, false)!;

            var document = new SceneDocument(scene);

            document.MarkLoaded(Path.GetFullPath(Path.Combine(Engine.Instance.ProjectSystem.AssetPath, relativePath)));

            SetActiveDocument(document);

            SetupEnvironment();
            ConfigureInputBindings();
        }

        private void SetupEnvironment()
        {
            //Engine.Renderer.SkyRenderer.Sky = new HDRISky
            //{
            //    PanoramaTexture = Asset.Load<Texture>("HDRIs/soil_puresky.hdr")!
            //};
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

            if (ToolbarButton("New", LucideIconFont.IconFile, "New"))
            {
                RequestNewScene();
            }
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

            if (!IsPlaying)
            {
                if (ToolbarButton("Play", LucideIconFont.IconPlay, "Play"))
                {
                    PlayScene();
                    gameViewPanel.RequestFocus();
                }
            }
            else
            {
                if (ToolbarButton("Stop", LucideIconFont.IconSquare, "Stop"))
                {
                    StopScene();
                    sceneViewPanel.RequestFocus();
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
            if (IsPlaying)
                return;

            Scene editScene = activeDocument.Scene;

            playScene = SceneTools.Copy(editScene);

            SetActiveScene(
                playScene,
                SceneMode.Play,
                disposeOld: false);

            editorContext.SelectedObject = null;

            Application.MainWindow.Window.Title =
                $"Elemental Editor - Playing Scene: " +
                $"{playScene.SceneName ?? "Empty Scene"}";
        }

        private void StopScene()
        {
            if (!IsPlaying)
                return;

            Scene editScene = activeDocument.Scene;

            SetActiveScene(
                editScene,
                SceneMode.Edit,
                disposeOld: true);

            Engine.Instance.SceneTree.QueueFree(playScene!);
            playScene = null;

            editorContext.SelectedObject = null;

            Application.MainWindow.Window.Title =
                $"Elemental Editor - Editing Scene: " +
                $"{editScene.SceneName ?? "Empty Scene"}";
        }
        private bool LoadSceneFromPath(string relativePath)
        {
            if (IsPlaying)
                return false;

            try
            {
                Scene? scene = Asset.Load<Scene>(
                    relativePath,
                    false);

                if (scene == null)
                {
                    Console.WriteLine(
                        $"[Editor] Failed to load scene: {relativePath}");

                    return false;
                }

                string fullPath =
                    Path.GetFullPath(
                        Path.Combine(
                            Engine.Instance.ProjectSystem.AssetPath,
                            relativePath));

                var document = new SceneDocument(scene);
                document.MarkLoaded(fullPath);

                SetActiveDocument(document);

                Console.WriteLine(
                    $"[Editor] Loaded scene: {document.FilePath}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[Editor] Failed to load scene '{relativePath}':\n{ex}");

                return false;
            }
        }
        private bool SaveScene()
        {
            if (IsPlaying)
                return false;

            if (!activeDocument.HasFile)
            {
                OpenSaveSceneDialog();
                return false;
            }

            return SaveSceneToPath(activeDocument.FilePath!);
        }

        private void SaveSceneAs()
        {
            if (IsPlaying)
                return;

            OpenSaveSceneDialog();
        }
        private void RequestNewScene()
        {
            if (IsPlaying)
                return;

            if (!activeDocument.IsDirty)
            {
                CreateNewScene();
                return;
            }

            dialogs.Open(
                new UnsavedChangesDialog(
                    "Unsaved Changes",

                    $"Scene '{activeDocument.Scene.SceneName ?? "Untitled"}' " +
                    "has unsaved changes.\n\n" +
                    "Do you want to save your changes before creating a new scene?",

                    onSave: SaveBeforeNewScene,

                    onDiscard: CreateNewScene));
        }
        private void CreateNewScene()
        {
            if (IsPlaying)
                return;

            var scene = new Scene
            {
                SceneName = "Untitled"
            };

            var document = new SceneDocument(scene);

            SetActiveDocument(document);

            editorContext.SelectedObject = null;

            Application.MainWindow.Window.Title =
                "Elemental Editor - Untitled Scene";

            Console.WriteLine("[Editor] Created new scene.");
        }
        private void SaveBeforeNewScene()
        {
            if (activeDocument.HasFile)
            {
                if (SaveScene())
                {
                    CreateNewScene();
                }

                return;
            }

            OpenSaveSceneDialog(
                _ => CreateNewScene());
        }
        private void OpenSaveSceneDialog(Action<string>? onSaved = null)
        {
            string sceneName =
                string.IsNullOrWhiteSpace(
                    activeDocument.Scene.SceneName)
                    ? "Scene"
                    : activeDocument.Scene.SceneName;

            string directory =
                activeDocument.HasFile
                    ? Path.GetDirectoryName(
                        activeDocument.FilePath!)
                        ?? DefaultSceneDirectory
                    : DefaultSceneDirectory;

            dialogs.Open(
                new SaveSceneDialog(
                    sceneName,
                    directory,
                    SaveSceneToPath,
                    onSaved));
        }

        private bool SaveSceneToPath(string path)
        {
            Console.WriteLine(
                $"[SCENE SAVE] " +
                $"isPlaying={IsPlaying}, " +
                $"activeDocument.Scene={activeDocument.Scene.SceneName}, " +
                $"activeScene={editorContext.ActiveScene?.SceneName}, " +
                $"playScene={playScene?.SceneName}, " +
                $"sameAsPlay={ReferenceEquals(activeDocument.Scene, playScene)}");

            try
            {
                string? directory =
                    Path.GetDirectoryName(path);

                if (string.IsNullOrWhiteSpace(directory))
                {
                    Console.WriteLine(
                        "[Editor] Invalid scene directory.");

                    return false;
                }

                Directory.CreateDirectory(directory);

                byte[] data =
                    SceneTools.Serialize(activeDocument.Scene);

                File.WriteAllBytes(path, data);

                activeDocument.Scene.SceneName =
                    Path.GetFileNameWithoutExtension(path);

                activeDocument.MarkSaved(path);

                Console.WriteLine(
                    $"[Editor] Saved scene: {activeDocument.FilePath}");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[Editor] Failed to save scene:\n{ex}");

                return false;
            }
        }
        private void SetActiveScene(Scene scene, SceneMode mode, bool disposeOld = true)
        {
            Engine.Instance.SceneTree.LoadScene(scene, disposeOld);

            scene.SetMode(mode);

            sceneViewPanel.Viewport.TargetScene = scene;
            gameViewPanel.Viewport.TargetScene = scene;

            editorContext.ActiveScene = scene;
        }
        private void SetActiveDocument(SceneDocument document)
        {
            activeDocument = document;

            editorContext.ActiveDocument = document;

            SetActiveScene(
                document.Scene,
                SceneMode.Edit);
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
        private void HandleAssetDrop(string relativePath)
        {
            if (IsPlaying)
                return;

            string extension = Path.GetExtension(relativePath);

            if (extension.Equals(".scene", StringComparison.OrdinalIgnoreCase))
            {
                HandleSceneDrop(relativePath);
                return;
            }

            if (extension.Equals(".gltf", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".glb", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".fbx", StringComparison.OrdinalIgnoreCase) ||
                extension.Equals(".obj", StringComparison.OrdinalIgnoreCase))
            {
                HandleModelDrop(relativePath);
                return;
            }
        }
        private void HandleModelDrop(string relativePath)
        {
            try
            {
                PackedScene? modelScene =
                    Asset.Load<PackedScene>(relativePath);

                if (modelScene == null)
                {
                    Console.WriteLine(
                        $"[Editor] Failed to load model: {relativePath}");
                    return;
                }

                modelScene.Instantiate(activeDocument.Scene);

                activeDocument.MarkDirty();

                Console.WriteLine(
                    $"[Editor] Instantiated model: {relativePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[Editor] Failed to instantiate model '{relativePath}':\n{ex}");
            }
        }
        private void HandleSceneDrop(string relativePath)
        {
            RequestLoadScene(relativePath);
        }

        private void RequestLoadScene(string relativePath)
        {
            if (IsPlaying)
                return;

            if (!activeDocument.IsDirty)
            {
                LoadSceneFromPath(relativePath);
                return;
            }

            dialogs.Open(
                new UnsavedChangesDialog(
                    "Unsaved Changes",

                    $"Scene '{activeDocument.Scene.SceneName ?? "Untitled"}' " +
                    "has unsaved changes.\n\n" +
                    "Do you want to save your changes before loading the new scene?",

                    onSave: () =>
                    {
                        SaveBeforeLoad(relativePath);
                    },

                    onDiscard: () =>
                    {
                        LoadSceneFromPath(relativePath);
                    }));
        }

        private void SaveBeforeLoad(string relativePath)
        {
            if (activeDocument.HasFile)
            {
                if (SaveScene())
                {
                    LoadSceneFromPath(relativePath);
                }

                return;
            }

            OpenSaveSceneDialog(
                _ => LoadSceneFromPath(relativePath));
        }
        private void DumpScene(string label, Scene scene)
        {
            byte[] data = SceneTools.Serialize(scene);

            Console.WriteLine(
                $"[{label}] " +
                $"Scene={scene.GetHashCode()}, " +
                $"SerializedBytes={data.Length}");
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