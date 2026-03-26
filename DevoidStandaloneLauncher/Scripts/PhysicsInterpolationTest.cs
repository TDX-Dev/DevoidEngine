using DevoidEngine.Engine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Scripts
{
    public class PhysicsInterpolationTest : Component
    {
        public override string Type => nameof(PhysicsInterpolationTest);

        public bool isPhysicsMovement = false;

        public override void OnStart()
        {
            
        }

        public override void OnUpdate(float dt)
        {
            if (!isPhysicsMovement)
            {
                timer += dt;
                var pos = gameObject.Transform.Position;
                pos.X = (float)Math.Sin(timer * 5) * 5;
                gameObject.Transform.Position = pos;
            }
        }

        public override void OnFixedUpdate(float dt)
        {
            if (isPhysicsMovement)
            {
                timer += dt;
                var pos = gameObject.Transform.Position;
                pos.X = (float)Math.Sin(timer * 5) * 5;
                gameObject.Transform.Position = pos;
            }
        }

        float timer = 0f;

    }
}
