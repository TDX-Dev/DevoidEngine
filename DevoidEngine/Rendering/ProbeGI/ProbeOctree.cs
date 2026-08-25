using DevoidEngine.Gizmos;
using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DevoidEngine.Rendering.ProbeGI
{
    public struct ProbeOctreeNode
    {
        public BoundingBox Bounds;

        public int FirstChild;

        public int CandidateStart;
        public int CandidateCount;

        public int Depth;

        public OctreeNodeClassification Classification;

        public bool IsLeaf;
    }
    public enum OctreeNodeClassification
    {
        Empty,
        Uniform,
        Mixed
    }


    public class ProbeOctree
    {
        readonly List<ProbeOctreeNode> nodes = [];

        public IReadOnlyList<ProbeOctreeNode> Nodes => nodes;

        static readonly Vector3[] SampleOffsets =
        [
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(0.5f, 0.5f, 0.5f)
        ];

        public void Build(Vector3 min, Vector3 max, BVH bvh, int maxDepth, float minCellSize)
        {
            nodes.Clear();

            if (maxDepth < 0)
                return;

            BoundingBox bounds = new(Vector3.Min(min, max), Vector3.Max(min, max));

            nodes.Add(new ProbeOctreeNode
            {
                Bounds = bounds,
                FirstChild = -1,
                CandidateStart = -1,
                CandidateCount = 0,
                Depth = 0,
                Classification = OctreeNodeClassification.Empty,
                IsLeaf = true
            });

            BuildNode(0, bvh, maxDepth, minCellSize);
        }

        void BuildNode(int nodeIndex, BVH bvh, int maxDepth, float minCellSize)
        {
            ProbeOctreeNode node = nodes[nodeIndex];

            OctreeNodeClassification classification = ClassifyNode(
                node.Bounds,
                bvh);

            node.Classification = classification;

            if (classification == OctreeNodeClassification.Empty)
            {
                node.IsLeaf = true;
                nodes[nodeIndex] = node;
                return;
            }

            Vector3 size = node.Bounds.max - node.Bounds.min;

            if (node.Depth >= maxDepth ||
                (size.X <= minCellSize &&
                 size.Y <= minCellSize &&
                 size.Z <= minCellSize))
            {
                node.IsLeaf = true;
                nodes[nodeIndex] = node;
                return;
            }

            //if (!bvh.Intersects(node.Bounds))
            //{
            //    node.Classification = OctreeNodeClassification.Empty;
            //    node.IsLeaf = true;
            //    nodes[nodeIndex] = node;
            //    return;
            //}

            Vector3 center = (node.Bounds.min + node.Bounds.max) * 0.5f;

            int firstChild = nodes.Count;

            for (int i = 0; i < 8; i++)
            {
                Vector3 childMin = new(
                    (i & 1) == 0 ? node.Bounds.min.X : center.X,
                    (i & 2) == 0 ? node.Bounds.min.Y : center.Y,
                    (i & 4) == 0 ? node.Bounds.min.Z : center.Z);

                Vector3 childMax = new(
                    (i & 1) == 0 ? center.X : node.Bounds.max.X,
                    (i & 2) == 0 ? center.Y : node.Bounds.max.Y,
                    (i & 4) == 0 ? center.Z : node.Bounds.max.Z);

                nodes.Add(new ProbeOctreeNode
                {
                    Bounds = new BoundingBox(childMin, childMax),
                    FirstChild = -1,
                    CandidateStart = -1,
                    CandidateCount = 0,
                    Depth = node.Depth + 1,
                    Classification = OctreeNodeClassification.Empty,
                    IsLeaf = true
                });
            }

            node.FirstChild = firstChild;
            node.IsLeaf = false;

            nodes[nodeIndex] = node;

            for (int i = 0; i < 8; i++)
            {
                BuildNode(
                    firstChild + i,
                    bvh,
                    maxDepth,
                    minCellSize);
            }
        }

        public int GetMaxDepth()
        {
            if (nodes.Count == 0)
                return 0;

            return GetMaxDepth(0);
        }

        int GetMaxDepth(int nodeIndex)
        {
            ProbeOctreeNode node = nodes[nodeIndex];

            if (node.IsLeaf)
                return node.Depth;

            int maxDepth = node.Depth;

            for (int i = 0; i < 8; i++)
                maxDepth = Math.Max(
                    maxDepth,
                    GetMaxDepth(node.FirstChild + i));

            return maxDepth;
        }
        public bool IsCandidate(Vector3 position)
        {
            if (nodes.Count == 0)
                return false;

            return IsCandidate(0, position);
        }

        bool IsCandidate(int nodeIndex, Vector3 position)
        {
            ProbeOctreeNode node = nodes[nodeIndex];

            if (position.X < node.Bounds.min.X ||
                position.X > node.Bounds.max.X ||
                position.Y < node.Bounds.min.Y ||
                position.Y > node.Bounds.max.Y ||
                position.Z < node.Bounds.min.Z ||
                position.Z > node.Bounds.max.Z)
                return false;

            if (node.Classification == OctreeNodeClassification.Empty)
                return false;

            if (node.IsLeaf)
                return true;

            Vector3 center = (node.Bounds.min + node.Bounds.max) * 0.5f;

            int child = 0;

            if (position.X >= center.X)
                child |= 1;

            if (position.Y >= center.Y)
                child |= 2;

            if (position.Z >= center.Z)
                child |= 4;

            return IsCandidate(
                node.FirstChild + child,
                position);
        }
        public void GetMixedLeaves(List<BoundingBox> leaves)
        {
            leaves.Clear();

            if (nodes.Count == 0)
                return;

            GetMixedLeaves(0, leaves);
        }

        void GetMixedLeaves(
            int nodeIndex,
            List<BoundingBox> leaves)
        {
            ProbeOctreeNode node = nodes[nodeIndex];

            if (node.IsLeaf)
            {
                if (node.Classification == OctreeNodeClassification.Mixed)
                    leaves.Add(node.Bounds);

                return;
            }

            for (int i = 0; i < 8; i++)
            {
                GetMixedLeaves(
                    node.FirstChild + i,
                    leaves);
            }
        }

        public void DrawGizmos(
            GizmoContext context,
            GizmoMaterial material,
            int level)
        {
            if (nodes.Count == 0)
                return;

            DrawNodeGizmos(
                context,
                material,
                0,
                level);
        }

        void DrawNodeGizmos(
            GizmoContext context,
            GizmoMaterial material,
            int nodeIndex,
            int targetDepth)
        {
            ProbeOctreeNode node = nodes[nodeIndex];

            if (node.Depth == targetDepth)
            {
                context.DrawList.AddWireBox(
                    node.Bounds.min,
                    node.Bounds.max,
                    material);

                return;
            }

            if (node.IsLeaf)
                return;

            for (int i = 0; i < 8; i++)
            {
                DrawNodeGizmos(
                    context,
                    material,
                    node.FirstChild + i,
                    targetDepth);
            }
        }

        public void DrawAllGizmos(
            GizmoContext context,
            GizmoMaterial material)
        {
            if (nodes.Count == 0)
                return;

            DrawAllNodeGizmos(
                context,
                material,
                0);
        }

        void DrawAllNodeGizmos(
            GizmoContext context,
            GizmoMaterial material,
            int nodeIndex)
        {
            ProbeOctreeNode node = nodes[nodeIndex];

            context.DrawList.AddWireBox(
                node.Bounds.min,
                node.Bounds.max,
                material);

            if (node.IsLeaf)
                return;

            for (int i = 0; i < 8; i++)
            {
                DrawAllNodeGizmos(
                    context,
                    material,
                    node.FirstChild + i);
            }
        }

        public void DrawLeafGizmos(
            GizmoContext context,
            GizmoMaterial material)
        {
            if (nodes.Count == 0)
                return;

            DrawLeafNodeGizmos(
                context,
                material,
                0);
        }

        void DrawLeafNodeGizmos(
            GizmoContext context,
            GizmoMaterial material,
            int nodeIndex)
        {
            ProbeOctreeNode node = nodes[nodeIndex];

            if (node.IsLeaf)
            {
                context.DrawList.AddWireBox(
                    node.Bounds.min,
                    node.Bounds.max,
                    material);

                return;
            }

            for (int i = 0; i < 8; i++)
            {
                DrawLeafNodeGizmos(
                    context,
                    material,
                    node.FirstChild + i);
            }
        }

        OctreeNodeClassification ClassifyNode(BoundingBox bounds, BVH bvh)
        {
            Vector3 size = bounds.max - bounds.min;

            bool foundInside = false;
            bool foundOutside = false;

            for (int i = 0; i < SampleOffsets.Length; i++)
            {
                Vector3 position = bounds.min + size * SampleOffsets[i];

                int insideVotes = 0;

                for (int j = 0; j < 6; j++)
                {
                    int intersections = bvh.CountIntersections(
                        position,
                        ProbeGISystem.ProbeDirections[j],
                        10000.0f);

                    if ((intersections & 1) != 0)
                        insideVotes++;
                }

                bool inside = insideVotes >= 4;

                if (inside)
                    foundInside = true;
                else
                    foundOutside = true;

                if (foundInside && foundOutside)
                    return OctreeNodeClassification.Mixed;
            }

            if (foundInside)
                return OctreeNodeClassification.Uniform;

            return OctreeNodeClassification.Empty;
        }
    }
}
