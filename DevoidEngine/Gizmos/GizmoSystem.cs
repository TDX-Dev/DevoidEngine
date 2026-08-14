using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Rendering;
using System.Numerics;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoSystem
    {
        private GizmoContext? HoveredContext;
        private GizmoContext? CapturedContext;

        public void Initialize()
        {
            Engine.InputSystem.Router.Push(new GizmoInputLayer());
        }

        public void Update(float deltaTime, List<Viewport> viewports)
        {
            //foreach (var viewport in viewports)
            //{
            //    //GizmoContext context = viewport.GizmoContext;

            //    // Per-frame gizmo logic can go here later.
            //}
        }

        public void MouseMove(Vector2 globalMouse)
        {
            List<Viewport> viewports = Engine.Instance.ViewportManager.GetViewports();

            if (CapturedContext != null)
            {
                ProcessMouseMove(
                    CapturedContext,
                    CapturedContext.Viewport,
                    globalMouse);

                return;
            }

            for (int i = viewports.Count - 1; i >= 0; i--)
            {
                Viewport viewport = viewports[i];

                if (!viewport.Bounds.Contains(globalMouse))
                    continue;

                ProcessMouseMove(
                    viewport.GizmoContext,
                    viewport,
                    globalMouse);

                return;
            }

            // Mouse isn't over any viewport.
            foreach (Viewport viewport in viewports)
            {
                GizmoContext context = viewport.GizmoContext;

                if (context.HotHit.HasValue)
                    context.HotHit = null;
            }

            HoveredContext = null;
        }

        private void ProcessMouseMove(
            GizmoContext context,
            Viewport viewport,
            Vector2 globalMouse)
        {
            Vector2 mouse =
                globalMouse - viewport.Bounds.Position;

            context.MouseDelta =
                mouse - context.MousePosition;

            context.MousePosition = mouse;

            // Don't change the selected handle while dragging.
            if (context.ActiveHit.HasValue)
            {
                context.ActiveHit.Value.Gizmo.OnDrag(
                    context,
                    context.ActiveHit.Value);

                return;
            }

            GizmoHit? previous = context.HotHit;

            context.HotHit = FindHit(
                context,
                mouse);

            if (previous?.Gizmo != context.HotHit?.Gizmo ||
                previous?.Handle != context.HotHit?.Handle)
            {
                // Handle hover transition here later if desired.
            }

            if (context.HotHit.HasValue)
                HoveredContext = context;
            else
                HoveredContext = null;
        }

        public bool MouseDown(Vector2 globalMouse)
        {
            GizmoContext? context =
                GetTargetContext(globalMouse);

            if (context == null)
                return false;

            if (!context.HotHit.HasValue)
                return false;

            GizmoHit hit = context.HotHit.Value;

            context.ActiveHit = hit;
            CapturedContext = context;

            hit.Gizmo.OnBeginDrag(
                context,
                hit);

            return true;
        }

        public bool MouseUp(Vector2 globalMouse)
        {
            GizmoContext? context =
                CapturedContext ?? GetTargetContext(globalMouse);

            if (context == null ||
                !context.ActiveHit.HasValue)
            {
                return false;
            }

            GizmoHit hit = context.ActiveHit.Value;

            hit.Gizmo.OnEndDrag(
                context,
                hit);

            context.ActiveHit = null;
            context.MouseDelta = Vector2.Zero;

            CapturedContext = null;

            return true;
        }

        private GizmoHit? FindHit(
            GizmoContext context,
            Vector2 mouse)
        {
            GizmoHit? bestHit = null;

            foreach (Gizmo gizmo in context.Gizmos)
            {
                if (!gizmo.Enabled)
                    continue;

                if (!gizmo.HitTest(
                        context,
                        mouse,
                        out GizmoHit hit))
                {
                    continue;
                }

                if (!bestHit.HasValue ||
                    hit.Distance < bestHit.Value.Distance)
                {
                    bestHit = hit;
                }
            }

            return bestHit;
        }

        private GizmoContext? GetTargetContext(
            Vector2 globalMouse)
        {
            if (CapturedContext != null)
                return CapturedContext;

            List<Viewport> viewports = Engine.Instance.ViewportManager.GetViewports();

            for (int i = viewports.Count - 1; i >= 0; i--)
            {
                Viewport viewport = viewports[i];

                if (viewport.Bounds.Contains(globalMouse))
                    return viewport.GizmoContext;
            }

            return null;
        }

        public void Draw(GizmoContext context)
        {
            context.DrawList.Clear();

            foreach (Gizmo gizmo in context.Gizmos)
            {
                if (!gizmo.Enabled)
                    continue;

                gizmo.Draw(context);
            }
        }
    }
}