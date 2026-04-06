using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using System;
using System.Numerics;

namespace DevoidStandaloneLauncher.Scripts
{
    public class MovingCollider : Component
    {
        public override string Type => nameof(MovingCollider);

        RigidBodyComponent body;

        Vector3 localAxis = Vector3.UnitX;

        Vector3 startingPos;
        float timer = 0;

        public float MoveSpeed = 2;

        public override void OnStart()
        {
            body = gameObject.GetComponent<RigidBodyComponent>();
            startingPos = gameObject.Transform.Position;
        }

        public override void OnFixedUpdate(float dt)
        {
            if (body == null)
                return;

            timer += dt;

            // Convert local axis to world axis
            Vector3 worldAxis = Vector3.Transform(localAxis, gameObject.Transform.Rotation);

            float distance = (float)Math.Sin(timer * 2) * 2f;
            float amplitude = Vector3.Dot(localAxis, gameObject.Transform.Scale) * 2f;

            gameObject.Transform.Position =
                startingPos + worldAxis * (float)Math.Sin(timer * MoveSpeed) * amplitude;
        }
    }
}