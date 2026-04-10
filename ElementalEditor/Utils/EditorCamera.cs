using DevoidEngine.Engine.InputSystem;
using System.Numerics;

namespace ElementalEditor.Utils
{
    public class EditorCamera
    {
        public Vector3 Position = new(0, 2, 5);
        public float Pitch;
        public float Yaw = -90f;

        public float MoveSpeed = 10f;
        public float MouseSensitivity = 0.15f;
        public float Fov = 60f;
        public float AspectRatio = 1.77f;

        public void Update(float dt)
        {

            Vector2 delta = new Vector2(Input.GetAction("LookX"),Input.GetAction("LookY"));

            Yaw += delta.X * MouseSensitivity;
            Pitch -= delta.Y * MouseSensitivity;

            Pitch = Math.Clamp(Pitch, -89f, 89f);

            Vector3 forward = GetForward();
            Vector3 right = GetRight();

            if (Input.GetActionDown("Forward"))
                Position += forward * MoveSpeed * dt;

            //if (Input.KeyDown(Keys.S))
            //    Position -= forward * MoveSpeed * dt;

            //if (Input.KeyDown(Keys.A))
            //    Position -= right * MoveSpeed * dt;

            //if (Input.KeyDown(Keys.D))
            //    Position += right * MoveSpeed * dt;
        }

        public Matrix4x4 GetViewMatrix()
        {
            Vector3 forward = GetForward();

            return Matrix4x4.CreateLookAt(
                Position,
                Position + forward,
                Vector3.UnitY
            );
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(
                MathF.PI / 180f * Fov,
                AspectRatio,
                0.01f,
                1000f
            );
        }

        public Vector3 GetForward()
        {
            Vector3 forward;

            forward.X = MathF.Cos(Deg2Rad(Yaw)) * MathF.Cos(Deg2Rad(Pitch));
            forward.Y = MathF.Sin(Deg2Rad(Pitch));
            forward.Z = MathF.Sin(Deg2Rad(Yaw)) * MathF.Cos(Deg2Rad(Pitch));

            return Vector3.Normalize(forward);
        }

        public Vector3 GetRight()
        {
            return Vector3.Normalize(Vector3.Cross(GetForward(), Vector3.UnitY));
        }

        float Deg2Rad(float deg) => deg * (MathF.PI / 180f);
    }
}