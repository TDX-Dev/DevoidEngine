using System.Numerics;

namespace DevoidEngine.Physics
{
    public interface IPhysicsObject
    {
        int Id { get; }
        Vector3 Position { get; set; }
        Quaternion Rotation { get; set; }
        void Remove();
    }
}
