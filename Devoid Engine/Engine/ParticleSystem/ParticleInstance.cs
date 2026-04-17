using DevoidGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.ParticleSystem
{
    struct ParticleInstance
    {
        public Vector3 Position;
        public float Size;
        public Vector4 Color;

        public static readonly VertexInfo VertexInfo = new VertexInfo(
            typeof(ParticleInstance),
            new VertexAttribute("INSTANCE_POS", 0, 3, 0, 1, VertexAttribType.Float, VertexStepMode.Instance),
            new VertexAttribute("INSTANCE_SIZE", 0, 1, 3 * sizeof(float), 1, VertexAttribType.Float, VertexStepMode.Instance),
            new VertexAttribute("INSTANCE_COLOR", 0, 4, 4 * sizeof(float), 1, VertexAttribType.Float, VertexStepMode.Instance)
        );

        public ParticleInstance(Vector3 position, float size, Vector4 color)
        {
            Position = position;
            Size = size;
            Color = color;
        }
    }
}
