using EmberaEngine.Engine.Core;
using nkast.Aether.Physics2D.Dynamics;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmberaEngine.Engine.Components
{
    public class RigidBody2D : Component
    {
        public override string Type => nameof(RigidBody2D);

        public EmberaEngine.Engine.Core.BodyType BodyType { get; set; } = Core.BodyType.Static;
        public bool freezeRotation = false;

        private Body body;
        private nkast.Aether.Physics2D.Common.Vector2 bodyTransform = new nkast.Aether.Physics2D.Common.Vector2();

        public override void OnStart()
        {
            body = gameObject.scene.PhysicsManager2D.CreateBox(BodyType, new Vector2(gameObject.transform.position.X / PhysicsManager2D.PPM, gameObject.transform.position.Y / PhysicsManager2D.PPM), MathHelper.DegreesToRadians(gameObject.transform.rotation.X), (gameObject.transform.scale.X / PhysicsManager2D.PPM * 2), (gameObject.transform.scale.Y / PhysicsManager2D.PPM * 2), 1f, Vector2.Zero);

            //gameObject.transform.scale = new Vector3((gameObject.transform.scale.X / PhysicsManager2D.PPM) / 2, (gameObject.transform.scale.Y / PhysicsManager2D.PPM) / 2, 1);
        }

        public void ApplyImpulse(Vector2 impulse)
        {
            body.ApplyLinearImpulse(new nkast.Aether.Physics2D.Common.Vector2(impulse.X, impulse.Y));
        }

        public void ApplyForce(Vector2 force)
        {
            nkast.Aether.Physics2D.Common.Vector2 forceRef = new nkast.Aether.Physics2D.Common.Vector2(force.X, force.Y);
            body.ApplyForce(ref forceRef);
        }

        public void SetRestitution(float value)
        {
            body.SetRestitution(value);
        }

        public void SetFriction(float value)
        {
            body.SetFriction(value);
        }

        public void SetMass(float mass)
        {
            body.Mass = mass;
        }

        public void Rotate(float angle)
        {
            gameObject.transform.rotation.X = angle;
            body.Rotation = MathHelper.DegreesToRadians(angle);
        }

        public override void OnUpdate(float dt)
        {

            if (BodyType == Core.BodyType.Static)
            {
                return;
            }


            gameObject.transform.position.X = (body.Position.X * PhysicsManager2D.PPM);// + gameObject.transform.scale.X * 2;
            gameObject.transform.position.Y = (body.Position.Y * PhysicsManager2D.PPM);// + gameObject.transform.scale.Y * 2;

            if (!freezeRotation)
            {
                gameObject.transform.rotation.X = MathHelper.RadiansToDegrees(body.Rotation);
            }

        }



    }
}