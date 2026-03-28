// HLSL compute shader (fixed for group sync / varying-flow issues)

// Constants
static const uint LOCAL_X = 16;
static const uint LOCAL_Y = 9;
static const uint LOCAL_Z = 4;
static const uint THREAD_COUNT = LOCAL_X * LOCAL_Y * LOCAL_Z;
static const float CULL_PAD = 0.01f; // tune if needed

#define TYPE_POINT 0u
#define TYPE_SPOT  1u
#define TYPE_DIRECTIONAL 2u

static const uint MAX_LIGHTS_PER_TILE = 100;

// Structs
struct LightGrid
{
    uint offset;
    uint count;
};

struct PointLight
{
    float4 position;
    float4 color;
    float4 range;
};

struct SpotLight
{
    float4 position; // xyz = pos, w = enabled
    float4 color; // xyz = color, w = intensity
    float4 direction; // xyz = dir, w = range
    float innerCutoff;
    float outerCutoff;
};

struct Cluster
{
    float4 minPoint;
    float4 maxPoint;
};

struct ScreenViewData
{
    float4x4 inverseProjectionMatrix;
    uint tileCountX;
    uint tileCountY;
    uint sliceCountZ;
    uint tilePixelWidth;
    uint screenWidth;
    uint screenHeight;
    uint tilePixelHeight;
    float sliceScaling;
    float sliceBias;
    float3 _padding;
};

// CBs and buffers
cbuffer CameraData : register(b0)
{
    float4x4 View;
    float4x4 Projection;
    float3 Position;
    float _padding;
    float zNear;
    float zFar;
    float2 _padding1;
};

cbuffer LightCounts : register(b1)
{
    uint numPointLights;
    uint numSpotLights;
    uint numDirectionalLights;
    uint _padding2;
};

cbuffer ClusteredRendererData : register(b2)
{
    float4 groupSize;
};

StructuredBuffer<PointLight> pointLights : register(t0);
StructuredBuffer<SpotLight> spotLights : register(t1);
StructuredBuffer<ScreenViewData> screenViewData : register(t7);

RWStructuredBuffer<LightGrid> lightGrids : register(u3);
RWStructuredBuffer<Cluster> clusters : register(u4);
RWStructuredBuffer<uint> globalIndexCountBuf : register(u5);
RWStructuredBuffer<uint> globalLightIndexList : register(u6);

// IMPORTANT: groupshared arrays must have size THREAD_COUNT (not half) so all threads can write/read their element
groupshared PointLight sharedPointLights[THREAD_COUNT];
//groupshared SpotLight sharedSpotLights[1];

// helpers
float sqDistPointAABB(float3 pos, uint tile)
{
    float sqDist = 0.0;
    Cluster currentCell = clusters[tile];

    for (int i = 0; i < 3; ++i)
    {
        float v = pos[i];
        if (v < currentCell.minPoint[i])
        {
            float d = currentCell.minPoint[i] - v;
            sqDist += d * d;
        }
        if (v > currentCell.maxPoint[i])
        {
            float d = v - currentCell.maxPoint[i];
            sqDist += d * d;
        }
    }
    return sqDist;
}

bool testSphereAABB_Point(uint lightIndexInShared, uint tile)
{
    float radius = sharedPointLights[lightIndexInShared].range.x;
    float3 center = mul(float4(sharedPointLights[lightIndexInShared].position.xyz, 1.0), View).xyz;
    float squaredDistance = sqDistPointAABB(center, tile);
    return squaredDistance <= (radius * radius);
}

//bool testSphereAABB_Spot(uint lightIndexInShared, uint tile)
//{
//    float radius = sharedSpotLights[lightIndexInShared].direction.w;
//    float3 center = mul(float4(sharedSpotLights[lightIndexInShared].position.xyz, 1.0), View).xyz;
//    float squaredDistance = sqDistPointAABB(center, tile);
//    return squaredDistance <= (radius * radius);
//}

