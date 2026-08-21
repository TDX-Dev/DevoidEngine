using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.ProbeGI
{
    public sealed class KDTree
    {
        private readonly Vector3[] points;
        private readonly int[] pointIndices;

        private readonly int[] nodePointIndices;
        private readonly int[] nodeLeft;
        private readonly int[] nodeRight;
        private readonly byte[] nodeAxes;

        private int nodeCount;
        private readonly int root;

        public int Count => points.Length;

        public KDTree(Vector3[] points)
        {
            ArgumentNullException.ThrowIfNull(points);

            this.points = points;

            if (points.Length == 0)
            {
                pointIndices = [];
                nodePointIndices = [];
                nodeLeft = [];
                nodeRight = [];
                nodeAxes = [];
                root = -1;
                return;
            }

            pointIndices = new int[points.Length];

            for (int i = 0; i < points.Length; i++)
                pointIndices[i] = i;

            nodePointIndices = new int[points.Length];
            nodeLeft = new int[points.Length];
            nodeRight = new int[points.Length];
            nodeAxes = new byte[points.Length];

            Array.Fill(nodeLeft, -1);
            Array.Fill(nodeRight, -1);

            root = Build(0, pointIndices.Length - 1, 0);
        }

        public int Nearest(Vector3 position, int excludedIndex)
        {
            if (root == -1)
                return -1;

            int bestIndex = -1;
            float bestDistanceSquared = float.MaxValue;

            SearchNearest(root, position, excludedIndex, ref bestIndex, ref bestDistanceSquared);

            return bestIndex;
        }
        public int Nearest(Vector3 position)
        {
            if (root == -1)
                return -1;

            int bestIndex = -1;
            float bestDistanceSquared = float.MaxValue;

            SearchNearest(root, position, -1, ref bestIndex, ref bestDistanceSquared);

            return bestIndex;
        }
        public int Nearest(Vector3 position, float radius, int excludedIndex)
        {
            if (root == -1)
                return -1;

            float radiusSquared = radius * radius;

            int bestIndex = -1;
            float bestDistanceSquared = radiusSquared;

            SearchNearest(
                root,
                position,
                radiusSquared,
                excludedIndex,
                ref bestIndex,
                ref bestDistanceSquared);

            return bestIndex;
        }

        public Vector3 NearestPoint(Vector3 position)
        {
            int index = Nearest(position);

            if (index == -1)
                throw new InvalidOperationException("KD-tree contains no points.");

            return points[index];
        }

        private int Build(int left, int right, int depth)
        {
            if (left > right)
                return -1;

            int axis = depth % 3;
            int middle = left + ((right - left) >> 1);

            QuickSelect(pointIndices, left, right, middle, axis);

            int node = nodeCount++;

            nodePointIndices[node] = pointIndices[middle];
            nodeAxes[node] = (byte)axis;

            nodeLeft[node] = Build(left, middle - 1, depth + 1);
            nodeRight[node] = Build(middle + 1, right, depth + 1);

            return node;
        }

        private void SearchNearest(int node, Vector3 target, int excludedIndex, ref int bestIndex, ref float bestDistanceSquared)
        {
            if (node == -1)
                return;

            int pointIndex = nodePointIndices[node];
            Vector3 point = points[pointIndex];

            if (pointIndex != excludedIndex)
            {
                Vector3 delta = point - target;
                float distanceSquared = Vector3.Dot(delta, delta);

                if (distanceSquared < bestDistanceSquared)
                {
                    bestDistanceSquared = distanceSquared;
                    bestIndex = pointIndex;
                }
            }

            int axis = nodeAxes[node];

            float targetCoordinate = GetAxis(target, axis);
            float pointCoordinate = GetAxis(point, axis);

            int nearNode;
            int farNode;

            if (targetCoordinate < pointCoordinate)
            {
                nearNode = nodeLeft[node];
                farNode = nodeRight[node];
            }
            else
            {
                nearNode = nodeRight[node];
                farNode = nodeLeft[node];
            }

            SearchNearest(
                nearNode,
                target,
                excludedIndex,
                ref bestIndex,
                ref bestDistanceSquared);

            float axisDistance = targetCoordinate - pointCoordinate;

            if (axisDistance * axisDistance < bestDistanceSquared)
            {
                SearchNearest(
                    farNode,
                    target,
                    excludedIndex,
                    ref bestIndex,
                    ref bestDistanceSquared);
            }
        }
        private void SearchNearest(int node, Vector3 target, float radiusSquared, int excludedIndex, ref int bestIndex, ref float bestDistanceSquared)
        {
            if (node == -1)
                return;

            int pointIndex = nodePointIndices[node];
            Vector3 point = points[pointIndex];

            if (pointIndex != excludedIndex)
            {
                Vector3 delta = point - target;
                float distanceSquared = Vector3.Dot(delta, delta);

                if (distanceSquared <= bestDistanceSquared)
                {
                    bestDistanceSquared = distanceSquared;
                    bestIndex = pointIndex;
                }
            }

            int axis = nodeAxes[node];

            float targetCoordinate = GetAxis(target, axis);
            float pointCoordinate = GetAxis(point, axis);

            int nearNode;
            int farNode;

            if (targetCoordinate < pointCoordinate)
            {
                nearNode = nodeLeft[node];
                farNode = nodeRight[node];
            }
            else
            {
                nearNode = nodeRight[node];
                farNode = nodeLeft[node];
            }

            SearchNearest(
                nearNode,
                target,
                radiusSquared,
                excludedIndex,
                ref bestIndex,
                ref bestDistanceSquared);

            float axisDistance = targetCoordinate - pointCoordinate;

            if (axisDistance * axisDistance <= bestDistanceSquared)
            {
                SearchNearest(
                    farNode,
                    target,
                    radiusSquared,
                    excludedIndex,
                    ref bestIndex,
                    ref bestDistanceSquared);
            }
        }
        private static float GetAxis(Vector3 value, int axis)
        {
            return axis switch
            {
                0 => value.X,
                1 => value.Y,
                _ => value.Z
            };
        }

        private void QuickSelect(int[] array, int left, int right, int target, int axis)
        {
            while (left < right)
            {
                float pivot = GetAxis(points[array[target]], axis);

                int less = left;
                int current = left;
                int greater = right;

                while (current <= greater)
                {
                    float value = GetAxis(points[array[current]], axis);

                    if (value < pivot)
                    {
                        Swap(array, less, current);
                        less++;
                        current++;
                    }
                    else if (value > pivot)
                    {
                        Swap(array, current, greater);
                        greater--;
                    }
                    else
                    {
                        current++;
                    }
                }

                if (target < less)
                {
                    right = less - 1;
                }
                else if (target > greater)
                {
                    left = greater + 1;
                }
                else
                {
                    return;
                }
            }
        }

        private static void Swap(int[] array, int a, int b)
        {
            if (a == b)
                return;

            (array[b], array[a]) = (array[a], array[b]);
        }
    }
}
