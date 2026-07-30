using System.Numerics;

namespace DevoidGPU
{
    public readonly struct Vertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 UV;
        public readonly Vector4 Tangent;

        public static readonly VertexInfo VertexInfo = new(
            typeof(Vertex),
            new VertexAttribute("POSITION", 0, 3, 0),
            new VertexAttribute("NORMAL", 0, 3, 3 * sizeof(float)),
            new VertexAttribute("TEXCOORD", 0, 2, 6 * sizeof(float)),
            new VertexAttribute("TANGENT", 0, 4, 8 * sizeof(float))
        );

        public Vertex(Vector3 position)
        {
            this.Position = position;
            this.Normal = Vector3.Zero;
            this.UV = Vector2.Zero;
            this.Tangent = Vector4.Zero;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 texcoord)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV = texcoord;
            this.Tangent = Vector4.Zero;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 texcoord, Vector4 tangent)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV = texcoord;
            this.Tangent = tangent;
        }
    }

    public enum VertexAttribType
    {
        Float,
        UnsignedByte,
        Int,
    }

    public readonly struct VertexAttribute
    {
        public readonly string Name;
        public readonly int Index;
        public readonly int ComponentCount;
        public readonly int Offset;
        public readonly int Slot;
        public readonly VertexAttribType Type;
        public readonly VertexStepMode StepMode;
        public readonly bool Normalized;

        public VertexAttribute(
            string name,
            int index,
            int componentCount,
            int offset,
            int slot = 0,
            VertexAttribType type = VertexAttribType.Float,
            VertexStepMode mode = VertexStepMode.Vertex,
            bool normalized = false)
        {
            Name = name;
            Index = index;
            ComponentCount = componentCount;
            Offset = offset;
            Type = type;
            StepMode = mode;
            Slot = slot;
            Normalized = normalized;
        }
    }

    public sealed class VertexInfo
    {
        public readonly Type Type;
        public readonly int SizeInBytes;
        public readonly VertexAttribute[] VertexAttributes;

        public VertexInfo(Type type, params VertexAttribute[] attributes)
        {
            this.Type = type;
            this.VertexAttributes = attributes;
            this.SizeInBytes = 0;

            int max = 0;

            for (int i = 0; i < attributes.Length; i++)
            {
                var attr = attributes[i];
                int size = attr.ComponentCount * Utils.GetVertexTypeSize(attr.Type);
                int end = attr.Offset + size;

                if (end > max)
                    max = end;
            }

            SizeInBytes = max;
        }

        public static VertexInfo Create<T>(params (string name, int index, int count, int slot, VertexAttribType type, VertexStepMode step, bool normalize)[] attrs)
        {
            var offsets = new Dictionary<int, int>();
            VertexAttribute[] attributes = new VertexAttribute[attrs.Length];

            int i = 0;

            foreach (var (name, index, count, slot, type, step, normalize) in attrs)
            {
                if (!offsets.TryGetValue(slot, out int offset))
                    offset = 0;

                attributes[i++] = new VertexAttribute(
                    name,
                    index,
                    count,
                    offset,
                    slot,
                    type,
                    step,
                    normalize
                );

                offset += count * Utils.GetVertexTypeSize(type);
                offsets[slot] = offset;
            }
            return new VertexInfo(typeof(T), attributes);
        }
    }


}