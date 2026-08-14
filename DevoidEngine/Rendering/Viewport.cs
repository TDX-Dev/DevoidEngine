using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.UI;
using DevoidEngine.Util;
using DevoidGPU;
using System;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class Viewport : IDisposable
    {
        public Rect Bounds => bounds;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Camera? ActiveCamera => CameraOverride ?? TargetScene?.MainCamera?.GetCamera();

        public Scene TargetScene { get; set; } = null!;
        public UIContext UIContext { get; private set; }
        public GizmoContext GizmoContext { get; private set; }

        public Camera? CameraOverride { get; set; }

        public Texture? OutputTexture { get; private set; } = null;

        private Rect bounds;

        public Viewport(int width = 1280, int height = 720)
        {
            Width = width;
            Height = height;

            UIContext = new()
            {
                Viewport = this,
            };

            GizmoContext = new()
            {
                Viewport = this,
            };

            Engine.Renderer.RegisterViewport(this);

            bounds = new Rect(Vector2.Zero, new Vector2(Width, Height));

            Width = Math.Max(1, Width);
            Height = Math.Max(1, Height);

            ReallocateTexture();
        }

        private void ReallocateTexture()
        {
            if (OutputTexture != null)
            {
                OutputTexture.Dispose();
                OutputTexture = null;
            }

            OutputTexture = Texture.Create2D(
                Width,
                Height,
                TextureFormat.RGBA16_Float,
                TextureUsage.ShaderResource | TextureUsage.RenderTarget
            );
        }

        public void Resize(int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            if (Width == width && Height == height)
                return;

            Width = width;
            Height = height;

            bounds = new Rect(Vector2.Zero, new Vector2(Width, Height));

            ReallocateTexture();

            Engine.Renderer.ResizeViewport(this);
        }

        public void Dispose()
        {
            Engine.Renderer.RemoveViewport(this);

            if (OutputTexture != null)
            {
                Engine.Instance.TextureManager.Unregister(OutputTexture);
                OutputTexture.Dispose();
                OutputTexture = null;
            }

            GC.SuppressFinalize(this);
        }
    }
}