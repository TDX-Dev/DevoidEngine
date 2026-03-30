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
    float4 CORNER_RADIUS;
    float4 BORDER_COLOR;

    float2 RECT_SIZE;
    float BORDER_THICKNESS;
    float DASH_SIZE;

    float GAP_SIZE;
    int DEBUG_DASHED;
    int useTexture;
    float _pad;
};


float sdRoundRect(float2 p, float2 halfSize, float r)
{
    float2 q = abs(p) - halfSize + r;
    return length(max(q, 0)) + min(max(q.x, q.y), 0) - r;
}


//float DashPattern(float2 p)
//{
//    float coord;

//    // choose edge direction
//    if (abs(p.x) > abs(p.y))
//        coord = p.y;
//    else
//        coord = p.x;

//    float period = DASH_SIZE + GAP_SIZE;

//    float m = fmod(abs(coord), period);

//    return step(m, DASH_SIZE);
//}

float DashPattern(float2 p, float2 halfSize)
{
    float coord;

    float dx = halfSize.x - abs(p.x);
    float dy = halfSize.y - abs(p.y);

    // choose nearest edge
    if (dx < dy)
    {
        // vertical edge → dash along Y
        coord = p.y;
    }
    else
    {
        // horizontal edge → dash along X
        coord = p.x;
    }

    float period = DASH_SIZE + GAP_SIZE;

    float m = fmod(abs(coord), period);

    return step(m, DASH_SIZE);
}


float4 PSMain(PSInput input) : SV_TARGET
{
    float2 halfSize = RECT_SIZE * 0.5;
    float2 p = input.LocalPos;

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

    // outer SDF
    float distOuter = sdRoundRect(p, halfSize, radius);

    // inner SDF
    float innerRadius = max(radius - BORDER_THICKNESS, 0);
    float distInner = sdRoundRect(p, halfSize - BORDER_THICKNESS, innerRadius);

    float aa = fwidth(distOuter);

    float outerAlpha = smoothstep(aa, -aa, distOuter);
    float innerAlpha = smoothstep(aa, -aa, distInner);

    float borderMask = outerAlpha * (1 - innerAlpha);

    if (DEBUG_DASHED == 1)
    {
        borderMask *= DashPattern(p, halfSize);
    }

    float fillMask = innerAlpha;

    float4 color = 0;

    color += COLOR * fillMask;
    color += BORDER_COLOR * borderMask;

    color.a = max(color.a, BORDER_COLOR.a * borderMask);

    if (color.a <= 0.001)
        discard;

    return color;
}