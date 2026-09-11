using DevoidEngine.Core;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Logging;
using Elemental.Tools.EditorServices;
using Elemental.Tools.Shortcuts;
using System;
using System.Collections.Generic;
using System.Linq;
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
                Shortcuts = new(),
                EditorActions = new(),
            };

        }

        // The init method.
        public override void OnAttach()
        {
            RegisterPanels(Context);


            EditorAction action = new()
            {
                Id = new EditorActionId("Editor.Test"),
                Name = "DebugPrint",
                Label = "Print debug",
                Shortcut = new Shortcut(KeyModifiers.Shift, Keys.I),
                Execute = () =>
                {
                    DevoidLog.Info(LogCategory.Editor, "This shortcut was triggered");
                },
            };

            Context.EditorActions.Register(action);
            Context.Shortcuts.Register(action.Shortcut ?? new Shortcut(), action.Id);
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
            Context.PanelManager.OnImGuiRender(Context);
        }

        public override void OnDetach()
        {
            Context.PanelManager.Clear();
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
