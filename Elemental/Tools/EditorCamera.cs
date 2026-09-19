using DevoidEngine.Core;
using ImGuiNET;
using System;
using System.Numerics;

namespace Elemental.Tools
{
    public class EditorCamera
    {
        public Camera Camera { get; }

        public bool CanInteract { get; set; }
        public bool IsInteracting { get; private set; }

        public Vector3 OrbitTarget { get; set; }
        public float OrbitDistance { get; set; } = 10.0f;

        public float OrbitSensitivity { get; set; } = 0.005f;
        public float PanSensitivity { get; set; } = 0.0025f;
        public float ZoomSensitivity { get; set; } = 1.0f;
        public float LookSensitivity { get; set; } = 0.005f;

        public float MoveSpeed { get; set; } = 5.0f;
        public float FastMoveMultiplier { get; set; } = 4.0f;

        public float MinOrbitDistance { get; set; } = 0.05f;
        public float MaxOrbitDistance { get; set; } = 10000.0f;

        private float orbitYaw;
        private float orbitPitch;

        private float freeYaw;
        private float freePitch;

        private bool wasOrbiting;
        private bool wasPanning;
        private bool wasLooking;

        private bool ignoreNextMouseDelta;

        public EditorCamera()
        {
            Camera = new Camera();

            OrbitTarget = Vector3.Zero;
            OrbitDistance = 10.0f;

            UpdateOrbitCamera();

            // Both camera modes start from the same state.
            freeYaw = orbitYaw;
            freePitch = orbitPitch;
        }

        public void Update(EditorContext context, float deltaTime, float wheel)
        {
            if (!CanInteract)
            {
                IsInteracting = false;

                wasOrbiting = false;
                wasPanning = false;
                wasLooking = false;

                Engine.Cursor.SetCursorState(CursorState.Normal);

                return;
            }

            bool middle = ImGui.IsMouseDown(ImGuiMouseButton.Middle);

            bool right = ImGui.IsMouseDown(ImGuiMouseButton.Right);

            bool shift = ImGui.GetIO().KeyShift;

            bool orbiting = middle && !shift;

            bool panning = middle && shift;

            bool looking = right;

            IsInteracting = orbiting || panning || looking;

            bool modeChanged = orbiting != wasOrbiting || panning != wasPanning || looking != wasLooking;

            if (modeChanged)
            {
                ignoreNextMouseDelta = true;
            }

            // RMB was just pressed.
            if (looking && !wasLooking)
            {
                Engine.Cursor.SetCursorState(CursorState.Grabbed);
            }

            // RMB was just released.
            if (!looking && wasLooking)
            {
                Engine.Cursor.SetCursorState(CursorState.Normal);
            }

            Vector2 mouseDelta = GetMouseDelta();

            if (IsInteracting)
            {
                if (orbiting)
                {
                    Orbit(mouseDelta);
                }
                else if (panning)
                {
                    Pan(mouseDelta);
                }
                else if (looking)
                {
                    Look(mouseDelta);
                    Move(deltaTime);
                }
            }

            if (!looking && MathF.Abs(wheel) > float.Epsilon)
            {
                Zoom(wheel);
            }

            wasOrbiting = orbiting;
            wasPanning = panning;
            wasLooking = looking;
        }

        private void Orbit(Vector2 delta)
        {
            orbitYaw += delta.X * OrbitSensitivity;
            orbitPitch += delta.Y * OrbitSensitivity;

            UpdateOrbitCamera();

            freeYaw = orbitYaw;
            freePitch = orbitPitch;
        }

        private void Pan(Vector2 delta)
        {
            float scale =
                OrbitDistance *
                PanSensitivity;

            OrbitTarget +=
                Camera.Right *
                delta.X *
                scale;

            OrbitTarget +=
                Camera.Up *
                delta.Y *
                scale;

            UpdateOrbitCamera();

            /*
             * Pan does not change orientation, but explicitly
             * keeping the free state synchronized makes the
             * relationship between both modes obvious.
             */
            freeYaw = orbitYaw;
            freePitch = orbitPitch;
        }

