struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
};

cbuffer Material : register(b1)
{
    float4 Albedo;
}

Texture2D Albedo_Texture : register(t0);
SamplerState Albedo_Sampler : register(s0);

float4 PSMain(PSInput input) : SV_Target0
{
    
    return float4(Albedo.xyz + Albedo_Texture.Sample(Albedo_Sampler, float2(0, 0)), 1);
}