using DevoidEngine.Core;
using DevoidEngine.Util;
using DevoidGPU;
using System.Numerics;
using System.Runtime.InteropServices;

namespace DevoidEngine.Rendering.ProbeGI
{
    public struct ProbeInfo
    {
        public Vector3 Offset;
        public float Validity;
    }

    public class ProbeGISystem
    {
        public Texture ProbeColorImage = null!;
        public Texture ProbeVisibilityImage = null!;
        public Texture ProbeInfoImage = null!;
        public Texture CellRequestsImage = null!;

        public Mesh DebugSphereMesh;
        public Shader DebugSphereShader;
        public MaterialInstance DebugSphereMaterialInstance;

        public float ProbeDebugSize = 1;

        const float DesiredRelativeDistance = 0.3f;
        const float RelativeRepositioningStep = 0.2f;
        const float MaxRelativeRepositioning = 0.66f;
        const float AcceptableSurfaceDistance = DesiredRelativeDistance * 0.3f;
        const float BackfaceEscapeRange =  DesiredRelativeDistance * 3.0f;
        const float ProbeViewDistance = 1.0f;

        public ProbeGISystem()
        {
            DebugSphereMesh = PrimitiveMeshes.CreateIcosphere(3);

            DebugSphereShader = Shader.FromDescriptorFile(Engine.GraphicsDevice, "Content/DevoidShaderDescriptors/probe_debug.dsd");
            DebugSphereMaterialInstance = new MaterialInstance(new Material(DebugSphereShader));


        }

        public void ResolveSettings(ref ProbeGISettings settings)
        {
            settings.ProbeCount.X = MathHelper.RoundUpToPo2((uint)settings.ProbeCount.X);
            settings.ProbeCount.Y = MathHelper.RoundUpToPo2((uint)settings.ProbeCount.Y);
            settings.ProbeCount.Z = MathHelper.RoundUpToPo2((uint)settings.ProbeCount.Z);

            settings.ProbeCountLog2.X = MathHelper.MsbIndex((uint)settings.ProbeCount.X);
            settings.ProbeCountLog2.Y = MathHelper.MsbIndex((uint)settings.ProbeCount.Y);
            settings.ProbeCountLog2.Z = MathHelper.MsbIndex((uint)settings.ProbeCount.Z);

            settings.ProbeTraceResolution = Math.Max(16, settings.ProbeTraceResolution);

            static uint ConstrainResolution(uint resolution)
            {
                uint[] validResolutions = [48, 24, 16, 12, 8, 6, 4];

                for (int i = 0; i < validResolutions.Length; i++)
                {
                    if (resolution >= validResolutions[i])
                    {
                        return validResolutions[i];
                    }
                }
                return validResolutions[^1];
            }

            settings.ProbeColorResolution = ConstrainResolution(settings.ProbeColorResolution);
            settings.ProbeVisibilityResolution = ConstrainResolution(settings.ProbeVisibilityResolution);

            settings.ProbeCountRCP = new(1f / settings.ProbeCount.X, 1f / settings.ProbeCount.Y, 1f / settings.ProbeCount.Z);

            settings.IrradianceBorderWidth = settings.ProbeColorResolution + 2f;
            settings.IrradianceBorderWidthRCP = 1f / (settings.ProbeColorResolution + 2f);

            settings.VisibilityBorderWidth = settings.ProbeVisibilityResolution + 2f;
            settings.VisibilityBorderWidthRCP = 1f / (settings.ProbeVisibilityResolution + 2f);

            Vector3 volumeMin = Vector3.Min(settings.VolumeMin, settings.VolumeMax);
            Vector3 volumeMax = Vector3.Max(settings.VolumeMax, settings.VolumeMin);

            settings.VolumeMax = volumeMax;
            settings.VolumeMin = volumeMin;

            settings.VolumeSize = volumeMax - volumeMin;

            settings.ProbeSpacing = new(
                settings.VolumeSize.X / (settings.ProbeCount.X - 1),
                settings.VolumeSize.Y / (settings.ProbeCount.Y - 1),
                settings.VolumeSize.Z / (settings.ProbeCount.Z - 1)
            );

            settings.ProbeSpacingRCP = new(
                1f / (settings.ProbeSpacing.X),
                1f / (settings.ProbeSpacing.Y),
                1f / (settings.ProbeSpacing.Z)
            );

            settings.MaxVisibilityDistance = (settings.ProbeSpacing * 1.01f).Length();

            settings.ProbeGridOrigin = settings.VolumeMin;
        }

