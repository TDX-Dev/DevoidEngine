using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Rendering;
using DevoidEngine.Rendering.ProbeGI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Components
{
    public class ProbeGI : Component, IGizmoProviderComponent
    {
        public override string Type => nameof(ProbeGI);
        public override ComponentTickMode TickMode => ComponentTickMode.All;

        GizmoMaterial GizmoMaterial = new();

        public bool Regenerate = false;

        private readonly List<Vector3> subDividedPoints = [];

        public ProbeGI()
        {
            GizmoMaterial.Color = new Vector4(0, 1, 0, 1);
        }

        public override void OnStart()
        {
            RegeneratePoints();
        }

        public void RegeneratePoints()
        {
            subDividedPoints.Clear();
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<RenderMeshData> data_meshes = [];
            gameObject.Scene.World.GetStaticMeshes(data_meshes);

            for (int i = 0; i < data_meshes.Count; i++)
            {
                List<Vector3> points = MeshTriangularSubdivide.Subdivide(data_meshes[i].render_mesh.Positions!, data_meshes[i].render_mesh.Indices!, 0.4f);
                for (int j = 0; j < points.Count; j++)
                {
                    points[j] = Vector3.Transform(
                        points[j],
                        data_meshes[i].render_transform
                    );
                }
                subDividedPoints.AddRange(points);
            }

            stopwatch.Stop();
            Console.WriteLine($"Output points: {subDividedPoints.Count}");
            Console.WriteLine($"Subdivision time: {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
        }

        public void OnDrawGizmos(GizmoContext context)
        {
            if (Regenerate)
            {
                RegeneratePoints();
                Regenerate = false;
            }


            List<GameObject> gameObjects = gameObject.Scene.GameObjects;

            foreach (var gameObject in gameObjects)
            {
                foreach (var component in gameObject.Components)
                {
                    if (component is MeshRenderer mr)
                    {
                        if (mr.mesh == null)
                            continue;

                        for (int i = 0; i < mr.mesh.Positions!.Length; i++)
                        {
                            Vector3 position = Vector3.Transform(mr.mesh.Positions![i], mr.gameObject.Transform.WorldMatrix);
                            context.DrawList.AddCircle(position, 0.5f, Vector3.UnitY, GizmoMaterial);

                        }

                    }
                }
            }
            for (int i = 0; i < subDividedPoints.Count; i++)
            {
                Vector3 position = subDividedPoints[i];
                context.DrawList.AddCircle(position, 0.5f, Vector3.UnitY, GizmoMaterial);

            }
        }
    }
}
