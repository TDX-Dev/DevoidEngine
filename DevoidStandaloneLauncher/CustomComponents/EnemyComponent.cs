using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.UI.Nodes;
using DevoidEngine.Engine.UI.Text;
using DevoidEngine.Engine.Utilities;
using System.Numerics;
using static System.Net.Mime.MediaTypeNames;

namespace DevoidEngine.Engine.Components
{
    public class Enemy : Component, ICollisionListener
    {
        public override string Type => nameof(Enemy);

        public float MaxHealth = 200f;
        public float Health = 200f;

        private bool isDying = false;
        private float deathTimer = 0f;
        private const float deathDuration = 2f;
        public float MoveSpeed = 4f;

        public event Action OnDeath;

        private RigidBodyComponent rb;
        private MeshRenderer meshRenderer;
        private CanvasComponent HealthBarUI;
        private LabelNode HealthBarText;
        private GameObject player;
        private GameObject HealthBarObject;

        private EnemySpawner spawner;

        public void SetSpawner(EnemySpawner spawnerRef)
        {
            spawner = spawnerRef;
        }

        public override void OnStart()
        {
            Console.WriteLine("Enemy created: " + gameObject.Id);

            rb = gameObject.GetComponent<RigidBodyComponent>();
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

            HealthBarObject = gameObject.Scene.addGameObject("HealthBarObject");
            HealthBarObject.SetParent(gameObject, true);
            HealthBarObject.transform.LocalPosition = new Vector3(0, 0.7f, 0);

            HealthBarUI = HealthBarObject.AddComponent<CanvasComponent>();
            FontInternal font = FontLibrary.LoadFont("Engine/Content/Fonts/JetBrainsMono-Regular.ttf", 32);
            HealthBarText = new LabelNode("Health", font, 32);
            HealthBarUI.Canvas.Add(HealthBarText);
            HealthBarUI.PixelsPerUnit = 200;
            HealthBarUI.RenderMode = CanvasRenderMode.WorldSpace;

            player = gameObject.Scene.GetGameObject("Player"); // adjust to your API
        }

        public override void OnUpdate(float dt)
        {
            HealthBarText.Text = $"Health: {Health}/{MaxHealth}";
            if (isDying)
            {
                HandleDeath(dt);
                return;
            }

            ChasePlayer();
        }

        private void HandleDeath(float dt)
        {
            deathTimer += dt;

            var euler = gameObject.transform.EulerAngles;
            euler.Z = MathHelper.Lerp(euler.Z, 90f, dt * 5f);
            gameObject.transform.EulerAngles = euler;

            if (deathTimer >= deathDuration)
            {
                spawner?.NotifyEnemyDied();
                gameObject.Scene.Destroy(gameObject);

            }
        }

        private void ChasePlayer()
        {
            if (player == null || rb == null)
                return;

            Vector3 direction =
                player.transform.Position - gameObject.transform.Position;

            direction.Y = 0f;

            if (direction.LengthSquared() < 0.01f)
                return;

            direction = Vector3.Normalize(direction);

            // Compute yaw angle (Y-axis rotation)
            float targetYaw = MathF.Atan2(direction.X, direction.Z);

            // Convert to quaternion (Y-axis only)
            Quaternion targetRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, targetYaw);

            // Apply to rigidbody
            rb.Rotation = targetRotation;

            rb.LinearVelocity = new Vector3(
                direction.X * MoveSpeed,
                rb.LinearVelocity.Y,
                direction.Z * MoveSpeed
            );
        }

        public void OnCollisionEnter(GameObject other)
        {
            if (isDying) return;

            if (other.GetComponent<BulletComponent>() != null)
            {
                TakeDamage(25f);
            }
        }

        private void TakeDamage(float amount)
        {
            Health -= amount;

            if (Health <= 0f)
                Die();
        }

        private void Die()
        {
            if (isDying) return;

            isDying = true;
            OnDeath?.Invoke();

            // 1️⃣ Remove physics
            if (rb != null)
                gameObject.RemoveComponent(rb);

            // 2️⃣ Change color to red
            if (meshRenderer != null && meshRenderer.material != null)
            {
                meshRenderer.material.SetVector4("Albedo", new Vector4(1, 0, 0, 1));
            }

            // 3️⃣ Disable further collisions
            gameObject.Enabled = true; // keep rendering
        }

        public void OnCollisionStay(GameObject other)
        {
        }

        public void OnCollisionExit(GameObject other)
        {
        }
    }
}