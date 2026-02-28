using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Physics
{
    public interface IPhysicsObject
    {
        int Id { get; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        void Remove();
    }

}
