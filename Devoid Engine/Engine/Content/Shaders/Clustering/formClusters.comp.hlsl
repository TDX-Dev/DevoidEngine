struct Cluster
{
    float4 minPoint;
    float4 maxPoint;
    uint count;
    uint lightIndices[100];
};

RWStructuredBuffer<Cluster> clusters : register(u1);

cbuffer CameraData : register(b0)
{
    float4x4 View;
    float4x4 Projection;
    float4x4 InverseProjection;
    float3 Position;
    float NearClip;
    float FarClip;
    float2 ScreenSize;
    float _padding0;
};

cbuffer ClusteredRendererData : register(b1)
{
    float4 groupSize;
};

float3 screenToView(float2 screenCoord);
float3 linearIntersectionWithZPlane(float3 start, float3 end, float zDistance);

[numthreads(1,1,1)]
void CSMain(uint3 groupID : SV_GroupID, uint3 dispatchThreadID : SV_DispatchThreadID,
    uint3 groupThreadID : SV_GroupThreadID, uint groupIndex : SV_GroupIndex)
{
    float3 eyePosition = float3(0, 0, 0);
    
    
    // This index is used to write into a linear flat array.
    // instead of a 2D/3D array
    uint tileIndex = groupID.x + (groupID.y * groupSize.x)
        + (groupID.z * groupSize.x * groupSize.y);
    
    
    // this contains the width and height of each tile
    // depth is based on any custom function eg. Log
    float2 tileSize = ScreenSize / groupSize.xy;
    
    // Computing actual clusters in screen space
    
    float2 minTile_screenspace = groupID.xy * tileSize;
    float2 maxTile_screenspace = (groupID.xy + 1) * tileSize;
    
    // Computing view space clusters from screen space.
    float3 minTileView = screenToView(minTile_screenspace);
    float3 maxTileView = screenToView(maxTile_screenspace);
    
    float planeNear = NearClip * pow(FarClip / NearClip, groupID.z / (float) groupSize.z);
    float planeFar = NearClip * pow(FarClip / NearClip, (groupID.z + 1) / (float) groupSize.z);

    
    float3 minPointNear = linearIntersectionWithZPlane(eyePosition, minTileView, planeNear);
    float3 minPointFar = linearIntersectionWithZPlane(eyePosition, minTileView, planeFar);
    float3 maxPointNear = linearIntersectionWithZPlane(eyePosition, maxTileView, planeNear);
    float3 maxPointFar = linearIntersectionWithZPlane(eyePosition, maxTileView, planeFar);
    
    clusters[tileIndex].minPoint = float4(min(minPointNear, minPointFar), 0.0);
    clusters[tileIndex].maxPoint = float4(max(maxPointNear, maxPointFar), 0.0);
    
}

float3 linearIntersectionWithZPlane(float3 start, float3 end, float zDistance)
{
    float3 direction = end - start;
    float3 normal = float3(0.0, 0.0, -1.0);
    
    float t = (zDistance - dot(normal, start)) / dot(normal, direction);
    
    return start + t * direction;
}

float3 screenToView(float2 screenCoord)
{
    float4 ndc = float4(screenCoord / ScreenSize * 2.0 - 1.0, -1.0, 1.0);
    
    // conversion from screen to view via InverseProjection multiplication.
    float4 viewCoordinate = mul(InverseProjection, ndc);
    viewCoordinate /= viewCoordinate.w;
    return viewCoordinate.xyz;
}