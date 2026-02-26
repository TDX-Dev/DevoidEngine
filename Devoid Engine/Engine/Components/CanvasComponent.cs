using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using SharpFont;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public enum CanvasRenderMode
    {
        ScreenSpace,
        WorldSpace
    }

    public class CanvasComponent : Component, IRenderComponent
    {
        public override string Type => nameof(CanvasComponent);

        public CanvasRenderMode RenderMode = CanvasRenderMode.ScreenSpace;
        public int PixelsPerUnit = 100;

        public CanvasNode Canvas = new CanvasNode()
        {
            Direction = FlexDirection.Row,
            Align = AlignItems.Center,
            Justify = JustifyContent.Center
        };

        public Matrix4x4 GetCanvasModelMatrix()
        {
            if (RenderMode == CanvasRenderMode.ScreenSpace)
                return Matrix4x4.Identity;

            return gameObject.transform.WorldMatrix;
        }

        public void Collect(CameraComponent3D camera, CameraRenderContext viewData)
        {
            if (RenderMode == CanvasRenderMode.ScreenSpace)
            {
                Canvas.Render(viewData.renderItemsUI, Matrix4x4.Identity);
            }
            else
            {
                Matrix4x4 world =
                    Matrix4x4.CreateScale(1f / PixelsPerUnit) *
                    gameObject.transform.WorldMatrix;

                Canvas.Render(viewData.renderItems3D, world);
            }
        }

        public override void OnStart()
        {
            UISystem.Roots.Add(Canvas);

            
            base.OnStart();
        }

        public override void OnDestroy()
        {
            UISystem.Roots.Remove(Canvas);
        }

        public override void OnUpdate(float dt)
        {

        }

    }
}
