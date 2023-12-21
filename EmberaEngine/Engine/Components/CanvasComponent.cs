using EmberaEngine.Engine.Core;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Rendering;

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

            Matrix4 projection = Graphics.CreateOrthographicCenter(-(scaleWidth * ReferenceWidth/2) + ReferenceWidth/2,  scaleWidth * ReferenceWidth/2 + ReferenceWidth/2, -(scaleHeight * ReferenceHeight/2) + ReferenceHeight/2, scaleHeight * ReferenceHeight/2 + ReferenceHeight/2, 1f, -1f);

            return projection;
        }

    }


}
