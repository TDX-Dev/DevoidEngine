using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class Viewport : IDisposable
    {
        public bool Render { get; set; } = true;
        public Rect Bounds => _bounds;
        public int Width { get; private set; }
        public int Height { get; private set; }
        // This will cause a problem where the viewport only targets the main camera, so having multiple cameras for splitscreen for example, will not work.
        public Camera? ActiveCamera => CameraOverride;// ?? TargetScene?.MainCamera?.GetCamera();
        //public Scene TargetScene { get; set; } = null!;
        public Camera? CameraOverride { get; set; }

        public Texture? OutputTexture { get; private set; } = null;

        private Rect _bounds;
        private ObjectPickRequest? _pendingPick;
        private ObjectPickResult? _completedPick;

        public Viewport(int width = 1280, int height = 720)
        {
            Width = width;
            Height = height;

            //Engine.Renderer.RegisterViewport(this);

            _bounds = new Rect(Vector2.Zero, new Vector2(Width, Height));

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
                TextureFormat.RGBA16Float,
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

            _bounds = new Rect(Vector2.Zero, new Vector2(Width, Height));

            ReallocateTexture();

            //Engine.Renderer.ResizeViewport(this);
        }
        public void Dispose()
        {
            //Engine.Renderer.RemoveViewport(this);

            if (OutputTexture != null)
            {
                Engine.Instance.TextureManager.Unregister(OutputTexture);
                OutputTexture.Dispose();
                OutputTexture = null;
            }

            GC.SuppressFinalize(this);
        }


        public void RequestObjectPick(Vector2 location)
        {
            _pendingPick = new ObjectPickRequest(location);
        }

        public bool TryGetObjectPick(out ObjectPickResult result)
        {
            if (_completedPick.HasValue)
            {
                result = _completedPick.Value;
                _completedPick = null;
                return true;
            }

            result = default;
            return false;
        }

        internal bool TryConsumeObjectPickRequest(out ObjectPickRequest request)
        {
            if (_pendingPick.HasValue)
            {
                request = _pendingPick.Value;
                _pendingPick = null;
                return true;
            }

            request = default;
            return false;
        }

        internal void CompleteObjectPick(ObjectPickResult result)
        {
            _completedPick = result;
        }
    }
}