using DevoidEngine.Engine.UI.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public abstract class UIComponent : Component
    {
        public UINode? Root { get; protected set; }

        public override void OnStart()
        {
            Root = BuildNode();

            AttachToHierarchy();
        }

        protected abstract UINode BuildNode();

        private void AttachToHierarchy()
        {
            if (Root == null) return;
            var parentUI = gameObject.GetParentComponent<UIComponent>();

            if (parentUI != null)
            {
                if (parentUI.Root == null) return;
                parentUI.Root.Add(Root);
                return;
            }

            var canvas = gameObject.GetParentComponent<CanvasComponent>() ?? gameObject.GetComponent<CanvasComponent>();

            if (canvas != null)
            {
                canvas.Canvas.Add(Root);
                return;
            }

            throw new Exception(
                $"{GetType().Name} requires a CanvasComponent in its parent hierarchy."
            );
        }
    }
}
