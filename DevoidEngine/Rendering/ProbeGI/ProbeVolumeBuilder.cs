using DevoidEngine.Util;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DevoidEngine.Rendering.ProbeGI
{
    public struct ProbeVolume
    {
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 GridMin;

        public int ResolutionX;
        public int ResolutionY;
        public int ResolutionZ;

        public float Spacing;

        // Number of probes in the complete grid.
        public int ProbeCount;

        public readonly Vector3 Size =>
            Max - Min;

        public readonly int GetIndex(int x, int y, int z)
        {
            return x + y * ResolutionX + z * ResolutionX * ResolutionY;
        }

        public readonly Vector3 GetPosition(int x, int y, int z)
        {
            return new Vector3(GridMin.X + (x + 0.5f) * Spacing, GridMin.Y + (y + 0.5f) * Spacing, GridMin.Z + (z + 0.5f) * Spacing);
        }

        public readonly Vector3 GetPosition(int index)
        {
            int xy = ResolutionX * ResolutionY;

            int z = index / xy;

            int remainder = index - z * xy;

            int y = remainder / ResolutionX;

            int x = remainder - y * ResolutionX;

            return GetPosition(x, y, z);
        }
    }

    public sealed class ProbeVolumeBuilder
    {
        public float ProbeSpacing { get; set; } = 2.0f;

        public float MaxProbeDistance { get; set; } = 4.0f;

        public int MaxProbes { get; set; } = 2000;

        public ProbeVolume Build(Vector3 min, Vector3 max, BVH bvh, List<Probe> probes, List<(int x, int y, int z)> validCells)
        {
            probes.Clear();
            validCells.Clear();

            if (ProbeSpacing <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(paramName: null, message: "Probe spacing must be greater than zero.");
            }

            if (MaxProbeDistance < 0.0f)
            {
                throw new ArgumentOutOfRangeException(paramName: null, message: "Maximum probe distance cannot be negative.");
            }

            if (MaxProbes <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName: null, message: "Maximum probe count must be greater than zero.");
            }

            Vector3 boundsMin = Vector3.Min(min, max);

            Vector3 boundsMax = Vector3.Max(min, max);

            Vector3 size = boundsMax - boundsMin;

            if (size.X <= 0.0f || size.Y <= 0.0f || size.Z <= 0.0f)
            {
                throw new ArgumentException("Probe volume must have non-zero extents.");
            }

            float spacing = ProbeSpacing;

            int xCount = Math.Max(1, (int)MathF.Floor(size.X / spacing));

            int yCount = Math.Max(1, (int)MathF.Floor(size.Y / spacing));

            int zCount = Math.Max(1, (int)MathF.Floor(size.Z / spacing));

            long totalProbes = (long)xCount * yCount * zCount;

            if (totalProbes > MaxProbes)
            {
                float scale =
                    MathF.Pow(
                        (float)totalProbes /
                        MaxProbes,
                        1.0f / 3.0f);

                spacing *= scale;

                xCount =
                    Math.Max(
                        1,
                        (int)MathF.Floor(
                            size.X / spacing));

                yCount =
                    Math.Max(
                        1,
                        (int)MathF.Floor(
                            size.Y / spacing));

                zCount =
                    Math.Max(
                        1,
                        (int)MathF.Floor(
                            size.Z / spacing));

                while (
                    (long)xCount *
                    yCount *
                    zCount >
                    MaxProbes)
                {
                    spacing *= 1.001f;

                    xCount =
                        Math.Max(
                            1,
                            (int)MathF.Floor(
                                size.X / spacing));

                    yCount =
                        Math.Max(
                            1,
                            (int)MathF.Floor(
                                size.Y / spacing));

                    zCount =
                        Math.Max(
                            1,
                            (int)MathF.Floor(
                                size.Z / spacing));
                }
            }

            int probeCount =
                checked(
                    xCount *
                    yCount *
                    zCount);

            Vector3 gridSize =
                new(
                    xCount * spacing,
                    yCount * spacing,
                    zCount * spacing);

            Vector3 offset =
                (size - gridSize) * 0.5f;

            Vector3 gridMin =
                boundsMin + offset;

            probes.Capacity =
                Math.Max(
                    probes.Capacity,
                    probeCount);

            validCells.Capacity =
                Math.Max(
                    validCells.Capacity,
                    probeCount);

            for (int z = 0;
                 z < zCount;
                 z++)
            {
                for (int y = 0;
                     y < yCount;
                     y++)
                {
                    for (int x = 0;
                         x < xCount;
                         x++)
                    {
                        Vector3 position =
                            new(
                                gridMin.X +
                                    (x + 0.5f) *
                                    spacing,

                                gridMin.Y +
                                    (y + 0.5f) *
                                    spacing,

                                gridMin.Z +
                                    (z + 0.5f) *
                                    spacing);

                        probes.Add(new Probe
                        {
                            Position = position,
                            Weight = 1.0f
                        });

                        validCells.Add(
                            (x, y, z));
                    }
                }
            }

            FilterProbes(bvh, probes);

            return new ProbeVolume
            {
                Min = boundsMin,
                Max = boundsMax,
                GridMin = gridMin,

                ResolutionX = xCount,
                ResolutionY = yCount,
                ResolutionZ = zCount,

                Spacing = spacing,

                ProbeCount = probeCount
            };
        }

        void FilterProbes(
            BVH bvh,
            List<Probe> probes)
        {
            float maxDistanceSquared =
                MaxProbeDistance *
                MaxProbeDistance;

            for (int i = 0;
                 i < probes.Count;
                 i++)
            {
                Probe probe =
                    probes[i];

                int insideVotes = 0;

                for (int j = 0;
                     j < ProbeGISystem.ProbeDirections.Length;
                     j++)
                {
                    int intersections =
                        bvh.CountIntersections(
                            probe.Position,
                            ProbeGISystem.ProbeDirections[j],
                            10000.0f);

                    if ((intersections & 1) != 0)
                        insideVotes++;
                }

                if (insideVotes < 4)
                {
                    probe.Weight = 0.0f;
                    probes[i] = probe;
                    continue;
                }

                if (!bvh.ClosestPoint(
                    probe.Position,
                    out _,
                    out float distanceSquared))
                {
                    probe.Weight = 0.0f;
                    probes[i] = probe;
                    continue;
                }

                if (distanceSquared >
                    maxDistanceSquared)
                {
                    probe.Weight = 0.0f;
                    probes[i] = probe;
                    continue;
                }

                probe.Weight = 1.0f;
                probes[i] = probe;
            }
        }
    }
}