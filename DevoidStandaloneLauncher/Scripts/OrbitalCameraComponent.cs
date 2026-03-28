using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidStandaloneLauncher.Scripts
{
    public class OrbitalCamera : Component
    {
        public override string Type => nameof(OrbitalCamera);

        public Vector3 Target = Vector3.Zero;

        public float Radius;
        public float Yaw;     // horizontal angle
        public float Pitch;   // vertical angle

        public float OrbitSpeed = 1.5f;

        public override void OnStart()
        {
            // derive spherical coords from starting position
            Vector3 offset = gameObject.Transform.Position - Target;
            

            Radius = offset.Length();

            Yaw = MathF.Atan2(offset.Z, offset.X);
            Pitch = MathF.Asin(offset.Y / Radius);
        }

        public override void OnUpdate(float dt)
        {
            // auto orbit (like demo)
            Yaw += OrbitSpeed * dt;

            // convert spherical → cartesian
            Vector3 pos;
            pos.X = Radius * MathF.Cos(Pitch) * MathF.Cos(Yaw);
            pos.Y = Radius * MathF.Sin(Pitch);
            pos.Z = Radius * MathF.Cos(Pitch) * MathF.Sin(Yaw);

            pos += Target;

            gameObject.Transform.Position = pos;

            LookAt(Target);
        }

        private void LookAt(Vector3 target)
        {
            Vector3 forward = Vector3.Normalize(target - gameObject.Transform.Position);

            float yaw = MathF.Atan2(forward.Z, forward.X);
            float pitch = MathF.Asin(forward.Y);

            gameObject.Transform.EulerAngles = new Vector3(
                MathHelper.RadToDeg(pitch),
                -MathHelper.RadToDeg(yaw) + 90f,
                0
            );
        }
    }
}