struct VSInput
{
    float3 Position : POSITION;
    float3 Normal : NORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 Tangent : TANGENT;
    float3 BiTangent : BINORMAL; // not needed, but kept if mesh provides it
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz + handedness
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 WorldspacePosition : TEXCOORD3;
};

#include "../Common/render_constants.hlsl"

PSInput VSMain(VSInput input)
{
    PSInput output;

    float4 worldPos = mul(Model, float4(input.Position, 1.0));
    output.Position = mul(Projection, mul(View, worldPos));
    output.WorldspacePosition = worldPos.xyz;

    float3x3 normalMatrix = transpose((float3x3) invModel);

    float3 N = normalize(mul(normalMatrix, input.Normal));
    float3 T = normalize(mul(normalMatrix, input.Tangent));
    float3 B = normalize(mul(normalMatrix, input.BiTangent));
    float handedness = (dot(cross(N, T), B) < 0.0) ? -1.0 : 1.0;

    output.Normal = N;
    output.Tangent = float4(T, handedness);

    output.UV0 = input.UV0;
    output.UV1 = input.UV1;

    return output;
}