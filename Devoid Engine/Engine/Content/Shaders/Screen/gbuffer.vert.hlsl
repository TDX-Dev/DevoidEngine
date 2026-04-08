struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 Tangent : TANGENT;
    float3 BiTangent : BINORMAL;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float3 WorldspacePosition : TEXCOORD3;
};

#include "../Common/render_constants.hlsl"


float4x4 inverse(float4x4 m);

PSInput VSMain(VSInput input)
{
    PSInput output;

    
    float3x3 normalMatrix = transpose((float3x3) invModel);

    //float3 N = normalize(mul(normalMatrix, input.Normal));
    
    //output.Normal = N;
    // World position
    float4 worldPos = mul(Model, float4(input.Position, 1.0f));
    output.WorldspacePosition = worldPos.xyz;
    
    // Transform to clip space
    float4 viewPos = mul(View, worldPos);
    output.Position = mul(Projection, viewPos);

    return output;
}