cbuffer Shadow : register(b0)
{
    float4x4 Model;
    float4x4 LightVP;

    float3 LightPosition;
    float LightRange;
}

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


PSInput VSMain(VSInput input)
{
    PSInput o;

    float4 worldPos = mul(Model, float4(input.Position, 1));

    o.WorldspacePosition = worldPos.xyz;

    o.Position = mul(LightVP, worldPos);

    return o;
}