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
    float4 CORNER_RADIUS; // TL TR BR BL
    float2 RECT_SIZE;

    float BORDER_THICKNESS;
    float4 BORDER_COLOR;

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
    float4 fillColor;

    if (useTexture)
        fillColor = MAT_Texture.Sample(MAT_TextureSampler, input.UV0);
    else
        fillColor = COLOR;

    float2 halfSize = RECT_SIZE * 0.5;
    float2 p = input.LocalPos;

    // clamp radii
    float maxRadius = min(halfSize.x, halfSize.y);
    float4 r = min(CORNER_RADIUS, maxRadius);

    float radius;

    if (p.x < 0 && p.y > 0)
        radius = r.x;
    else if (p.x > 0 && p.y > 0)
        radius = r.y;
    else if (p.x > 0 && p.y < 0)
        radius = r.z;
    else
        radius = r.w;
    
    if (BORDER_THICKNESS <= 0.0001)
    {
        float dist = sdRoundRect(p, halfSize, radius);

        float aa = fwidth(dist);
        float alpha = smoothstep(aa, -aa, dist);

        fillColor.a *= alpha;

        if (fillColor.a <= 0.001)
            discard;

        return fillColor;
    }

    // OUTER SHAPE
    float distOuter = sdRoundRect(p, halfSize, radius);

    // INNER SHAPE
    float innerRadius = max(radius - BORDER_THICKNESS, 0);
    float distInner = sdRoundRect(p, halfSize - BORDER_THICKNESS, innerRadius);

    float aa = fwidth(distOuter);

    float outerAlpha = smoothstep(aa, -aa, distOuter);
    float innerAlpha = smoothstep(aa, -aa, distInner);

    float borderMask = outerAlpha * (1 - innerAlpha);

    float fillMask = innerAlpha;

    float4 finalColor = 0;

    finalColor += fillColor * fillMask;
    finalColor += BORDER_COLOR * borderMask;

    finalColor.a = max(finalColor.a, BORDER_COLOR.a * borderMask);

    if (finalColor.a <= 0.001)
        discard;

    return finalColor;
}