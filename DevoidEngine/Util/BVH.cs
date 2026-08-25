using DevoidEngine.Core;
using DevoidEngine.Gizmos;
using DevoidEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DevoidEngine.Util
{
    public struct BVHTriangle
    {
        public int TriangleIndex;

        public Vector3 V0;
        public Vector3 V1;
        public Vector3 V2;

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

        public void AddTriangle(int triangleIndex, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            Vector3 min = Vector3.Min(v0, Vector3.Min(v1, v2));
            Vector3 max = Vector3.Max(v0, Vector3.Max(v1, v2));

            triangles.Add(new BVHTriangle
            {
                TriangleIndex = triangleIndex,
                V0 = v0,
                V1 = v1,
                V2 = v2,
                Bounds = new BoundingBox(min, max),
                Centroid = (v0 + v1 + v2) / 3.0f
            });
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
        static bool IntersectTriangle(Vector3 origin, Vector3 direction, Vector3 v0, Vector3 v1, Vector3 v2, out float distance)
        {
            const float epsilon = 1e-7f;

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;

            Vector3 p = Vector3.Cross(direction, edge2);

            float determinant = Vector3.Dot(edge1, p);

            if (MathF.Abs(determinant) < epsilon)
            {
                distance = 0;
                return false;
            }

            float inverseDeterminant = 1.0f / determinant;

            Vector3 t = origin - v0;

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

        public bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out float distance, out int triangleIndex)
        {
            distance = maxDistance;
            triangleIndex = -1;

            if (nodes.Count == 0)
                return false;

            if (!BoundingBox.IntersectsRay(nodes[0].Bounds, origin, direction, maxDistance, out _))
            {
                return false;
            }

            return RaycastNode(0, origin, direction, ref distance, ref triangleIndex);
        }

        bool RaycastNode(int nodeIndex, Vector3 origin, Vector3 direction, ref float closestDistance, ref int closestTriangle)
        {
            BVHNode node = nodes[nodeIndex];

            if (!BoundingBox.IntersectsRay(
                node.Bounds,
                origin,
                direction,
                closestDistance,
                out _))
            {
                return false;
            }

            if (node.Left < 0)
            {
                bool leafHit = false;

                int end = node.FirstTriangle + node.TriangleCount;

                for (int i = node.FirstTriangle; i < end; i++)
                {
                    BVHTriangle triangle = triangles[i];

                    if (!IntersectTriangle(
                        origin,
                        direction,
                        triangle.V0,
                        triangle.V1,
                        triangle.V2,
                        out float triangleDistance))
                    {
                        continue;
                    }

                    if (triangleDistance >= closestDistance)
                        continue;

                    closestDistance = triangleDistance;
                    closestTriangle = triangle.TriangleIndex;

                    leafHit = true;
                }

                return leafHit;
            }

            int leftChild = node.Left;
            int rightChild = node.Right;

            bool leftHit = BoundingBox.IntersectsRay(
                nodes[leftChild].Bounds,
                origin,
                direction,
                closestDistance,
                out float leftDistance);

            bool rightHit = BoundingBox.IntersectsRay(
                nodes[rightChild].Bounds,
                origin,
                direction,
                closestDistance,
                out float rightDistance);

            if (!leftHit && !rightHit)
                return false;

            bool hit = false;

            if (leftHit && rightHit)
            {
                if (leftDistance <= rightDistance)
                {
                    hit |= RaycastNode(
                        leftChild,
                        origin,
                        direction,
                        ref closestDistance,
                        ref closestTriangle);

                    if (rightDistance <= closestDistance)
                    {
                        hit |= RaycastNode(
                            rightChild,
                            origin,
                            direction,
                            ref closestDistance,
                            ref closestTriangle);
                    }
                }
                else
                {
                    hit |= RaycastNode(
                        rightChild,
                        origin,
                        direction,
                        ref closestDistance,
                        ref closestTriangle);

                    if (leftDistance <= closestDistance)
                    {
                        hit |= RaycastNode(
                            leftChild,
                            origin,
                            direction,
                            ref closestDistance,
                            ref closestTriangle);
                    }
                }
            }
            else if (leftHit)
            {
                hit = RaycastNode(
                    leftChild,
                    origin,
                    direction,
                    ref closestDistance,
                    ref closestTriangle);
            }
            else
            {
                hit = RaycastNode(
                    rightChild,
                    origin,
                    direction,
                    ref closestDistance,
                    ref closestTriangle);
            }

            return hit;
        }

        public int CountIntersections(Vector3 origin, Vector3 direction, float maxDistance)
        {
            if (nodes.Count == 0)
                return 0;

            if (!BoundingBox.IntersectsRay(
                nodes[0].Bounds,
                origin,
                direction,
                maxDistance,
                out _))
            {
                return 0;
            }

            return CountIntersectionsNode(
                0,
                origin,
                direction,
                maxDistance);
        }

        int CountIntersectionsNode(int nodeIndex, Vector3 origin, Vector3 direction, float maxDistance)
        {
            BVHNode node = nodes[nodeIndex];

            if (!BoundingBox.IntersectsRay(
                node.Bounds,
                origin,
                direction,
                maxDistance,
                out _))
            {
                return 0;
            }

            if (node.Left < 0)
            {
                int count = 0;

                int end =
                    node.FirstTriangle +
                    node.TriangleCount;

                for (int i = node.FirstTriangle; i < end; i++)
                {
                    BVHTriangle triangle = triangles[i];

                    if (IntersectTriangle(
                        origin,
                        direction,
                        triangle.V0,
                        triangle.V1,
                        triangle.V2,
                        out float distance))
                    {
                        if (distance <= maxDistance)
                            count++;
                    }
                }

                return count;
            }

            return
                CountIntersectionsNode(
                    node.Left,
                    origin,
                    direction,
                    maxDistance)
                +
                CountIntersectionsNode(
                    node.Right,
                    origin,
                    direction,
                    maxDistance);
        }
        public bool Intersects(BoundingBox bounds)
        {
            if (nodes.Count == 0)
                return false;

            return IntersectsNode(0, bounds);
        }

        bool IntersectsNode(int nodeIndex, BoundingBox bounds)
        {
            BVHNode node = nodes[nodeIndex];

            if (!BoundingBox.Intersects(node.Bounds, bounds))
                return false;

            if (node.Left < 0)
                return true;

            if (IntersectsNode(node.Left, bounds))
                return true;

            if (IntersectsNode(node.Right, bounds))
                return true;

            return false;
        }
        public bool ClosestPoint(Vector3 position, out Vector3 closestPoint, out float distanceSquared)
        {
            closestPoint = Vector3.Zero;
            distanceSquared = float.MaxValue;

            if (nodes.Count == 0)
                return false;

            ClosestPointNode(
                0,
                position,
                ref closestPoint,
                ref distanceSquared);

            return distanceSquared < float.MaxValue;
        }

        void ClosestPointNode(int nodeIndex, Vector3 position, ref Vector3 closestPoint, ref float closestDistanceSquared)
        {
            BVHNode node = nodes[nodeIndex];

            // If this node's AABB is already farther away than
            // our current closest point, nothing underneath it
            // can improve the result.
            float nodeDistanceSquared =
                BoundingBox.DistanceSquared(
                    node.Bounds,
                    position);

            if (nodeDistanceSquared >= closestDistanceSquared)
                return;

            // Leaf: test every triangle.
            if (node.Left < 0)
            {
                int end =
                    node.FirstTriangle +
                    node.TriangleCount;

                for (int i = node.FirstTriangle; i < end; i++)
                {
                    BVHTriangle triangle = triangles[i];

                    Vector3 point = ClosestPointOnTriangle(
                        position,
                        triangle.V0,
                        triangle.V1,
                        triangle.V2);

                    float distanceSquared =
                        Vector3.DistanceSquared(
                            position,
                            point);

                    if (distanceSquared >= closestDistanceSquared)
                        continue;

                    closestDistanceSquared = distanceSquared;
                    closestPoint = point;
                }

                return;
            }

            int left = node.Left;
            int right = node.Right;

            float leftDistanceSquared =
                BoundingBox.DistanceSquared(
                    nodes[left].Bounds,
                    position);

            float rightDistanceSquared =
                BoundingBox.DistanceSquared(
                    nodes[right].Bounds,
                    position);

            // Visit the closer child first so that we get a
            // small closestDistanceSquared as early as possible.
            if (leftDistanceSquared <= rightDistanceSquared)
            {
                if (leftDistanceSquared < closestDistanceSquared)
                {
                    ClosestPointNode(
                        left,
                        position,
                        ref closestPoint,
                        ref closestDistanceSquared);
                }

                if (rightDistanceSquared < closestDistanceSquared)
                {
                    ClosestPointNode(
                        right,
                        position,
                        ref closestPoint,
                        ref closestDistanceSquared);
                }
            }
            else
            {
                if (rightDistanceSquared < closestDistanceSquared)
                {
                    ClosestPointNode(
                        right,
                        position,
                        ref closestPoint,
                        ref closestDistanceSquared);
                }

                if (leftDistanceSquared < closestDistanceSquared)
                {
                    ClosestPointNode(
                        left,
                        position,
                        ref closestPoint,
                        ref closestDistanceSquared);
                }
            }
        }

        static Vector3 ClosestPointOnTriangle( Vector3 point, Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 ap = point - a;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);

            // Vertex A
            if (d1 <= 0.0f && d2 <= 0.0f)
                return a;

            Vector3 bp = point - b;

            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);

            // Vertex B
            if (d3 >= 0.0f && d4 <= d3)
                return b;

            float vc = d1 * d4 - d3 * d2;

            // Edge AB
            if (vc <= 0.0f &&
                d1 >= 0.0f &&
                d3 <= 0.0f)
            {
                float v = d1 / (d1 - d3);

                return a + v * ab;
            }

            Vector3 cp = point - c;

            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);

            // Vertex C
            if (d6 >= 0.0f && d5 <= d6)
                return c;

            float vb = d5 * d2 - d1 * d6;

            // Edge AC
            if (vb <= 0.0f &&
                d2 >= 0.0f &&
                d6 <= 0.0f)
            {
                float w = d2 / (d2 - d6);

                return a + w * ac;
            }

            float va = d3 * d6 - d5 * d4;

            // Edge BC
            if (va <= 0.0f &&
                (d4 - d3) >= 0.0f &&
                (d5 - d6) >= 0.0f)
            {
                float w =
                    (d4 - d3) /
                    ((d4 - d3) + (d5 - d6));

                return b + w * (c - b);
            }

            // Inside the triangle.
            float denominator = 1.0f / (va + vb + vc);

            float v2 = vb * denominator;
            float w2 = vc * denominator;

            return a + ab * v2 + ac * w2;
        }

        void DrawNodeGizmos(GizmoContext context, GizmoMaterial material, int nodeIndex, int depth, int targetDepth)
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

        public void DrawAllGizmos(GizmoContext context, GizmoMaterial material)
        {
            if (nodes.Count == 0)
                return;

            DrawAllNodeGizmos(context, material, 0);
        }

        void DrawAllNodeGizmos(GizmoContext context, GizmoMaterial material, int nodeIndex)
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

        void DrawLeafGizmos(GizmoContext context, GizmoMaterial material, int nodeIndex)
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