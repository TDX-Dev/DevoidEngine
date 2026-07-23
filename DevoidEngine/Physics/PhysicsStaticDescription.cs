using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Physics
{
    [MessagePackObject]
    public struct PhysicsStaticDescription
    {
        [Key(0)]
        public Vector3 Position;

        [Key(1)]
        public Quaternion Rotation;

        [Key(2)]
        public PhysicsShapeDescription Shape;

        [Key(3)]
        public PhysicsMaterial Material;
    }
}
