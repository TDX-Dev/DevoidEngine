using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Scripts
{
    public class OrbComponent : Component, ICollisionListener
    {

        public override string Type => nameof(OrbComponent);

        AreaComponent? area;

        float bobMovement = 0.75f;
        float bobSpeed = 2.5f;
        float rotSpeed = 90;

        Vector3 startPos;
        float timer = 0;
        Random rand = new Random();

        public override void OnStart()
        {
            area = gameObject.GetComponent<AreaComponent>();
            startPos = gameObject.Transform.Position;
            timer = (float)(rand.NextDouble() * Math.PI * 2);
        }
        public override void OnFixedUpdate(float dt)
        {
            timer += dt;

            // rotate
            gameObject.Transform.EulerAngles += new System.Numerics.Vector3(0, rotSpeed * dt, 0);

            // bob
            var pos = startPos;
            pos.Y += (float)Math.Sin(timer * bobSpeed) * bobMovement;
            gameObject.Transform.Position = pos;
        }

        public void OnCollisionEnter(GameObject other)
        {
            if (other.Name == "Player")
            {
                other.GetComponent<ThirdPersonController>().OrbsCollected++;
                gameObject.Scene.DestroyGameObject(gameObject);
            }
        }

        public void OnCollisionStay(GameObject other)
        {

        }

        public void OnCollisionExit(GameObject other)
        {

        }
    }
}
