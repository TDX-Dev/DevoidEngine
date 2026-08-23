using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Rendering;
using DevoidEngine.Rendering.ProbeGI;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Components
{
    public enum OctreeDebugMode
    {
        Level,
        All,
        Leaves
    }

    public class ProbeGI : Component, IGizmoProviderComponent
    {
        public override string Type => nameof(ProbeGI);
        public override ComponentTickMode TickMode => ComponentTickMode.All;

        GizmoMaterial GizmoMaterial = new();
        GizmoMaterial GizmoMaterialGrid = new();
        GizmoMaterial GizmoMaterialOctree = new();

        public bool Regenerate = false;
        public Vector3 Min;
        public Vector3 Max;
        public float ProbeSpacing = 2f;
        public int MaxProbes = 2000;


        private readonly List<SurfacePoint> subDividedPoints = [];
        private readonly List<Vector3> positions = [];
        private readonly List<(int x, int y, int z)> validCells = [];

        readonly ProbeOctree octree = new();

        public bool ShowOctree = false;
        public int OctreeLevel = 0;
        public OctreeDebugMode OctreeMode = OctreeDebugMode.Level;

        private Vector3 gridMin;
        private int xCount;
        private int yCount;
        private int zCount;
        private float spacing;

        public ProbeGI()
        {
            GizmoMaterial.Color = new Vector4(0, 1, 0, 1);
            GizmoMaterialGrid.Color = new Vector4(0.5f, 0.5f, 0.5f, 1);
            GizmoMaterialOctree.Color = new Vector4(0, 1, 1, 1);
        }

        public override void OnStart()
        {
            RegeneratePoints();
        }

        public void RegeneratePoints()
        {
            subDividedPoints.Clear();
            positions.Clear();
            validCells.Clear();

            if (ProbeSpacing <= 0 || MaxProbes <= 0)
                return;

            List<RenderMeshData> data_meshes = [];
            gameObject.Scene.World.GetStaticMeshes(data_meshes);

            Stopwatch stopwatch = Stopwatch.StartNew();

            Vector3 boundsMin = Vector3.Min(Min, Max);
            Vector3 boundsMax = Vector3.Max(Min, Max);

            Vector3 size = boundsMax - boundsMin;

            spacing = ProbeSpacing;

            xCount = Math.Max(1, (int)MathF.Floor(size.X / spacing));
            yCount = Math.Max(1, (int)MathF.Floor(size.Y / spacing));
            zCount = Math.Max(1, (int)MathF.Floor(size.Z / spacing));

            long totalProbes = (long)xCount * yCount * zCount;

            if (totalProbes > MaxProbes)
            {
                float scale = MathF.Pow((float)totalProbes / MaxProbes, 1f / 3f);
                spacing *= scale;

                xCount = Math.Max(1, (int)MathF.Floor(size.X / spacing));
                yCount = Math.Max(1, (int)MathF.Floor(size.Y / spacing));
                zCount = Math.Max(1, (int)MathF.Floor(size.Z / spacing));

                while ((long)xCount * yCount * zCount > MaxProbes)
                {
                    spacing *= 1.001f;

                    xCount = Math.Max(1, (int)MathF.Floor(size.X / spacing));
                    yCount = Math.Max(1, (int)MathF.Floor(size.Y / spacing));
                    zCount = Math.Max(1, (int)MathF.Floor(size.Z / spacing));
                }
            }

            Vector3 gridSize = new(
                xCount * spacing,
                yCount * spacing,
                zCount * spacing);

            Vector3 offset = (size - gridSize) * 0.5f;

            gridMin = boundsMin + offset;

            positions.Capacity = Math.Min(MaxProbes, (int)((long)xCount * yCount * zCount));

            for (int z = 0; z < zCount; z++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    for (int x = 0; x < xCount; x++)
                    {
                        Vector3 position = new(
                            gridMin.X + (x + 0.5f) * spacing,
                            gridMin.Y + (y + 0.5f) * spacing,
                            gridMin.Z + (z + 0.5f) * spacing);

                        positions.Add(position);
                        validCells.Add((x, y, z));
                    }
                }
            }

            BVH? bvh = gameObject.Scene.World.StaticBVH;

            if (bvh == null)
                return;

            octree.Build(boundsMin, boundsMax, gameObject.Scene.World.StaticBVH!, 4, ProbeSpacing);

            int beforeCount = positions.Count;

            for (int i = positions.Count - 1; i >= 0; i--)
            {
                Vector3 position = positions[i];

                int insideVotes = 0;

                for (int j = 0; j < ProbeGISystem.ProbeDirections.Length; j++)
                {
                    int intersections = bvh.CountIntersections(
                        position,
                        ProbeGISystem.ProbeDirections[j],
                        10000.0f);

                    if ((intersections & 1) != 0)
                        insideVotes++;
                }

                if (insideVotes < 4)
                {
                    positions.RemoveAt(i);
                    validCells.RemoveAt(i);
                }
                    
            }

            stopwatch.Stop();

            Console.WriteLine($"Input points: {beforeCount}");
            Console.WriteLine($"Output points: {positions.Count}");
            Console.WriteLine($"Eliminated points: {beforeCount - positions.Count}");
            Console.WriteLine($"Grid: {xCount} x {yCount} x {zCount}");
            Console.WriteLine($"Probe spacing: {spacing:F3}");
            Console.WriteLine($"Generation time: {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
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

            for (int i = 0; i < positions.Count; i++)
            {
                context.DrawList.AddCircle(positions[i], 0.5f, Vector3.UnitY, GizmoMaterialGrid);
            }

            context.DrawList.AddWireBox(Min, Max, GizmoMaterial);

            for (int i = 0; i < validCells.Count; i++)
            {
                (int x, int y, int z) = validCells[i];

                Vector3 cellMin = new(
                    gridMin.X + x * spacing,
                    gridMin.Y + y * spacing,
                    gridMin.Z + z * spacing
                );

                Vector3 cellMax = cellMin + new Vector3(spacing);

                context.DrawList.AddWireBox(cellMin, cellMax, GizmoMaterialGrid);
            }

            if (ShowOctree)
            {
                switch (OctreeMode)
                {
                    case OctreeDebugMode.Level:
                        {
                            int maxDepth = octree.GetMaxDepth();
                            int level = Math.Clamp(OctreeLevel, 0, maxDepth);

                            octree.DrawGizmos(context, GizmoMaterialOctree, level);

                            break;
                        }

                    case OctreeDebugMode.All:
                        octree.DrawAllGizmos(context, GizmoMaterialOctree);
                        break;

                    case OctreeDebugMode.Leaves:
                        octree.DrawLeafGizmos(context, GizmoMaterialOctree);
                        break;
                }
            }
        }
    }
}
