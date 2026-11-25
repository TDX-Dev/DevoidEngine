using DevoidEngine.Core;
using DevoidEngine.Nodes;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;

namespace DevoidEngine.Rendering
{
    public class Viewport : Node, IDisposable
    {
        public bool CanRender { get; set; } = true;
        public Rect Bounds => _bounds;
        public int Width { get; private set; }
        public int Height { get; private set; }
        // This will cause a problem where the viewport only targets the main camera, so having multiple cameras for splitscreen for example, will not work.
        public Camera? ActiveCamera => CameraOverride ?? _activeCamera; public Scene TargetScene { get; set; } = null!;
        public Camera? CameraOverride { get; set; }

        public Texture? OutputTexture { get; private set; } = null;

        private Rect _bounds;
        private ObjectPickRequest? _pendingPick;
        private ObjectPickResult? _completedPick;
        private readonly List<Camera> _cameras = [];
        private Camera? _activeCamera;


        public Viewport(int width = 1280, int height = 720)
        {
            Width = width;
            Height = height;

            Engine.Instance.ViewportManager.RegisterViewport(this);
            Engine.Renderer.RegisterViewport(this);

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

            Engine.Renderer.ResizeViewport(this);
        }
        public void Dispose()
        {
            Engine.Renderer.RemoveViewport(this);
            Engine.Instance.ViewportManager.UnregisterViewport(this);

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

        internal void RegisterCamera(Camera camera)
        {
            if (_cameras.Contains(camera))
                return;

            _cameras.Add(camera);

            // Automatically select the first registered camera.
            if (_activeCamera == null)
                SetActiveCamera(camera);
        }

        internal void UnregisterCamera(Camera camera)
        {
            _cameras.Remove(camera);

            if (_activeCamera == camera)
            {
                SetActiveCamera(null);
                MakeNextCameraCurrent();
            }
        }

        public void SetActiveCamera(Camera? camera)
        {
            if (_activeCamera == camera)
                return;

            if (camera != null && !_cameras.Contains(camera))
                throw new InvalidOperationException("Camera is not registered with this viewport.");
            _activeCamera = camera;
        }

        private void MakeNextCameraCurrent(Camera? exclude = null)
        {
            foreach (Camera camera in _cameras)
            {
                if (camera == exclude)
                    continue;

                if (_activeCamera != null)
                    return;

                SetActiveCamera(camera);
            }
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