struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float3 BiTangent : BINORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 FragPos : TEXCOORD2;
    float3 WorldPos : TEXCOORD3;
};


#include "../Common/render_constants.hlsl"

cbuffer Material : register(b2)
{
    float4 Albedo;
}

Texture2D MAT_TEX_OVERRIDE : register(t0);
SamplerState MAT_TEX_OVERRIDE_SAMPLER : register(s0);

float4 PSMain(PSInput input) : SV_TARGET
{
    float _BorderThickness = 0.02; // in UV space (0.02 = 2%)
    float4 _BorderColor = float4(0, 0.3, 0, 1);
    
    float2 uv = input.UV0;
    uv.y = 1.0 - uv.y;
    

    // distance to closest edge
    float2 edgeDist = min(uv, 1.0 - uv);

    float border = step(edgeDist.x, _BorderThickness) +
                   step(edgeDist.y, _BorderThickness);
    
    
    float4 baseColor = MAT_TEX_OVERRIDE.SampleLevel(MAT_TEX_OVERRIDE_SAMPLER, uv, 0);

    if (border > 0)
        return _BorderColor;

    return baseColor;
}