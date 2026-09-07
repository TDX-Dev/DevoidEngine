using DevoidEngine.Core;
using DevoidEngine.Physics;
using DevoidEngine.Rendering;
using DevoidEngine.Util;
using System.Numerics;

namespace DevoidEngine.Components
{
    public class GunComponent : PickupItem
    {
        public override string Type => nameof(GunComponent);

        public float BulletLifetime = 3f;

        private readonly List<GameObject> bullets = [];
        private readonly List<float> bulletLifetimes = [];

        public float Damage = 25f;
        public float Range = 100f;
        public float FireRate = 10f;

        public int MagazineSize = 30000;

        private int ammo;
        private GameObject? holder;
        private Camera3D? camera;

        public GunComponent()
        {
            HoldEulerOffset = new Vector3(0f, 86.5f, 0f);
        }

        public override void OnStart()
        {
            ammo = MagazineSize;
        }

        public override void OnUpdate(float dt)
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                bulletLifetimes[i] -= dt;

                if (bulletLifetimes[i] <= 0f)
                {
                    gameObject.Scene!.RemoveGameObject(
                        bullets[i]);

                    bullets.RemoveAt(i);
                    bulletLifetimes.RemoveAt(i);
                }
            }
        }

        public override void OnPickup(GameObject player)
        {
            holder = player;

            FirstPersonController? controller =
                player.GetComponent<FirstPersonController>();

            camera = controller?.GetCamera();
        }

        public override void OnDrop()
        {
            holder = null;
            camera = null;
        }

        public override void OnPrimary()
        {
            Console.WriteLine("Firing!");
            Fire();
        }

        public override void OnSecondary()
        {
            // ADS
        }

        public override void OnUse()
        {
            // Reload / inspect
        }

        private void Fire()
        {
            if (HeldCamera == null || ammo <= 0)
                return;

            Vector3 origin = HeldCamera.gameObject.Transform.Position;

            Vector3 direction = HeldCamera.gameObject.Transform.Forward;

            Ray ray = new(origin, direction);

            if (gameObject.Scene!.Physics.Raycast(ray, Range, out RaycastHit _))
            {
                // Apply damage.
            }

            GameObject bulletObj = new GameObject();

            bulletObj.Transform.Position = origin + direction * 2f;

            bulletObj.Transform.Scale = new Vector3(0.2f);

            gameObject.Scene.AddGameObject(bulletObj);

            RigidBodyComponent rb = bulletObj.AddComponent<RigidBodyComponent>();

            rb.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Box,
                Size = new Vector3(0.2f)
            };

            MeshRenderer mr =
                bulletObj.AddComponent<MeshRenderer>();

            mr.Mesh = PrimitiveMeshes.GetCube();

            mr.Material =
                new MaterialInstance(
                    Engine.Renderer.DefaultMaterial);

            mr.Material.SetFloat(
                "EmissiveStrength",
                30);

            mr.Material.SetVector3(
                "EmissiveColor",
                new Vector3(1f, 0.2f, 0.2f));

            bullets.Add(bulletObj);
            bulletLifetimes.Add(BulletLifetime);

            rb.Mass = 1;
            rb.LinearVelocity = direction * 100;

            ammo--;
        }
    }
}