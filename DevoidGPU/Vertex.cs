﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidGPU
{
    public readonly struct Vertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 UV1;
        public readonly Vector3 Tangent;
        public readonly Vector3 BiTangent;
        public readonly Vector2 UV2;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(Vertex),
            new VertexAttribute("POSITION", 0, 3, 0),
            new VertexAttribute("NORMAL", 0, 3, 3 * sizeof(float)),
            new VertexAttribute("TEXCOORD", 0, 2, 6 * sizeof(float)),
            new VertexAttribute("TANGENT", 0, 3, 8 * sizeof(float)),
            new VertexAttribute("BINORMAL", 0, 3, 11 * sizeof(float)),
            new VertexAttribute("TEXCOORD", 1, 2, 14 * sizeof(float))
        );


        public Vertex(Vector3 position, Vector3 normal, Vector2 texcoord)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV1 = texcoord;
            this.Tangent = Vector3.Zero;
            this.BiTangent = Vector3.Zero;
            this.UV2 = texcoord;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 texcoord, Vector3 tangent, Vector3 bitangent)
        {
            this.Position = position;
            this.Normal = normal;
            this.UV1 = texcoord;
            this.Tangent = tangent;
            this.BiTangent = bitangent;
            this.UV2 = texcoord;
        }

    }

    public readonly struct VertexAttribute
    {
        public readonly string Name;
        public readonly int Index;
        public readonly int ComponentCount;
        public readonly int Offset;

        public VertexAttribute(string name, int index, int componentcount, int offset)
        {
            Name = name;
            Index = index;
            ComponentCount = componentcount;
            Offset = offset;
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

            for (int i = 0; i < VertexAttributes.Length; i++)
            {
                VertexAttribute attribute = this.VertexAttributes[i];
                this.SizeInBytes += attribute.ComponentCount * sizeof(float);
            }
        }
    }

}
