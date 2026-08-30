#include "./Common/SH9.hlsl"

TextureCube<float4> MAT_Cubemap : register(t0);
SamplerState MAT_CubemapSampler : register(s0);


RWStructuredBuffer<SH9> PartialSH : register(u0);
groupshared float4 SharedSH[9][64];

cbuffer Material : register(b5)
{
    float CubemapResolution;
    float3 padding;
}


float3 CubeDirection(uint face, float2 uv)
{
    uv = uv * 2.0 - 1.0;

    switch (face)
    {
        case 0:
            return normalize(float3(1, -uv.y, -uv.x)); // +X
        case 1:
            return normalize(float3(-1, -uv.y, uv.x)); // -X
        case 2:
            return normalize(float3(uv.x, -1, -uv.y)); // +Y (flipped)
        case 3:
            return normalize(float3(uv.x, 1, uv.y));  // -Y (flipped)
        case 4:
            return normalize(float3(-uv.x, -uv.y, -1)); // +Z (flipped)
        default:
            return normalize(float3(uv.x, -uv.y, 1));  // -Z (flipped)
    }
}

float AreaElement(float x, float y)
{
    return atan2(x * y, sqrt(x * x + y * y + 1.0));
}

float TexelSolidAngle(uint2 pixel, uint resolution)
{
    float invRes = 2.0 / resolution;

    float u = ((pixel.x + 0.5) / resolution) * 2.0 - 1.0;
    float v = ((pixel.y + 0.5) / resolution) * 2.0 - 1.0;

    float x0 = u - invRes * 0.5;
    float y0 = v - invRes * 0.5;

    float x1 = u + invRes * 0.5;
    float y1 = v + invRes * 0.5;

    return
        AreaElement(x0, y0)
      - AreaElement(x0, y1)
      - AreaElement(x1, y0)
      + AreaElement(x1, y1);
}

[numthreads(8, 8, 1)]
void CSMain(
    uint3 DTid : SV_DispatchThreadID,
    uint3 GTid : SV_GroupThreadID,
    uint3 Gid : SV_GroupID)
{

    
    float2 uv = (float2(DTid.xy) + 0.5) / float(CubemapResolution);

    float3 dir = CubeDirection(DTid.z, uv);
    
    float sh[9];
    EvaluateSHBasis(dir, sh);

    float3 color = MAT_Cubemap.SampleLevel(MAT_CubemapSampler, dir, 0).rgb;
    
    float omega = TexelSolidAngle(DTid.xy, CubemapResolution);

    float4 coeff[9];

    [unroll]
    for (uint i = 0; i < 9; i++)
    {
        coeff[i] = float4(color * sh[i] * omega, 0.0);
    }
    
    uint localIndex = GTid.y * 8 + GTid.x;

    [unroll]
    for (uint j = 0; j < 9; j++)
    {
        SharedSH[j][localIndex] = coeff[j];
    }
    
    GroupMemoryBarrierWithGroupSync();
    
    for (uint stride = 32; stride > 0; stride >>= 1)
    {
        if (localIndex < stride)
        {
        [unroll]
            for (uint i = 0; i < 9; i++)
            {
                SharedSH[i][localIndex] +=
                SharedSH[i][localIndex + stride];
            }
        }

        GroupMemoryBarrierWithGroupSync();
    }
    
    if (localIndex == 0)
    {
        uint groupsX = CubemapResolution / 8;
        uint groupsY = CubemapResolution / 8;

        uint groupIndex =
            Gid.z * groupsX * groupsY +
            Gid.y * groupsX +
            Gid.x;

        [unroll]
        for (uint k = 0; k < 9; k++)
        {
            PartialSH[groupIndex].C[k] = SharedSH[k][0];
        }
    }
}