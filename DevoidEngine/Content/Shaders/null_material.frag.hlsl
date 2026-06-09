struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

float4 PSMain(PSInput input) : SV_Target0
{
    
    return float4(0, 0.5, 1, 1);
}