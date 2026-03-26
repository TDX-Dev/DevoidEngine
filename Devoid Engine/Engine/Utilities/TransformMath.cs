using System.Numerics;

namespace DevoidEngine.Engine.Utilities
{
    public static class TransformMath
    {
        public static Vector3 DegToRad(Vector3 deg)
        {
            return new Vector3(MathHelper.DegToRad(deg.X), MathHelper.DegToRad(deg.Y), MathHelper.DegToRad(deg.Z));
        }

        public static Quaternion EulerToQuaternion(Vector3 eulerDegrees)
        {
            Vector3 radians = eulerDegrees * (MathF.PI / 180f);

            return Quaternion.CreateFromYawPitchRoll(
                radians.Y,
                radians.X,
                radians.Z);
        }

        public static Vector3 QuaternionToEuler(Quaternion q)
        {
            // Basic conversion (sufficient for gameplay)
            Vector3 euler;

            // yaw (Y axis rotation)
            float siny_cosp = 2 * (q.W * q.Y + q.Z * q.X);
            float cosy_cosp = 1 - 2 * (q.Y * q.Y + q.X * q.X);
            euler.Y = MathF.Atan2(siny_cosp, cosy_cosp);

            // pitch (X axis rotation)
            float sinp = 2 * (q.W * q.X - q.Z * q.Y);
            if (MathF.Abs(sinp) >= 1)
                euler.X = MathF.CopySign(MathF.PI / 2, sinp);
            else
                euler.X = MathF.Asin(sinp);

            // roll (Z axis rotation)
            float sinr_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            float cosr_cosp = 1 - 2 * (q.Z * q.Z + q.X * q.X);
            euler.Z = MathF.Atan2(sinr_cosp, cosr_cosp);

            return euler * (180f / MathF.PI);
        }
    }

}