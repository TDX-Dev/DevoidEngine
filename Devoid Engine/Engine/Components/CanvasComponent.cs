using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class CanvasComponent : Component, IRenderComponent
    {
        public override string Type => nameof(CanvasComponent);

        public CanvasNode Canvas = new CanvasNode()
        {
            Direction = FlexDirection.Row,
            Align = AlignItems.Center,
            Justify = JustifyContent.Center
        };

        public void Collect(CameraComponent3D camera, CameraRenderContext viewData)
        {
            Canvas.Render(viewData.renderItemsUI);
        }

        public override void OnStart()
        {
            UISystem.Roots.Add(Canvas);

            
            base.OnStart();
        }

        public override void OnUpdate(float dt)
        {

        }

    }
}
