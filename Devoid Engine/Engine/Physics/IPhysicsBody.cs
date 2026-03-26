using System.Numerics;

namespace DevoidEngine.Engine.Physics
{
    public interface IPhysicsBody : IPhysicsObject
    {
        Vector3 LinearVelocity { get; set; }
        Vector3 AngularVelocity { get; set; }

        float Mass { get; }
        bool IsKinematic { get; set; }


        void AddForce(Vector3 force, float dt);
        void AddImpulse(Vector3 impulse);
        void AddTorque(Vector3 torque);

        void WakeUp();
        void Sleep();
    }


}