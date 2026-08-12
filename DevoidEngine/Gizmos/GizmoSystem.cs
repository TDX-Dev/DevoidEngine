using DevoidEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoSystem
    {
        private readonly Dictionary<Viewport, GizmoContext> contexts = new();

        private readonly GizmoRenderer renderer;

        public GizmoSystem(GizmoRenderer renderer)
        {
            this.renderer = renderer;
        }

        public void Draw(
            Viewport viewport,
            ReadOnlySpan<Gizmo> gizmos)
        {
            GizmoContext context = GetContext(viewport);

            UpdateInput(context);

            if (context.ActiveGizmo != null)
            {
                context.ActiveGizmo.Drag(context);

                if (context.MouseReleased)
                {
                    context.ActiveGizmo.EndDrag(context);

                    context.ActiveGizmo = null;
                    context.ActiveHit = null;
                    context.DragState = null;
                }
            }
            else
            {
                UpdateHover(context, gizmos);

                if (context.MousePressed &&
                    context.HoveredGizmo != null &&
                    context.HoveredHit.HasValue)
                {
                    context.ActiveGizmo =
                        context.HoveredGizmo;

                    context.ActiveHit =
                        context.HoveredHit;

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

            renderer.Render(
                viewport,
                drawList);
        }

        private GizmoContext GetContext(
            Viewport viewport)
        {
            if (!contexts.TryGetValue(
                    viewport,
                    out GizmoContext? context))
            {
                context = new GizmoContext(
                    viewport,
                    viewport.Camera3D!.GetCamera());

                contexts.Add(
                    viewport,
                    context);
            }

            return context;
        }
    }
}
