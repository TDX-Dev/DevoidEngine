using DevoidEngine.Components;
using System.Numerics;

namespace DevoidEngine.Serialization
{
    public static class ValueWriter
    {
        static Type[] SupportedTypes = [
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(string),
            typeof(bool),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Matrix4x4),
            typeof(byte),
        ];



        public static bool IsValueSerializable(Type type)
        {
            if (type.IsEnum)
                return true;

            if (type.IsArray)
                return IsValueSerializable(type.GetElementType()!);

            return Array.IndexOf(SupportedTypes, type) >= 0;
        }

        public static string SerializeType(Type type, object value)
        {
            if (type.IsArray)
            {
                Array array = (Array)value;

                string[] values = new string[array.Length];

                for (int i = 0; i < array.Length; i++)
                {
                    object element = array.GetValue(i)!;
                    values[i] = SerializeType(type.GetElementType()!, element);
                }

                return $"[{string.Join(",", values)}]";
            }

            if (type.IsEnum)
                return Convert.ToInt32(value).ToString();

            if (type == typeof(int))
                return ((int)value).ToString();

            if (type == typeof(long))
                return ((long)value).ToString();

            if (type == typeof(float))
                return ((float)value).ToString();

            if (type == typeof(double))
                return ((double)value).ToString();

            if (type == typeof(string))
                return (string)value;

            if (type == typeof(bool))
                return ((bool)value).ToString();

            if (type == typeof(Vector2))
            {
                Vector2 v = (Vector2)value;
                return $"{v.X},{v.Y}";
            }

            if (type == typeof(Vector3))
            {
                Vector3 v = (Vector3)value;
                return $"{v.X},{v.Y},{v.Z}";
            }

            if (type == typeof(Vector4))
            {
                Vector4 v = (Vector4)value;
                return $"{v.X},{v.Y},{v.Z},{v.W}";
            }

            throw new NotSupportedException(
                $"Type '{type.FullName}' is not serializable.");
        }




    }
}
