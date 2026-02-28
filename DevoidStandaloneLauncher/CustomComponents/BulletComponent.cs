using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class BulletComponent : Component, ICollisionListener
    {
        public override string Type => nameof(BulletComponent);

        public float LifeTime = 3f;
        public float Damage = 25f;

        private float lifeTimer;
        private RigidBodyComponent rb;

        public override void OnStart()
        {
            rb = gameObject.GetComponent<RigidBodyComponent>();
        }

        public override void OnUpdate(float dt)
        {
            lifeTimer += dt;

            if (lifeTimer >= LifeTime)
            {
                gameObject.Scene.Destroy(gameObject);
            }
        }

        public void OnCollisionEnter(GameObject other)
        {
            // Always destroy bullet on impact
            if (other.Name == "Enemy")
            {
                gameObject.Scene.Destroy(gameObject);
            }
        }

        public void OnCollisionStay(GameObject other) { }
        public void OnCollisionExit(GameObject other) { }

        // ===============================
        // Static Spawn Helper
        // ===============================

        public static GameObject Spawn(
            Scene scene,
            Mesh mesh,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            float speed,
            float mass)
        {
            GameObject bullet = scene.addGameObject("Projectile");

            bullet.transform.Position = position;
            bullet.transform.Rotation = rotation;
            bullet.transform.Scale = scale;

            // Render
            var renderer = bullet.AddComponent<MeshRenderer>();
            renderer.AddMesh(mesh);

            // Physics
            var body = bullet.AddComponent<RigidBodyComponent>();
            body.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Box,
                Size = scale
            };

            body.Mass = mass;

            Vector3 forward =
                Vector3.Normalize(
                    Vector3.Transform(Vector3.UnitZ, rotation)
                );

            body.LinearVelocity = forward * speed;

            // Bullet logic
            bullet.AddComponent<BulletComponent>();

            return bullet;
        }
    }
}