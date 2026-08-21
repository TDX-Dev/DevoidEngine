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
        public float SubdivisionEdgeThreshold = 0.2f;
        public float InitialShrinkBallRadius = 25f;
        public float ClusterMergeDistance = 1f;
        public float MedialSphereThreshold = 2;
        public float ProbeSpacing = 0.75f;

        private List<MedialBall> medialBalls = [];
        private List<SurfacePoint> subDividedPoints = [];
        private KDTree pointCloud = null!;

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

            List<RenderMeshData> data_meshes = [];
            gameObject.Scene.World.GetStaticMeshes(data_meshes);

            Stopwatch stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < data_meshes.Count; i++)
            {
                List<SurfacePoint> points = ProbeGeometrySubdivide.GetSurfacePoints(data_meshes[i].render_mesh.Positions!, data_meshes[i].render_mesh.Indices!, SubdivisionEdgeThreshold);
                for (int j = 0; j < points.Count; j++)
                {
                    Vector3 position = Vector3.Transform(points[j].Position, data_meshes[i].render_transform);
                    SurfacePoint sp = points[j];
                    sp.Position = position;
                    points[j] = sp;
                }
                subDividedPoints.AddRange(points);
            }

            subDividedPoints = ProbeSurfacePointMerge.MergeClosePoints(subDividedPoints, ClusterMergeDistance);

            Vector3[] positions = new Vector3[subDividedPoints.Count];
            for (int i = 0; i < subDividedPoints.Count; i++)
                positions[i] = subDividedPoints[i].Position;
            pointCloud = new(positions);


            medialBalls.Clear();
            medialBalls.AddRange(new MedialBall[subDividedPoints.Count]);

            SurfacePoint[] finalPointArray = [.. subDividedPoints];

            Parallel.For(0, subDividedPoints.Count, i =>
            {
                medialBalls[i] = ProbeShrinkBall.ShrinkBall(
                    pointCloud,
                    finalPointArray,
                    i,
                    InitialShrinkBallRadius
                );
            });

            List<MedialBall> candidates = [];

            for (int i = 0; i < medialBalls.Count; i++)
            {
                if (medialBalls[i].Radius >= MedialSphereThreshold)
                    candidates.Add(medialBalls[i]);
            }

            candidates = ProbeSurfacePointMerge.ReduceCandidates(candidates, 0.5f);

            medialBalls = candidates;

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



            //foreach (var gameObject in gameObjects)
            //{
            //    foreach (var component in gameObject.Components)
            //    {
            //        if (component is MeshRenderer mr)
            //        {
            //            if (mr.mesh == null)
            //                continue;

            //            for (int i = 0; i < mr.mesh.Positions!.Length; i++)
            //            {
            //                Vector3 position = Vector3.Transform(mr.mesh.Positions![i], mr.gameObject.Transform.WorldMatrix);
            //                context.DrawList.AddCircle(position, 0.5f, Vector3.UnitY, GizmoMaterial);

            //            }

            //        }
            //    }
            //}
            for (int i = 0; i < medialBalls.Count; i++)
            {
                context.DrawList.AddCircle(
                    medialBalls[i].Center,
                    0.1f,
                    Vector3.UnitY,
                    GizmoMaterial);
            }
        }
    }
}
