using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.ParticleSystem
{
    public readonly struct ParticleVertex
    {
        public readonly Vector2 Position;
        public readonly Vector2 UV;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(ParticleVertex),
            new VertexAttribute("POSITION", 0, 2, 0, 0, VertexAttribType.Float),
            new VertexAttribute("TEXCOORD", 0, 2, 2 * sizeof(float), 0, VertexAttribType.Float)
        );

        public ParticleVertex(Vector2 position, Vector2 uv)
        {
            this.Position = position;
            this.UV = uv;
        }
    }
}