        public void RecreateAndClear(ICommandList cmd, ProbeGISettings settings)
        {
            ProbeColorImage?.Dispose();
            ProbeVisibilityImage?.Dispose();
            ProbeInfoImage?.Dispose();
            CellRequestsImage?.Dispose();

            ProbeColorImage = new(new TextureDescription()
            {
                Width = (int)(settings.ProbeCount.X * (settings.ProbeColorResolution + 2)),
                Height = (int)(settings.ProbeCount.Y * (settings.ProbeColorResolution + 2)),
                Depth = 1,

                ArraySize = (int)(settings.ProbeCount.Z * 1 * 2),
                Usage = TextureUsage.ShaderResource | TextureUsage.UnorderedAccess,
                Format = TextureFormat.RGBA16_Float,
                Dimension = TextureDimension.Texture2D,

                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1
            });

            ProbeVisibilityImage = new(new TextureDescription()
            {
                Width = (int)(settings.ProbeCount.X * (settings.ProbeVisibilityResolution + 2)),
                Height = (int)(settings.ProbeCount.Y * (settings.ProbeVisibilityResolution + 2)),
                Depth = 1,

                ArraySize = (int)(settings.ProbeCount.Z * 1),
                Usage = TextureUsage.ShaderResource | TextureUsage.UnorderedAccess,
                Format = TextureFormat.RG16_Float,
                Dimension = TextureDimension.Texture2D,

                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1
            });

            ProbeInfoImage = new(new TextureDescription()
            {
                Width = (int)(settings.ProbeCount.X),
                Height = (int)(settings.ProbeCount.Y),
                Depth = 1,

                ArraySize = (int)(settings.ProbeCount.Z * 1),
                Usage = TextureUsage.ShaderResource | TextureUsage.UnorderedAccess,
                Format = TextureFormat.RGBA32_Float,
                Dimension = TextureDimension.Texture2D,

                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1
            });

            CellRequestsImage = new(new TextureDescription()
            {
                Width = (int)(settings.ProbeCount.X),
                Height = (int)(settings.ProbeCount.Y),
                Depth = 1,

                ArraySize = (int)(settings.ProbeCount.Z * 1),
                Usage = TextureUsage.ShaderResource | TextureUsage.UnorderedAccess,
                Format = TextureFormat.R32_UInt,
                Dimension = TextureDimension.Texture2D,

                Samples = new TextureSampleDescription(1, 0),
                MipLevels = 1
            });
            
            cmd.ClearTextureResource(ProbeColorImage.GPU, ClearValue.Float(0, 0, 0, 0));
            cmd.ClearTextureResource(ProbeVisibilityImage.GPU, ClearValue.Float(0, 0, 0, 0));
            cmd.ClearTextureResource(ProbeInfoImage.GPU, ClearValue.Float(0, 0, 0, 0));
            cmd.ClearTextureResource(CellRequestsImage.GPU, ClearValue.UInt(0, 0, 0, 0));

        }

        public void Bake(ProbeGISettings settings, RenderWorld world)
        {
            if (world.StaticBVH == null) return;

            Console.WriteLine("Baking with trace resolution: " + settings.ProbeTraceResolution);

            List<ProbeInfo> probeRelocatedInfo = RunProbeRelocation(settings, world.StaticBVH, 24);
            ReadOnlySpan<ProbeInfo> span = CollectionsMarshal.AsSpan(probeRelocatedInfo);
            ProbeInfoImage.GPU.Update(span);

        }

        public void DrawProbeSpheres(ICommandList cmd, ProbeGISettings settings)
        {
            DebugSphereMaterialInstance.DescriptorSet.SetTexture(8, ProbeInfoImage.GPU);
            cmd.SetPipeline(DebugSphereMaterialInstance.BaseMaterial.DefaultPass.GetPipeline(DebugSphereMaterialInstance.BaseMaterial.Variant, DebugSphereMesh.VertexInfo));

            cmd.SetDescriptorSet(1, DebugSphereMaterialInstance.DescriptorSet);
            Engine.Renderer.UpdatePerObjectData(Matrix4x4.CreateScale(ProbeDebugSize));
            DebugSphereMesh.DrawInstanced(cmd, (int)(settings.ProbeCount.X * settings.ProbeCount.Y * settings.ProbeCount.Z));

        }

        
        
