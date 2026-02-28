using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Physics;
using DevoidEngine.Engine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.PrototypeSystems
{
    public static class PChamber1
    {

        public static void CreateRoom(Scene scene, Vector3 center, Vector3 size)
        {
            float halfX = size.X / 2f;
            float halfY = size.Y / 2f;
            float halfZ = size.Z / 2f;

            // Floor
            CreateWall(scene,
                center + new Vector3(0, -halfY, 0),
                new Vector3(size.X, 1, size.Z));

            // Ceiling
            CreateWall(scene,
                center + new Vector3(0, halfY, 0),
                new Vector3(size.X, 1, size.Z));

            // Front
            var front = CreateWall(scene,
                center + new Vector3(0, 0, halfZ),
                new Vector3(size.X, 1, size.Y));
            front.transform.EulerAngles = new Vector3(90, 0, 0);

            // Back
            var back = CreateWall(scene,
                center + new Vector3(0, 0, -halfZ),
                new Vector3(size.X, 1, size.Y));
            back.transform.EulerAngles = new Vector3(90, 0, 0);

            // Left
            var left = CreateWall(scene,
                center + new Vector3(-halfX, 0, 0),
                new Vector3(size.Z, 1, size.Y));
            left.transform.EulerAngles = new Vector3(90, 0, 90);

            // Right
            var right = CreateWall(scene,
                center + new Vector3(halfX, 0, 0),
                new Vector3(size.Z, 1, size.Y));
            right.transform.EulerAngles = new Vector3(90, 0, 90);
        }

        public static GameObject CreateWall(Scene scene, Vector3 position, Vector3 scale)
        {
            var wall = scene.addGameObject("Wall");

            wall.transform.Position = position;
            wall.transform.Scale = scale;

            wall.AddComponent<MeshRenderer>().AddMesh(Primitives.Cube);

            var collider = wall.AddComponent<StaticCollider>();
            collider.Shape = new PhysicsShapeDescription()
            {
                Type = PhysicsShapeType.Box,
                Size = scale
            };

            return wall;
        }
    }
}
