struct PSInput
{
    float4 Position : SV_POSITION;
    float2 NDC : TEXCOORD4;
    float2 UV0 : TEXCOORD0;
};

#include "../Common/render_constants.hlsl"

cbuffer MATERIAL
{
    float4 COLOR;
    int useTexture;
    int3 pad;
};

Texture2D MAT_Texture : register(t0);
SamplerState MAT_TextureSampler : register(s0);

float4 PSMain(PSInput input) : SV_TARGET
{   
    float4 finalColor = float4(1,1,1,1);
    if (useTexture)
    {
        finalColor = MAT_Texture.Sample(MAT_TextureSampler, input.UV0);
    }
    else
    {
        finalColor = COLOR;
    }
    return finalColor;

}
