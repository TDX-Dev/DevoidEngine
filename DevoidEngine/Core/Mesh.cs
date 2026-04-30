using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Core
{
    public class Mesh
    {
        public Vector3[] Positions { get; } = [];
        public Vector2[] UVs { get; } = [];
        public Vector3[] Normals { get; } = [];
        public Vector4[] Tangents { get; } = []; // W is handedness for calculating bitangents

        

        public Mesh()
        {

        }

        void BuildGPU()
        {
            Debug.Assert(Positions.Length == UVs.Length);
            Debug.Assert(Positions.Length == Normals.Length);
            Debug.Assert(Positions.Length == Tangents.Length);
            Vertex[] vertices = new Vertex[Positions.Length];

            for (int i = 0; i < Positions.Length; i++)
            {
                Vertex vertex = new(Positions[i], Normals[i], UVs[i]);
                vertices[i] = vertex;
            }


        }

        void Validate()
        {

        }
    }
}
