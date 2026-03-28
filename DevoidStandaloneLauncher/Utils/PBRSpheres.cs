using DevoidEngine.Engine.Components;
using DevoidEngine.Engine.Core;
using DevoidEngine.Engine.Rendering;
using DevoidEngine.Engine.Utilities;
using DevoidStandaloneLauncher.Scripts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidStandaloneLauncher.Utils
{
    public static class PBRSpheres
    {
        public static void SpawnCubes(Scene scene)
        {

            GameObject cubeObject = scene.AddGameObject("Cube1");
            cubeObject.AddComponent<MeshRenderer>().AddMesh(RenderConstants.Cube);
            cubeObject.Transform.EulerAngles = new System.Numerics.Vector3(0, 45, 0);
            var phyInterp1 = cubeObject.AddComponent<PhysicsInterpolationTest>();
            phyInterp1.isPhysicsMovement = false;

            GameObject cubeObject1 = scene.AddGameObject("Cube2");
            cubeObject1.Transform.Position = new System.Numerics.Vector3(0, 0, -1.454f);
            cubeObject1.AddComponent<MeshRenderer>().AddMesh(RenderConstants.Cube);
            cubeObject1.AddComponent<PhysicsInterpolationTest>().isPhysicsMovement = true;
        }

        public static void SpawnFurnaceTest(Scene scene)
        {
            Mesh testRender = new Mesh();
            testRender.SetVertices(Primitives.GetSphereVertices(128, 128, 0.75f));

            int grid = 7;
            float spacing = 2f;

            float half = (grid - 1) / 2f;

            for (int j = 0; j < grid; j++)
            {
                GameObject sphereObject = scene.AddGameObject($"Sphere_{j}");

                sphereObject.Transform.Position = new Vector3(
                    (j - half) * spacing,
                    0,
                    0
                );

                MeshRenderer mr = sphereObject.AddComponent<MeshRenderer>();

                float metallic = 1.0f;
                float roughness = j / (float)(grid - 1);

                mr.AddMesh(testRender);

                mr.material.SetFloat("Metallic", metallic);
                mr.material.SetFloat("Roughness", roughness);
                Console.WriteLine(roughness);
            }
        }

        public static void SpawnSphereGrid(Scene scene, Vector3 offset)
        {
            Mesh testRender = new Mesh();
            testRender.SetVertices(Primitives.GetSphereVertices(128, 128, 0.75f));

            int grid = 5;
            float spacing = 2f;

            float half = (grid - 1) / 2f;

            for (int i = 0; i < grid; i++)
            {
                for (int j = 0; j < grid; j++)
                {
                    GameObject sphereObject = scene.AddGameObject($"Sphere_{i}_{j}");

                    sphereObject.Transform.Position = new Vector3(
                        (j - half) * spacing,
                        (half - i) * spacing,
                        0
                    ) + offset;

                    MeshRenderer mr = sphereObject.AddComponent<MeshRenderer>();

                    float metallic = j / (float)(grid - 1);
                    float roughness = i / (float)(grid - 1);

                    mr.AddMesh(testRender);

                    mr.material.SetFloat("Metallic", metallic);
                    mr.material.SetFloat("Roughness", roughness);
                }
            }
        }

    }
}
