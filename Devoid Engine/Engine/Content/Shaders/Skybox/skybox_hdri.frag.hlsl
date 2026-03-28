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

TextureCube MAT_Skybox : register(t0);
SamplerState MAT_SkyboxSampler : register(s0);

float4 PSMain(PSInput input) : SV_Target
{
    float2 uv = input.UV0;
    uv.y = 1 - uv.y;
    float2 ndc = uv * 2.0f - 1.0f;

    float4 clip = float4(ndc, 1.0f, 1.0f);

    float4 world = mul(InverseViewProjection, clip);
    world.xyz /= world.w;

    float3 dir = normalize(world.xyz - Position);

    float3 color = MAT_Skybox.Sample(MAT_SkyboxSampler, dir).rgb;

    return float4(color, 1.0f);
}