// Thread group size
[numthreads(16, 9, 4)] // each group has 16*9*4 = 576 threads
void CSMain(uint3 DTid : SV_DispatchThreadID,
            uint3 GTid : SV_GroupThreadID,
            uint3 Gid : SV_GroupID,
            uint GI : SV_GroupIndex)
{
    // --- ensure globalIndexCountBuf is zeroed by exactly one thread in the first group ---
    if (GI == 0 && Gid.x == 0 && Gid.y == 0 && Gid.z == 0)
    {
        globalIndexCountBuf[0] = 0;
    }

    // compute local indices
    uint lx = GI % LOCAL_X;
    uint ly = (GI / LOCAL_X) % LOCAL_Y;
    uint lz = GI / (LOCAL_X * LOCAL_Y);

    uint tileX = Gid.x * LOCAL_X + lx;
    uint tileY = Gid.y * LOCAL_Y + ly;
    uint tileZ = Gid.z * LOCAL_Z + lz;

    uint tileCountX = screenViewData[0].tileCountX;
    uint tileCountY = screenViewData[0].tileCountY;
    uint tileCountZ = screenViewData[0].sliceCountZ;

    // IMPORTANT: do not 'return' here — some threads in group could return while others continue,
    // which makes subsequent GroupMemoryBarrierWithGroupSync() illegal. Use an 'active' flag instead.
    bool active = (tileX < tileCountX) && (tileY < tileCountY) && (tileZ < tileCountZ);

    uint tileIndex = tileX + tileY * tileCountX + tileZ * (tileCountX * tileCountY);

    // Point lights
    uint lightCount = numPointLights;
    uint numBatches = (lightCount + THREAD_COUNT - 1) / THREAD_COUNT;

    // per-tile results (local)
    uint visibleLightCount = 0;
    uint visibleLightIndices[MAX_LIGHTS_PER_TILE];

    // iterate batches — numBatches is same across group, so flow is uniform
    for (uint batch = 0; batch < numBatches; ++batch)
    {
        uint lightIndex = batch * THREAD_COUNT + GI;

        // write into shared memory if in range and thread active; all threads still reach barrier
        if (lightIndex < lightCount && active)
        {
            sharedPointLights[GI] = pointLights[lightIndex];
        }
        else
        {
            // Provide a harmless default to avoid uninitialized reads on other threads
            sharedPointLights[GI].position = float4(0, 0, 0, 0);
            sharedPointLights[GI].range = float4(0, 0, 0, 0);
        }

        // synchronized read of shared memory — executed by all threads in group (uniform flow)
        GroupMemoryBarrierWithGroupSync();

        // test all entries in shared memory (threads that are inactive or entries out of range simply won't match)
        for (uint s = 0; s < THREAD_COUNT; ++s)
        {
            // check shared entry's enabled flag (position.w == 1.0) and only process if we are active
            if (active && sharedPointLights[s].position.w == 1.0f)
            {
                if (testSphereAABB_Point(s, tileIndex))
                {
                    uint globalLightIndex = batch * THREAD_COUNT + s;
                    uint encoded = (TYPE_POINT << 30) | (globalLightIndex & 0x3FFFFFFF);

                    if (visibleLightCount < MAX_LIGHTS_PER_TILE)
                    {
                        visibleLightIndices[visibleLightCount++] = encoded;
                    }
                    // else: overflow — you could count/report it if you need to debug
                }
            }
        }

        // next batch; continue — all threads execute same number of barriers
    }

    // Spot lights (note renamed loop variable to avoid shadow)
    //uint spotCount = numSpotLights;
    //uint spotBatches = (spotCount + THREAD_COUNT - 1) / THREAD_COUNT;

    //for (uint spotBatch = 0; spotBatch < spotBatches; ++spotBatch)
    //{
    //    uint lightIndex = spotBatch * THREAD_COUNT + GI;

    //    if (lightIndex < spotCount && active)
    //    {
    //        sharedSpotLights[GI] = spotLights[lightIndex];
    //    }
    //    else
    //    {
    //        sharedSpotLights[GI].position = float4(0, 0, 0, 0);
    //        sharedSpotLights[GI].direction = float4(0, 0, 0, 0);
    //    }

    //    GroupMemoryBarrierWithGroupSync();
        
    //    // FIX FIX FIX FIX FIX FIX FIX
    //    // FIX FIX FIX FIX FIX FIX FIX
    //    // FIX FIX FIX FIX FIX FIX FIX
    //    for (uint s = 0; s < 0; ++s)
    //    {
    //        if (active && s < spotCount && sharedSpotLights[s].position.w == 1.0f)
    //        {
    //            if (testSphereAABB_Spot(s, tileIndex))
    //            {
    //                uint globalLightIndex = spotBatch * THREAD_COUNT + s;
    //                uint encoded = (TYPE_SPOT << 30) | (globalLightIndex & 0x3FFFFFFF);

    //                if (visibleLightCount < MAX_LIGHTS_PER_TILE)
    //                {
    //                    visibleLightIndices[visibleLightCount++] = encoded;
    //                }
    //            }
    //        }
    //    }
    //}

    // If not active, don't perform writes to global lists; but do the InterlockedAdd with zero
    if (!active)
        return; // safe to return here because we are at end of all group barriers / uniform flow

    // Atomically reserve range in global list
    uint offset = 0;
    InterlockedAdd(globalIndexCountBuf[0], visibleLightCount, offset);

    // write visible indices to global list (bounded by MAX_LIGHTS_PER_TILE)
    for (uint i = 0; i < visibleLightCount; ++i)
    {
        globalLightIndexList[offset + i] = visibleLightIndices[i];
    }

    // fill light grid for this tile
    lightGrids[tileIndex].offset = offset;
    lightGrids[tileIndex].count = visibleLightCount;
}
