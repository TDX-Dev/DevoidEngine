struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

#include "./Common/RenderConstants.hlsl"

PSInput VSMain(VSInput input)
{
    PSInput output;

    float3 localPos = input.Position;

    // Remove camera translation
    float4x4 view = View;
    view._14 = 0.0;
    view._24 = 0.0;
    view._34 = 0.0;

    float3 viewPos = mul((float3x3) View, localPos);
    float4 clip = mul(Projection, float4(viewPos, 1.0));

    // Force the skybox to the far plane
    clip.z = clip.w;

    output.Position = clip;

    // Cubemap lookup direction
    output.WorldspacePosition = localPos;

    output.UV = input.UV;

    return output;
}