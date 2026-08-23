using DevoidEngine.Gizmos;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DevoidEngine.Util
{
    public struct BVHTriangle
    {
        public int MeshIndex;
        public int TriangleIndex;

        public BoundingBox Bounds;
        public Vector3 Centroid;
    }

    public struct BVHNode
    {
        public BoundingBox Bounds;

        public int Left;
        public int Right;

        public int FirstTriangle;
        public int TriangleCount;
    }

    struct SAHBucket
    {
        public int Count;
        public BoundingBox Bounds;
    }

    public class BVH
    {
        const int BucketCount = 12;

        readonly uint maxPrimitivesPerNode;

        readonly List<BVHTriangle> triangles = [];
        readonly List<BVHNode> nodes = [];

        public IReadOnlyList<BVHTriangle> Triangles => triangles;
        public IReadOnlyList<BVHNode> Nodes => nodes;

        public BVH(uint maxPrimitivesPerNode)
        {
            this.maxPrimitivesPerNode = Math.Min(255, maxPrimitivesPerNode);
        }

        public void Build()
        {
            nodes.Clear();

            if (triangles.Count == 0)
                return;

            nodes.Add(new BVHNode());

            BuildNode(0, 0, triangles.Count);
        }

        void BuildNode(int nodeIndex, int firstTriangle, int triangleCount)
        {
            BoundingBox bounds = BoundingBox.CreateEmptyBounds();

            for (int i = firstTriangle; i < firstTriangle + triangleCount; i++)
                bounds = BoundingBox.Union(bounds, triangles[i].Bounds);

            if (triangleCount <= maxPrimitivesPerNode)
            {
                BVHNode leaf = new()
                {
                    Bounds = bounds,
                    Left = -1,
                    Right = -1,
                    FirstTriangle = firstTriangle,
                    TriangleCount = triangleCount
                };

                nodes[nodeIndex] = leaf;
                return;
            }

            BoundingBox centroidBounds = BoundingBox.CreateEmptyBounds();

            for (int i = firstTriangle; i < firstTriangle + triangleCount; i++)
            {
                Vector3 centroid = triangles[i].Centroid;
                centroidBounds = BoundingBox.Union(centroidBounds, new BoundingBox(centroid, centroid));
            }

            int axis = LargestAxis(centroidBounds.max - centroidBounds.min);

            if (centroidBounds.max[axis] <= centroidBounds.min[axis])
            {
                BVHNode leaf = new()
                {
                    Bounds = bounds,
                    Left = -1,
                    Right = -1,
                    FirstTriangle = firstTriangle,
                    TriangleCount = triangleCount
                };

                nodes[nodeIndex] = leaf;
                return;
            }

            int bestBucket = FindBestSplit(firstTriangle, triangleCount, centroidBounds, bounds, axis);

            if (bestBucket < 0)
            {
                BVHNode leaf = new()
                {
                    Bounds = bounds,
                    Left = -1,
                    Right = -1,
                    FirstTriangle = firstTriangle,
                    TriangleCount = triangleCount
                };

                nodes[nodeIndex] = leaf;
                return;
            }

            int mid = Partition(firstTriangle, triangleCount, centroidBounds, axis, bestBucket);

            if (mid == firstTriangle || mid == firstTriangle + triangleCount)
            {
                BVHNode leaf = new()
                {
                    Bounds = bounds,
                    Left = -1,
                    Right = -1,
                    FirstTriangle = firstTriangle,
                    TriangleCount = triangleCount
                };

                nodes[nodeIndex] = leaf;
                return;
            }

            int leftNode = nodes.Count;
            nodes.Add(new BVHNode());

            int rightNode = nodes.Count;
            nodes.Add(new BVHNode());

            nodes[nodeIndex] = new BVHNode
            {
                Bounds = bounds,
                Left = leftNode,
                Right = rightNode,
                FirstTriangle = -1,
                TriangleCount = 0
            };

            BuildNode(
                leftNode,
                firstTriangle,
                mid - firstTriangle);

            BuildNode(
                rightNode,
                mid,
                firstTriangle + triangleCount - mid);
        }

        public void AddTriangle(int meshIndex, int triangleIndex, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 min = Vector3.Min(v0, Vector3.Min(v1, v2));
            Vector3 max = Vector3.Max(v0, Vector3.Max(v1, v2));

            BoundingBox bounds = new(min, max);

            BVHTriangle triangle = new()
            {
                MeshIndex = meshIndex,
                TriangleIndex = triangleIndex,
                Bounds = bounds,
                Centroid = (v0 + v1 + v2) / 3.0f
            };

            triangles.Add(triangle);
        }

        int FindBestSplit(int firstTriangle, int triangleCount, BoundingBox centroidBounds, BoundingBox nodeBounds, int axis)
        {
            Span<SAHBucket> buckets = stackalloc SAHBucket[BucketCount];

            float min = centroidBounds.min[axis];
            float max = centroidBounds.max[axis];
            float scale = BucketCount / (max - min);

            for (int i = firstTriangle; i < firstTriangle + triangleCount; i++)
            {
                int bucketIndex = GetBucket(triangles[i].Centroid, min, scale, axis);

                if (buckets[bucketIndex].Count == 0)
                    buckets[bucketIndex].Bounds = triangles[i].Bounds;
                else
                    buckets[bucketIndex].Bounds = BoundingBox.Union(buckets[bucketIndex].Bounds, triangles[i].Bounds);

                buckets[bucketIndex].Count++;
            }

            float parentArea = BoundingBox.SurfaceArea(nodeBounds);

            float bestCost = float.MaxValue;
            int bestSplit = -1;

            Span<int> leftCounts = stackalloc int[BucketCount - 1];
            Span<int> rightCounts = stackalloc int[BucketCount - 1];

            Span<BoundingBox> leftBounds = stackalloc BoundingBox[BucketCount - 1];
            Span<BoundingBox> rightBounds = stackalloc BoundingBox[BucketCount - 1];

            int count = 0;
            BoundingBox bounds = BoundingBox.CreateEmptyBounds();

            for (int i = 0; i < BucketCount - 1; i++)
            {
                if (buckets[i].Count > 0)
                {
                    bounds = count == 0 ? buckets[i].Bounds : BoundingBox.Union(bounds, buckets[i].Bounds);
                    count += buckets[i].Count;
                }

                leftCounts[i] = count;
                leftBounds[i] = bounds;
            }

            count = 0;
            bounds = BoundingBox.CreateEmptyBounds();

            for (int i = BucketCount - 1; i > 0; i--)
            {
                if (buckets[i].Count > 0)
                {
                    bounds = count == 0 ? buckets[i].Bounds : BoundingBox.Union(bounds, buckets[i].Bounds);
                    count += buckets[i].Count;
                }

                rightCounts[i - 1] = count;
                rightBounds[i - 1] = bounds;
            }

            for (int i = 0; i < BucketCount - 1; i++)
            {
                if (leftCounts[i] == 0 || rightCounts[i] == 0)
                    continue;

                float leftArea = BoundingBox.SurfaceArea(leftBounds[i]);
                float rightArea = BoundingBox.SurfaceArea(rightBounds[i]);

                float cost = 1.0f + (leftArea / parentArea) * leftCounts[i] + (rightArea / parentArea) * rightCounts[i];

                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestSplit = i;
                }
            }

            float leafCost = triangleCount;

            if (bestCost >= leafCost)
                return -1;

            return bestSplit;
        }

        int Partition(int firstTriangle, int triangleCount, BoundingBox centroidBounds, int axis, int split)
        {
            int left = firstTriangle;
            int right = firstTriangle + triangleCount - 1;

            float min = centroidBounds.min[axis];
            float max = centroidBounds.max[axis];
            float scale = BucketCount / (max - min);

            while (left <= right)
            {
                int leftBucket = GetBucket(triangles[left].Centroid, min, scale, axis);

                if (leftBucket <= split)
                {
                    left++;
                    continue;
                }

                int rightBucket = GetBucket(triangles[right].Centroid, min, scale, axis);

                if (rightBucket > split)
                {
                    right--;
                    continue;
                }

                (triangles[right], triangles[left]) = (triangles[left], triangles[right]);
                left++;
                right--;
            }

            return left;
        }

        static int GetBucket(Vector3 centroid, float min, float scale, int axis)
        {
            int bucket = (int)((centroid[axis] - min) * scale);
            return Math.Clamp(bucket, 0, BucketCount - 1);
        }

        static int LargestAxis(Vector3 v)
        {
            if (v.X > v.Y && v.X > v.Z)
                return 0;

            if (v.Y > v.Z)
                return 1;

            return 2;
        }

        public int GetMaxDepth()
        {
            if (nodes.Count == 0)
                return 0;

            return GetMaxDepth(0, 0);
        }

        int GetMaxDepth(int nodeIndex, int depth)
        {
            BVHNode node = nodes[nodeIndex];

            int maxDepth = depth;

            if (node.Left >= 0)
                maxDepth = Math.Max(maxDepth, GetMaxDepth(node.Left, depth + 1));

            if (node.Right >= 0)
                maxDepth = Math.Max(maxDepth, GetMaxDepth(node.Right, depth + 1));

            return maxDepth;
        }

        public void DrawGizmos(GizmoContext context, GizmoMaterial material, int level)
        {
            if (nodes.Count == 0)
                return;

            DrawNodeGizmos(context, material, 0, 0, level);
        }
        static bool IntersectTriangle(
    Vector3 origin,
    Vector3 direction,
    BVHTriangle triangle,
    out float distance)
        {
            const float epsilon = 1e-7f;

            Vector3 edge1 = triangle.V1 - triangle.V0;
            Vector3 edge2 = triangle.V2 - triangle.V0;

            Vector3 p = Vector3.Cross(direction, edge2);

            float determinant = Vector3.Dot(edge1, p);

            if (MathF.Abs(determinant) < epsilon)
            {
                distance = 0;
                return false;
            }

            float inverseDeterminant = 1.0f / determinant;

            Vector3 t = origin - triangle.V0;

            float u = Vector3.Dot(t, p) * inverseDeterminant;

            if (u < 0 || u > 1)
            {
                distance = 0;
                return false;
            }

            Vector3 q = Vector3.Cross(t, edge1);

            float v = Vector3.Dot(direction, q) * inverseDeterminant;

            if (v < 0 || u + v > 1)
            {
                distance = 0;
                return false;
            }

            float tHit = Vector3.Dot(edge2, q) * inverseDeterminant;

            if (tHit < epsilon)
            {
                distance = 0;
                return false;
            }

            distance = tHit;

            return true;
        }
        void DrawNodeGizmos(
            GizmoContext context,
            GizmoMaterial material,
            int nodeIndex,
            int depth,
            int targetDepth)
        {
            BVHNode node = nodes[nodeIndex];

            if (depth == targetDepth)
            {
                context.DrawList.AddWireBox(
                    node.Bounds.min,
                    node.Bounds.max,
                    material);

                return;
            }

            if (node.Left >= 0)
            {
                DrawNodeGizmos(
                    context,
                    material,
                    node.Left,
                    depth + 1,
                    targetDepth);
            }

            if (node.Right >= 0)
            {
                DrawNodeGizmos(
                    context,
                    material,
                    node.Right,
                    depth + 1,
                    targetDepth);
            }
        }

        public void DrawAllGizmos(
            GizmoContext context,
            GizmoMaterial material)
        {
            if (nodes.Count == 0)
                return;

            DrawAllNodeGizmos(context, material, 0);
        }

        void DrawAllNodeGizmos(
            GizmoContext context,
            GizmoMaterial material,
            int nodeIndex)
        {
            BVHNode node = nodes[nodeIndex];

            context.DrawList.AddWireBox(
                node.Bounds.min,
                node.Bounds.max,
                material);

            if (node.Left >= 0)
                DrawAllNodeGizmos(context, material, node.Left);

            if (node.Right >= 0)
                DrawAllNodeGizmos(context, material, node.Right);
        }

        public void DrawLeafGizmos(GizmoContext context, GizmoMaterial material)
        {
            if (nodes.Count == 0)
                return;

            DrawLeafGizmos(context, material, 0);
        }

        void DrawLeafGizmos(
            GizmoContext context,
            GizmoMaterial material,
            int nodeIndex)
        {
            BVHNode node = nodes[nodeIndex];

            if (node.Left < 0 && node.Right < 0)
            {
                context.DrawList.AddWireBox(
                    node.Bounds.min,
                    node.Bounds.max,
                    material);

                return;
            }

            if (node.Left >= 0)
                DrawLeafGizmos(context, material, node.Left);

            if (node.Right >= 0)
                DrawLeafGizmos(context, material, node.Right);
        }
    }
}