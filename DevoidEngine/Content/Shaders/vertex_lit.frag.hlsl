struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD3;
};

cbuffer Material : register(b1)
{
    float4 Albedo;
}

Texture2D Albedo_Texture : register(t0);
SamplerState Albedo_Sampler : register(s0);

float4 PSMain(PSInput input) : SV_Target0
{
    
    return float4(Albedo.xyz, 1);
}