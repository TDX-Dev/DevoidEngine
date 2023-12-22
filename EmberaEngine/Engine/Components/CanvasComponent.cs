using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Rendering;
using EmberaEngine.Engine.Utilities;

namespace EmberaEngine.Engine.Components
{
    public enum CanvasScaleMode
    {
        ScaleWithScreen,
        ConstantSize,
    }

    public class CanvasComponent : Component
    {
        public override string Type => nameof(CanvasComponent);

        public int ReferenceWidth = 800;
        public int ReferenceHeight = 600;

        public CanvasScaleMode ScaleMode { get; set; } = CanvasScaleMode.ScaleWithScreen;

        private int prevWidth;
        private int prevHeight;

        private float scaledWidth;
        private float scaledHeight;
        private float scaledX;
        private float scaledY;

        public Vector2 innerMousePos;

        public Matrix4 OrthoGraphicProjection = Matrix4.Identity;
        public RenderCanvas canvas = new RenderCanvas();


        public override void OnStart()
        {


            OrthoGraphicProjection = Graphics.CreateOrthographic2D(ReferenceWidth, ReferenceHeight, -1f, 1f);

            canvas.Projection = OrthoGraphicProjection;

            SpriteManager.AddRenderCanvas(canvas);
        }

        public override void OnUpdate(float dt)
        {

            if (ScaleMode == CanvasScaleMode.ScaleWithScreen && (prevHeight != Screen.Size.Y || prevWidth != Screen.Size.X))
            {
                prevHeight = Screen.Size.Y;
                prevWidth = Screen.Size.X;

                OrthoGraphicProjection = CalculateScaleWithScreen();
                canvas.Projection = OrthoGraphicProjection;
            }
            CalculateMouseScaleWithScreen();
        }

        void CalculateMouseScaleWithScreen()
        {
            innerMousePos.X = (((Input.GetMousePos().X) * (scaledWidth - scaledX)) / (Screen.Size.X)) + scaledX;
            innerMousePos.Y = (((Input.GetMousePos().Y) * (scaledHeight - scaledY)) / (Screen.Size.Y)) + scaledY;
        }

        Matrix4 CalculateScaleWithScreen()
        {

            float deviceRatio = (float)Screen.Size.X / Screen.Size.Y;
            float virtualRatio = (float)ReferenceWidth / ReferenceHeight;

            float scaleWidth;
            float scaleHeight;

            if (deviceRatio > virtualRatio)
            {
                // The window is wider than the desired aspect ratio
                scaleWidth = deviceRatio / virtualRatio;
                scaleHeight = 1.0f;
            }
            else
            {
                // The window is taller than the desired aspect ratio
                scaleWidth = 1.0f;
                scaleHeight = virtualRatio / deviceRatio;
            }

            float left = -(scaleWidth * ReferenceWidth / 2) + ReferenceWidth / 2;
            float right = scaleWidth * ReferenceWidth / 2 + ReferenceWidth / 2;
            float bottom = -(scaleHeight * ReferenceHeight / 2) + ReferenceHeight / 2;
            float top = scaleHeight * ReferenceHeight / 2 + ReferenceHeight / 2;


            Matrix4 projection = Graphics.CreateOrthographicCenter(left, right, bottom, top, 1f, -1f);


            scaledWidth = right;
            scaledHeight = bottom;
            scaledX = left;
            scaledY = scaleHeight * ReferenceHeight / 2 + ReferenceHeight / 2;

            Console.WriteLine(new Vector4(scaledX, scaledY, scaledWidth, scaledHeight));

            return projection;
        }

    }


}
