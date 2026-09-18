using System;
using System.Numerics;
using System.Text.Json;

namespace DevoidEngine.Serialization
{
    public static class ValueReader
    {
        public static Guid ReadGuid(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.String)
                throw new JsonException();

            return Guid.Parse(reader.GetString()!);
        }

        public static Vector2 ReadVector2(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            Vector2 result = default;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return result;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propertyName = reader.GetString()!;

                if (!reader.Read())
                    throw new JsonException();

                switch (propertyName)
                {
                    case "X":
                        result.X = reader.GetSingle();
                        break;

                    case "Y":
                        result.Y = reader.GetSingle();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException();
        }

        public static Vector3 ReadVector3(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            Vector3 result = default;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return result;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propertyName = reader.GetString()!;

                if (!reader.Read())
                    throw new JsonException();

                switch (propertyName)
                {
                    case "X":
                        result.X = reader.GetSingle();
                        break;

                    case "Y":
                        result.Y = reader.GetSingle();
                        break;

                    case "Z":
                        result.Z = reader.GetSingle();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException();
        }

        public static Vector4 ReadVector4(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            Vector4 result = default;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return result;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propertyName = reader.GetString()!;

                if (!reader.Read())
                    throw new JsonException();

                switch (propertyName)
                {
                    case "X":
                        result.X = reader.GetSingle();
                        break;

                    case "Y":
                        result.Y = reader.GetSingle();
                        break;

                    case "Z":
                        result.Z = reader.GetSingle();
                        break;

                    case "W":
                        result.W = reader.GetSingle();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException();
        }

        public static Quaternion ReadQuaternion(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            Quaternion result = Quaternion.Identity;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    return result;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propertyName = reader.GetString()!;

                if (!reader.Read())
                    throw new JsonException();

                switch (propertyName)
                {
                    case "X":
                        result.X = reader.GetSingle();
                        break;

                    case "Y":
                        result.Y = reader.GetSingle();
                        break;

                    case "Z":
                        result.Z = reader.GetSingle();
                        break;

                    case "W":
                        result.W = reader.GetSingle();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException();
        }

        public static Matrix4x4 ReadMatrix4x4(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            float m11 = 0;
            float m12 = 0;
            float m13 = 0;
            float m14 = 0;

            float m21 = 0;
            float m22 = 0;
            float m23 = 0;
            float m24 = 0;

            float m31 = 0;
            float m32 = 0;
            float m33 = 0;
            float m34 = 0;

            float m41 = 0;
            float m42 = 0;
            float m43 = 0;
            float m44 = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Matrix4x4(
                        m11, m12, m13, m14,
                        m21, m22, m23, m24,
                        m31, m32, m33, m34,
                        m41, m42, m43, m44);
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException();

                string propertyName = reader.GetString()!;

                if (!reader.Read())
                    throw new JsonException();

                switch (propertyName)
                {
                    case "M11":
                        m11 = reader.GetSingle();
                        break;

                    case "M12":
                        m12 = reader.GetSingle();
                        break;

                    case "M13":
                        m13 = reader.GetSingle();
                        break;

                    case "M14":
                        m14 = reader.GetSingle();
                        break;

                    case "M21":
                        m21 = reader.GetSingle();
                        break;

                    case "M22":
                        m22 = reader.GetSingle();
                        break;

                    case "M23":
                        m23 = reader.GetSingle();
                        break;

                    case "M24":
                        m24 = reader.GetSingle();
                        break;

                    case "M31":
                        m31 = reader.GetSingle();
                        break;

                    case "M32":
                        m32 = reader.GetSingle();
                        break;

                    case "M33":
                        m33 = reader.GetSingle();
                        break;

                    case "M34":
                        m34 = reader.GetSingle();
                        break;

                    case "M41":
                        m41 = reader.GetSingle();
                        break;

                    case "M42":
                        m42 = reader.GetSingle();
                        break;

                    case "M43":
                        m43 = reader.GetSingle();
                        break;

                    case "M44":
                        m44 = reader.GetSingle();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonException();
        }
    }
}