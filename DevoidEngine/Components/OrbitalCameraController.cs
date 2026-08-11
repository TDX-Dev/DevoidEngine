using DevoidEngine.Core;
using DevoidEngine.InputSystem;
using System.Numerics;
using System;

namespace DevoidEngine.Components
{
    public class OrbitalCameraController : Component
    {
        public override string Type => nameof(OrbitalCameraController);

        // --------------------------------------------------------
        // Settings
        // --------------------------------------------------------

        public float OrbitSensitivity = 0.005f;
        public float PanSensitivity = 0.02f;
        public float ZoomSensitivity = 1.5f;

        public float MinDistance = 1.0f;
        public float MaxDistance = 100.0f;
        public float InitialDistance = 10.0f;

        // Input Actions - You will need to define these in your InputSystem
        // OrbitAction -> e.g., Middle Mouse Button
        // PanAction   -> e.g., Shift + Middle Mouse Button
        // ZoomAction  -> e.g., Scroll Wheel Delta
        public string OrbitAction = "Orbit";
        public string PanAction = "Pan";
        public string ZoomAction = "Zoom";
        public string LookXAction = "LookX";
        public string LookYAction = "LookY";

        // --------------------------------------------------------

        private GameObject pitchPivot = null!;
        private GameObject cameraObject = null!;
        private Camera3D camera = null!;

        private float yaw;
        private float pitch;
        private float distance;

        public override void OnAttach()
        {
            CreateCameraHierarchy();
        }

        public override void OnStart()
        {
            yaw = gameObject.Transform.EulerAngles.Y;
            pitch = pitchPivot.Transform.EulerAngles.X;
            distance = InitialDistance;

            // Apply initial setup
            UpdateCameraDistance();
        }

        private void CreateCameraHierarchy()
        {
            // Create a child pivot to handle the up/down tilt (Pitch)
            pitchPivot = gameObject.Scene!.AddGameObject($"{gameObject.Name}_PitchPivot");
            pitchPivot.SetParent(gameObject);

            // Create the camera object and attach it to the pitch pivot
            cameraObject = gameObject.Scene.AddGameObject($"{gameObject.Name}_Camera");
            cameraObject.SetParent(pitchPivot);

            camera = cameraObject.AddComponent<Camera3D>();
        }

        public override void OnUpdate(float dt)
        {
            float lookX = Engine.InputSystem.GetAction(LookXAction);
            float lookY = Engine.InputSystem.GetAction(LookYAction);


            bool isOrbitingButton = Engine.InputSystem.GetAction("Orbit") > 0.5f;
            bool isShiftPressed = Engine.InputSystem.GetAction("Sprint") > 0.5f;

            float zoomDelta = Engine.InputSystem.GetAction("Zoom");


            // 1. Handle Zooming (Scroll Wheel)
            if (MathF.Abs(zoomDelta) > 0.01f)
            {
                distance -= zoomDelta * ZoomSensitivity;
                distance = Math.Clamp(distance, MinDistance, MaxDistance);
                UpdateCameraDistance();
            }

            // 2. Blender Style: Shift + Middle Mouse = Pan
            if (isOrbitingButton && isShiftPressed)
            {
                float panX = -lookX * PanSensitivity;
                float panY = lookY * PanSensitivity;

                Vector3 right = pitchPivot.Transform.Right;
                Vector3 up = pitchPivot.Transform.Up;

                gameObject.Transform.Position += (right * panX) + (up * panY);
            }
            // 3. Middle Mouse alone = Orbit
            else if (isOrbitingButton)
            {
                yaw += lookX * OrbitSensitivity;
                pitch += lookY * OrbitSensitivity;

                pitch = Math.Clamp(
                    pitch,
                    -MathF.PI * 0.49f,
                     MathF.PI * 0.49f);

                gameObject.Transform.Rotation = Quaternion.CreateFromAxisAngle(
                    Vector3.UnitY,
                    yaw);

                pitchPivot.Transform.LocalRotation =
                    Quaternion.CreateFromAxisAngle(
                        Vector3.UnitX,
                        pitch);
            }
        }

        private void UpdateCameraDistance()
        {
            // In a left-handed system, -Z is "backward".
            // Since the camera is looking +Z (forward), placing it at -Z 
            // makes it look directly at the pivot point (0,0,0 local).
            cameraObject.Transform.LocalPosition = new Vector3(0, 0, -distance);
        }

        // --------------------------------------------------------
        // Public API to manipulate the camera via other scripts
        // --------------------------------------------------------

        public void SetFocusPoint(Vector3 position)
        {
            gameObject.Transform.Position = position;
        }

        public Camera3D GetCamera() => camera;
        public GameObject GetCameraObject() => cameraObject;
        public float GetDistance() => distance;
    }
}