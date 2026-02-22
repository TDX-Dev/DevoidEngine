using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Utilities;
using System.Numerics;

namespace DevoidEngine.Engine.Components
{
    public class EnemySpawner : Component
    {
        public override string Type => nameof(EnemySpawner);

        public float RespawnDelay = 3f;
        public Vector3 SpawnPosition = new Vector3(0, 5, 10);
        public Vector3 EnemyScale = new Vector3(1, 4, 1);

        private float respawnTimer = 0f;
        private bool waitingForRespawn = false;

        private Mesh enemyMesh;

        public event Action OnDeath;

        public override void OnStart()
        {
            enemyMesh = new Mesh();
            enemyMesh.SetVertices(Primitives.GetCubeVertex());

            SpawnEnemy();
        }

        public override void OnUpdate(float dt)
        {
            if (!waitingForRespawn)
                return;

            respawnTimer += dt;

            if (respawnTimer >= RespawnDelay)
            {
                SpawnEnemy();
                waitingForRespawn = false;
                respawnTimer = 0f;
            }
        }

        public void NotifyEnemyDied()
        {
            waitingForRespawn = true;
        }

        private void SpawnEnemy()
        {
            GameObject enemy = gameObject.Scene.addGameObject("Enemy");

            float groundTop = 0.5f; // since ground height is 1
            float enemyHalfHeight = EnemyScale.Y * 0.5f;

            Vector3 spawnPos = new Vector3(
                SpawnPosition.X,
                groundTop + enemyHalfHeight,
                SpawnPosition.Z
            );

            enemy.transform.Position = spawnPos;

            enemy.transform.Scale = EnemyScale;

            enemy.AddComponent<MeshRenderer>().AddMesh(enemyMesh);

            var rb = enemy.AddComponent<RigidBodyComponent>();
            rb.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Box,
                Size = EnemyScale
            };

            rb.Mass = 100;
            rb.Material = new PhysicsMaterial()
            {
                Friction = 2f,
                Restitution = 0.1f
            };

            rb.FreezeRotationX = true;
            rb.FreezeRotationZ = true;

            var enemyComp = enemy.AddComponent<Enemy>();
            enemyComp.SetSpawner(this);
            enemyComp.OnDeath += () =>
            {
                OnDeath?.Invoke();
            };
        }
    }
}