using System;
using System.Numerics;

namespace DevoidEngine.Engine.Utilities
{
    public enum Axis
    {
        X,
        Y,
        Z,
        NegX,
        NegY,
        NegZ
    }

    public static class AxisHelper
    {
        public static Matrix4x4 BuildAxisMatrix(Axis sourceUp, Axis sourceForward)
        {
            Vector3 up = AxisToVector(sourceUp);
            Vector3 forward = AxisToVector(sourceForward);

            // Right-handed coordinate system
            Vector3 right = Vector3.Cross(up, forward);

            // Build source basis (vectors must be columns)
            Matrix4x4 sourceBasis = new Matrix4x4(
                right.X, up.X, forward.X, 0,
                right.Y, up.Y, forward.Y, 0,
                right.Z, up.Z, forward.Z, 0,
                0, 0, 0, 1
            );

            // Convert from source space → engine space
            Matrix4x4.Invert(sourceBasis, out Matrix4x4 conversion);

            return conversion;
        }

        static Vector3 AxisToVector(Axis axis)
        {
            return axis switch
            {
                Axis.X => new Vector3(1, 0, 0),
                Axis.Y => new Vector3(0, 1, 0),
                Axis.Z => new Vector3(0, 0, 1),

                Axis.NegX => new Vector3(-1, 0, 0),
                Axis.NegY => new Vector3(0, -1, 0),
                Axis.NegZ => new Vector3(0, 0, -1),

                _ => Vector3.UnitY
            };
        }
    }
}