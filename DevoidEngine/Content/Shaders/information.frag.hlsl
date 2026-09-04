struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 NormalViewSpace : TEXCOORD2;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
    float3 ViewspacePosition : TEXCOORD3;
};

struct PSOutput
{
    float4 NormalViewSpace : SV_RenderTarget0;
    float Identifier : SV_RenderTarget1;
    float ViewDepth : SV_RenderTarget2;
};

#include "./Common/RenderConstants.hlsl"

PSOutput PSMain(PSInput input) : SV_Target0
{
    PSOutput output;
    output.Identifier = UniqueIdentifier;
    output.NormalViewSpace = float4(input.NormalViewSpace, 1);
    output.ViewDepth = input.ViewspacePosition.z;
    return output;
}