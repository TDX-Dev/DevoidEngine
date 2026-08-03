struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

TextureCube MAT_Skybox : register(t0);
SamplerState MAT_Skybox_Sampler : register(s0);

#include "./Common/MathConstants.hlsl"

float4 PSMain(PSInput input) : SV_Target
{
    
    float3 dir = normalize(input.WorldspacePosition);

    float3 color = MAT_Skybox.Sample(MAT_Skybox_Sampler, dir).rgb;
    return float4(color, 1);
}