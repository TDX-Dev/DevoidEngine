struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

#include "./Common/PBRMethods.hlsl"

float2 PSMain(PSInput input) : SV_Target
{

    float2 uv = float2(input.UV.x, input.UV.y);

    float NdotV = uv.x;
    float roughness = uv.y;
    float linearRoughness = roughness * roughness;
    
    float2 brdf = IntegrateBRDF_Multiscatter(NdotV, linearRoughness, 1024u);

    return brdf;
}