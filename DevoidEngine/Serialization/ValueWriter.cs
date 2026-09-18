using System;
using System.Numerics;
using System.Text.Json;

namespace DevoidEngine.Serialization
{
    public static class ValueWriter
    {
        public static void Write(Utf8JsonWriter writer, Guid value)
        {
            writer.WriteStringValue(value.ToString());
        }

        public static void Write(Utf8JsonWriter writer, Vector2 value)
        {
            writer.WriteStartObject();

            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);

            writer.WriteEndObject();
        }

        public static void Write(Utf8JsonWriter writer, Vector3 value)
        {
            writer.WriteStartObject();

            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);

            writer.WriteEndObject();
        }

        public static void Write(Utf8JsonWriter writer, Vector4 value)
        {
            writer.WriteStartObject();

            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteNumber("W", value.W);

            writer.WriteEndObject();
        }

        public static void Write(Utf8JsonWriter writer, Quaternion value)
        {
            writer.WriteStartObject();

            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Z", value.Z);
            writer.WriteNumber("W", value.W);

            writer.WriteEndObject();
        }

        public static void Write(Utf8JsonWriter writer, Matrix4x4 value)
        {
            writer.WriteStartObject();

            writer.WriteNumber("M11", value.M11);
            writer.WriteNumber("M12", value.M12);
            writer.WriteNumber("M13", value.M13);
            writer.WriteNumber("M14", value.M14);

            writer.WriteNumber("M21", value.M21);
            writer.WriteNumber("M22", value.M22);
            writer.WriteNumber("M23", value.M23);
            writer.WriteNumber("M24", value.M24);

            writer.WriteNumber("M31", value.M31);
            writer.WriteNumber("M32", value.M32);
            writer.WriteNumber("M33", value.M33);
            writer.WriteNumber("M34", value.M34);

            writer.WriteNumber("M41", value.M41);
            writer.WriteNumber("M42", value.M42);
            writer.WriteNumber("M43", value.M43);
            writer.WriteNumber("M44", value.M44);

            writer.WriteEndObject();
        }
    }
}