using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Engine.Physics
{
    public interface IPhysicsBackend
    {
        void Initialize();
        void Step(float deltaTime);


        IPhysicsBody CreateBody(PhysicsBodyDescription desc, GameObject owner);
        IPhysicsStatic CreateStatic(PhysicsStaticDescription desc, GameObject owner);
        void RemoveBody(IPhysicsBody body);
        void RemoveStatic(IPhysicsStatic body);

        bool Raycast(Ray ray, float maxDistance, out RaycastHit hit);

        event Action<IPhysicsObject, IPhysicsObject> CollisionDetected;
    }
}
