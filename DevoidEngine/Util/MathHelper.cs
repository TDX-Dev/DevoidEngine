using System.Numerics;
using System.Runtime.CompilerServices;

namespace DevoidEngine.Util
{
    public static class MathHelper
    {
        public const float PI = (float)Math.PI;
        public const float TwoPI = (float)(Math.PI * 2.0);

        public static float DegToRad(float degrees)
        {
            return degrees * (PI / 180f);
        }
        public static float RadToDeg(float radians)
        {
            return radians * (180f / PI);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint MsbIndex(uint value)
        {
            return 31u - (uint)BitOperations.LeadingZeroCount(value | 1u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RoundUpToPo2(uint value)
        {
            uint valueM1 = Math.Max(1u, value) - 1u;
            return 1u << (int)(MsbIndex(valueM1) + 1u);
        }
    }
}
