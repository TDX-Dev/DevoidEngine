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

float2 DirectionToEquirectUV(float3 dir)
{
    dir = normalize(dir);

    float2 uv;

    uv.x = atan2(dir.x, dir.z) * (1.0 / PI);
    uv.y = asin(clamp(dir.y, -1.0, 1.0)) * (2.0 / PI);

    uv.x = uv.x * 0.5 + 0.5;
    uv.y = 1.0 - (uv.y * 0.5 + 0.5);

    return uv;
}

float4 PSMain(PSInput input) : SV_Target
{
    
    float3 dir = normalize(input.WorldspacePosition);

    float2 uv = DirectionToEquirectUV(dir);

    float3 color = MAT_Panorama.SampleLevel(MAT_Panorama_Sampler, uv, 0).rgb;
    //float3 color = float3(1, 1, 1);
    return float4(color, 1);
}