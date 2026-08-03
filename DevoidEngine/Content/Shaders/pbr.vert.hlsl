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
    float4 worldPos = mul(Model, float4(input.Position, 1.0));
    output.Position = mul(Projection, mul(View, worldPos));
    output.WorldspacePosition = worldPos.xyz;
    output.UV = input.UV;
    
    float3x3 normalMatrix = transpose((float3x3) invModel);
    float3 N = mul(normalMatrix, input.Normal);
    float3 T = normalize(mul(normalMatrix, input.Tangent.xyz));
    //float3 B = normalize(mul(normalMatrix, input.BiTangent));
    //float handedness = (dot(cross(N, T), B) < 0.0) ? -1.0 : 1.0;
    
    output.Normal = N;
    output.Tangent = float4(T, input.Tangent.w);
    
    return output;
}

//PSInput VSMain(VSInput input)
//{
//    PSInput output;

//    float4 worldPos = mul(Model, float4(input.Position, 1.0));
//    output.Position = mul(Projection, mul(View, worldPos));
//    output.WorldspacePosition = worldPos.xyz;

//    float3x3 normalMatrix = transpose((float3x3) invModel);

//    float3 N = normalize(mul(normalMatrix, input.Normal));
//    float3 T = normalize(mul(normalMatrix, input.Tangent));
//    float3 B = normalize(mul(normalMatrix, input.BiTangent));
//    float handedness = (dot(cross(N, T), B) < 0.0) ? -1.0 : 1.0;

//    output.Normal = N;
//    output.Tangent = float4(T, handedness);

//    output.UV0 = input.UV0;
//    output.UV1 = input.UV1;

//    return output;
//}