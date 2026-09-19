using DevoidEngine.Core;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Logging;
using DevoidEngine.Metadata;
using Elemental.Tools.EditorServices;
using Elemental.Tools.Menu;
using Elemental.Tools.Panels;
using Elemental.Tools.Shortcuts;
using Elemental.Tools.Themes;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Elemental
{
    sealed class EditorLayer : Layer
    {
        public EditorContext Context;

        public EditorLayer()
        {
            Context = new()
            {
                Services = new(),
                PanelManager = new(),
                Menu = new(),
                Toolbar = new(),
                Shortcuts = new(),
                EditorActions = new(),
                SceneService = new(),
            };

            Context.PanelManager.AddPanel(new SceneViewPanel());
            Context.PanelManager.AddPanel(new OutlinerPanel());
            Context.PanelManager.AddPanel(new InspectorPanel());
            Context.PanelManager.ProcessPendingChanges(Context);
        }

        // The init method.
        public override void OnAttach()
        {
            SetupEditorTheme();

            RegisterPanels(Context);

            EditorAction action = new()
            {
                Id = new EditorActionId("Editor.Test"),
                Name = "DebugPrint",
                Label = "Print debug",
                Shortcut = new Shortcut(KeyModifiers.Shift, Keys.I),
                Execute = () =>
                {
                    DevoidLog.Info(LogCategory.Editor, "Saving scene");
                    Context.SceneService.SaveScene();
                },
            };

            Context.EditorActions.Register(action);
            Context.Shortcuts.Register(action.Shortcut ?? new Shortcut(), action.Id);

            Context.Menu.Register("File/Save", new MenuItem()
            {
                Name = "Save Scene",
                EditorAction = action
            });

            Context.SceneService.OnSceneChanged += scene =>
            {
                Context.PanelManager.GetPanel<SceneViewPanel>()!.Viewport.TargetScene = scene;
            };


            EditorTestingScene.LoadTestScene(Context);
        }

        void RegisterPanels(EditorContext context)
        {
            
        }

        public override void OnUpdate(float deltaTime)
        {
            Context.PanelManager.OnUpdate(Context, deltaTime);
        }

        public override void OnGUIRender()
        {
            Application.ImguiRenderer.SetCustomToolbarHeight(Context.Toolbar.ToolbarHeight);

            Context.Menu.OnImguiRender(Context);
            Context.Toolbar.OnImguiRender(Context);
            Context.PanelManager.OnImGuiRender(Context);
        }

        public override void OnDetach()
        {
            Context.PanelManager.Clear();
        }
        public void SetupEditorTheme()
        {
            ImFontPtr font = Application.ImguiRenderer.AddFontFromFile("Assets/Arimo-Regular.ttf", 14);

            Application.ImguiRenderer.LoadIconFont("Assets/lucide.ttf", 16, (LucideIconFont.FontUnicodeMin, LucideIconFont.FontUnicodeMax));

            ImFontPtr boldFont = Application.ImguiRenderer.AddFontFromFile("Assets/Arimo-Bold.ttf", 14);

            Application.ImguiRenderer.LoadIconFont("Assets/lucide.ttf", 16, (LucideIconFont.FontUnicodeMin, LucideIconFont.FontUnicodeMax));
            Application.ImguiRenderer.SetDefaultFont(font);

            DefaultTheme.Apply();

            Context.DefaultFont = font;
            Context.BoldFont = boldFont;
        }
        public override void OnKeyDown(Keys keys, int Scancode, KeyModifiers modifiers, bool isRepeated)
        {
            if (isRepeated)
                return;

            if (Context.Shortcuts.TryGetAction(keys, modifiers, out EditorActionId id))
            {
                if (Context.EditorActions.TryGet(id, out EditorAction? action))
                {
                    action!.Execute.Invoke();
                }
            }
        }
    }
}
