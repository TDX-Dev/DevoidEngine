struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

Texture2D MAT_Panorama : register(t0);
SamplerState MAT_Panorama_Sampler : register(s0);

#include "./Common/MathConstants.hlsl"

float3 GetDirection(int face, float2 uv)
{
    float2 xy = uv * 2.0f - 1.0f;
    float3 dir;

    if (face == 0)
        dir = float3(1.0, -xy.y, -xy.x); // +X
    else if (face == 1)
        dir = float3(-1.0, -xy.y, xy.x); // -X
    else if (face == 2)
        dir = float3(xy.x, -1.0, -xy.y); // +Y (flipped)
    else if (face == 3)
        dir = float3(xy.x, 1.0, xy.y); // -Y (flipped)
    else if (face == 4)
        dir = float3(-xy.x, -xy.y, -1.0); // +Z (flipped)
    else
        dir = float3(xy.x, -xy.y, 1.0); // -Z (flipped)

    return normalize(dir);
}

float4 PSMain(PSInput input) : SV_Target
{
    
    float3 dir = normalize(input.WorldspacePosition);

    float2 uv;
    uv.x = atan2(dir.z, dir.x) / (2 * PI) + 0.5;
    uv.y = asin(dir.y) / PI + 0.5;

    float3 color = MAT_Panorama.Sample(MAT_Panorama_Sampler, uv).rgb;
    //float3 color = float3(1, 1, 1);
    return float4(color, 1);
}