struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float3 BiTangent : BINORMAL;
    float2 UV0 : TEXCOORD0;
    float3 FragPos : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

cbuffer Material : register(b2)
{
    float4 color;
};

Texture2D MAT_Albedo : register(t0);
SamplerState MAT_AlbedoSampler : register(s0);


float4 PSMain(PSInput input) : SV_TARGET
{
    return float4(color + MAT_Albedo.Sample(MAT_AlbedoSampler, input.UV0));
}
