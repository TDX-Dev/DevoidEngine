struct PSInput
{
    float4 Position : SV_POSITION;
    float2 NDC : TEXCOORD4;
    float2 UV0 : TEXCOORD0;
    float2 LocalPos : TEXCOORD1;
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

Texture2D MAT_Texture : register(t0);
SamplerState MAT_TextureSampler : register(s0);


float sdRoundRect(float2 p, float2 halfSize, float r)
{
    float2 q = abs(p) - halfSize + r;
    return length(max(q, 0)) + min(max(q.x, q.y), 0) - r;
}


float4 PSMain(PSInput input) : SV_TARGET
{
    float4 finalColor;

    if (useTexture)
        finalColor = MAT_Texture.Sample(MAT_TextureSampler, input.UV0);
    else
        finalColor = COLOR;

    float2 halfSize = RECT_SIZE * 0.5;
    float2 p = input.LocalPos;
    
    finalColor.a = finalColor.a * COLOR.a;

    // clamp radii so they can't exceed rectangle
    float maxRadius = min(halfSize.x, halfSize.y);
    float4 r = min(CORNER_RADIUS, maxRadius);

    // choose radius based on quadrant
    float radius;

    if (p.x < 0 && p.y > 0)
        radius = r.x; // TL
    else if (p.x > 0 && p.y > 0)
        radius = r.y; // TR
    else if (p.x > 0 && p.y < 0)
        radius = r.z; // BR
    else
        radius = r.w; // BL

    float dist = sdRoundRect(p, halfSize, radius);

    // anti-aliasing
    float aa = fwidth(dist);
    float alpha = smoothstep(aa, -aa, dist);

    finalColor.a *= alpha;

    if (finalColor.a <= 0.001)
        discard;

    return finalColor;
}