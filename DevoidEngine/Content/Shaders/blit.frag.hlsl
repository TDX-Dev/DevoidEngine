struct PSInput
{
    float4 Position : SV_POSITION;
    float3 Normal : NORMAL;
    float2 UV : TEXCOORD0;
    float4 Tangent : TANGENT;
    float3 WorldspacePosition : TEXCOORD1;
};

Texture2D ScreenTexture : register(t0);
SamplerState ScreenTextureSampler : register(s0);

float4 PSMain(PSInput input) : SV_Target0
{
    
    return ScreenTexture.Sample(ScreenTextureSampler, input.UV);

}