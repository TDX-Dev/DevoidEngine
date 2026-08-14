using DevoidEngine.Core;
using DevoidEngine.Rendering;
using ImGuiNET;
using System;
using System.Numerics;

namespace Elemental.Panels
{
    public abstract class ViewportPanel : Panel
    {
        public Viewport Viewport { get; protected set; }
        public Vector2 PanelSize { get; private set; } = Vector2.Zero;
        public Vector2 LocalMousePos { get; private set; } = Vector2.Zero;

        public bool IsHovered { get; private set; }
        public bool IsFocused { get; private set; }

        protected ViewportPanel(string title, Scene activeScene) : base(title)
        {
            Viewport = new Viewport(1280, 720);
            Engine.Instance.ViewportManager.RegisterViewport(Viewport);
        }

        protected override bool OnBeginWindow()
        {
            ImGui.PushStyleVar(
                ImGuiStyleVar.WindowPadding,
                new Vector2(0, 0));

            bool visible = base.OnBeginWindow();

            IsHovered = visible && ImGui.IsWindowHovered();
            IsFocused = visible && ImGui.IsWindowFocused();

            return visible;
        }

        protected override void OnImGuiRender()
        {
            // 1. Detect dynamic panel resize and update Viewport
            Vector2 contentSize = ImGui.GetContentRegionAvail();
            if (contentSize.X != PanelSize.X || contentSize.Y != PanelSize.Y)
            {
                PanelSize = contentSize;
                if (PanelSize.X > 0 && PanelSize.Y > 0)
                {
                    Viewport.Resize((int)PanelSize.X, (int)PanelSize.Y);
                }
            }

            // 2. Render output texture inside ImGui using TextureManager ID
            if (Viewport.OutputTexture != null)
            {
                ulong managerId = Engine.Instance.TextureManager.GetId(Viewport.OutputTexture);

                ImGui.Image(
                    (IntPtr)managerId,
                    PanelSize,
                    new Vector2(0, 1),
                    new Vector2(1, 0));
            }

            // 3. Compute relative mouse position inside the viewport
            Vector2 imageMin = ImGui.GetItemRectMin();
            LocalMousePos = ImGui.GetMousePos() - imageMin;

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