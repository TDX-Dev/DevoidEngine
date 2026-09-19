using DevoidEngine.Core;
using DevoidEngine.Rendering;
using ImGuiNET;
using System.Numerics;

namespace Elemental.Tools.Panels
{
    public class ViewportPanel : Panel
    {
        public Viewport Viewport { get; protected set; } = null!;
        public Vector2 PanelSize { get; private set; }
        public Vector2 LocalMousePosition { get; private set; }

        public bool IsHovered { get; private set; }
        public bool IsFocused { get; private set; }

        private Vector2 pendingSize;
        private Vector2 allocatedSize;
        private int resizeFrames;

        public ViewportPanel(string title) : base(title)
        {
            Viewport = new Viewport();
            Engine.Instance.ViewportManager.RegisterViewport(Viewport);
        }

        protected override bool OnBeginWindow()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);

            bool visible = base.OnBeginWindow();

            Viewport.Render = visible;

            IsHovered = visible && ImGui.IsWindowHovered();
            IsFocused = visible && ImGui.IsWindowFocused();

            return visible;
        }

        protected override void OnImGuiRender(EditorContext context)
        {
            Vector2 contentSize = ImGui.GetContentRegionAvail();

            if (contentSize.X > 0 && contentSize.Y > 0)
            {
                PanelSize = contentSize;

                if (contentSize != pendingSize)
                {
                    pendingSize = contentSize;
                    resizeFrames = 0;
                }
                else
                {
                    resizeFrames++;
                }

                if (resizeFrames >= 2 && pendingSize != allocatedSize)
                {
                    allocatedSize = pendingSize;

                    Viewport.Resize((int)allocatedSize.X, (int)allocatedSize.Y);
                }
            }

            // 2. Render output texture inside ImGui using TextureManager ID
            if (Viewport.ActiveCamera != null)
            {
                ulong managerId = Engine.Instance.TextureManager.GetId(Viewport.OutputTexture!);

                ImGui.Image((IntPtr)managerId, PanelSize, new Vector2(0, 0), new Vector2(1, 1));
            }

            // 3. Compute relative mouse position inside the viewport
            Vector2 imageMin = ImGui.GetItemRectMin();
            LocalMousePosition = ImGui.GetMousePos() - imageMin;

            OnViewportOverlayRender();
        }

        protected virtual void OnViewportOverlayRender() { }

        protected override void OnEndWindow()
        {
            base.OnEndWindow();
            ImGui.PopStyleVar(); // Balance PushStyleVar
        }

        public override void OnDetach()
        {
            Viewport?.Dispose();
        }
    }
}
