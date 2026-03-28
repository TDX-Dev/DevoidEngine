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
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float3 BiTangent : BINORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 FragmentPosition : TEXCOORD2;
    float3 WorldspacePosition : TEXCOORD3;
};


#include "../Common/render_constants.hlsl"


PSInput VSMain(VSInput input)
{
    PSInput output = (PSInput) 0;

    float4 worldPos = mul(Model, float4(input.Position, 1.0f));
    float4 viewPos = mul(View, worldPos);

    output.Position = mul(Projection, viewPos);

    // Pass world position
    output.WorldspacePosition = worldPos.xyz;

    // Transform normal to world space
    float3 worldNormal = mul((float3x3) Model, input.Normal);
    output.Normal = normalize(worldNormal);

    output.UV0 = input.UV0;
    output.UV1 = input.UV1;

    return output;
}
