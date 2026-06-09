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

PSInput VSMain(VSInput input)
{
    PSInput output;
    output.Position = float4(input.Position.x, input.Position.y, 0, 1);
    output.UV = input.UV;
    
    return output;
}