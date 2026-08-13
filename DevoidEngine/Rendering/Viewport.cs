using Assimp;
using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.UI;
using DevoidEngine.Util;
using DevoidGPU;
using Microsoft.VisualBasic;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class Viewport : IDisposable
    {
        public Rect Bounds => bounds;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Camera3D? Camera3D { get; private set; }
        public UIContext UIContext { get; private set; }
        public GizmoContext GizmoContext { get; private set; }

        public List<Camera3D> Camera3Ds { get; private set; }
        public Texture? OutputTexture = null!;

        private Rect bounds;

        public Viewport()
        {

            Camera3Ds = [];
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

            OutputTexture = Texture.Create2D(Width, Height, TextureFormat.RGBA16_Float, TextureUsage.ShaderResource | TextureUsage.RenderTarget);
        }

        public bool AddCamera3D(Camera3D camera)
        {
            Camera3Ds.Add(camera);
            return Camera3Ds.Count == 1;
        }

        public void SetCamera3D(Camera3D camera)
        {
            Camera3D = camera;
        }

        public void Resize(int width, int height)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            Width = width;
            Height = height;

            bounds = new Rect(Vector2.Zero, new Vector2(Width, Height));

            OutputTexture?.Dispose();

            OutputTexture = Texture.Create2D(width, height, TextureFormat.RGBA16_Float, TextureUsage.ShaderResource | TextureUsage.RenderTarget);

            Engine.Renderer.ResizeViewport(this);
        }

        public void Dispose()
        {
            Engine.Renderer.RemoveViewport(this);
            OutputTexture?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
