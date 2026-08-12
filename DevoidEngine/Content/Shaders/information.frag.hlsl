struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 NormalViewSpace : TEXCOORD2;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

struct PSOutput
{
    float4 NormalViewSpace : SV_RenderTarget0;
    float Identifier : SV_RenderTarget1;
};

#include "./Common/RenderConstants.hlsl"

PSOutput PSMain(PSInput input) : SV_Target0
{
    PSOutput output;
    output.Identifier = UniqueIdentifier;
    output.NormalViewSpace = float4(input.NormalViewSpace, 1);
    return output;
}