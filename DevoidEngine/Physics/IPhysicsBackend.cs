using BepuPhysics.Collidables;
using DevoidEngine.Core;
using DevoidEngine.Util;

namespace DevoidEngine.Physics
{
    public interface IPhysicsBackend
    {
        void Initialize();
        void Step(float deltaTime);

        bool IsTrigger(CollidableReference c);
        bool TryGetGameObject(CollidableReference collidable, out GameObject gameObject);

        IPhysicsBody CreateBody(PhysicsBodyDescription desc, GameObject owner);
        IPhysicsStatic CreateStatic(PhysicsStaticDescription desc, GameObject owner);
        void RemoveBody(IPhysicsBody body);
        void RemoveStatic(IPhysicsStatic body);

        bool Raycast(Ray ray, float maxDistance, out RaycastHit hit);
        bool Raycast<TFilter>(Ray ray, float maxDistance, out RaycastHit hit, TFilter filter) where TFilter : struct, IRaycastFilter;

        event Action<IPhysicsObject, IPhysicsObject> CollisionDetected;
    }
}
