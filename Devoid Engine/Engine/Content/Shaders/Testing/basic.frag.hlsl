struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz = tangent, w = handedness
    float3 BiTangent : BINORMAL;
    float2 UV1 : TEXCOORD0;
    float3 FragPos : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

float4 PSMain(PSInput input) : SV_TARGET
{
    return float4(input.UV1.x, input.UV1.y, 1, 1);
}
