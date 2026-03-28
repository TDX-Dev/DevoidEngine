struct PSInput
{
    float4 Position : SV_POSITION;
    float2 NDC : TEXCOORD4;
    float2 UV0 : TEXCOORD0;
};

#include "../Common/render_constants.hlsl"

cbuffer MATERIAL : register(b3)
{
    float4 COLOR;
    float4 CORNER_RADIUS; // TL TR BR BL (pixels)
    float2 RECT_SIZE;
    int useTexture;
    int _pad;
};

Texture2D MAT_fontSDFAtlas : register(t0);
SamplerState MAT_fontSDFAtlasSampler : register(s0);

float4 PSMain(PSInput input) : SV_TARGET
{
    //float distance = MAT_fontSDFAtlas.Sample(MAT_fontSDFAtlasSampler, input.UV0).r;
    //float smoothing = fwidth(distance);
    //float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, distance);
    
    //if (alpha <= 0.01)
    //    discard;
    //return float4(float3(1, 1, 1), alpha);
   
    
    float distance = MAT_fontSDFAtlas.Sample(MAT_fontSDFAtlasSampler, input.UV0).r;

// compute pixel size in UV space
    float2 uv_dx = ddx(input.UV0);
    float2 uv_dy = ddy(input.UV0);
    float pixel = max(length(uv_dx), length(uv_dy));

// scale by atlas resolution
    float smoothing = pixel * 64.0; // adjust depending on atlas spread

    float alpha = smoothstep(0.5 - smoothing, 0.5 + smoothing, distance);

    if (alpha <= 0.01)
        discard;

    return float4(COLOR.rgb, alpha * COLOR.a);
}
