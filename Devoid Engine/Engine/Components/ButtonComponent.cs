using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Components
{
    public class ButtonComponent : Component
    {
        public override string Type => nameof(ButtonComponent);

        private CanvasComponent canvasComponent;

        public override void OnStart()
        {
            canvasComponent = gameObject.GetParentComponent<CanvasComponent>() ?? gameObject.GetComponent<CanvasComponent>();
            if (canvasComponent  == null )
                throw new Exception("Button Component must be part of tree with root as CanvasComponent: " +  gameObject.Name);
        }
    }
}
