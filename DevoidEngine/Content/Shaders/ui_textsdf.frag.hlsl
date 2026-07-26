struct PSInput
{
    float4 Position : SV_POSITION;
    float2 NDC : TEXCOORD4;
    float2 UV0 : TEXCOORD0;
    float2 LocalPos : TEXCOORD1;
};

#include "./Common/RenderConstants.hlsl"

cbuffer MATERIAL : register(b3)
{
    float4 COLOR;
    float2 RECT_SIZE;
    int SDF_PIXEL_RANGE;
    float _pad;
};

Texture2D MAT_Texture : register(t0);
SamplerState MAT_TextureSampler : register(s0);


float4 PSMain(PSInput input) : SV_TARGET
{
    float distance = MAT_Texture.Sample(MAT_TextureSampler, input.UV0).r;
    float sdfRange = SDF_PIXEL_RANGE;

    float2 uv_dx = ddx(input.UV0);
    float2 uv_dy = ddy(input.UV0);

    float pixel = max(length(uv_dx), length(uv_dy));

    float smoothing = pixel * sdfRange;
    
    float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, distance);

    if (alpha <= 0.01)
        discard;

    return float4(COLOR.rgb, alpha * COLOR.a);
    
}