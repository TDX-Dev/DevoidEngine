struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
};

cbuffer Hello : register(b1)
{
    uint HelloWorld;
};

float4 PSMain(PSInput input) : SV_Target0
{
    
    return float4(HelloWorld,0,0, 1);
}