using System;
using System.Collections.Generic;

using OpenTK.Mathematics;

namespace EmberaEngine.Engine.Utilities
{
    public class Primitives
    {

        public static Vertex[] GetCubeVertex()
        {
            return new Vertex[]
            {
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f,  0.0f, -1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(0.0f,  0.0f, -1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0.0f,  0.0f, -1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0.0f,  0.0f, -1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0.0f,  0.0f, -1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f,  0.0f, -1.0f), new Vector2(0.0f,0.0f)),

                new Vertex(new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(0.0f,  0.0f,  1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0.0f,  0.0f,  1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0.0f,  0.0f,  1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0.0f,  0.0f,  1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(0.0f,  0.0f,  1.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0.0f,  0.0f,  1.0f), new Vector2(0.0f,0.0f)),

                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(-1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(-1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),

                new Vertex(new Vector3(0.5f,  0.5f,  0.5f), new Vector3(1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f,  0.5f, -0.5f), new Vector3(1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector3(1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f,  0.5f,  0.5f), new Vector3(1.0f,  0.0f,  0.0f), new Vector2(0.0f,0.0f)),

                //BOTTOM
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f, -1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector3(0.0f, -1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.0f, -1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.0f, -1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f, -0.5f,  0.5f), new Vector3(0.0f, -1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f, -0.5f,  0.5f), new Vector3(0.0f, -1.0f,  0.0f), new Vector2(0.0f,0.0f)),

                //TOP
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0.0f,  1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(0.0f,  1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f,  0.5f,  0.5f), new Vector3(0.0f,  1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(0.5f,  0.5f,  0.5f), new Vector3(0.0f,  1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(0.0f,  1.0f,  0.0f), new Vector2(0.0f,0.0f)),
                new Vertex(new Vector3(-0.5f,  0.5f, -0.5f), new Vector3(0.0f,  1.0f,  0.0f), new Vector2(0.0f,0.0f)),
        };

        }

        public static Vertex[] GetPlaneVertices()
        {
            return new Vertex[] {
                new Vertex(new Vector3(1.0f, -1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f), new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-1.0f, -1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f),  new Vector2(0.0f, 0.0f)),
                new Vertex(new Vector3(-1.0f,  1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f),  new Vector2(0.0f, 1.0f)),
                new Vertex(new Vector3(1.0f, 1.0f, 0.0f), new Vector3(1.0f, -1.0f, 0.0f), new Vector2(1.0f, 1.0f)),
                new Vertex(new Vector3(1.0f, -1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f),  new Vector2(1.0f, 0.0f)),
                new Vertex(new Vector3(-1.0f, 1.0f, 0.0f),new Vector3(1.0f, -1.0f, 0.0f),  new Vector2(0.0f, 1.0f))
            };
        }

        public static Vertex[] GetSphereVertices(int segments = 32, double radius = 1.0)
        {
            List<Vertex> vertices = new List<Vertex>();

            double phi;
            double theta;
            float x;
            float y;
            float z;
            double[] vertex;

            for (int i = 0; i < segments; i++)
            {
                phi = Math.PI * i / (segments - 1);
                for (int j = 0; j < segments; j++)
                {
                    theta = 2 * Math.PI * j / (segments - 1);
                    x = (float)(radius * Math.Sin(phi) * Math.Cos(theta));
                    y = (float)(radius * Math.Sin(phi) * Math.Sin(theta));
                    z = (float)(radius * Math.Cos(phi));
                    vertices.Add(new Vertex( new Vector3(x,y,z), Vector3.One, Vector2.One));
                }
            }
            return vertices.ToArray();
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


    public readonly struct Vertex
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 TexCoord;
        public readonly Vector3 Tangent;
        public readonly Vector3 BiTangent;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(Vertex),
            new VertexAttribute("Position", 0, 3, 0),
            new VertexAttribute("Normal", 1, 3, 3 * sizeof(float)),
            new VertexAttribute("TexCoord", 2, 2, 6 * sizeof(float)),
            new VertexAttribute("Tangent", 3, 3, 8 * sizeof(float)),
            new VertexAttribute("BiTangent", 4, 3, 11 * sizeof(float))
            );

        public Vertex(Vector3 position, Vector3 normal, Vector2 texcoord)
        {
            this.Position = position;
            this.Normal = normal;
            this.TexCoord = texcoord;
            this.Tangent = Vector3.Zero;
            this.BiTangent = Vector3.Zero;
        }

        public Vertex(Vector3 position, Vector3 normal, Vector2 texcoord, Vector3 tangent, Vector3 bitangent)
        {
            this.Position = position;
            this.Normal = normal;
            this.TexCoord = texcoord;
            this.Tangent = tangent;
            this.BiTangent = bitangent;
        }

    }

    public readonly struct LineVertex
    {
        public readonly Vector3 Position;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(LineVertex),
            new VertexAttribute("Position", 0, 3, 0)
            );

        public LineVertex(Vector3 Position)
        {
            this.Position = Position;
        }
    }
}
