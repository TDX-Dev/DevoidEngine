using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.InputSystem;
using DevoidEngine.Engine.InputSystem.InputDevices;
using System.Numerics;

namespace DevoidStandaloneLauncher.Scripts
{
    public class MenuCameraPanComponent : Component
    {
        public override string Type => nameof(MenuCameraPanComponent);

        // Strength of the camera motion
        public float PanAmount = 0.25f;

        // Smoothness of movement
        public float SmoothSpeed = 3f;

        private Vector3 basePosition;
        private Vector3 currentOffset;

        public override void OnStart()
        {
            basePosition = gameObject.Transform.Position;
        }

        public override void OnUpdate(float dt)
        {
            Vector2 mouse = GetMousePosition();
            Vector2 screen = Screen.Size;

            // normalize mouse to -1..1
            Vector2 normalized =
                (mouse / screen) * 2f - Vector2.One;

            Vector3 targetOffset = new Vector3(
                normalized.X * PanAmount,
                -normalized.Y * PanAmount,
                0f
            );

            currentOffset = Vector3.Lerp(
                currentOffset,
                targetOffset,
                SmoothSpeed * dt
            );

            gameObject.Transform.Position =
                basePosition + currentOffset;
        }

        private Vector2 GetMousePosition()
        {
            float x = Input.GetAction("PosX");

            float y = Input.GetAction("PosY");

            return new Vector2(x, y);
        }
    }
}