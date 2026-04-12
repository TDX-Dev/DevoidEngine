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

cbuffer DebugShaderData : register(b1)
{
    float4 Color;
}

#include "../Common/render_constants.hlsl"

float4 PSMain(PSInput input) : SV_TARGET
{
    
    return Color;
}
