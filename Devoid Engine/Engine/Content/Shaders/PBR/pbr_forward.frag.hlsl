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

cbuffer Material : register(b3)
{
    float4 Albedo;
};

#include "../Common/render_constants.hlsl"
#include "../Common/light_constructs.hlsl"

Texture2D MAT_TEX_OVERRIDE : register(t0);
SamplerState MAT_TEX_OVERRIDE_SAMPLER : register(s0);

float4 PSMain(PSInput input) : SV_TARGET
{
    // Normalize interpolated normal
    float3 normal = normalize(input.Normal);

    // View direction (camera position must exist in your camera cbuffer)
    float3 viewDir = normalize(Position - input.WorldPos);

    // Base color from material
    float3 albedo = Albedo.rgb;

    // Compute lighting
    float3 lighting = ComputeLighting(
        input.WorldPos,
        normal,
        viewDir,
        albedo);
    
    return float4(lighting, 1);// + MAT_TEX_OVERRIDE.SampleLevel(MAT_TEX_OVERRIDE_SAMPLER, input.UV0, 0);
}
