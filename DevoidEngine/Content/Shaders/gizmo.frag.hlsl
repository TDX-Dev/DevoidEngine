struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 NormalViewSpace : TEXCOORD2;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

cbuffer Material : register(b4)
{
    float4 Color;
    float2 UVMin;
    float2 UVMax;
};

#include "./Common/RenderConstants.hlsl"

Texture2D<float> DepthTexture : register(t0);
Texture2D<float4> Texture : register(t1);

SamplerState PointSampler
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 PSMain(PSInput input) : SV_Target0
{
    float2 screenUV = input.Position.xy / ScreenSize;

    float sceneDepth = DepthTexture.Sample(
        PointSampler,
        screenUV
    );

    float gizmoDepth = input.Position.z;

    //// Remap primitive UV into the material's UV rectangle.
    //float2 uv = lerp(UVMin, UVMax, input.UV);

    float4 textureColor = Texture.Sample(
        PointSampler,
        screenUV
    );

    float4 color = textureColor * Color;

    if (gizmoDepth > sceneDepth)
    {
        color.a *= 0.5;
    }

    return color;
}