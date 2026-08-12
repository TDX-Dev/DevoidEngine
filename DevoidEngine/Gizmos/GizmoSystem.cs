using DevoidEngine.Rendering;
using System;
using System.Collections.Generic;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoSystem
    {
        private readonly Dictionary<Viewport, GizmoContext> contexts = new();

        public GizmoSystem()
        {
        }

        public GizmoDrawList Draw(
            Viewport viewport,
            ReadOnlySpan<Gizmo> gizmos)
        {
            GizmoContext context = GetContext(viewport);

            // Input/interaction will happen here.
            UpdateInput(context);

            if (context.ActiveGizmo != null)
            {
                context.ActiveGizmo.Drag(context);

                if (context.MouseReleased)
                {
                    context.ActiveGizmo.EndDrag(context);

                    context.ActiveGizmo = null;
                    context.ActiveHit = null;
                }
            }
            else
            {
                UpdateHover(context, gizmos);

                if (context.MousePressed &&
                    context.HoveredGizmo != null &&
                    context.HoveredHit.HasValue)
                {
                    context.ActiveGizmo = context.HoveredGizmo;
                    context.ActiveHit = context.HoveredHit;

                    context.ActiveGizmo.BeginDrag(
                        context,
                        context.ActiveHit.Value);
                }
            }

            GizmoDrawList drawList = new();

            foreach (Gizmo gizmo in gizmos)
            {
                gizmo.Draw(
                    context,
                    drawList);
            }

            return drawList;
        }

        private GizmoContext GetContext(Viewport viewport)
        {
            if (!contexts.TryGetValue(
                    viewport,
                    out GizmoContext? context))
            {
                context = new GizmoContext(
                    viewport,
                    viewport.Camera3D!.GetCamera());

                contexts.Add(viewport, context);
            }

            return context;
        }

        private void UpdateInput(GizmoContext context)
        {
            // TODO: connect this to your actual input system.
        }

        private void UpdateHover(
            GizmoContext context,
            ReadOnlySpan<Gizmo> gizmos)
        {
            // TODO: raycast against gizmos using context.Camera.
        }
    }
}