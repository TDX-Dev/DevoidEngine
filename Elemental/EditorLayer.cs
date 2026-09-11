using DevoidEngine.Core;
using Elemental.Tools.EditorServices;
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
            };

        }

        // The init method.
        public override void OnAttach()
        {
            RegisterPanels(Context);

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
    }
}
