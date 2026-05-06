struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
};

float4 PSMain(PSInput input) : SV_Target0
{
    
    return float4(0.5, 1, 0.2, 1);
}