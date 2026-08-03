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

    float2 uv = float2(input.UV.x, 1.0 - input.UV.y);

    float NdotV = max(uv.x, 1e-4);
    float roughness = uv.y;
    //roughness = max(roughness, 0.04);
    
    float2 brdf = IntegrateBRDF(NdotV, roughness);

    return brdf;
}