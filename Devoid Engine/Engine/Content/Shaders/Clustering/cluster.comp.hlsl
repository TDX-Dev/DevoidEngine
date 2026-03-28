//=====================================================
// HLSL Compute Shader (converted from GLSL)
//=====================================================

struct Cluster
{
    float4 minPoint;
    float4 maxPoint;
};

// ----------------------------------------------------
// Buffers
// ----------------------------------------------------

// Cluster buffer (like GLSL SSBO)
RWStructuredBuffer<Cluster> clusters : register(u4); // binding = 4 in GLSL

// Screen / view data buffer
struct ScreenViewData
{
    float4x4 inverseProjectionMatrix;

    // tile info
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


StructuredBuffer<ScreenViewData> screenViewData : register(t7); // binding = 7
// (Or make this a cbuffer with matching fields if it’s constant for all threads)

// ----------------------------------------------------
// Constants
// ----------------------------------------------------
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

cbuffer ClusteredRendererData : register(b1)
{
    float4 groupSize;
};

float3 lineIntersectionToZPlane(float3 A, float3 B, float zDistance);
float4 clipToView(float4 clip);
float4 screen2View(float4 screen);

// ----------------------------------------------------
// Thread group size — matches layout(local_size_x =1,...)
// ----------------------------------------------------
[numthreads(1, 1, 1)]
void CSMain(uint3 groupID : SV_GroupID,
          uint3 dispatchThreadID : SV_DispatchThreadID,
          uint3 groupThreadID : SV_GroupThreadID,
          uint groupIndex : SV_GroupIndex)
{
    
    
    float3 eyePos = float3(0.0, 0.0, 0.0);

    uint tileSizePx = screenViewData[0].tilePixelWidth;

    uint tileIndex = groupID.x +
                 groupID.y * screenViewData[0].tileCountX +
                 groupID.z * (screenViewData[0].tileCountX * screenViewData[0].tileCountY);

    float2 minPixel = float2(groupID.x * screenViewData[0].tilePixelWidth,
                         groupID.y * screenViewData[0].tilePixelHeight);
    float2 maxPixel = float2((groupID.x + 1) * screenViewData[0].tilePixelWidth,
                         (groupID.y + 1) * screenViewData[0].tilePixelHeight);

    maxPixel = min(maxPixel, float2(screenViewData[0].screenWidth, screenViewData[0].screenHeight));

    // after computing minPixel and maxPixel and clamping:
    float2 minPixelClamped = minPixel;
    float2 maxPixelClamped = maxPixel;

    // convert to clip (NDC) coordinates -1..1
        float2 minNDC = (minPixelClamped / float2(screenViewData[0].screenWidth, screenViewData[0].screenHeight)) * 2.0 - 1.0;
        float2 maxNDC = (maxPixelClamped / float2(screenViewData[0].screenWidth, screenViewData[0].screenHeight)) * 2.0 - 1.0;

    // near-plane clip coords (z = -1, w = 1)
        float4 minPoint_sS = float4(minNDC, -1.0, 1.0);
        float4 maxPoint_sS = float4(maxNDC, -1.0, 1.0);

    // then unproject via screen2View/ inverse projection
        float3 maxPoint_vS = screen2View(maxPoint_sS).xyz;
        float3 minPoint_vS = screen2View(minPoint_sS).xyz;
    

    //// Screen space coordinates for min/max corners
    //float4 maxPoint_sS = float4(
    //    (float2(groupID.x + 1, groupID.y + 1) * tileSizePx),
    //    -1.0, 1.0); // Top Right

    //float4 minPoint_sS = float4(
    //    (float2(groupID.xy) * tileSizePx),
    //    -1.0, 1.0); // Bottom Left

    ////Pass min and max to view space
    //float3 maxPoint_vS = screen2View(maxPoint_sS).xyz;
    //float3 minPoint_vS = screen2View(minPoint_sS).xyz;

    //Near and far values of the cluster in view space
    float tileNear = -zNear * pow(zFar / zNear,
                    (float) groupID.z / (float) groupSize.z);
    float tileFar = -zNear * pow(zFar / zNear,
                    (float) (groupID.z + 1) / (float) groupSize.z);

    //Find intersection points with Z planes
    float3 minPointNear = lineIntersectionToZPlane(eyePos, minPoint_vS, tileNear);
    float3 minPointFar = lineIntersectionToZPlane(eyePos, minPoint_vS, tileFar);
    float3 maxPointNear = lineIntersectionToZPlane(eyePos, maxPoint_vS, tileNear);
    float3 maxPointFar = lineIntersectionToZPlane(eyePos, maxPoint_vS, tileFar);

    float3 minPointAABB = min(min(minPointNear, minPointFar),
                              min(maxPointNear, maxPointFar));
    float3 maxPointAABB = max(max(minPointNear, minPointFar),
                              max(maxPointNear, maxPointFar));

    //clusters[tileIndex].minPoint = float4(420.0, 420.0, 420.0, 420.0);
    //clusters[tileIndex].maxPoint = float4(69.0, 69.0, 69.0, 69.0);
    
    //clusters[tileIndex].minPoint = mul(inverse(View), float4(minPointAABB, 0.0));
    //clusters[tileIndex].maxPoint = mul(inverse(View), float4(maxPointAABB, 0.0));
    
    clusters[tileIndex].minPoint = float4(minPointAABB, 1.0);
    clusters[tileIndex].maxPoint = float4(maxPointAABB, 1.0);

}

// ----------------------------------------------------
// Helper Functions
// ----------------------------------------------------

// Intersection of line with a Z plane
float3 lineIntersectionToZPlane(float3 A, float3 B, float zDistance)
{
    float3 normal = float3(0.0, 0.0, 1.0);
    float3 ab = B - A;
    float t = (zDistance - dot(normal, A)) / dot(normal, ab);
    return A + t * ab;
}

// Clip to View
float4 clipToView(float4 clip)
{
    float4 view = mul(screenViewData[0].inverseProjectionMatrix, clip);
    view /= view.w;
    return view;
}

// Screen to View
float4 screen2View(float4 screen)
{
    float2 texCoord = screen.xy / float2(screenViewData[0].screenWidth,
                                         screenViewData[0].screenHeight);

    float4 clip = float4(texCoord * 2.0 - 1.0, screen.z, screen.w);
    return clipToView(clip);
}