        int ProbeCount(ProbeGISettings settings)
        {
            return (int)(
                settings.ProbeCount.X *
                settings.ProbeCount.Y *
                settings.ProbeCount.Z);
        }
        Vector3 GetProbePosition(ProbeGISettings settings, int x, int y, int z, Vector3 offset)
        {
            Vector3 gridPosition = settings.ProbeGridOrigin + new Vector3(x, y, z) * settings.ProbeSpacing;

            return gridPosition + offset * settings.ProbeSpacing;
        }
        int GetProbeIndex(int x, int y, int z, ProbeGISettings settings)
        {
            int countX = (int)settings.ProbeCount.X;
            int countY = (int)settings.ProbeCount.Y;

            return x + y * countX + z * countX * countY;
        }
        bool IsBackface(Vector3 direction, Vector3 normal)
        {
            return Vector3.Dot(direction, normal) > 0.0f;
        }
        public List<ProbeInfo> RunProbeRelocation(
            ProbeGISettings settings,
            BVH bvh,
            int iterations = 8)
        {
            int countX = (int)settings.ProbeCount.X;
            int countY = (int)settings.ProbeCount.Y;
            int countZ = (int)settings.ProbeCount.Z;

            int probeCount = countX * countY * countZ;

            int traceResolution = (int)settings.ProbeTraceResolution;

            List<ProbeInfo> probeInfo = new(probeCount);

            for (int i = 0; i < probeCount; i++)
            {
                probeInfo.Add(new ProbeInfo
                {
                    Offset = Vector3.Zero,
                    Validity = 0.0f
                });
            }

            ProbeInfo[] nextProbeInfo =
                new ProbeInfo[probeCount];

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    for (int y = 0; y < countY; y++)
                    {
                        for (int x = 0; x < countX; x++)
                        {
                            int index =
                                GetProbeIndex(x, y, z, settings);

                            ProbeInfo current =
                                probeInfo[index];

                            Vector3 probePosition =
                                GetProbePosition(
                                    settings,
                                    x,
                                    y,
                                    z,
                                    current.Offset);

                            Vector3 averageHitOffset =
                                Vector3.Zero;

                            Vector3 averageBackfaceHitOffset =
                                Vector3.Zero;

                            float closestBackfaceDistance =
                                10000.0f;

                            float closestFrontfaceDistance =
                                10000.0f;

                            Vector3 closestBackfaceDirection =
                                Vector3.Zero;

                            int backfaceCount = 0;
                            int hitCount = 0;

                            for (int py = 0; py < traceResolution; py++)
                            {
                                for (int px = 0; px < traceResolution; px++)
                                {
                                    Vector2 uv =
                                        new(
                                            (px + 0.5f) /
                                            traceResolution,

                                            (py + 0.5f) /
                                            traceResolution);

                                    Vector3 rayDirection = OctahedralDirection(uv);

                                    AccumulateRelocationRay(
                                        bvh,
                                        probePosition,
                                        rayDirection,
                                        settings,
                                        ref averageHitOffset,
                                        ref averageBackfaceHitOffset,
                                        ref closestBackfaceDistance,
                                        ref closestBackfaceDirection,
                                        ref closestFrontfaceDistance,
                                        ref backfaceCount,
                                        ref hitCount
                                    );
                                }
                            }

                            averageHitOffset /=
                                traceResolution *
                                traceResolution;

                            if (backfaceCount > 0)
                            {
                                averageBackfaceHitOffset /=
                                    backfaceCount;
                            }

                            Vector3 springForce =
                                CalculateSpringForce(
                                    x,
                                    y,
                                    z,
                                    settings,
                                    probeInfo);

                            Vector3 adjustment =
                                CalculateRelocationAdjustment(
                                    traceResolution,
                                    averageHitOffset,
                                    closestBackfaceDistance,
                                    closestBackfaceDirection,
                                    closestFrontfaceDistance,
                                    backfaceCount,
                                    springForce,
                                    current.Validity,
                                    true);

                            float validity =
                                MathF.Min(
                                    1.0f,
                                    current.Validity + 0.05f);

                            //Console.WriteLine(
                            //    $"Probe {index}: " +
                            //    $"hits={hitCount}, " +
                            //    $"backfaces={backfaceCount}, " +
                            //    $"front={closestFrontfaceDistance:F3}, " +
                            //    $"back={closestBackfaceDistance:F3}, " +
                            //    $"avg={averageHitOffset}, " +
                            //    $"spring={springForce}"
                            //);

                            if (closestBackfaceDistance != 10000.0f ||
                                closestFrontfaceDistance <
                                AcceptableSurfaceDistance)
                            {
                                validity = 0.0f;
                            }

                            bool isBorderProbe =
                                x == 0 ||
                                y == 0 ||
                                z == 0 ||
                                x == countX - 1 ||
                                y == countY - 1 ||
                                z == countZ - 1;

                            if (isBorderProbe)
                            {
                                validity = 0.0f;
                                adjustment = Vector3.Zero;
                            }

                            Vector3 newOffset =
                                current.Offset + adjustment;

                            newOffset = Vector3.Clamp(
                                newOffset,
                                new Vector3(-MaxRelativeRepositioning),
                                new Vector3(MaxRelativeRepositioning));

                            nextProbeInfo[index] =
                                new ProbeInfo
                                {
                                    Offset = newOffset,
                                    Validity = validity
                                };
                        }
                    }
                }