        private void Zoom(float wheel)
        {
            OrbitDistance *=
                MathF.Pow(
                    0.85f,
                    wheel * ZoomSensitivity);

            OrbitDistance =
                Math.Clamp(
                    OrbitDistance,
                    MinOrbitDistance,
                    MaxOrbitDistance);

            /*
             * Zoom changes the orbit camera position. The free
             * camera orientation remains the same.
             */
            UpdateOrbitCamera();

        }

        private void Look(Vector2 delta)
        {
            freeYaw += delta.X * LookSensitivity;
            freePitch += delta.Y * LookSensitivity;

            UpdateFreeCamera();
        }

        private void Move(float deltaTime)
        {
            Vector3 movement =
                Vector3.Zero;

            if (ImGui.IsKeyDown(ImGuiKey.W))
                movement += Camera.Front;

            if (ImGui.IsKeyDown(ImGuiKey.S))
                movement -= Camera.Front;

            if (ImGui.IsKeyDown(ImGuiKey.D))
                movement -= Camera.Right;

            if (ImGui.IsKeyDown(ImGuiKey.A))
                movement += Camera.Right;

            if (ImGui.IsKeyDown(ImGuiKey.E))
                movement += Vector3.UnitY;

            if (ImGui.IsKeyDown(ImGuiKey.Q))
                movement -= Vector3.UnitY;

            if (movement.LengthSquared() <
                float.Epsilon)
            {
                /*
                 * Even if the camera did not move this frame,
                 * keep the orbit state synchronized with the
                 * current free-camera orientation and position.
                 */
                SyncOrbitState();

                return;
            }

            movement =
                Vector3.Normalize(movement);

            float speed =
                MoveSpeed;

            if (ImGui.GetIO().KeyShift)
                speed *= FastMoveMultiplier;

            Vector3 position =
                Camera.Position +
                movement *
                speed *
                deltaTime;

            Camera.UpdateView(
                position,
                Camera.Front,
                Camera.Up);

            /*
             * This is the important part.
             *
             * The old camera did:
             *
             * FocalPoint = Position + Forward * Distance;
             *
             * We do the same thing here, but also keep the orbit
             * quaternion synchronized.
             */
            SyncOrbitState();
        }

        private void UpdateOrbitCamera()
        {
            Quaternion rotation =
                Quaternion.CreateFromYawPitchRoll(
                    orbitYaw,
                    orbitPitch,
                    0.0f);

            Vector3 front =
                Vector3.Normalize(
                    Vector3.Transform(
                        Vector3.UnitZ,
                        rotation));

            Vector3 up =
                Vector3.Normalize(
                    Vector3.Transform(
                        Vector3.UnitY,
                        rotation));

            Vector3 position =
                OrbitTarget -
                front * OrbitDistance;

            Camera.UpdateView(
                position,
                front,
                up);
        }

        private void UpdateFreeCamera()
        {
            Quaternion rotation =
                Quaternion.CreateFromYawPitchRoll(
                    freeYaw,
                    freePitch,
                    0.0f);

            Vector3 front =
                Vector3.Normalize(
                    Vector3.Transform(
                        Vector3.UnitZ,
                        rotation));

            Vector3 up =
                Vector3.Normalize(
                    Vector3.Transform(
                        Vector3.UnitY,
                        rotation));

            Camera.UpdateView(
                Camera.Position,
                front,
                up);

            SyncOrbitState();
        }


        private void SyncOrbitState()
        {
            OrbitTarget =
                Camera.Position +
                Camera.Front *
                OrbitDistance;

            orbitYaw = freeYaw;
            orbitPitch = freePitch;
        }

        public void Focus(
            Vector3 position,
            float distance = 5.0f)
        {
            OrbitTarget = position;

            OrbitDistance =
                Math.Clamp(
                    distance,
                    MinOrbitDistance,
                    MaxOrbitDistance);

            UpdateOrbitCamera();

            /*
             * Focus establishes a new shared camera state.
             */
            freeYaw = orbitYaw;
            freePitch = orbitPitch;
        }

        private Vector2 GetMouseDelta()
        {
            if (ignoreNextMouseDelta)
            {
                ignoreNextMouseDelta = false;
                return Vector2.Zero;
            }

            return ImGui.GetIO().MouseDelta;
        }

        public void NotifyMouseWarp()
        {
            ignoreNextMouseDelta = true;
        }
    }
}