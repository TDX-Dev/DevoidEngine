struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float4 Tangent : TANGENT; // xyz + handedness
    float2 UV0 : TEXCOORD0;
    float2 UV1 : TEXCOORD1;
    float3 WorldspacePosition : TEXCOORD3;
};

float PSMain(PSInput input) : SV_Target
{
    float d = distance(input.WorldspacePosition, LightPosition);

    return d + fwidth(d);
}