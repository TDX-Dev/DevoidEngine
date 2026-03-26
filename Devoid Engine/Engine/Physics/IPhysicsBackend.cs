using BepuPhysics.Collidables;
using DevoidEngine.Engine.Core;

namespace DevoidEngine.Engine.Physics
{
    public interface IPhysicsBackend
    {
        void Initialize();
        void Step(float deltaTime);

        bool IsTrigger(CollidableReference c);
        IPhysicsBody CreateBody(PhysicsBodyDescription desc, GameObject owner);
        IPhysicsStatic CreateStatic(PhysicsStaticDescription desc, GameObject owner);
        void RemoveBody(IPhysicsBody body);
        void RemoveStatic(IPhysicsStatic body);

        bool Raycast(Ray ray, float maxDistance, out RaycastHit hit);

        event Action<IPhysicsObject, IPhysicsObject> CollisionDetected;
    }
}