struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 NormalViewSpace : TEXCOORD2;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
    uint ProbeIndex : TEXCOORD3;
};

#include "./Common/RenderConstants.hlsl"
#include "./Common/ProbeGI.hlsl"

float4 PSMain(PSInput input) : SV_TARGET
{
    float4 probeInfo = ProbeInfoImage.Load(
        int4(input.ProbeIndex % (uint) ProbeGISettings.ProbeCount.x, (input.ProbeIndex / (uint) ProbeGISettings.ProbeCount.x) % (uint) ProbeGISettings.ProbeCount.y, input.ProbeIndex / ((uint) ProbeGISettings.ProbeCount.x * (uint) ProbeGISettings.ProbeCount.y), 0)
    );

    float3 offset = probeInfo.xyz;
    float validity = probeInfo.w;

    // Visualize offset direction.
    float3 color = offset * 0.5f + 0.5f;

    return float4(color, 1.0f);
}