                for (int i = 0; i < probeCount; i++)
                {
                    probeInfo[i] = nextProbeInfo[i];
                }
            }

            return probeInfo;
        }

        void AccumulateRelocationRay(
            BVH bvh, 
            Vector3 probePosition, 
            Vector3 rayDirection, 
            ProbeGISettings settings, 
            ref Vector3 averageHitOffset,
            ref Vector3 averageBackfaceHitOffset,
            ref float closestBackfaceDistance,
            ref Vector3 closestBackfaceDirection,
            ref float closestFrontfaceDistance,
            ref int backfaceCount,
            ref int hitCount
        )
        {
            //const float SomeLargeValue = 10000.0f;
            const float ProbeViewDistance = 1.0f;
            const float BackfaceEscapeRange = 0.9f;

            if (!bvh.Raycast(probePosition, rayDirection, settings.MaxVisibilityDistance, out float distance, out _, out Vector3 normal))
            {
                return;
            }
            hitCount++;

            bool isBackface = Vector3.Dot(rayDirection, normal) > 0.0f;

            Vector3 probeSpaceHit = distance * rayDirection * settings.ProbeSpacingRCP;

            float probeSpaceDistance = MathF.Min(ProbeViewDistance, probeSpaceHit.Length());

            Vector3 probeSpaceDirection = Vector3.Normalize(rayDirection * settings.ProbeSpacingRCP);

            averageHitOffset += probeSpaceDirection * MathF.Max(0.5f, probeSpaceDistance);

            if (isBackface)
            {
                if (probeSpaceDistance < BackfaceEscapeRange)
                {
                    if (probeSpaceDistance < closestBackfaceDistance)
                    {
                        closestBackfaceDistance = probeSpaceDistance;
                        closestBackfaceDirection = probeSpaceDirection;
                    }

                    averageBackfaceHitOffset += probeSpaceDirection * probeSpaceDistance;

                    backfaceCount++;
                }
            }
            else
            {
                closestFrontfaceDistance = MathF.Min(closestFrontfaceDistance, probeSpaceDistance);
            }
        }

        Vector3 CalculateSpringForce(int x, int y, int z, ProbeGISettings settings, IReadOnlyList<ProbeInfo> probeInfo)
        {
            Vector3 springForce = Vector3.Zero;

            int countX = (int)settings.ProbeCount.X;
            int countY = (int)settings.ProbeCount.Y;
            int countZ = (int)settings.ProbeCount.Z;

            int currentIndex = GetProbeIndex(x, y, z, settings);

            Vector3 currentOffset = probeInfo[currentIndex].Offset;

            ReadOnlySpan<(int X, int Y, int Z)> neighbors =
            [
                ( 1,  0,  0),
                (-1,  0,  0),
                ( 0,  1,  0),
                ( 0, -1,  0),
                ( 0,  0,  1),
                ( 0,  0, -1)
            ];

            foreach (var (dx, dy, dz) in neighbors)
            {
                int nx = Math.Clamp(x + dx, 0, countX - 1);
                int ny = Math.Clamp(y + dy, 0, countY - 1);
                int nz = Math.Clamp(z + dz, 0, countZ - 1);

                if (nx == x && ny == y && nz == z)
                    continue;

                int neighborIndex = GetProbeIndex(nx, ny, nz, settings);

                Vector3 otherOffset = probeInfo[neighborIndex].Offset;

                Vector3 equilibriumDiff = Vector3.Lerp(otherOffset - currentOffset, -currentOffset, 0.1f);

                float magnitude = MathF.Min(1.0f, equilibriumDiff.Length());

                if (magnitude > 0.0f)
                {
                    springForce += equilibriumDiff * magnitude * magnitude * magnitude;
                }
            }

            return springForce;
        }

        Vector3 CalculateRelocationAdjustment(
            int traceResolution,
            Vector3 averageHitOffset,
            float closestBackfaceDistance,
            Vector3 closestBackfaceDirection,
            float closestFrontfaceDistance,
            int backfaceCount,
            Vector3 springForce,
            float validity,
            bool springEnabled
        )
        {
            const float SomeLargeValue = 10000.0f;

            bool tooFewBackfaceHits =
                backfaceCount <
                MathF.Ceiling(traceResolution * 0.1f);

            if (tooFewBackfaceHits)
            {
                closestBackfaceDistance = SomeLargeValue;
                closestBackfaceDirection = Vector3.Zero;
            }

            float backfaceEscapeDistance =
                tooFewBackfaceHits
                    ? 0.0f
                    : closestBackfaceDistance +
                      AcceptableSurfaceDistance * 0.5f;

            Vector3 estimatedFreedomDirection =
                averageHitOffset.LengthSquared() > 1e-12f
                    ? Vector3.Normalize(averageHitOffset)
                    : Vector3.Zero;

            float frontfaceRepulseDistance = 0.0f;
            float frontfaceRepulsePower = 0.0f;

            if (backfaceEscapeDistance == 0.0f)
            {
                frontfaceRepulseDistance =
                    Math.Clamp(
                        DesiredRelativeDistance -
                        closestFrontfaceDistance,
                        0.0f,
                        DesiredRelativeDistance);

                frontfaceRepulsePower =
                    frontfaceRepulseDistance /
                    DesiredRelativeDistance;
            }

            float springForceDistance = 0.0f;
            Vector3 springForceDirection = Vector3.Zero;

            if (backfaceEscapeDistance == 0.0f &&
                springEnabled)
            {
                springForceDistance =
                    springForce.Length();

                springForceDistance *=
                    1.0f - frontfaceRepulsePower;

                if (springForceDistance > 0.001f)
                {
                    springForceDirection =
                        Vector3.Normalize(springForce);

                    Vector3 towardsGeometryDirection =
                        -estimatedFreedomDirection;

                    float projected =
                        MathF.Max(0.0f, Vector3.Dot(towardsGeometryDirection, springForceDirection));

                    springForceDirection -=
                        projected *
                        towardsGeometryDirection;

                    if (springForceDirection.LengthSquared() >
                        1e-12f)
                    {
                        springForceDirection =
                            Vector3.Normalize(springForceDirection);
                    }
                    else
                    {
                        springForceDirection = Vector3.Zero;
                        springForceDistance = 0.0f;
                    }

                    springForceDistance *=
                        1.0f - projected;
                }
            }

            if (validity >= 1.0f)
            {
                backfaceEscapeDistance = 0.0f;
                frontfaceRepulseDistance = 0.0f;
                springForceDistance = 0.0f;
            }

            Vector3 adjustment =
                closestBackfaceDirection *
                MathF.Min(
                    backfaceEscapeDistance,
                    RelativeRepositioningStep);

            adjustment +=
                estimatedFreedomDirection *
                MathF.Min(
                    frontfaceRepulseDistance *
                    RelativeRepositioningStep,
                    RelativeRepositioningStep);

            adjustment +=
                springForceDirection *
                MathF.Min(
                    springForceDistance *
                    RelativeRepositioningStep,
                    RelativeRepositioningStep);

            return adjustment;
        }

        Vector3 OctahedralDirection(Vector2 uv)
        {
            Vector3 v = new(uv.X * 2.0f - 1.0f, uv.Y * 2.0f - 1.0f, 1.0f);

            v.Z -= MathF.Abs(v.X) + MathF.Abs(v.Y);

            if (v.Z < 0.0f)
            {
                float x = v.X;
                float y = v.Y;

                v.X = (1.0f - MathF.Abs(y)) * MathF.Sign(x);

                v.Y = (1.0f - MathF.Abs(x)) * MathF.Sign(y);
            }

            return Vector3.Normalize(v);
        }
    }
}
