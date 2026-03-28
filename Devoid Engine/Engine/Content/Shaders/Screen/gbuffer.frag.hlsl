struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float3 BiTangent : BINORMAL;
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 FragPos : TEXCOORD2;
    float3 WorldPos : TEXCOORD3;
};

struct PSOutput
{
    float3 Position;
    float3 Normal;
};

PSOutput PSMain(PSInput input) : SV_TARGET
{
    PSOutput output;
    output.Position = float3(input.Position.xyz / input.Position.w);
    output.Normal = float3(input.Normal);
    return output;
}