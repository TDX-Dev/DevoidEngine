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
        public int PixelsPerUnit = 10;

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
                Matrix4x4 flipY = Matrix4x4.CreateScale(1f, -1f, 1f);
                Matrix4x4 scale = Matrix4x4.CreateScale(1f / PixelsPerUnit);

                // shift UI so center becomes pivot
                Vector2 canvasSize = Canvas.DesiredSize; // or manually track root size
                Matrix4x4 centerOffset =
                    Matrix4x4.CreateTranslation(
                        -canvasSize.X * 0.5f,
                        -canvasSize.Y * 0.5f,
                        0f);

                Matrix4x4 world =
                    centerOffset *
                    flipY *
                    scale *
                    gameObject.transform.WorldMatrix;

                Canvas.Render(viewData.renderItems3D, world);
            }
        }

        public override void OnStart()
        {
            UISystem.Roots.Add(Canvas);
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
