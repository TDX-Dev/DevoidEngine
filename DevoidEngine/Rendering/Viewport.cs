using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidEngine.UI;
using DevoidEngine.Util;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public class Viewport
    {
        public Rect Bounds => bounds;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Camera3D? Camera3D { get; private set; }
        public UIContext UIContext { get; private set; }

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

            Engine.Renderer.RegisterViewport(this);
        
            bounds = new Rect(Vector2.Zero, new Vector2(Width, Height));
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

            Width = width;
            Height = height;

            bounds = new Rect(Vector2.Zero, new Vector2(Width, Height));

            OutputTexture?.Dispose();

            OutputTexture = Texture.Create2D(width, height, TextureFormat.RGBA16_Float, TextureUsage.ShaderResource | TextureUsage.RenderTarget);

            Engine.Renderer.ResizeViewport(this);
        }

        ~Viewport()
        {
            Engine.Renderer.RemoveViewport(this);
        }
    }
}
