using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.UI;
using DevoidEngine.Engine.UI.Nodes;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{

    public class CanvasComponent : Component, IRenderComponent
    {
        public override string Type => nameof(CanvasComponent);

        public bool isEnabled = true;
        public CameraComponent3D? CameraConstraint;
        public CanvasRenderMode RenderMode
        {
            get => renderMode;
            set
            {
                renderMode = value;

                // its stored in canvas node for input projection.
                Canvas.RenderMode = value;
            }
        }
        public int PixelsPerUnit
        {
            get => pixelsperunit;
            set
            {
                pixelsperunit = value;
                Canvas.PixelsPerUnit = value;
            }
        }

        public Vector2 CanvasSize
        {
            get => canvasSize;
            set
            {
                canvasSize = value;
                if (IsInitialized)
                    Canvas.Size = canvasSize;
            }
        }

        public int Order = 0;

        private CanvasRenderMode renderMode = CanvasRenderMode.ScreenSpace;
        private int pixelsperunit = 300;
        private Vector2 canvasSize = Screen.Size;

        public CanvasNode Canvas = new CanvasNode()
        {
            Direction = FlexDirection.Row,
            Align = AlignItems.Center,
            Justify = JustifyContent.Center,
        };


        public Matrix4x4 GetCanvasModelMatrix()
        {
            if (RenderMode == CanvasRenderMode.ScreenSpace)
                return Matrix4x4.Identity;

            return gameObject.Transform.WorldMatrix;
        }

        public void Collect(CameraComponent3D camera, CameraRenderContext viewData)
        {
            if (!isEnabled) return;
            if (RenderMode == CanvasRenderMode.ScreenSpace)
            {
                if (CameraConstraint != null && CameraConstraint != camera)
                    return;

                Canvas.Render(viewData.renderItemsUI, Matrix4x4.Identity, Order);
            }
            else
            {
                Matrix4x4 flipY = Matrix4x4.CreateScale(1f, -1f, 1f);
                Matrix4x4 scale = Matrix4x4.CreateScale(1f / PixelsPerUnit);

                // shift UI so center becomes pivot
                Vector2 canvasSize = Canvas.Size.GetValueOrDefault(); // or manually track root size
                Matrix4x4 centerOffset =
                    Matrix4x4.CreateTranslation(
                        -canvasSize.X * 0.5f,
                        -canvasSize.Y * 0.5f,
                        0f);

                Matrix4x4 world =
                    centerOffset *
                    flipY *
                    scale *
                    gameObject.Transform.WorldMatrix;

                Canvas.Render(viewData.renderItems3D, world, Order);
            }
        }

        public override void OnStart()
        {
            Canvas.BlockInput = false;
            Canvas.RenderMode = renderMode;
            Canvas.PixelsPerUnit = pixelsperunit;
            Canvas.Size = canvasSize;
            UISystem.AddRoot(Canvas);
        }

        public override void OnDestroy()
        {
            UISystem.RemoveRoot(Canvas);
            Canvas.Dispose();
        }

        public override void OnUpdate(float dt)
        {
            Canvas.RenderMode = renderMode;
            Canvas.PixelsPerUnit = pixelsperunit;
            Canvas.Size = canvasSize;
            Canvas.WorldMatrix = gameObject.Transform.WorldMatrix;
            Canvas.WorldPosition = gameObject.Transform.Position;
            Canvas.WorldForward = gameObject.Transform.Forward;
            if (renderMode == CanvasRenderMode.WorldSpace)
            {
                Canvas.Size = canvasSize;
            }
        }

    }
}