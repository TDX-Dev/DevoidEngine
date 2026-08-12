using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using DevoidEngine.InputSystem.InputDevices;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Gizmos
{
    public sealed class GizmoSystem
    {
        private readonly Dictionary<Viewport, GizmoContext> contexts = [];

        public GizmoContext GetContext(Viewport viewport)
        {
            if (!contexts.TryGetValue(
                    viewport,
                    out GizmoContext? context))
            {
                if (viewport.Camera3D == null)
                {
                    throw new InvalidOperationException(
                        "Cannot create a GizmoContext without a Camera3D.");
                }

                context = new GizmoContext(
                    viewport,
                    viewport.Camera3D.GetCamera());

                contexts.Add(viewport, context);
            }

            return context;
        }

        public GizmoDrawList Draw(
            Viewport viewport,
            ReadOnlySpan<Gizmo> gizmos)
        {
            GizmoContext context = GetContext(viewport);

            context.ResetFrame();

            UpdateInput(context);
            UpdateInteraction(context, gizmos);

            foreach (Gizmo gizmo in gizmos)
            {
                gizmo.Draw(
                    context,
                    context.DrawList);
            }

            context.EndFrame();

            return context.DrawList;
        }

        private void UpdateInteraction(
            GizmoContext context,
            ReadOnlySpan<Gizmo> gizmos)
        {
            // Currently dragging a gizmo.
            if (context.ActiveGizmo != null)
            {
                context.ActiveGizmo.Drag(context);

                if (context.MouseReleased)
                {
                    context.ActiveGizmo.EndDrag(context);

                    context.ActiveGizmo = null;
                    context.ActiveHit = null;
                }

                return;
            }

            // Nothing is being dragged, so determine what is hovered.
            UpdateHover(
                context,
                gizmos);

            // Start dragging when the mouse is pressed.
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

        private void UpdateInput(
            GizmoContext context)
        {
            InputState input =
                Engine.InputSystem.State;

            Vector2 mousePosition = new(
                input.Get(
                    InputDeviceType.Mouse,
                    (ushort)MouseAxis.X),

                input.Get(
                    InputDeviceType.Mouse,
                    (ushort)MouseAxis.Y));

            Vector2 mouseDelta = new(
                input.Get(
                    InputDeviceType.Mouse,
                    (ushort)MouseAxis.DeltaX),

                input.Get(
                    InputDeviceType.Mouse,
                    (ushort)MouseAxis.DeltaY));

            bool mouseDown =
                input.Get(
                    InputDeviceType.Mouse,
                    (ushort)MouseButton.Left) != 0f;

            bool mousePressed =
                input.GetDown(
                    InputDeviceType.Mouse,
                    (ushort)MouseButton.Left);

            bool mouseReleased =
                input.GetUp(
                    InputDeviceType.Mouse,
                    (ushort)MouseButton.Left);

            Vector2 viewportPosition =
                mousePosition -
                context.Viewport.Bounds.Position;

            context.MousePosition =
                viewportPosition;

            context.MouseDelta =
                mouseDelta;

            context.MouseDown =
                mouseDown;

            context.MousePressed =
                mousePressed;

            context.MouseReleased =
                mouseReleased;
        }

        private void UpdateHover(
            GizmoContext context,
            ReadOnlySpan<Gizmo> gizmos)
        {
            context.HoveredGizmo = null;
            context.HoveredHit = null;

            if (gizmos.Length == 0)
                return;

            Viewport viewport =
                context.Viewport;

            if (viewport.Width <= 0 ||
                viewport.Height <= 0)
            {
                return;
            }

            Vector2 mouse =
                context.MousePosition;

            // Mouse is outside viewport.
            if (mouse.X < 0 ||
                mouse.Y < 0 ||
                mouse.X >= viewport.Width ||
                mouse.Y >= viewport.Height)
            {
                return;
            }

            Ray ray = CreateMouseRay(
                context.Camera,
                mouse,
                viewport.Width,
                viewport.Height);

            Gizmo? closestGizmo = null;
            GizmoHit closestHit = default;

            float closestDistance =
                float.MaxValue;

            for (int i = 0; i < gizmos.Length; i++)
            {
                Gizmo gizmo = gizmos[i];

                if (!gizmo.HitTest(
                        context,
                        ray,
                        out GizmoHit hit))
                {
                    continue;
                }

                if (hit.Distance < closestDistance)
                {
                    closestDistance =
                        hit.Distance;

                    closestGizmo =
                        gizmo;

                    closestHit =
                        hit;
                }
            }

            context.HoveredGizmo =
                closestGizmo;

            if (closestGizmo != null)
            {
                context.HoveredHit =
                    closestHit;
            }
        }

        private static Ray CreateMouseRay(
            Camera camera,
            Vector2 mousePosition,
            int width,
            int height)
        {
            float x =
                (mousePosition.X / width) * 2f - 1f;

            float y =
                1f -
                (mousePosition.Y / height) * 2f;

            Vector4 nearClip = new(
                x,
                y,
                0f,
                1f);

            Vector4 farClip = new(
                x,
                y,
                1f,
                1f);

            Vector4 nearWorld =
                Vector4.Transform(
                    nearClip,
                    camera.InverseViewProjection);

            Vector4 farWorld =
                Vector4.Transform(
                    farClip,
                    camera.InverseViewProjection);

            nearWorld /= nearWorld.W;
            farWorld /= farWorld.W;

            Vector3 origin = new(
                nearWorld.X,
                nearWorld.Y,
                nearWorld.Z);

            Vector3 direction =
                Vector3.Normalize(
                    new Vector3(
                        farWorld.X,
                        farWorld.Y,
                        farWorld.Z) -
                    origin);

            return new Ray(
                origin,
                direction);
        }

        public void RemoveViewport(
            Viewport viewport)
        {
            contexts.Remove(viewport);
        }
    }
}