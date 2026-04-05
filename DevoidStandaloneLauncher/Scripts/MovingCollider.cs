using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Scripts
{
    public class MovingCollider : Component
    {
        public override string Type => nameof(MovingCollider);


        RigidBodyComponent body;
        Vector3 movingAxis = Vector3.UnitX;

        public override void OnStart()
        {
            body = gameObject.GetComponent<RigidBodyComponent>();
            startingPos = gameObject.Transform.Position;
        }

        Vector3 startingPos;
        float timer = 0;
        public override void OnFixedUpdate(float dt)
        {
            if (body == null)
                return;

            timer += dt;

            //gameObject.Transform.Position = startingPos + (movingAxis * (float)(Math.Sin(timer * 10) * 5));
        }

    }
}
