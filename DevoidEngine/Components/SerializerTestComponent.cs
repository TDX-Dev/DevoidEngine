using DevoidEngine.Attributes;
using DevoidEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Components
{
    [DevoidClass]
    public class SerializerTestComponent : Component
    {
        public override string Type => nameof(SerializerTestComponent);

        public Mesh MeshObject { get; set; } = null!;
        public Vector3[] Vector3s { get; set; } = [new Vector3(1, 2, 1), new Vector3(1, 2, 2), new Vector3(1, 2, 3),];
    }
}
