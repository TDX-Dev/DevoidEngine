using DevoidEngine.Components;
using DevoidEngine.Core;
using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering
{
    public class Viewport
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Camera3D? Camera3D { get; private set; }

        public List<Camera3D> Camera3Ds { get; private set; }
        public Texture? OutputTexture = null!;



        public Viewport()
        {
            Console.WriteLine("Root Viewport Created");

            Camera3Ds = [];

            Engine.Renderer.RegisterViewport(this);
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
