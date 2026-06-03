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
        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public Camera3D? Camera3D { get; private set; }

        public List<Camera3D> Camera3Ds { get; private set; }
        public Texture? OutputTexture = null!;



        public Viewport()
        {
            Console.WriteLine("Root Viewport Created");

            Camera3Ds = [];
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
            Width = (uint)width;
            Height = (uint)height;

            OutputTexture?.Dispose();

            OutputTexture = Texture.Create2D(width, height, TextureFormat.RGBA16_Float, TextureUsage.ShaderResource | TextureUsage.RenderTarget);
        }
    }
}
