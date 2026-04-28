using DevoidEngine.Util;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DevoidEngine.Core
{
    public struct WindowSpecification
    {
        public string Title;

        public int Width;
        public int Height;

        public int MinWidth;
        public int MinHeight;

        public bool Resizable;
        public bool StartVisible;
        public bool StartFocused;

        public bool Transparency;
    }
    public class Window : NativeWindow
    {
        public IntPtr Handle => GetWindowHandle();

        public event Action<int, int>? OnWindowResize;

        public Window(WindowSpecification specification) : base(new NativeWindowSettings()
        {
            API = ContextAPI.NoAPI,
            AutoLoadBindings = false,

            StartVisible = specification.StartVisible,
            StartFocused = specification.StartFocused,
            IsEventDriven = false,
            TransparentFramebuffer = specification.Transparency,

            ClientSize = new Vector2i(specification.Width, specification.Height),
            MinimumClientSize = new Vector2i(specification.MinWidth, specification.MinHeight),

            WindowBorder = specification.Resizable ? WindowBorder.Resizable : WindowBorder.Fixed,

            Title = specification.Title,

        }
        )
        {
            WindowUtil.EnableDarkMode(Handle);
        }

        public void PumpEvents() => ProcessEvents(0);

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            OnWindowResize?.Invoke(e.Width, e.Height);
        }

        public IntPtr GetWindowHandle()
        {
            unsafe
            {
                return GLFW.GetWin32Window(WindowPtr);
            }
        }
    }
}
